using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using CommerceOps.Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Customers;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Customers;
using Sitecore.Diagnostics;

namespace Feature.LoyaltyPoints.Website.Pipelines
{
    public class GetAccountLoyaltyCoupons: Sitecore.Commerce.Engine.Connect.Pipelines.PipelineProcessor
    {
        public IEntityFactory EntityFactory { get; set; }

        public GetAccountLoyaltyCoupons(IEntityFactory entityFactory)
        {
            Assert.ArgumentNotNull((object)entityFactory, nameof(entityFactory));
            this.EntityFactory = entityFactory;
        }

        public override void Process(ServicePipelineArgs args)
        {
            GetCouponsRequest request;
            GetCouponsResult result;

            // TODO Replicate behavior or private method below.
            //PipelineUtility.ValidateArguments<GetCouponsRequest, GetCouponsResult>(args, out request, out result);


            Assert.ArgumentNotNull((object)request.CommerceCustomer, "request.CommerceCustomer");
            EntityView entityView1 = this.GetEntityView(this.GetContainer(request.Shop.Name, string.Empty, request.CommerceCustomer.ExternalId, "", args.Request.CurrencyCode, new DateTime?()), request.CommerceCustomer.ExternalId, string.Empty, "LoyaltyCoupons", string.Empty, (ServiceProviderResult)result);
            if (!result.Success)
                return;
             List<Coupon> couponList = new List<Coupon>();
            EntityView entityView2 = entityView1.ChildViews.Where<Model>((Func<Model, bool>)(v => v.Name.Equals("LoyaltyPonts", StringComparison.OrdinalIgnoreCase))).FirstOrDefault<Model>() as EntityView;
            if (entityView2 != null || entityView2.ChildViews.Any<Model>())
            {
                foreach (Model childView in (Collection<Model>)entityView2.ChildViews)
                {
                    Coupon couponEntitye = this.TranslateViewToCoupon(childView as EntityView, (ServiceProviderResult)result);
                    couponList.Add((Sitecore.Commerce.Entities.Party)couponEntitye);
                }
            }
            result.Coupons = (IReadOnlyCollection<Coupon>)couponList.AsReadOnly();
        }

        private CommerceParty TranslateViewToCoupon(EntityView entityView, ServiceProviderResult result)
        {
            throw new NotImplementedException();
        }
    }

    public class GetCouponsResult:ServiceProviderResult
    {
    }

    public class GetCouponsRequest:ServiceProviderRequest
    {
        public CommerceCustomer CommerceCustomer { get; set; }
    }
}
   