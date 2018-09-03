using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    [PipelineDisplayName("LoyaltyPoints.ProcessCustomer")]
    public interface IProcessCustomerPipeline : IPipeline<Customer, Customer, CommercePipelineExecutionContext>
    {
    }
}