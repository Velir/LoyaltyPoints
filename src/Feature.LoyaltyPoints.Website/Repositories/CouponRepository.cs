using Feature.LoyaltyPoints.Website.Managers;
using Feature.LoyaltyPoints.Website.Models;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using Sitecore.Commerce.XA.Foundation.Common.Repositories;
using Sitecore.Commerce.XA.Foundation.Connect;

namespace Feature.LoyaltyPoints.Website.Repositories
{
    /// <summary>
    /// Returns model, drawing on Sitecore XA settings. Engine data delegated to <see cref="ICouponManager"/>
    /// </summary>
    public class CouponRepository : BaseCommerceModelRepository, ICouponRepository
    {

        private readonly ICouponManager _manager;
        private readonly IStorefrontContext _storefrontContext;
        private readonly IVisitorContext _visitorContext;

        public CouponRepository(ICouponManager manager, IStorefrontContext storefrontContext,
            IVisitorContext visitorContext)
        {

            _manager = manager;
            _storefrontContext = storefrontContext;
            _visitorContext = visitorContext;
        }

        public object GetUnusedCouponsRenderingModel()
        {
            // TODO Pass a parameter to limit to unused coupons.
            var coupons = _manager.GetCoupons(_storefrontContext, _visitorContext);
            return new LoyaltyCouponsRenderingModel(coupons);


            //TODO Add JSON call (supports AJAX loading?)
        }
    }
}