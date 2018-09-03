using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Blocks.MakeComponent")]
    public class MakeComponentBlock : PipelineBlock<MakeComponentArgument, LoyaltyPointsComponent, CommercePipelineExecutionContext>
    {
        private readonly IGetBasisPricePipeline _getBasisPricePipeline;


        public MakeComponentBlock(IGetBasisPricePipeline getBasisPricePipeline)
        {
            _getBasisPricePipeline = getBasisPricePipeline;
        }


        public override async Task<LoyaltyPointsComponent> Run(MakeComponentArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            var basisPrice = await _getBasisPricePipeline.Run(arg.SellableItem, context);
            if (basisPrice <= 0)
            {
                context.Logger.LogWarning($"{this.Name}: Product must have a postive basis price. Product \"{arg.SellableItem.ProductId}\" has price of {basisPrice:C}.");
                return null;
            }

            if (arg.Percent < 1 || arg.Percent > 100)
            {
                context.Logger.LogWarning($"{this.Name}: Invalid percent value: {arg.Percent}");
                return null;
            }

            int points = (int) Math.Floor(basisPrice * arg.Percent / 100);

            var loyaltyPointsComponent = new LoyaltyPointsComponent {Points = points};

            context.Logger.LogTrace($"{this.Name}: LoyaltyPointsComponent {loyaltyPointsComponent} created for SellableItem {arg.SellableItem.ProductId} with basis price of {basisPrice:C} using percent {arg.Percent}.");
            return loyaltyPointsComponent;

        }
    }
}