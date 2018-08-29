using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    [PipelineDisplayName("LoyaltyPoints.Blocks.GetListPriceAmount")]
    public class GetListPriceAmountBlock : PipelineBlock<SellableItem, decimal, CommercePipelineExecutionContext>
    {
        public override Task<decimal> Run(SellableItem sellableItem, CommercePipelineExecutionContext context)
        {
            Condition.Requires(sellableItem).IsNotNull($"{this.Name}: The SellableItem cannot be null");

            decimal listPriceAmount = sellableItem.ListPrice.Amount;

            return Task.FromResult(listPriceAmount);
        }
    }
}