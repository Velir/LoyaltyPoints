using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Interfaces
{
    public interface IApplyLoyaltyPointsMinionPipeline:IPipeline<Customer, Customer, CommercePipelineExecutionContext>
    {
    }
}