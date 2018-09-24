using System;
using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Models
{
    public class CouponModel:Model
    {
        public string EntityId { get; set; }
        public string Code { get; set; }
        public bool IsApplied { get; set; }
        public DateTimeOffset? DateEarned { get; set; }
        public DateTimeOffset? DateUsed { get; set; }
    }
}