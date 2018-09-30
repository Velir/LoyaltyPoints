using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Components
{
    public class LoyaltyPointsComponent : Component
    {
        public int Points { get; set; }
        public override string ToString()
        {
            return $"LoyaltyPointsComponent {{ID:{this.Id}, Points:{this.Points}}}";
        }
    }
}