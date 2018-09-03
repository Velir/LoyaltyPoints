using System;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Pipelines;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.LoyaltyPoints.Commands
{

    public class AddLoyatlyPointsCommand : CommerceCommand
    {
        private readonly IAddToProductPipeline _pipeline;
        public AddLoyatlyPointsCommand(IAddToProductPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }
   
        /// <returns>true if succesful</returns>
        public async Task<SellableItem> Process(CommerceContext commerceContext, string productId)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                int percent = commerceContext.GetPolicy<LoyaltyPointsPolicy>().LoyaltyPointPercent;
                var arg = new AddLoyaltyPointsArgument(productId,percent);
                var result = await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));

                return result;
            }
        }
    }
}