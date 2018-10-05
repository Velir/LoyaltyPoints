using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public LoyaltyPointsEntity()
        {
            Id = Constants.EntityId;
        }
        public int SequenceNumber { get; set; }
    }
}