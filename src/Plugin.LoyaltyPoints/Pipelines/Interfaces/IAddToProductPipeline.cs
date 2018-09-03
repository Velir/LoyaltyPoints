using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{

    [PipelineDisplayName("LoyaltyPoints.AddToProduct")]
    public interface IAddToProductPipeline : IPipeline<AddLoyaltyPointsArgument, SellableItem, CommercePipelineExecutionContext>
    {
    }
}