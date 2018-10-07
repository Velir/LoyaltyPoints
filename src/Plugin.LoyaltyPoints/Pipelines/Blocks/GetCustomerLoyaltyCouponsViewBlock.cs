using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Plugin.LoyaltyPoints.Components;
using Plugin.LoyaltyPoints.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Customers;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Coupons;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Block.GetCustomerLoyaltyCouponsViews")]
    public class GetCustomerLoyaltyCouponsViewBlock:PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntity;


        public GetCustomerLoyaltyCouponsViewBlock(IFindEntityPipeline findEntity)
        {
            _findEntity = findEntity;
        }
        public async override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            EntityViewArgument request = context.CommerceContext.GetObject<EntityViewArgument>();
            

            if (request?.ViewName == "LoyaltyCoupons")
            {
                entityView.DisplayName = "Loyalty Coupons";


                var customer =
                    await _findEntity.Run(new FindEntityArgument(typeof(Customer), entityView.EntityId), context) as Customer;

                if (customer == null || !customer.HasComponent<LoyaltySummary>() || customer.GetComponent<LoyaltySummary>().CouponEntities.Count == 0)
                {
                    return entityView;
                }

                IEnumerable<EntityView> models = await Task.WhenAll(customer.GetComponent<LoyaltySummary>().CouponEntities.Select(id => GetCouponView(id, context)));


                entityView.ChildViews.AddRange(models);
            }

            return entityView;
        }

        private async Task<EntityView> GetCouponView(string id, CommercePipelineExecutionContext context)
        {
            var coupon = (Coupon) await _findEntity.Run(new FindEntityArgument(typeof(Coupon), id), context);
            if (coupon == null)
            {
                return null;
            }
            var view = new EntityView
            {
                DisplayName = "Coupon",
                EntityId = coupon.Id,
                Name = "Coupon",
                Properties = new List<ViewProperty>
                {
                    new ViewProperty
                    {
                        Name="Code",
                        DisplayName="Code",
                        IsReadOnly = true,
                        UiType = "String",
                        RawValue = coupon.Code,
                        OriginalType = typeof(string).ToString(),
                        Value =  coupon.Code
                    },
                    new ViewProperty
                    {
                        Name="Date Earned",
                        DisplayName="Date Earned",
                        IsReadOnly = true,
                        UiType = "DateTime",
                        RawValue = coupon.DateCreated,
                        OriginalType = typeof(DateTimeOffset?).ToString(),
                        Value =  coupon.DateCreated.HasValue? coupon.DateCreated.Value.ToString(): ""
                    },
                    new ViewProperty
                    {
                        Name="Is Applied",
                        DisplayName="Is Applied",
                        IsReadOnly = true,
                        UiType = "Boolean",
                        RawValue = coupon.UsageCount>0,
                        OriginalType = typeof(bool).ToString(),
                        Value =  (coupon.UsageCount>0).ToString()
                    },
                    new ViewProperty
                    {
                        Name="Date Used",
                        DisplayName="Date Used",
                        IsReadOnly = true,
                        UiType = "DateTime",
                        RawValue = coupon.UsageCount>0,
                        OriginalType = typeof(DateTimeOffset?).ToString(),
                        Value =  coupon.DateUpdated.HasValue? coupon.DateUpdated.Value.ToString(): ""
                    }
                }


            };
            return view;


        }
    }
}
