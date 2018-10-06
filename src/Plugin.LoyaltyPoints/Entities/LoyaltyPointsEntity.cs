using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public LoyaltyPointsEntity()
        {
            Id = Constants.EntityId;
        }

        /// <summary>
        /// Controls coupon batch suffix value.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Prevents simultaneous provisioning of coupons.
        /// </summary>
        public bool IsLocked { get; set; }
    }
}