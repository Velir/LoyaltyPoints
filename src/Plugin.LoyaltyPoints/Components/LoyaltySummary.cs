using System;
using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Components
{
    internal class LoyaltySummary:Component

    {
        public DateTimeOffset? LastProcessedDate { get; set; }
        public int TotalPoints { get; set; }
    }
}