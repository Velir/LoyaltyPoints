using System.Collections.Generic;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Services;

namespace Feature.LoyaltyPoints.Website.Managers
{
    public class GetCouponsResult : ServiceProviderResult
    {
        public GetCouponsResult()
        {
            Coupons = new List<Coupon>();
        }
        public List<Coupon> Coupons { get; set; }
    }
}