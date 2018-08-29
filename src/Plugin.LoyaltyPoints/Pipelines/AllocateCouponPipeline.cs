using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public class AllocateCouponPipeline : CommercePipeline<AllocateCouponArgument, AllocateCouponArgument>, IAllocateCouponPipeline
    {
        public AllocateCouponPipeline(IPipelineConfiguration<IAllocateCouponPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}