using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Customers;

namespace Feature.LoyaltyPoints.Website.Pipelines
{
    public class GetUserWithCoupons: Sitecore.Commerce.Engine.Connect.Pipelines.Customers.GetUser
    {
        public GetUserWithCoupons(IEntityFactory entityFactory) : base(entityFactory)
        {
        }

        protected override CommerceUser TranslateViewToCommerceUser(Sitecore.Commerce.EntityViews.EntityView view)
        {
            return base.TranslateViewToCommerceUser(view);
        }
    }
}