using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Minions
{
    /// <summary>
    /// Gets customers from a list of customers to process.
    /// Within transaction:
    ///   Remove coupon from list of coupons.
    ///   Mark coupon as applied to customer.
    ///   Mark coupon earned as applied on customer.
    ///   Fire live event.
    ///   Mark applied as notified.
    ///   Fire transaction.
    ///
    ///   If transaction rolls back, move customer to error queue.
    /// </summary>
    class IssueCouponMinion:Minion
    {
        public override void Initialize(IServiceProvider serviceProvider, ILogger logger, MinionPolicy policy, CommerceEnvironment environment,
            CommerceContext globalContext)
        {
            base.Initialize(serviceProvider, logger, policy, environment, globalContext);
            this.MinionPipeline = serviceProvider.GetService<IApplyLoyaltyPointsMinionPipeline>();
        }

        protected IApplyLoyaltyPointsMinionPipeline MinionPipeline { get; set; }

        public async override Task<MinionRunResultsModel> Run()
        {
            MinionRunResultsModel result = new MinionRunResultsModel();

            Logger.LogInformation($"{this.Name}: Invoked.");
            string listName = Policy.ListToWatch;
            Task<long> itemCount = GetListCount(listName);
             
            //TODO correct handling of <cref='MinionResultArgument.HasMoreItems'>HasMoreItems</cref>
            
            IEnumerable<CommerceEntity> entities = await GetListItems<CommerceEntity>(listName, Policy.ItemsPerBatch);
            foreach (Customer customer in entities.OfType<Customer>())
            {
                // There is some ceremony in invoking a pipeline from a minion. This is 
                // borrowed from Sitecore.Commerce.Plugin.Search.FullIndexMinion.
                var executionContext = new CommercePipelineExecutionContextOptions(
                        new CommerceContext(MinionContext.Logger,MinionContext.TelemetryClient, (IGetLocalizableMessagePipeline)null){Environment = this.Environment}); 
                var pipelineResult = await this.MinionPipeline.Run(customer, executionContext);

                result.ItemsProcessed++;
            }

            return result;
        }
    }
}