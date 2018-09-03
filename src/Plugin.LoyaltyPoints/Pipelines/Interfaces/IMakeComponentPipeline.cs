using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Interfaces
{
    [PipelineDisplayName("LoyaltyPoints.MakeComponent")]
    public interface IMakeComponentPipeline : IPipeline<MakeComponentArgument, LoyaltyPointsComponent, CommercePipelineExecutionContext>
    {
    }
}