using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Pipelines.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public LoyaltyPointsEntity()
        {
            SequenceNumber = 26;
        }
        public string CurrentPromotion { get; set; }
        public int SequenceNumber { get; set; }
    }
}