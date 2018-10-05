using System;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Pipelines.Blocks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.XConnect;

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
            this.LoyaltyPointPromotion = "Entity-Promotion-Habitat_LoyaltyPointsPromotionBook-Loyalty Points Promotion";
            this.CouponBlockSize = 20;
            this.ReprovisionTriggerCount = 5;
            this.CouponPrefix = "LP";
            this.CustomerProcessingInterval = new TimeSpan(0,5,0); // Five minutes for demo purposes.
            this.PointsForCoupon = 1000;
            this.LoyaltyPointPercent = 10;

        }

        /// <summary>
        /// Sets percent amount for loyalty point calculation.
        /// </summary>
        public int LoyaltyPointPercent { get; set; }

        /// <summary>
        /// This promotion will be used to generate promotions for the Loyalty Points functionality.
        /// Note that the generated promotions will be automatically approved.
        /// </summary>
        public string LoyaltyPointPromotion { get; set; }

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

        public TimeSpan CustomerProcessingInterval { get; set; }

        public int PointsForCoupon { get; set; }

        public string XConnectClientCertConnectionString { get; set; }

        public string XConnectUrl { get; set; }

        public Guid ChannelId { get; set; }

        public Guid EventId { get; set; }

        public virtual bool IsCustomerProcessed(Customer customer)
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
        public virtual bool IsValid(Order order)
        {
            return true;
        }

        /// <summary>
        /// Override this to implement validation on line.
        /// </summary>
        public virtual bool IsValid(CartLineComponent line)
        {
            return true;
        }
    }

   /// <summary>
   /// This class demonstrates policy inheritance. By bootstrapping it instead of <see cref="LoyaltyPointsPolicy"/>
   /// the order validation logic is made more strict.
   /// </summary>
    public class LoyaltyPointsCustomizedPolicy : LoyaltyPointsPolicy
    {
        public LoyaltyPointsCustomizedPolicy()
        {
            RequiredOrderAge = TimeSpan.FromDays(30);
        }
        public override bool IsValid(Order order)
        {
            return base.IsValid(order) &&
                   order.OrderPlacedDate.Subtract(DateTimeOffset.UtcNow)
                   >= RequiredOrderAge;
        }

        public TimeSpan RequiredOrderAge { get; set; }
    }
}
