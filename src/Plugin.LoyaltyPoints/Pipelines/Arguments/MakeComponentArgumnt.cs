using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;

namespace Plugin.LoyaltyPoints.Pipelines.Arguments
{
    public class MakeComponentArgument : PipelineArgument
    { 
        public SellableItem SellableItem { get; set; }
        public int Percent { get; set; }
    }
}