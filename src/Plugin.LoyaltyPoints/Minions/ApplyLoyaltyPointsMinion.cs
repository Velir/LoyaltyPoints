﻿using System;
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
    /// Iterate through customers.
    /// TODO Access orders, calculate and process loyalty points.
    /// Note: Orders requires pipeline, as command is required, and minions do not support
    /// constructor injection.
    /// TODO Make pipeline, return correct arguments.
    /// </summary>
    class ApplyLoyaltyPointsMinion:Minion
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