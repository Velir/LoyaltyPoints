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
    class ProcessOrderBlock : PipelineBlock<Order, int, CommercePipelineExecutionContext>
    {
        

        public async override Task<int> Run(Order order, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            int points = 0;

            if (!policy.IsValid(order))
            {
                return 0;
            }

            foreach (var line in order.Lines)
            {
                if (policy.IsValid(line))
                {
                    points += GetPoints(line);
                }
            }
            
        }

        private int GetPoints(CartLineComponent line)
        {
            if (!line.HasComponent<LoyaltyPointsComponent>())
            {
                return 0;
            }

            var loyaltyPoints = line.GetComponent<LoyaltyPointsComponent>();
            if (loyaltyPoints.HasComponent<Applied>())
            {
                return 0;
            }

            loyaltyPoints.SetComponent(new Applied());

            return (int)Math.Floor(line.Quantity * loyaltyPoints.Points);
        }
    }
}