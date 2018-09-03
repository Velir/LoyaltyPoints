using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public class IssueCouponPipeline : CommercePipeline<IssueCouponArgument, IssueCouponArgument>,
        IIssueCouponPipeline
    {
        public IssueCouponPipeline(IPipelineConfiguration<IIssueCouponPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}