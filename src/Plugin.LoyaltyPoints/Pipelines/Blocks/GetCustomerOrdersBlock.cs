using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Blocks.GetCustomerOrders")]
    class GetCustomerOrdersBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
 
        private readonly IAllocateCouponPipeline _allocateCouponPipeline;

        public GetCustomerOrdersBlock(FindEntitiesInListCommand findEntitiesInListCommand, IAllocateCouponPipeline allocateCouponPipeline)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
       
            _allocateCouponPipeline = allocateCouponPipeline;
        }

        public async override Task<Customer> Run(Customer customer, CommercePipelineExecutionContext context)
        {
            var logger = context.Logger;
            logger.LogInformation(
                $"{this.Name}: Getting orders for Customer ID {customer.Id} {customer.Email}.");
            
            string listName = string.Format(context.GetPolicy<KnownOrderListsPolicy>().CustomerOrders, customer.Id);
            CommerceList<Order> commerceList = await _findEntitiesInListCommand.Process<Order>(context.CommerceContext, listName, 0, int.MaxValue);

            

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
                    //TODO NEXT SendCoupon pipeline (EnsureCoupons, Allocate, Notify)
                    /// use IDuplicatePromotionPipeline to create new promotion
                    /// 
                    //TODO LATER Validate LP count, mark LPs applied, handle within transaction
                    var result = await this._allocateCouponPipeline.Run(new AllocateCouponArgument(customer, commerceList.Items),
                        context);

                    // TODO Move to block.
                   
                }
            }
            return customer;
        }
    }
}
