using System.Collections.Generic;
using Feature.LoyaltyPoints.Website.Models;
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