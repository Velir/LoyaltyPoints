using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.LoyaltyPoints.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Block.DoActionEditLoyaltyPoints")]
    class DoActionEditLoyaltyPointsViewBlock : PipelineBlock<EntityView, EntityView,
        CommercePipelineExecutionContext>
    {
        private readonly EditSellableItemCommand _editSellableItemCommand;

        public DoActionEditLoyaltyPointsViewBlock(EditSellableItemCommand editSellableItemCommand)
        {
            _editSellableItemCommand = editSellableItemCommand;
        }


        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull(string.Format($"{Name}: The argument cannot be null"));
            if (string.IsNullOrEmpty(entityView.Action))
                return entityView;
            CommerceEntity commerceEntity = context.CommerceContext.GetObject<CommerceEntity>(p => p.Id.Equals(entityView.EntityId, StringComparison.OrdinalIgnoreCase));
            SellableItem sellableItem = commerceEntity as SellableItem;
            if (entityView.Action.Equals("EditSellableItemSpecifications", StringComparison.OrdinalIgnoreCase))
            {
                if (entityView.ContainsProperty(Constants.LoyaltyPoints))
                {
                    string s = entityView.Properties.FirstOrDefault(p =>
                        p.Name.Equals(Constants.LoyaltyPoints, StringComparison.OrdinalIgnoreCase))?.Value;
                    if (int.TryParse(s, out var value))
                    {
                        sellableItem.GetComponent<LoyaltyPointsComponent>().Points = value;
                        await _editSellableItemCommand.Process(context.CommerceContext, sellableItem);
                    }
                }
            }

            return entityView;
        }
    }
}