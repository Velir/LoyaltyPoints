using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Pipelines.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public bool Lock { get; set; }
        public string CurrentPromotion { get; set; }
        public int SequenceNumber { get; set; }
    }
}