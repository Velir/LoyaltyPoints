using System;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.LoyaltyPoints.Commands
{
    //TODO Remove this, as we don't want this called outside of the plugin.
    public class MakeComponentCommand : CommerceCommand
    {
        private readonly IMakeComponentPipeline _pipeline;
        public MakeComponentCommand(IMakeComponentPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        /// <returns>true if succesful</returns>
        public async Task<LoyaltyPointsComponent> Process(CommerceContext commerceContext, int percent, SellableItem sellableItem)      {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var arg = new MakeComponentArgument {Percent = percent, SellableItem = sellableItem};
                var result = await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));

                return result;
            }
        }
    }
}