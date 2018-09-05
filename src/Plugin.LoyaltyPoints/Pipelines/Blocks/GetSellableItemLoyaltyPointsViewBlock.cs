using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Plugin.LoyaltyPoints.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.block.getsellableitemview")]
    class GetSellableItemLoyaltyPointsViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext
    >
    {
        public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            EntityViewArgument entityViewArgument = context.CommerceContext.GetObject<EntityViewArgument>();

            Condition.Requires(entityView).IsNotNull($"{Name}: The argument {nameof(entityView)} cannot be null");

            string action = entityViewArgument?.ForAction;

            if (action == "EditSellableItemSpecifications")
            {
                EditProperties(entityView, (SellableItem) entityViewArgument?.Entity);
            }

            return Task.FromResult(entityView);

        }


        private void EditProperties(EntityView entityView, SellableItem entity)
        {
            if (entity == null)
            {
                return;
            }

            List<ViewProperty> properties = entityView.Properties;
            ViewProperty viewProperty = new ViewProperty();
            viewProperty.Name = Constants.LoyaltyPoints;
            viewProperty.RawValue = entity.GetComponent<LoyaltyPointsComponent>().Points;
            viewProperty.IsReadOnly = false;
            viewProperty.IsRequired = false;
            viewProperty.IsHidden = false;
            properties.Add(viewProperty);
        }
    }
}
