using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Configuration;
using AdaptiveCards;
using System.Collections.Generic;
using Newtonsoft.Json;
using SimpleEchoBot.Models;
using SimpleEchoBot.Dialogs;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var reply = context.MakeMessage();

            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["UserName"].ToString()))
            {
                string userName = message.Text;
                ConfigurationManager.AppSettings["UserName"] = userName;
                context.PostAsync(String.Format("Muchas gracias por darme tu nombre {0} ahora dime en qué tema buscas ayuda", userName));

                HttpClient client = new HttpClient();
                HttpResponseMessage response;
                AdaptiveCard card = new AdaptiveCard();
                response = await client.GetAsync(String.Format("https://chatbotsura.blob.core.windows.net/cardstemplates/mainMenuCard.json"));
                var json = await response.Content.ReadAsStringAsync();
                AdaptiveCardParseResult resultString = AdaptiveCard.FromJson(json);
                card = resultString.Card;
                IList<AdaptiveWarning> warnings = resultString.Warnings;


                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
                context.Wait(SelectedCategory);
            }
            else
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response;
                AdaptiveCard card = new AdaptiveCard();
                response = await client.GetAsync(String.Format("https://asistentesura9ad0.blob.core.windows.net/assetsbotsura/mainMenuCard.json"));
                var json = await response.Content.ReadAsStringAsync();
                AdaptiveCardParseResult resultString = AdaptiveCard.FromJson(json);
                card = resultString.Card;
                IList<AdaptiveWarning> warnings = resultString.Warnings;


                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);
                context.Wait(SelectedCategory);
            }
        }

        private async Task SelectedCategory(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activityValue = (Activity)context.Activity;
            var response = JsonConvert.DeserializeObject<AnswerHandler>(activityValue.Value.ToString());

            switch (response.id)
            {
                case "faq":
                    context.Call(new FAQDialog(), CallBack);
                    break;
                case "tramite":
                    context.Call(new SeguimientoDialog(), CallBack);
                    break;
                case "servicios":
                    context.PostAsync("¿Qué servicio quieres conocer?");
                    context.Call(new ServiciosDialog(), CallBack);
                    break;
                default:
                    break;
            }
        }

        private async Task CallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }
    }
}