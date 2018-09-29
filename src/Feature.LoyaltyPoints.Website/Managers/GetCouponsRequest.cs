using Sitecore.Commerce.Services;

namespace Feature.LoyaltyPoints.Website.Managers
{
    public class GetCouponsRequest:ServiceProviderRequest
    {
        public string CustomerId { get; }
        public string ShopName { get; }

        public GetCouponsRequest(string customerId, string shopName)
        {
            CustomerId = customerId;
            ShopName = shopName; 
        }
    }
}