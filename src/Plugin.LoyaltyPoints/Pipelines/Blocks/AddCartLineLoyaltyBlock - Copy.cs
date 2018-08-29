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
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    class AllocateCouponBlock: PipelineBlock<AllocateCouponArgument, AllocateCouponArgument, CommercePipelineExecutionContext>
    {
        private readonly FindEntityCommand _findEntityCommmand;
        private readonly PersistEntityCommand _persistEntityCommand;

        public AllocateCouponBlock(FindEntityCommand findEntityCommand, PersistEntityCommand persistEntityCommand)
        {
            _findEntityCommmand = findEntityCommand;
            _persistEntityCommand = persistEntityCommand;
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


            string id = "Entity-LoyaltyPoints";
            LoyaltyPointsEntity entity = await _findEntityCommmand.Process(context.CommerceContext, typeof(LoyaltyPointsEntity), id, shouldCreate:true) as LoyaltyPointsEntity;


            entity.Id = id;  // Entity is generated with random ID. We want a singleton.
            entity.Lock = true;  // This will prevent duplicate batches from getting created. TODO put access and lock in a transaction.


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
}
}
