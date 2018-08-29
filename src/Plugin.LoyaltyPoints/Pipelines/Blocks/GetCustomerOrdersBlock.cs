using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Blocks.GetCustomerOrders")]
    class GetCustomerOrdersBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        private readonly AddPrivateCouponCommand _addPrivateCouponCommand;
        private readonly NewCouponAllocationCommand _newCouponAllocationCommand;

        public GetCustomerOrdersBlock(FindEntitiesInListCommand findEntitiesInListCommand, AddPrivateCouponCommand addPrivateCouponCommand, NewCouponAllocationCommand newCouponAllocationCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _addPrivateCouponCommand = addPrivateCouponCommand;
            _newCouponAllocationCommand = newCouponAllocationCommand;
        }

        public async override Task<Customer> Run(Customer customer, CommercePipelineExecutionContext context)
        {
            var logger = context.Logger;
            logger.LogInformation(
                $"{this.Name}: Getting orders for Customer ID {customer.Id} {customer.Email}.");
            
            string listName = string.Format(context.GetPolicy<KnownOrderListsPolicy>().CustomerOrders, customer.Id);
            CommerceList<Order> commerceList = await _findEntitiesInListCommand.Process<Order>(context.CommerceContext, listName, 0, int.MaxValue);

            var loyaltyPointsPolicy = context.GetPolicy<LoyaltyPointsPolicy>();

            foreach (var order in commerceList.Items)
            {
                logger.LogInformation(
                    $"{this.Name}: Order {order.Id} found, placed on {order.OrderPlacedDate}");
  
                var linesWithLoyaltyPoints =
                    order.Lines.Where(l => l.ChildComponents.Any(c => c is LoyaltyPointsComponent)).ToList();
                if (!linesWithLoyaltyPoints.Any())
                {
                    logger.LogInformation($"{this.Name}: No Loyalty Points on oder");
                }
                else
                {
                    linesWithLoyaltyPoints.ForEach(
                        l =>
                        {

                            var lp = l.GetComponent<LoyaltyPointsComponent>();

                            logger.LogInformation(
                                $"{this.Name}: Product {l.ItemId}, quanity {l.Quantity}, Loyalty Points ID {lp.Id} per single {lp.Points} extended {lp.Points * l.Quantity}");
                        });
                    // Discovery, this should go to its own pipeline eventually
                    // TODO Mark loyalty points applied, generate coupon, notifiy xConnect (Get contact ID, create event, pass coupon ID)
                    //	 
                    string promotionId = loyaltyPointsPolicy.TemplatePromotion;

                    string prefix = loyaltyPointsPolicy.CouponPrefix;
                    string suffix = $"_{DateTime.Now.Ticks.ToString().Substring(0,9)}";
                    int total = 1; //TODO Discuss how this could be batched.


                    // create 1 (initially), allocate it. 

                    // later: create batch if needed, allocate first off of unallocated list.
                    var coupon = await
                        _addPrivateCouponCommand.Process(context.CommerceContext, promotionId, prefix, suffix, total);
                    var privateCouponGroup =
                        new PrivateCouponGroup {Id = $"Entity-PrivateCouponGroup-{prefix}-{suffix}"};
                    privateCouponGroup.FriendlyId = privateCouponGroup.Id;
                 //   var r = await _newCouponAllocationCommand.Process(context.CommerceContext, coupon,
                 //       privateCouponGroup, 5);

                    var model= context.CommerceContext.GetModel<PersistedEntityModel>();
                    logger.LogInformation(
                        coupon != null ? $"{Name}: Coupon (ID:{model.EntityFriendlyId}) created for customer {customer.Id}"
                        : "Coupon not created.");
                }
            }
            return customer;
        }
    }
}
