using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public class ProcessCustomerPipeline : CommercePipeline<Customer, Customer>,
        IProcessCustomerPipeline
    {
        public ProcessCustomerPipeline(IPipelineConfiguration<IProcessCustomerPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}