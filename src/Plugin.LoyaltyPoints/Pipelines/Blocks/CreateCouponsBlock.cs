using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Entities;
using Plugin.LoyaltyPoints.Pipelines.Arguments;
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
    /// Ensures existence of available coupon list, checks count.
    /// If necessary, creates coupons, add to list.
    ///
    /// Developer notes:
    /// I'm not sure whether allocation is a necessary step for coupons
    /// to be applied, or whether this is just for managing workflow on
    /// screens. I will add that step if necessary.
    /// Update: Allocation is required. TODO Add allocation.
    ///
    /// Update: It is not necessary to create new promotions, since coupon blocks can be added to an approved promotion.
    /// TODO: Remove logic to create new promotions, rename <see cref="LoyaltyPointsPolicy.TemmplatePromotion"/> to "Promotion" and simply add coupons to that promotion.
    /// Note: It is still necessary to 
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
            await this._getManagedListCommand.PerformTransaction(context.CommerceContext, async () =>
            {
                LoyaltyPointsEntity loyaltyPointsEntity = await _findEntityCommand.Process(context.CommerceContext,typeof(LoyaltyPointsEntity),
                            Constants.EntityId,shouldCreate: true) as LoyaltyPointsEntity;
                var policy = context.CommerceContext.GetPolicy<LoyaltyPointsPolicy>();
                var list = await EnsureList(context, Constants.AvailableCouponsList);

                long count = await _getListCountCommand.Process(context.CommerceContext, Constants.AvailableCouponsList);
                context.Logger.LogDebug(
                        $"{this.Name}: List {Constants.AvailableCouponsList} has {count} items.");

                if (count <= policy.ReprovisionTriggerCount)
                {
                    context.Logger.LogDebug(
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

        private async Task<ManagedList> EnsureList(CommercePipelineExecutionContext context, string listName)
        {
            ManagedList list = await _getManagedListCommand.Process(context.CommerceContext, listName);
            if (list == null)
            {
                context.Logger.LogInformation($"{this.Name}: List {listName} not found. Creating it.");
                list = await _createManagedListCommand.Process(context.CommerceContext, listName);
            }

            return list;
        }

        private async Task CopyCouponsToList(CommercePipelineExecutionContext context, LoyaltyPointsEntity entity, ManagedList targetList)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            string sourceListName = $"promotion-{policy.CouponPrefix}-{entity.SequenceNumber}-unallocatedcoupons";
            var coupons = await _getEntitiesInListCommand.Process(context.CommerceContext,
                sourceListName, 0,
                policy.CouponBlockSize);
            await _addListEntitiesPipeline.Run(new ListEntitiesArgument(coupons, targetList.Name), context);
        }

        

        private async Task AddCoupons(CommercePipelineExecutionContext context, LoyaltyPointsEntity entity)
        {
            var policy = context.GetPolicy<LoyaltyPointsPolicy>();
            entity.SequenceNumber++;
            string suffix = entity.SequenceNumber.ToString();
            var promotion = await CreatePromotion(context, suffix);
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

        private async Task<Promotion> CreatePromotion(CommercePipelineExecutionContext context, string suffix)
        {
            string templatePromotion = context.GetPolicy<LoyaltyPointsPolicy>().TemplatePromotion;

            string newPromotion = string.Format(Constants.GeneratedPromotion, suffix); 
            return await this._duplicatePromotionCommand.Process(context.CommerceContext, templatePromotion, newPromotion) as Promotion;
        }
    }
}
