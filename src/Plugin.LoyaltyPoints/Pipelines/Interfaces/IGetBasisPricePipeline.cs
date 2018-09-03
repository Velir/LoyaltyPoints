using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Interfaces
{
    [PipelineDisplayName("LoyaltyPoints.GetBasisPrice")]
    public interface IGetBasisPricePipeline : IPipeline<SellableItem, decimal, CommercePipelineExecutionContext>
    {
    }
}