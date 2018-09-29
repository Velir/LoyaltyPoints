using System;
using Sitecore.Commerce.Services;

namespace Feature.LoyaltyPoints.Website.Managers
{
    public class CouponsServiceProvider:ServiceProvider
    {
        public virtual GetCouponsResult GetCustomerCoupons(GetCouponsRequest request)
        {
            return this.RunPipeline<GetCouponsRequest, GetCouponsResult>("feature.loyaltypoints.getCustomerCoupons", request);
        }
    }
}