using System;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;
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
    class ProcessCustomerBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly IProcessOrderPipeline _processOrderPipeline;
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;
        private readonly IPersistEntityPipeline _persistEntityPipeline;

        public ProcessCustomerBlock(FindEntitiesInListCommand findEntitiesInListCommand, IPersistEntityPipeline persistEntityPipeline, IProcessOrderPipeline processOrderPipeline)
        {
            _processOrderPipeline = processOrderPipeline;
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _persistEntityPipeline = persistEntityPipeline;
            
        }
        public async override Task<Customer> Run(Customer customer, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();

            var summary = customer.GetComponent<LoyaltySummary>();

            if (policy.IsCustomerProcessed(customer))
            {
                return customer;
            }
            summary.LastProcessedDate = new DateTimeOffset?(DateTimeOffset.UtcNow);

            string listName = string.Format(context.GetPolicy<KnownOrderListsPolicy>().CustomerOrders, customer.Id);

            CommerceList<Order> orders = await _findEntitiesInListCommand.Process<Order>(context.CommerceContext, listName, 0, int.MaxValue);

            foreach (var order in orders.Items)
            {
                int pointsFromOrder = await _processOrderPipeline.Run(order, context);
                summary.TotalPoints += pointsFromOrder;
            }

            await _persistEntityPipeline.Run(new PersistEntityArgument(customer), context);
            return customer;
        }
    }
}