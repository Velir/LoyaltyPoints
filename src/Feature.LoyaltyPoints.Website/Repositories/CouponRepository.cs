using Feature.LoyaltyPoints.Website.Managers;
using Feature.LoyaltyPoints.Website.Models;
using Feature.LoyaltyPoints.Website.Models.JsonResults;
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

        public CouponRepository(ICouponManager manager)
        {
            _manager = manager;
        }

        public LoyaltyCouponsRenderingModel GetUnusedCouponsRenderingModel()
        {
            return new LoyaltyCouponsRenderingModel();
        }

        public CouponsBaseJsonResult GetUnusedCoupons(IStorefrontContext storefrontContext, IVisitorContext visitorContext)
        {
            var result = new CouponsBaseJsonResult(storefrontContext);
            result.Initialize(_manager.GetCoupons(storefrontContext,visitorContext));
            result.Success = true;
            return result;
        }
    }
}