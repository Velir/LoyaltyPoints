using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using Sitecore.Commerce.XA.Foundation.Common;
using Feature.LoyaltyPoints.Website.Managers;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.XA.Foundation.Connect.Managers;

namespace Feature.LoyaltyPoints.Website.Models
{
    public class LoyaltyCouponsRenderingModel : BaseCommerceRenderingModel
    {
        private readonly ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> managerResponse;

        public LoyaltyCouponsRenderingModel(ManagerResponse<GetCouponsResult, IEnumerable<Coupon>> managerResponse)
        {
            this.managerResponse = managerResponse;
        }

        // Note: This does not follow the CXA ajax paradigm. This should be a JSON call on the Repository.
        public IEnumerable<Coupon> Coupons =>
            this.IsEdit ? GetSamples() : GetUserData();

        private IEnumerable<Coupon> GetSamples() => new List<Coupon>
        {
            new Coupon{Code="LoyaltyCouponsRenderingModel1"},
            new Coupon{Code="LoyaltyCouponsRenderingModel2"},
            new Coupon{Code="LoyaltyCouponsRenderingModel3"}
        };

        private IEnumerable<Coupon> GetUserData() => this.managerResponse.Result;

    }
}