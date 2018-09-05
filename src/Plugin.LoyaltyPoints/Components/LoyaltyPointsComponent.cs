using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Components
{
    public class LoyaltyPointsComponent : Component
    {
        //TODO Build some demo logic where a customer gets double points due to some promotion or entitlement.
        //E.g. A GetPoints method that applies policies of type ILoyaltyPointModifier.

        public int Points { get; set; }
        public override string ToString()
        {
            return $"LoyaltyPointsComponent {{ID:{this.Id}, Points:{this.Points}}}";
        }
    }
}