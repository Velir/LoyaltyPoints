using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
using Plugin.LoyaltyPoints.Pipelines.Entities;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.SQL;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    /// <summary>
    /// Ensures existance of available coupon list, checks count.
    /// If necessary, creates new promotion, creates coupon, adds to
    /// list, and  approves promotion.
    ///
    /// Developer notes:
    /// I'm not sure whether allocation is a necessary step for coupons
    /// to be applied, or whether this is just for managing workflow on
    /// screens. I will add that step if necesary.
    ///
    /// I will try adding the coupon to a list by simply adding the list
    /// to the MemberOfLists component, and saving the item. That seens like
    /// a nice way of managing this, if possible.
    /// </summary>
    class CreateCouponsBlock : PipelineBlock<CreateCouponsArgument, CreateCouponsArgument, CommercePipelineExecutionContext>
    {
        private readonly GetManagedListCommand _getManagedListCommand;
        private readonly CreateManagedListCommand _createManagedListCommand;
        private readonly GetListCountCommand _getListCountCommand;
        private readonly FindEntityCommand _findEntityCommand;
        private readonly DuplicatePromotionCommand _duplicatePromotionCommand;
        private readonly AddPrivateCouponCommand _addPrivateCouponCommand;
        private readonly AddEntityToListsCommand _addEntityToListsCommand;
        private readonly PersistEntityCommand _persistEntityCommand;

        //TODO Break this monster class up. Too many concerns. :(

        public CreateCouponsBlock(
            GetManagedListCommand getManagedListCommand,
            CreateManagedListCommand createManagedListCommand,
            GetListCountCommand getListCountCommand,
            FindEntityCommand findEntityCommand,
            DuplicatePromotionCommand duplicatePromotionCommand,
            AddPrivateCouponCommand addPrivateCouponCommand,
            AddEntityToListsCommand addEntityToListsCommand,
            PersistEntityCommand persistEntityCommand
            )
        {
            _getManagedListCommand = getManagedListCommand;
            _createManagedListCommand = createManagedListCommand;
            _getListCountCommand = getListCountCommand;
            _findEntityCommand = findEntityCommand;
            _duplicatePromotionCommand = duplicatePromotionCommand;
            _addPrivateCouponCommand = addPrivateCouponCommand;
            _addEntityToListsCommand = addEntityToListsCommand;
            _persistEntityCommand = persistEntityCommand;
        }

        public override async Task<CreateCouponsArgument> Run(
            CreateCouponsArgument arg,
            CommercePipelineExecutionContext context)
        {
            // Does list exist? If not create.
            // Get count.
            // If count is above threashold, exit.
            // If below, for now, just log.

            await this._getManagedListCommand.PerformTransaction(context.CommerceContext, async () =>
            {
                LoyaltyPointsEntity loyaltyPointsEntity = await
                        _findEntityCommand.Process(
                            context.CommerceContext,
                            typeof(LoyaltyPointsEntity),
                            CommerceEntity.IdPrefix<LoyaltyPointsEntity>(),
                            shouldCreate: true)
                    as LoyaltyPointsEntity;
                ManagedList list = await _getManagedListCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                if (list == null)
                {
                    context.Logger.LogInformation($"{this.Name}: List {Constants.AvailableCouponsList} not found. Creating it.");
                    list = await _createManagedListCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                }

                long count = await _getListCountCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                context.Logger.LogInformation(
                        $"{this.Name}: List {Constants.AvailableCouponsList} has {count} items.");
                if (count <= loyaltyPointsEntity.ReprovisionTriggerCount)
                {
                    context.Logger.LogInformation(
                        $"{this.Name}: List {Constants.AvailableCouponsList} is at or under reprovision count of {loyaltyPointsEntity.ReprovisionTriggerCount}.");
                    var coupons = await AddCoupons(context.CommerceContext, loyaltyPointsEntity);
                    List<string> couponList = new List<string> { { Constants.AvailableCouponsList } };
                    foreach (var coupon in coupons)
                    {
                        await _addEntityToListsCommand.Process(context.CommerceContext, coupon.Id, couponList);
                    }

                    await _persistEntityCommand.Process(context.CommerceContext, loyaltyPointsEntity);
                    await _persistEntityCommand.Process(context.CommerceContext, list);

                }

            });

            return arg;
        }

        private async Task<List<CommerceEntity>> AddCoupons(CommerceContext context, LoyaltyPointsEntity entity)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();

            entity.SequenceNumber++;
            string suffix = entity.SequenceNumber.ToString();
            var promotion = await CreatePromtion(context, suffix);
            entity.CurrentPromotion = promotion.FriendlyId;

            string entityId = string.Format("{0}{1}-{2}",
                CommerceEntity.IdPrefix<PrivateCouponGroup>(),
                policy.CouponPrefix,
                suffix);
            await _addPrivateCouponCommand.Process(
                context,
                promotion.Id,
                policy.CouponPrefix,
                suffix,
                policy.CouponBlockSize);
            PrivateCouponGroup @group = await _findEntityCommand.Process(context, typeof(PrivateCouponGroup), entityId) as PrivateCouponGroup;
            return group.AsList();
        }
        private async Task<Promotion> CreatePromtion(CommerceContext context, string suffix)
        {
            string templatePromotion = context.GetPolicy<LoyaltyPointsPolicy>().TemplatePromotion;

            string newPromotion = $"{templatePromotion.Substring(50 - suffix.Length)}-{suffix}";
            return await this._duplicatePromotionCommand.Process(context, templatePromotion, newPromotion) as Promotion;
        }
    }
}
