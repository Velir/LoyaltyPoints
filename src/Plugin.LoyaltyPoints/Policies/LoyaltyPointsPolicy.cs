using System;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Blocks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;

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
            this.CouponBlockSize = 20;
            this.ReprovisionTriggerCount = 5;
            this.CouponPrefix = "LP";
            this.CustomerProcessingInterval = new TimeSpan(0,5,0); // Five minutes for demo purposes.
            this.PointsForCoupon = 1000;
            this.LoyaltyPointPercent = 10;

        }

        /// <summary>
        /// Sets percent amount for loyalyt point calculation.
        /// </summary>

        public int LoyaltyPointPercent { get; set; }

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

        public int ReprovisionTriggerCount { get; set; }

        public TimeSpan CustomerProcessingInterval
        { get; set; }

        public int PointsForCoupon { get; set; }

        public virtual bool CustomerAlreadyProcessed(Customer customer)
        {
            LoyaltySummary summary = customer.GetComponent<LoyaltySummary>();
            return summary.LastProcessedDate.HasValue &&
                   DateTimeOffset.UtcNow.Subtract(summary.LastProcessedDate.Value)
                    <= this.CustomerProcessingInterval;
        }

        /// <summary>
        /// Override this to implement validation on order
        /// (date, status, etc.)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public virtual bool IsValid(Order order)
        {
            return true;
        }

        /// <summary>
        /// Override this to implment validation on line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public virtual bool IsValid(CartLineComponent line)
        {
            return true;
        }
    }
}
