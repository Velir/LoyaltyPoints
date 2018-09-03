using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    [PipelineDisplayName("LoyaltyPoints.IssueCoupon")]
    public interface IIssueCouponPipeline : IPipeline<IssueCouponArgument, IssueCouponArgument, CommercePipelineExecutionContext>
    {
    }
}