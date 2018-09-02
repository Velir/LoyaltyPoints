using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Pipelines.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public LoyaltyPointsEntity()
        {
            Id = Constants.EntityId;
        }

        public string CurrentPromotion { get; set; }
        public int SequenceNumber { get; set; }
    }
}