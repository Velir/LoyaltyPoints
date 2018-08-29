using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Policies
{
    /// <inheritdoc />
    /// <summary>
    /// Configures Loyalty Points module behavior
    /// </summary>
    /// <seealso cref="T:Sitecore.Commerce.Core.Policy" />
    public class LoyaltyPointsPolicy : Policy
    {
        public LoyaltyPointsPolicy()
        {
            this.TemplatePromotion = "Habitat_LoyaltyPointsPromotionBook-LoyalyPointsPromotion";
            this.CouponBlockSize = 10;
            this.CouponPrefix = "LP";
        }
        /// <summary>
        /// This promotion will be used to genearate promotions for the Loyalty Points functionality.
        /// Note that the generated promotions will be automatically approved.
        /// </summary>
        public string TemplatePromotion { get; set; }

        /// <summary>
        /// The number of coupon codes to genearate for each block
        /// </summary>
        public int CouponBlockSize { get; set; }

        /// <summary>
        /// The prefix to use for generated coupons. Note that the suffix is a sequence number that is
        /// managed by the plugin. The current sequence number will be maintained via a planned LoyaltyPointsConfigurationEntity
        /// object. 
        /// </summary>
        public string CouponPrefix { get; set; }
   
}
}
