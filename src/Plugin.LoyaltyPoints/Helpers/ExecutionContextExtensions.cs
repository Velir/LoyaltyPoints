using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Sitecore.Commerce.Core;

namespace Plugin.LoyaltyPoints.Helpers
{
    static class ExecutionContextExtensions
    {
        public static async Task AbortWithError(this CommercePipelineExecutionContext context, string message, string key,
            string id)
        {
            context.Abort(await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, key,
                new object[] { id }, message), context);
        }

        public static bool IsNullOrHasErrors(this CommercePipelineExecutionContext context)
        {
            return context?.CommerceContext == null || context.CommerceContext.AnyMessage(m => m.Code == context.GetPolicy<KnownResultCodes>().Error);
        }
    }
}
