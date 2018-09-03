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
    /// Iterates through Customer list and calls ProcessCustomer pipeline.
    /// </summary>
    class ProcessCustomersMinion:Minion
    {
        public override void Initialize(IServiceProvider serviceProvider, ILogger logger, MinionPolicy policy, CommerceEnvironment environment,
            CommerceContext globalContext)
        {
            base.Initialize(serviceProvider, logger, policy, environment, globalContext);
            this.MinionPipeline = serviceProvider.GetService<IProcessCustomerPipeline>();
            this.CommerceCommand = new CommerceCommand(serviceProvider);
        }

        protected ServiceProvider ServiceProvider {get;set;}

        protected IProcessCustomerPipeline MinionPipeline { get; set; }

        private CommerceCommand CommerceCommand {get;set;}

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
                await this.CommerceCommand.PerformTransaction(MinionContext, async () =>
                {
                    var executionContext = new CommercePipelineExecutionContextOptions(
                        new CommerceContext(MinionContext.Logger, MinionContext.TelemetryClient, (IGetLocalizableMessagePipeline)null) { Environment = this.Environment });
                    var pipelineResult = await this.MinionPipeline.Run(customer, executionContext);
                     
                    //TODO Think about error handling here. What happens if the transaction times out?
                });
                
                result.ItemsProcessed++;
            }

            return result;
        }
    }
}