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
        private readonly AddListEntitiesPipeline _addListEntitiesPipeline;
        private readonly PersistEntityCommand _persistEntityCommand;
        private readonly GetEntitiesInListCommand _getEntitiesInListCommand;

        //TODO Break this monster class up. Too many concerns. :(

        public CreateCouponsBlock(
            GetManagedListCommand getManagedListCommand,
            CreateManagedListCommand createManagedListCommand,
            GetListCountCommand getListCountCommand,
            FindEntityCommand findEntityCommand,
            DuplicatePromotionCommand duplicatePromotionCommand,
            AddPrivateCouponCommand addPrivateCouponCommand,
            AddListEntitiesPipeline addListEntitiesPipeline,
            PersistEntityCommand persistEntityCommand,
            GetEntitiesInListCommand getEntitiesInListCommand
            )
        {
            _getManagedListCommand = getManagedListCommand;
            _createManagedListCommand = createManagedListCommand;
            _getListCountCommand = getListCountCommand;
            _findEntityCommand = findEntityCommand;
            _duplicatePromotionCommand = duplicatePromotionCommand;
            _addPrivateCouponCommand = addPrivateCouponCommand;
            _addListEntitiesPipeline = addListEntitiesPipeline;
            _persistEntityCommand = persistEntityCommand;
            _getEntitiesInListCommand = getEntitiesInListCommand;
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
                            Constants.EntityId,
                            shouldCreate: true)
                    as LoyaltyPointsEntity;
                LoyaltyPointsPolicy policy = context.CommerceContext.GetPolicy<LoyaltyPointsPolicy>();

                ManagedList list = await _getManagedListCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                if (list == null)
                {
                    context.Logger.LogInformation($"{this.Name}: List {Constants.AvailableCouponsList} not found. Creating it.");
                    list = await _createManagedListCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                }

                long count = await _getListCountCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                context.Logger.LogInformation(
                        $"{this.Name}: List {Constants.AvailableCouponsList} has {count} items.");
                if (count <= policy.ReprovisionTriggerCount)
                {
                    context.Logger.LogInformation(
                        $"{this.Name}: List {Constants.AvailableCouponsList} is at or under reprovision count of {policy.ReprovisionTriggerCount}.");
                    await AddCoupons(context, loyaltyPointsEntity);
                    await CopyCouponsToList(context, loyaltyPointsEntity, list);

                    

                    await _persistEntityCommand.Process(context.CommerceContext, loyaltyPointsEntity);
                    await _persistEntityCommand.Process(context.CommerceContext, list);

                     
                }
            });

            // Note: I'm not sure if there is a way to ensure this minion is
            // a singleton (I think that's unlikely as someone could 
            // always spin up another minion server, so it is necessary to
            // add the coupons to the coupon list inside a transaction,
            // to avoid duplicate insertions.
            
            // TODO Discuss deadlock avoidance patterns with Rob.

            return arg;
        }

        private async Task CopyCouponsToList(CommercePipelineExecutionContext context, LoyaltyPointsEntity entity, ManagedList list)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            string listName = $"promotion-{policy.CouponPrefix}-{entity.SequenceNumber}-unallocatedcoupons";
            var coupons = await _getEntitiesInListCommand.Process(context.CommerceContext,
                listName, 0,
                policy.CouponBlockSize);
            await _addListEntitiesPipeline.Run(new ListEntitiesArgument(coupons, list.Name), context);
        }

        

        private async Task AddCoupons(CommercePipelineExecutionContext context, LoyaltyPointsEntity entity)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            entity.SequenceNumber++;
            string suffix = entity.SequenceNumber.ToString();
            var promotion = await CreatePromtion(context, suffix);
            if (promotion == null)
            {
                context.Abort($"{this.Name}: Unable to generate LoyaltyPoints promotion.", context);
                return;
            }
            entity.CurrentPromotion = promotion.FriendlyId;

       
            await _addPrivateCouponCommand.Process(
                context.CommerceContext,
                promotion.Id,
                policy.CouponPrefix,
                suffix,
                policy.CouponBlockSize);
            promotion.SetComponent(new ApprovalComponent(context.GetPolicy<ApprovalStatusPolicy>().Approved));
            await _persistEntityCommand.Process(context.CommerceContext, promotion);
        }

        private async Task<Promotion> CreatePromtion(CommercePipelineExecutionContext context, string suffix)
        {
            string templatePromotion = context.GetPolicy<LoyaltyPointsPolicy>().TemplatePromotion;

            string newPromotion = string.Format(Constants.GeneratedPromotion, suffix); 
            return await this._duplicatePromotionCommand.Process(context.CommerceContext, templatePromotion, newPromotion) as Promotion;
        }
    }
}
