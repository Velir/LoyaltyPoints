using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public interface IApplyLoyaltyPointsMinionPipeline:IPipeline<Customer, Customer, CommercePipelineExecutionContext>
    {
    }
}