
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
 
    public class AddToProductPipeline : CommercePipeline<AddLoyaltyPointsArgument, SellableItem>, IAddToProductPipeline
    {
   
        public AddToProductPipeline(IPipelineConfiguration<IAddToProductPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}

