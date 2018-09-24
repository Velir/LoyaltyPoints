using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Components
{
    public class LoyaltySummary:Component

    {
        public LoyaltySummary()
        {
            CouponEntities = new List<string>();
        }

        public DateTimeOffset? LastProcessedDate { get; set; }

        public int TotalPoints { get; set; }

        public int AppliedPoints { get; set; }

        public List<string> CouponEntities { get; set; }
        
    }
}