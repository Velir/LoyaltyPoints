﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Orders;
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
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        public ProcessOrderBlock(IPersistEntityPipeline persistEntityPipeline)
        {
            _persistEntityPipeline = persistEntityPipeline;
        }

        public override async Task<int> Run(Order order, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();

            if (!policy.IsValid(order))
            {
                return 0;
            }

            int points = order.Lines.Where(policy.IsValid).Sum(GetPoints);

            await _persistEntityPipeline.Run(new PersistEntityArgument(order), context);
             
            return points;
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