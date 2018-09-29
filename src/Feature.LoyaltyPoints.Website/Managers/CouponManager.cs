using System.Collections.Generic;
using System.Linq;
using CommerceOps.Sitecore.Commerce.Engine;

using Feature.LoyaltyPoints.Website.Pipelines;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.Commerce.XA.Foundation.Connect.Managers;
using Sitecore.Commerce.XA.Foundation.Connect.Providers;
using Sitecore.Diagnostics;

namespace Feature.LoyaltyPoints.Website.Managers
{
    public class CouponManager : ICouponManager
    {
        private readonly IConnectServiceProvider _connectServiceProvider;
        private readonly CouponsServiceProvider _couponsServiceProvider;


        public CouponManager(IConnectServiceProvider connectServiceProvider, CouponsServiceProvider couponsServiceProvider)
        {
            _connectServiceProvider = connectServiceProvider;
            _couponsServiceProvider = couponsServiceProvider;
        }

        public ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> GetCoupons(IStorefrontContext storefront, IVisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));

            string customerId = visitorContext.CustomerId;
            
            GetCouponsResult coupons = _couponsServiceProvider.GetCustomerCoupons(new GetCouponsRequest(customerId, storefront.CurrentStorefront.ShopName));

            if (coupons?.Coupons != null && coupons.Coupons.Any())
            {
                GetCouponsResult serviceProviderResult = coupons;
                return new ManagerResponse<GetCouponsResult, IEnumerable<Coupon>>(serviceProviderResult, serviceProviderResult.Coupons);
            }

            
            GetCouponsResult serviceProviderResult1 = new GetCouponsResult();
            serviceProviderResult1.Success = false;
            return new ManagerResponse<GetCouponsResult, IEnumerable<Coupon>>(serviceProviderResult1, null);
        }
    }
}