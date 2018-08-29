using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.LoyaltyPoints.Pipelines.Arguments
{
    public class AddLoyaltyPointsArgument : PipelineArgument
    {

        public AddLoyaltyPointsArgument(string productId, int percent)
        {
            Condition.Requires(productId).IsNotNull($"The parameter \"{nameof(productId)}\" cannot be null.");
            Condition.Requires(percent).IsNotNull($"The parameter \"{nameof(percent)}\" cannot be null.");

            this.ProductId = productId;
            this.Percent = percent;
        }

 
        public string ProductId { get; }
        public int Percent { get; }
    }
}
