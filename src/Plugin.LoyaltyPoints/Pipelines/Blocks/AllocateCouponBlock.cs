using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Pipelines.Entities;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Promotion = Sitecore.Commerce.Plugin.Promotions.Promotion;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{

  
    /// <summary>
    /// Design notes:
    /// This minion is responsible for processing customers, and
    /// issuing them coupons, and marking the loyalty points applied.
    ///
    /// Logically, it depends on a secondary command or pipeline:
    /// GetLoyaltyPointsCoupon, which is responsible for getting the next
    /// coupon from a queue.
    ///
    /// This could show best practices for managing work queues (managed lists?),
    /// but that is an extra.
    ///
    /// So:
    /// Get Customer. Pass Customer to command to determine eligibility for
    /// coupon.
    /// If yes, pass customer to command to command to get coupon. [Option: prime
    /// process to ensure coupon.]
    /// Within transaction:
    ///   * mark coupon applied by writing a component to the coupon entity
    ///     that has the customer id,
    ///   * update orders to mark loyalty points applied (include coupon reference)
    ///   * add component to customer that contains coupon
    /// Outside of transaction:
    ///   * fire live event, mark customer.couponcollection applied.
    ///
    /// So new pipelines:
    ///   * IsCustomerEligibleForCoupon (and block)
    ///   * GetCouponPipeline (and block, maybe broken up)
    ///   * ApplyCouponBlock
    ///
    /// An alternative approach:
    ///   * MakeCouponMinion (makes sure there is a queue of coupons)
    ///   * FindCustomersForLoyaltyPoints (adds a component with coupons to apply)
    ///   * IssueCouponsMinion
    ///
    /// This second approach looks cleaner. And each minion can be scaled as needed.
    /// All the minions would use Entity-LoyaltyPoints to control things like the
    /// template promotion.
    ///
    /// </summary>
    class AllocateCouponBlock: PipelineBlock<AllocateCouponArgument, AllocateCouponArgument, CommercePipelineExecutionContext>
    {
        private readonly FindEntityCommand _findEntityCommmand;
        private readonly PersistEntityCommand _persistEntityCommand;
        private DuplicatePromotionCommand _duplicatePromotionCommand;
        private readonly AddPrivateCouponCommand _addPrivateCouponCommand;
        private readonly NewCouponAllocationCommand _newCouponAllocationCommand;

        public AllocateCouponBlock(FindEntityCommand findEntityCommand, PersistEntityCommand persistEntityCommand, DuplicatePromotionCommand duplicatePromotionCommand, AddPrivateCouponCommand addPrivateCouponCommand, NewCouponAllocationCommand newCouponAllocationCommand)
        {
            _findEntityCommmand = findEntityCommand;
            _persistEntityCommand = persistEntityCommand;
            _duplicatePromotionCommand = duplicatePromotionCommand;
            _addPrivateCouponCommand = addPrivateCouponCommand;
            _newCouponAllocationCommand = newCouponAllocationCommand;
        }
        public override async Task<AllocateCouponArgument> Run(AllocateCouponArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: {nameof(arg)} cannot be null.");

            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            var logger = context.Logger;

            var orderLinesWithLoyaltyPoints =
                arg.Orders.SelectMany(o => o.Lines.Where(l => l.HasComponent<LoyaltyPointsComponent>()));
            
            orderLinesWithLoyaltyPoints.ForEach(
                       l =>
                       {

                           var lp = l.GetComponent<LoyaltyPointsComponent>();

                           logger.LogInformation(
                               $"{this.Name}: Product {l.ItemId}, quanity {l.Quantity}, Loyalty Points ID {lp.Id} per single {lp.Points} extended {lp.Points * l.Quantity}");
                       });


            LoyaltyPointsEntity entity = await 
                _findEntityCommmand.Process(
                    context.CommerceContext, 
                    typeof(LoyaltyPointsEntity), 
                    CommerceEntity.IdPrefix<LoyaltyPointsEntity>(),
                    shouldCreate:true)
                as LoyaltyPointsEntity;


            entity.Id = CommerceEntity.IdPrefix<LoyaltyPointsEntity>();  // Entity is generated with random ID. We want a singleton.
            entity.Lock = true;  // This will prevent duplicate batches from getting created. TODO put access and lock in a transaction.
            Promotion promotion;
            if (string.IsNullOrEmpty(entity.CurrentPromotion))
            {
                promotion = await CreatePromtion(context.CommerceContext, entity.SequenceNumber++);
                entity.CurrentPromotion = promotion.FriendlyId;
                promotion = await _addPrivateCouponCommand.Process(context.CommerceContext, promotion.Id, policy.CouponPrefix,
                    entity.SequenceNumber.ToString(), policy.CouponBlockSize);
            }
            else
            {
                promotion = await GetPromotion(context.CommerceContext, entity.CurrentPromotion);
            }

            string entityId = $"Entity-PrivateCouponGroup-{policy.CouponPrefix}-{entity.SequenceNumber}";
            PrivateCouponGroup @group= await _findEntityCommmand.Process(context.CommerceContext,typeof(PrivateCouponGroup), entityId) as PrivateCouponGroup;
            await _newCouponAllocationCommand.Process(context.CommerceContext, promotion, group, 1);
            var allocated = group.GetComponent<CouponAllocationComponent>();
            string coupon = allocated.Codes.First(); //This is always returning the same code.

             

            //NEXT: Each call to CouponAllocationCommand is creating a new CouponAlloctionComponent (which is contrary to how I thought
            // components worked (like properties). The required logic, which seems ugly, to get the last promotion is:
            // group.Components.OfType<CouponAllocationComponent>().Skip(AllocationCount -1).First()
            // and to see if any are left:
            // Total - Allocation > 0
            // I'm not sure how to avoid a race condition between two processes: Maybe a read lock on the Group??
            await _persistEntityCommand.Process(context.CommerceContext, group);
             
            
         

            entity = await _persistEntityCommand.Process(context.CommerceContext, entity) as LoyaltyPointsEntity;

              

            // Discovery, this should go to its own pipeline eventually
            // TODO Mark loyalty points applied, generate coupon, notifiy xConnect (Get contact ID, create event, pass coupon ID)
            //	 
            string promotionId = policy.TemplatePromotion;

            string prefix = policy.CouponPrefix;
            string suffix = $"_{DateTime.Now.Ticks.ToString().Substring(0, 9)}";
            int total = policy.CouponBlockSize;


            // create 1 (initially), allocate it. 

            // later: create batch if needed, allocate first off of unallocated list.
            //var coupon = await
            //    _addPrivateCouponCommand.Process(context.CommerceContext, promotionId, prefix, suffix, total);
            //var privateCouponGroup =
            //    new PrivateCouponGroup { Id = $"Entity-PrivateCouponGroup-{prefix}-{suffix}" };
            //privateCouponGroup.FriendlyId = privateCouponGroup.Id;
            ////   var r = await _newCouponAllocationCommand.Process(context.CommerceContext, coupon,
            ////       privateCouponGroup, 5);

            //var model = context.CommerceContext.GetModel<PersistedEntityModel>();
            //logger.LogInformation(
            //    coupon != null ? $"{Name}: Coupon (ID:{model.EntityFriendlyId}) created for customer {customer.Id}"
            //    : "Coupon not created.");
            //throw new NotImplementedException();

            return arg;
        }

        private async Task<Promotion> GetPromotion(CommerceContext context, string currentPromotion)
        {
            return await this._findEntityCommmand.Process(context, typeof(Promotion), $"Entity-Promotion-{currentPromotion}") as Promotion;
        }

        private async Task<Promotion> CreatePromtion(CommerceContext context, int sequenceNumber)
        {
            string templatePromotion = context.GetPolicy<LoyaltyPointsPolicy>().TemplatePromotion;
            string suffix = $"-{sequenceNumber}";
            string newPromotion = $"{templatePromotion.Substring(50-suffix.Length)}{suffix}";
            return await this._duplicatePromotionCommand.Process(context,templatePromotion, newPromotion) as Promotion;
        }
    }
}
