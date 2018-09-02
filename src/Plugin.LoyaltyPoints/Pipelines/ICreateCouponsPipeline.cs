using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    [PipelineDisplayName("LoyaltyPoints.CreateCoupons")]
    public interface ICreateCouponsPipeline : IPipeline<CreateCouponsArgument, CreateCouponsArgument, CommercePipelineExecutionContext>
    {
    }

    public class CreateCouponsPipeline : CommercePipeline<CreateCouponsArgument, CreateCouponsArgument>,
        ICreateCouponsPipeline
    {
        public CreateCouponsPipeline(IPipelineConfiguration<ICreateCouponsPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}