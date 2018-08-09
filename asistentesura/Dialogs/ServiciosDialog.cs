using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
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
    public class ServiciosDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            await GetMainOptions(reply);

            await context.PostAsync(reply);
            context.Wait(SelectedCategory);
        }

        private static async Task GetMainOptions(IMessageActivity reply)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            AdaptiveCard card = new AdaptiveCard();
            response = await client.GetAsync(String.Format("https://surastorage.blob.core.windows.net/cardstemplates/servicios.json"));
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
        }

        private async Task SelectedCategory(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activityValue = (Activity)context.Activity;
            var response = JsonConvert.DeserializeObject<AnswerHandler>(activityValue.Value.ToString());

            var reply = context.MakeMessage();
            await GetMainOptions(reply);

            switch (response.id)
            {
                case "acerca":
                    var videoMsg = context.MakeMessage();
                    VideoCard myCard = await VideoCardCreation("Acerca de nosotros", "Si una imagen vale más que mil palabras, imagina lo que un video puede explicar de nosotros. ¡Te invitamos a conocernos!");
                    videoMsg.Attachments.Add(myCard.ToAttachment());
                    await context.PostAsync(videoMsg);
                    break;
                case "correo":
                    var receivedQuestion = String.Format("Hola administrador, el usuario {0} ha pedido un proceso de seguimiento personalizado", ConfigurationManager.AppSettings["UserName"].ToString());
                    var postBody = $"{{\"message\": \"{receivedQuestion}\"}}";
                    var logicAppUrl = "https://prod-14.southcentralus.logic.azure.com:443/workflows/78720171b08842efb0e7c0c76a9b84a1/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=d5-qPi-wkrxP3TiqBo5E115vJCdO28oLUe7mWmRbZRM";

                    using (WebClient client = new WebClient())
                    {
                        client.Encoding = System.Text.Encoding.UTF8;
                        client.Headers.Add("Content-Type", "application/json");
                        client.UploadString(logicAppUrl, postBody);
                    }

                    await context.PostAsync("Muchas gracias por seleccionar esta opción, ahora te contactaremos por correo electrónico.");

                    break;
                default:
                    context.PostAsync("Lo siento no sé que hacer con esta respuesta");
                    break;
            }

            await context.PostAsync(reply);
        }

        private async Task<VideoCard> VideoCardCreation(string name, string content)
        {
            VideoCard myCard = new VideoCard
            {
                Title = name,
                Text = content,
                Media = new List<MediaUrl>
                 {
                  new MediaUrl()
                    {
                        Url = "https://www.youtube.com/watch?v=HgtO2L8FbpI"
                    }
                 }
            };
            return myCard;
        }

        private async Task CallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }
    }
}