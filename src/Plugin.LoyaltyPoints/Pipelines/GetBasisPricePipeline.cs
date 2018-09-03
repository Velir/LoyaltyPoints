using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public class GetBasisPricePipeline : CommercePipeline<SellableItem, decimal>, IGetBasisPricePipeline
    {
        public GetBasisPricePipeline(IPipelineConfiguration<IGetBasisPricePipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}