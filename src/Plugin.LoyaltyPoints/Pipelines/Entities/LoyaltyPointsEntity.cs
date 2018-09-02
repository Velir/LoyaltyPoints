﻿using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Pipelines.Entities
{
    public class LoyaltyPointsEntity : CommerceEntity
    {
        public LoyaltyPointsEntity()
        {
            ReprovisionTriggerCount = 100;

            ReprovisionBlockSize = 1000;
            SequenceNumber = 1;

        }
        public bool Lock { get; set; }
        public string CurrentPromotion { get; set; }
        public int SequenceNumber { get; set; }
        public int ReprovisionTriggerCount { get; set; }
        public int ReprovisionBlockSize { get; set; }
    }
}