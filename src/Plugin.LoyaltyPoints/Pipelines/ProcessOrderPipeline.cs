using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    /// <summary>
    /// Gets loyalty points for each order.
    /// Possible improvement: Use an arg, so that later processores
    /// can further manipulate the order or the returned value.
    /// </summary>
    public class ProcessOrderPipeline : CommercePipeline<Order, int>,
        IProcessOrderPipeline
    {
        public ProcessOrderPipeline(IPipelineConfiguration<IProcessOrderPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}