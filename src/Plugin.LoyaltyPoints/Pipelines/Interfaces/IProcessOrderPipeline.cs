using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    [PipelineDisplayName("LoyaltyPoints.ProcessOrder")]
    public interface IProcessOrderPipeline : IPipeline<Order, int, CommercePipelineExecutionContext>
    {
    }
}