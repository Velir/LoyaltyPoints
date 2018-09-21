using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Minions
{
    /// <summary>
    ///  Check available coupon count against policy.MinimumCouponTarget. (e.g 100)
    ///  Create coupons if under target. (e.g. 1000)
    ///  Ensures list of coupons exists, and listens to it.
    ///  Get list name form a constants class.
    /// </summary>
    class CreateCouponsMinion : Minion
    {
        public override void Initialize(IServiceProvider serviceProvider, ILogger logger, MinionPolicy policy, CommerceEnvironment environment,
            CommerceContext globalContext)
        {
            base.Initialize(serviceProvider, logger, policy, environment, globalContext);
            this.MinionPipeline = serviceProvider.GetService<ICreateCouponsPipeline>();
        }

        protected ICreateCouponsPipeline MinionPipeline { get; set; }

        public override async Task<MinionRunResultsModel> Run()
        {
            MinionRunResultsModel result = new MinionRunResultsModel();

            Logger.LogTrace($"{this.Name}: Invoked.");
           

            var executionContext = new CommercePipelineExecutionContextOptions(
                    new CommerceContext(
                        MinionContext.Logger, 
                        MinionContext.TelemetryClient, 
                        (IGetLocalizableMessagePipeline)null)
                        { Environment = this.Environment });
            var pipelineResult = await this.MinionPipeline.Run(new CreateCouponsArgument(), executionContext);

            return result;
        }
    }
}