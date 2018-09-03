using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Minions
{
    /// <summary>
    /// Gets customers from a list of customers to process.
    /// Within transaction:
    ///   Remove coupon from list of coupons.
    ///   Mark coupon earned as applied on customer.
    ///   Update applied points. 
    ///
    ///   Fire live event on xConnect
    ///   TODO If transaction rolls back, move customer to error queue.
    /// </summary>
    class IssueCouponMinion :Minion
    {
        public override void Initialize(IServiceProvider serviceProvider, ILogger logger, MinionPolicy policy, CommerceEnvironment environment,
            CommerceContext globalContext)
        {
            base.Initialize(serviceProvider, logger, policy, environment, globalContext);
            this.MinionPipeline = serviceProvider.GetService<IIssueCouponPipeline>();
            this.CommerceCommand = new CommerceCommand(serviceProvider);
        }

        protected IIssueCouponPipeline MinionPipeline { get; set; }

        private CommerceCommand CommerceCommand { get; set; }

        public override async Task<MinionRunResultsModel> Run()
        {
            MinionRunResultsModel result = new MinionRunResultsModel();

            Logger.LogTrace($"{this.Name}: Invoked.");
            string listName = Policy.ListToWatch;
            Task<long> itemCount = GetListCount(listName);

            //TODO correct handling of <cref='MinionResultArgument.HasMoreItems'>HasMoreItems</cref>

            IEnumerable<CommerceEntity> entities = await GetListItems<CommerceEntity>(listName, Policy.ItemsPerBatch);
            foreach (Customer customer in entities.OfType<Customer>())
            {
                var executionContext = new CommercePipelineExecutionContextOptions(
                        new CommerceContext(MinionContext.Logger, MinionContext.TelemetryClient)
                            { Environment = this.Environment });
                var issueCouponArgument = new IssueCouponArgument{Customer = customer};
                var pipelineResult = await this.MinionPipeline.Run(issueCouponArgument, executionContext);

                // TODO Appropriate handling of pipeline status. Look at other minions.
                result.ItemsProcessed++;
            }

            return result;
        }
    }
}