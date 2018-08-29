using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    [PipelineDisplayName("LoyaltyPoints.MakeComponent")]
    public interface IMakeComponentPipeline : IPipeline<MakeComponentArgument, LoyaltyPointsComponent, CommercePipelineExecutionContext>
    {
    }
}