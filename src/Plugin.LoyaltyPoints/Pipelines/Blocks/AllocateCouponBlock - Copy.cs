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
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines; 

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{


    /// <summary>
    /// This pipeline interates all orders for customer,
    /// finds qualifying ordrers (based on policy method),
    /// finds qualifying order lines (e.g. no returns, etc.),
    /// again using policy logic.
    /// 
    /// It takes the remaining loyalty point entities,
    /// calculates the total sum, markes them as applied, and
    /// updates a customer total component, which lists
    /// total entity points, and entity points for which
    /// coupons have been issued.
    /// A last processed date is kept, so that the policy can
    /// govern how frequently customers are processed (e.g.
    /// every 5 days).
     
    /// Note: I will assume the minion is calling the pipeline in a transaction.
    /// </summary>
    class ProcessCustomerBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public ProcessCustomerBlock(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }
        public async override Task<Customer> Run(Customer customer, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();

            var summary = customer.GetComponent<LoyaltySummary>();

            if (policy.CustomerAlreadyProcessed(customer))
            {
                return customer;
            }
            summary.LastProcessedDate = new DateTimeOffset?(DateTimeOffset.UtcNow);

            string listName = string.Format(context.GetPolicy<KnownOrderListsPolicy>().CustomerOrders, customer.Id);

            CommerceList<Order> orders = await _findEntitiesInListCommand.Process<Order>(context.CommerceContext, listName, 0, int.MaxValue);

            foreach (var order in orders.Items)
            {
                // Call order processing pipeline
                // Get loyalty points found, update
                // customer summary.
                int returnValue = 0;
                summary.TotalPoints += returnValue;
            }
            customer.SetComponent(summary);
            return customer;



        }
    }

    internal class LoyaltySummary:Component

    {
        public DateTimeOffset? LastProcessedDate { get; set; }
        public int TotalPoints { get; set; }
    }
}