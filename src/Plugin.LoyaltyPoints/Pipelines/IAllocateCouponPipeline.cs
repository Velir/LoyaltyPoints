using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public interface IAllocateCouponPipeline:IPipeline<AllocateCouponArgument, AllocateCouponArgument, CommercePipelineExecutionContext>
    {
    }
}