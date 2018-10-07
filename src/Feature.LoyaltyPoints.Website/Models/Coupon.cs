using System;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.EntityViews;

namespace Feature.LoyaltyPoints.Website.Models
{
    [Serializable]
    public class Coupon : MappedEntity
    {
        public Coupon()
        {
            
        }

        public Coupon(EntityView ev)
        {
            this.ExternalId = ev.EntityId;
            foreach (var property in ev.Properties)
            {
                if (property.Name.Equals("Code", StringComparison.OrdinalIgnoreCase))
                {
                    this.Code = property.Value;
                }
                if (property.Name.Equals("Is Applied", StringComparison.OrdinalIgnoreCase) &&
                    bool.TryParse(property.Value, out bool applied))
                {
                    this.IsApplied = applied;
                }
                if (property.Name.Equals("Date Earned", StringComparison.OrdinalIgnoreCase) &&
                    DateTimeOffset.TryParse(property.Value, out DateTimeOffset earned))
                {
                    this.DateEarned = earned;
                }
                if (property.Name.Equals("Date Used", StringComparison.OrdinalIgnoreCase) &&
                    DateTimeOffset.TryParse(property.Value, out DateTimeOffset used))
                {
                    this.DateUsed = used;
                }

            }



        }

        public DateTimeOffset? DateUsed { get; set; }

        public DateTimeOffset? DateEarned { get; set; }

        public bool IsApplied { get; set; }

        public string Code { get; set; }
    }
}