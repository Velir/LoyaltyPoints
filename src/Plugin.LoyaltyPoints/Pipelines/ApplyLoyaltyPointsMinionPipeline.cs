using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    /// <summary>
    /// Expected steps:
    ///     Get orders,
    ///     calculate loyalty points,
    ///     issue promotion,
    ///     mark points (or order applied).
    /// </summary>

    public class ApplyLoyaltyPointsMinionPipeline : CommercePipeline<Customer, Customer>, IApplyLoyaltyPointsMinionPipeline {
        public ApplyLoyaltyPointsMinionPipeline(IPipelineConfiguration<IApplyLoyaltyPointsMinionPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
