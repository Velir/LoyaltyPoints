using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    [PipelineDisplayName("LoyaltyPoints.CreateCoupons")]
    public interface ICreateCouponsPipeline : IPipeline<CreateCouponsArgument, CreateCouponsArgument, CommercePipelineExecutionContext>
    {
    }
}