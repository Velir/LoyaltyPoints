using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Pipelines.Interfaces;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines
{
    public class MakeComponentPipeline : CommercePipeline<MakeComponentArgument, LoyaltyPointsComponent>, IMakeComponentPipeline
    {
        public MakeComponentPipeline(IPipelineConfiguration<IMakeComponentPipeline> configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}