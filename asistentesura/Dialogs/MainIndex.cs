using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using SimpleEchoBot.Models;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class MainIndex : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

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
                    context.Call(new TramitesDialog(), CallBack);
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