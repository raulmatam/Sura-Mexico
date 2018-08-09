using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Models;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class FondosInversionDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await StartConversation(context);
        }

        private async Task StartConversation(IDialogContext context)
        {
            await context.PostAsync("¿Qué necesitas saber acerca de los Fondos de Inversión Sura? Te recuerdo que si deseas volver al menú anterior solo escribe 'regresar'");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;
            string responseString = string.Empty;
            var query = activity.Text.ToString();

            if (query == "regresar")
            {
                context.Call(new FAQDialog(), CallBack);
            }
            else
            {
                var knowledgebaseId = "64b38a8c-0075-4bc9-8466-79c2148d7b05";
                var qnamakerSubscriptionKey = "EndpointKey c7eb7a4f-937e-47cc-8eca-5308d8f7cd07";

                Uri qnamakerUriBase = new Uri("https://inversionqna.azurewebsites.net/qnamaker");
                var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");
                var postBody = $"{{\"question\": \"{query}\"}}";

                using (WebClient client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers.Add("Authorization", qnamakerSubscriptionKey);
                    client.Headers.Add("Content-Type", "application/json");
                    responseString = client.UploadString(builder.Uri, postBody);
                }
                var receivedAnswer = QnAHelper.HandleResponse(responseString);
                await context.PostAsync(receivedAnswer);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task CallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }
    }
}