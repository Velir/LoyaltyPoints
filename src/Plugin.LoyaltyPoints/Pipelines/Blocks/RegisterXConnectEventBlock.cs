using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plugin.LoyaltyPoints.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;
using Sitecore.XConnect.Collection.Model;

namespace Plugin.LoyaltyPoints.Pipelines.Blocks
{
    class RegisterXConnectEventBlock:PipelineBlock<IssueCouponArgument, IssueCouponArgument, CommercePipelineExecutionContext>
    {
        public override async Task<IssueCouponArgument> Run(IssueCouponArgument arg, CommercePipelineExecutionContext context)
        {
            // Sitecore docs on hitting xConnect in a non-Sitecore context:
            // https://doc.sitecore.net/developers/xp/xconnect/xconnect-client-api/xconnect-client-api-overview/get-client-outside-sitecore.html

            // See https://doc.sitecore.net/developers/privacy-guide/user-contact-customer.html for how you get from Customer to Contact. 



            if (!arg.Coupons.Any())
            {
                return arg;
            }

            LoyaltyPointsPolicy policy = context.GetPolicy<LoyaltyPointsPolicy>();

            if (string.IsNullOrEmpty(policy.XConnectClientCertConnectionString))
            {
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, this.Name,
                    new Object[] {}, $"{nameof(policy.XConnectClientCertConnectionString)} not set.");
                return arg;
            }
            
            if (string.IsNullOrEmpty(policy.XConnectUrl))  
            {
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, this.Name,
                    new Object[] { }, $"{nameof(policy.XConnectUrl)} not set.");
                return arg;
            }

           
            CertificateWebRequestHandlerModifierOptions options =
                CertificateWebRequestHandlerModifierOptions.Parse(policy.XConnectClientCertConnectionString);
            var certificateModifier = new CertificateWebRequestHandlerModifier(options);
            List<IHttpClientModifier> clientModifiers = new List<IHttpClientModifier>();
            var timeoutClientModifier = new TimeoutHttpClientModifier(new TimeSpan(0, 0, 20));
            clientModifiers.Add(timeoutClientModifier);
            
            var collectionClient = new CollectionWebApiClient(new Uri($"{policy.XConnectUrl}/odata"), clientModifiers, new[] { certificateModifier });
            var searchClient = new SearchWebApiClient(new Uri($"{policy.XConnectUrl}/odata"), clientModifiers, new[] { certificateModifier });
            var configurationClient = new ConfigurationWebApiClient(new Uri($"{policy.XConnectUrl}/configuration"), clientModifiers, new[] { certificateModifier });

            var cfg = new XConnectClientConfiguration(
                new XdbRuntimeModel(CollectionModel.Model), collectionClient, searchClient, configurationClient);
            try
            {
                await cfg.InitializeAsync();

            }
            catch (XdbModelConflictException ce)
            {
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, this.Name,
                    new Object[] {ce }, $"{Name}: Exception: {ce.Message} ");
                return arg;
            }

            using (var client = new XConnectClient(cfg))
            {
                try
                {
                    string externalId = $"CommerceUsers\\{arg.Customer.Email}"; 
                    var reference = new IdentifiedContactReference(Constants.CommerceUser, externalId);

                    Task<Contact> contactTask = client.GetAsync<Contact>(reference, new ContactExpandOptions() { });

                    Contact contact = await contactTask;

                    if (contact == null)
                    {
                        return arg;
                    }
                  
                    var channel = policy.ChannelId; 
                    var interaction = new Interaction(contact, InteractionInitiator.Brand,
                        channel, this.GetType().FullName);  // TODO Not sure what a logical device would be for a commerce plugin.  Ask the Slackers.
                    
                    // TODO Replace with custom model, and include coupon entity ID.
                    arg.Coupons.ForEach(
                        couponCode => 
                            interaction.Events.Add(new Event(policy.EventId, DateTime.UtcNow){Text="Loyalty points coupon awarded", Data = couponCode, DataKey = "Coupon Code"}));
                     
                    client.AddInteraction(interaction);
                    
                    await client.SubmitAsync();
                    
                }
                catch (XdbExecutionException ex)
                {
                    context.Abort(ex.Message, ex);
                    // Manage exceptions
                }
            }

      

            return arg;
        }
    }
}
