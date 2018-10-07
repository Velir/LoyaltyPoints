using System.Collections.Generic;
using System.Linq;
using CommerceOps.Sitecore.Commerce.Engine;
using Feature.LoyaltyPoints.Website.Models;
using Feature.LoyaltyPoints.Website.Pipelines;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.Commerce.XA.Foundation.Connect.Managers;
using Sitecore.Diagnostics;

namespace Feature.LoyaltyPoints.Website.Managers
{
    public class CouponManager : ICouponManager
    {
        private readonly CouponsServiceProvider _couponsServiceProvider;


        public CouponManager(CouponsServiceProvider couponsServiceProvider)
        {
            _couponsServiceProvider = couponsServiceProvider;
        }

        public ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> GetCoupons(IStorefrontContext storefront, IVisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));

            string customerId = visitorContext.CustomerId;
            
            GetCouponsResult serviceResult = _couponsServiceProvider.GetCustomerCoupons(new GetCouponsRequest(customerId, storefront.CurrentStorefront.ShopName));

            if (serviceResult?.Coupons != null && serviceResult.Coupons.Any())
            {
                return new ManagerResponse<GetCouponsResult, IEnumerable<Coupon>>(serviceResult, serviceResult.Coupons);
            }
            
            serviceResult = serviceResult ?? new GetCouponsResult();
            serviceResult.Success = false;
            return new ManagerResponse<GetCouponsResult, IEnumerable<Coupon>>(serviceResult, null);
        }
    }
}