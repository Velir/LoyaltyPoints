using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Pipelines.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public bool Lock { get; set; }
    }
}