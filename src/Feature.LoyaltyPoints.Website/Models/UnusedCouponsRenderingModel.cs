using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using Sitecore.Commerce.XA.Foundation.Common;

namespace Feature.LoyaltyPoints.Website.Models
{
    public class UnusedCouponsRenderingModel : BaseCommerceRenderingModel
    {

        public IEnumerable<Coupon> Coupons =>
            this.IsEdit ? GetSamples() : GetUserData();

        private IEnumerable<Coupon> GetSamples() => new List<Coupon>
        {
            new Coupon{Name="COUPON1"},
            new Coupon{Name="COUPON2"},
            new Coupon{Name="COUPON3"}
        };

        private IEnumerable<Coupon> GetUserData() => GetSamples(); // TODO

    }

    public class Coupon
    {
        public string Name { get; set; }
    }
}