using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Entities;
using Plugin.LoyaltyPoints.Helpers;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.SQL;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    /// <summary>
    /// Ensures list <see cref="Constants.AvailableCouponsList"/> has number of coupons specified in <see cref="policy.ReprovisionTriggerCount"/>.
    ///
    /// Note: By running update in a transaction, this block guards against race conditions. If two processors ran this minion, the 
    /// </summary>
    class CreateCouponsBlock : PipelineBlock<CreateCouponsArgument, CreateCouponsArgument, CommercePipelineExecutionContext>
    {
        private readonly GetManagedListCommand _getManagedListCommand;
        private readonly CreateManagedListCommand _createManagedListCommand;
        private readonly AddListEntitiesPipeline _addListEntitiesPipeline;
        private readonly GetListCountCommand _getListCountCommand;
        private readonly FindEntityCommand _findEntityCommand;
        private readonly DuplicatePromotionCommand _duplicatePromotionCommand;
        private readonly AddPrivateCouponCommand _addPrivateCouponCommand;
        private readonly NewCouponAllocationCommand _newCouponAllocationCommand;
        private readonly PersistEntityCommand _persistEntityCommand;
        private readonly GetEntitiesInListCommand _getEntitiesInListCommand;
        

        public CreateCouponsBlock(
            GetManagedListCommand getManagedListCommand,
            CreateManagedListCommand createManagedListCommand,
            GetListCountCommand getListCountCommand,
            FindEntityCommand findEntityCommand,
            DuplicatePromotionCommand duplicatePromotionCommand,
            AddPrivateCouponCommand addPrivateCouponCommand,
            NewCouponAllocationCommand newCouponAllocationCommand,
            AddListEntitiesPipeline addListEntitiesPipeline,
            PersistEntityCommand persistEntityCommand,
            GetEntitiesInListCommand getEntitiesInListCommand
            )
        {
            _getManagedListCommand = getManagedListCommand;
            _createManagedListCommand = createManagedListCommand;
            _getListCountCommand = getListCountCommand;
            _findEntityCommand = findEntityCommand;
            _duplicatePromotionCommand = duplicatePromotionCommand;
            _addPrivateCouponCommand = addPrivateCouponCommand;
            _newCouponAllocationCommand = newCouponAllocationCommand;
            _addListEntitiesPipeline = addListEntitiesPipeline;
            _persistEntityCommand = persistEntityCommand;
            _getEntitiesInListCommand = getEntitiesInListCommand;
        }

 
        public override async Task<CreateCouponsArgument> Run(
            CreateCouponsArgument arg,
            CommercePipelineExecutionContext context)
        {
            await this._getManagedListCommand.PerformTransaction(context.CommerceContext, async () =>
            {
                var policy = context.CommerceContext.GetPolicy<LoyaltyPointsPolicy>();

                LoyaltyPointsEntity loyaltyPointsEntity = await _findEntityCommand.Process(context.CommerceContext,typeof(LoyaltyPointsEntity),
                            Constants.EntityId,shouldCreate: true) as LoyaltyPointsEntity;
                if (loyaltyPointsEntity == null)
                {
                    await context.AbortWithError("Unable to access or create LoyaltyPointsEntity {0}",
                        "LoyaltyPointsEntityNotReturned", Constants.EntityId);
                    return;
                }

                // Prevent simultaneous updates, in case multiple minion instances. Since these might be on scaled servers, mutex lock uses database field. 
                // Lock is read before count is checked, so that when first process finishes and releases row, counts will be replenished.
                // Notes:
                // 1. If this pipeline aborts, the lock should be rolled back.
                // 2. Assuming transactions are enabled, the second process should never see IsLocked=true, as the read won't return until the lock is released. However,
                //    this syntax should work on a non-transactional environment, and makes the intent of the code clearer.
                if (loyaltyPointsEntity.IsLocked)
                {
                    await context.AbortWithError("{0} is locked. If this condition persists, unset this value through the database.", "EntityLocked", Constants.EntityId);
                    return;
                }
                
                var list = await EnsureList(context, Constants.AvailableCouponsList);
                if (list == null)
                {
                    await context.AbortWithError("Unable to create list {0}", "UnableToCreateList", Constants.AvailableCouponsList);
                    return;
                }

                long count = await _getListCountCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                context.Logger.LogDebug($"{this.Name}: List {Constants.AvailableCouponsList} has {count} items.");

                

                if (count <= policy.ReprovisionTriggerCount)
                {
                    loyaltyPointsEntity.IsLocked = true;
                    await _persistEntityCommand.Process(context.CommerceContext, loyaltyPointsEntity);

                    context.Logger.LogDebug($"{this.Name}: List {Constants.AvailableCouponsList} is at or under reprovision count of {policy.ReprovisionTriggerCount}.");
                    
                    loyaltyPointsEntity.SequenceNumber++;
                    string suffix = loyaltyPointsEntity.SequenceNumber.ToString();

                    string promotionName = string.Format(Constants.GeneratedPromotion, suffix);
                    Promotion promotion = await GeneratePromotion(context, promotionName);
                    if (promotion == null)
                    {
                        await context.AbortWithError("Unable to generate promotion {0}.", "PromotionNotFound",
                            promotionName);
                        return;
                    }

                    await AddCoupons(context, promotion, suffix);
                    if (context.IsNullOrHasErrors())
                    {
                        return;
                    }

                    await AllocateCoupons(context, promotion, suffix);
                    if (context.IsNullOrHasErrors())
                    {
                        return;
                    }
                     
                    ApprovePromotion(context, promotion);

                    await CopyCouponsToList(context, loyaltyPointsEntity, list);
                    if (context.IsNullOrHasErrors())
                    {
                        return;
                    }

                    loyaltyPointsEntity.IsLocked = false;
                    await _persistEntityCommand.Process(context.CommerceContext, list);
                    await _persistEntityCommand.Process(context.CommerceContext, promotion);
                    await _persistEntityCommand.Process(context.CommerceContext, loyaltyPointsEntity);
                }
            });
            

            return arg;
        }

        private async Task<ManagedList> EnsureList(CommercePipelineExecutionContext context, string listName)
        {
            ManagedList list = await _getManagedListCommand.Process(context.CommerceContext, listName);
            if (list == null)
            {
                context.Logger.LogDebug($"{this.Name}: List {listName} not found. Creating it.");
                list = await _createManagedListCommand.Process(context.CommerceContext, listName);
            }

            return list;
        }

        /// <summary>
        /// Create a new promotion so that coupons can be provisioned.
        /// Coupons cannot be added to an approved promotion.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="promotionName"></param>
        /// <returns></returns>
        private async Task<Promotion> GeneratePromotion(CommercePipelineExecutionContext context, string promotionName)
        {
            string rootPromotion = context.GetPolicy<LoyaltyPointsPolicy>().TemplatePromotionFriendlyId;

            Promotion promotion = await this._duplicatePromotionCommand.Process(context.CommerceContext, rootPromotion, promotionName);
            if (context.IsNullOrHasErrors())
            {
                return null;
            }
            return promotion;
        }

        private async Task AddCoupons(CommercePipelineExecutionContext context, Promotion promotion, string suffix)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
       
            await _addPrivateCouponCommand.Process(
                context.CommerceContext,
                promotion.Id,
                policy.CouponPrefix,
                suffix,
                policy.CouponBlockSize);
        }

        /// <summary>
        /// Coupons must be in allocated to be usable by customers.
        /// </summary>
        private async Task  AllocateCoupons(CommercePipelineExecutionContext context, Promotion promotion, string suffix)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();

            string privateCouponGroupId = $"{CommerceEntity.IdPrefix<PrivateCouponGroup>()}{policy.CouponPrefix}-{suffix}";
            var privateCouponGroup = await _findEntityCommand.Process(context.CommerceContext,
                typeof(PrivateCouponGroup),
                privateCouponGroupId) as PrivateCouponGroup;

            if (privateCouponGroup == null)
            {
                await context.AbortWithError("Unable to find PrivateCouponGroup { 0}.", "PrivateCouponGroupNotFound", privateCouponGroupId);
                return;
            }
            
            await _newCouponAllocationCommand.Process(context.CommerceContext, promotion, privateCouponGroup,
                policy.CouponBlockSize);
        }

        private async Task CopyCouponsToList(CommercePipelineExecutionContext context, LoyaltyPointsEntity entity, ManagedList targetList)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            string sourceListName = $"promotion-{policy.CouponPrefix}-{entity.SequenceNumber}-allocatedcoupons";
            var coupons = await _getEntitiesInListCommand.Process(context.CommerceContext,
                sourceListName, 0,
                policy.CouponBlockSize);
            await _addListEntitiesPipeline.Run(new ListEntitiesArgument(coupons, targetList.Name), context);
        }

        private static void ApprovePromotion(CommercePipelineExecutionContext context, Promotion promotion)
        {
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
        }
    }
}
