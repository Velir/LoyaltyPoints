using System.Collections.Generic;
using Feature.LoyaltyPoints.Website.Models;
using Sitecore.Commerce.Services;
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
         

        public CouponManager(IConnectServiceProvider connectServiceProvider)
        {
            _connectServiceProvider = connectServiceProvider;
        }

        public ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> GetCoupons(IStorefrontContext storefront, IVisitorContext visitorContext)
        {
            Assert.ArgumentNotNull(storefront, nameof(storefront));
            Assert.ArgumentNotNull(visitorContext, nameof(visitorContext));

            string customerId = visitorContext.CustomerId;
            GetCustomerResult result = this._connectServiceProvider.GetCustomerServiceProvider()
                .GetCustomer(new GetCustomerRequest(customerId));
            return new ManagerResponse<GetCouponsResult, IEnumerable<Coupon>>(new GetCouponsResult() {Success = true}, new []{new Coupon(){Name="A"}, new Coupon() { Name = "B" } , new Coupon() { Name = "C" } });
        }
    }

    public class GetCouponsResult : ServiceProviderResult
    {
    }
}