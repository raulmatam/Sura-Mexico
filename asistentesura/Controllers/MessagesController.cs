using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System.Threading;
using System.Globalization;
using System;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new EchoDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity iConversationUpdated = message as IConversationUpdateActivity;
                if (iConversationUpdated != null)
                {
                    ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));
                    foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                    {
                        // if the bot is added, then   
                        if (member.Id == iConversationUpdated.Recipient.Id)
                        {

                            // Saludos dependiendo la hora
                            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
                            Int32 hora = DateTime.Now.Hour;

                            if (hora < 12)
                            {
                                var reply = ((Activity)iConversationUpdated).CreateReply($"¡Hola! Buen día. Para comenzar me encantaría saber tu nombre");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            if (hora < 19)
                            {
                                var reply = ((Activity)iConversationUpdated).CreateReply($"¡Hola! Buena tarde. Para comenzar me encantaría saber tu nombre");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            if (hora < 24)
                            {
                                var reply = ((Activity)iConversationUpdated).CreateReply($"¡Hola! Buena noche. Para comenzar me encantaría saber tu nombre");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }

                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}