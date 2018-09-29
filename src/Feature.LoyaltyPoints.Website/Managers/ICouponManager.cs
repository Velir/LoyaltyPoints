using System.Collections.Generic;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.Commerce.XA.Foundation.Connect.Managers;

namespace Feature.LoyaltyPoints.Website.Managers
{
    /// <summary>
    /// Manages communication with Commerce Engine.
    /// </summary>
    public interface ICouponManager
    {
        ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> GetCoupons(IStorefrontContext storefront, IVisitorContext visitorContext);
    }
}