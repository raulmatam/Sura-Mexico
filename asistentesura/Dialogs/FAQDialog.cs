using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SimpleEchoBot;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class FAQDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Bienvenido a la sección de Preguntas Frecuentes, te muestro el menú de opciones y te recuerdo que para regresar al menú principal solo debes escribir 'menu principal'");

            var reply = (Activity)context.MakeMessage();
            reply.Attachments.Clear();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = CarouselCardCreation();

            await context.PostAsync(reply);

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string responseString = string.Empty;
            var query = activity.Text.ToString();

            switch (query)
            {
                case "menu principal":
                    context.Call(new MainIndex(), CallBack);
                    break;
                case "afore":
                    context.Call(new AforeDialog(), CallBack);
                    break;
                case "fondos":
                    context.Call(new FondosInversionDialog(), CallBack);
                    break;
                case "pensiones":
                    context.Call(new PensionesDialog(), CallBack);
                    break;
                default:
                    break;
            }
        }

        private IList<Attachment> CarouselCardCreation()
        {
            HeroCard myCard = new HeroCard
            {
                Title = "Fondo de ahorro para el retiro",
                Subtitle = "Somos una de las Administradoras de Fondos de Ahorro para el Retiro más grandes de México"
            };

            List<CardImage> imageList = new List<CardImage>();
            List<CardAction> buttonsList = new List<CardAction>();
            CardImage characterImage = new CardImage("https://asistentesura9ad0.blob.core.windows.net/assetsbotsura/suraAfore.png");
            imageList.Add(characterImage);
            myCard.Images = imageList;

            CardAction moreInformationButton = new CardAction();
            moreInformationButton.Title = "Preguntas de Afore";
            moreInformationButton.Type = ActionTypes.ImBack;
            moreInformationButton.Value = "afore";
            buttonsList.Add(moreInformationButton);
            myCard.Buttons = buttonsList;

            HeroCard myCard2 = new HeroCard
            {
                Title = "Fondos de inversión",
                Subtitle = "Conoce las publicaciones que tenemos preparadas para ti y a continuar disfrutando de los beneficios de invertir con SURA"
            };

            List<CardImage> image2List = new List<CardImage>();
            List<CardAction> buttons2List = new List<CardAction>();
            CardImage character2Image = new CardImage("https://asistentesura9ad0.blob.core.windows.net/assetsbotsura/suraFondos.jpg");
            image2List.Add(character2Image);
            myCard2.Images = image2List;

            CardAction moreInformationButton2 = new CardAction();
            moreInformationButton2.Title = "Preguntas de Fondos de inversión";
            moreInformationButton2.Type = ActionTypes.ImBack;
            moreInformationButton2.Value = "fondos";
            buttons2List.Add(moreInformationButton2);
            myCard2.Buttons = buttons2List;

            HeroCard myCard3 = new HeroCard
            {
                Title = "Pensiones",
                Subtitle = "Conoce todo lo que necesites acerca de tu plan de pensión y todos los detalles relacionados"
            };

            List<CardImage> image3List = new List<CardImage>();
            List<CardAction> buttons3List = new List<CardAction>();
            CardImage character3Image = new CardImage("https://asistentesura9ad0.blob.core.windows.net/assetsbotsura/suraPensiones.png");
            image3List.Add(character3Image);
            myCard3.Images = image3List;

            CardAction moreInformationButton3 = new CardAction();
            moreInformationButton3.Title = "Más información";
            moreInformationButton3.Type = ActionTypes.ImBack;
            moreInformationButton3.Value = "pensiones";
            buttons3List.Add(moreInformationButton3);
            myCard3.Buttons = buttons3List;

            List<Attachment> listAttachment = new List<Attachment>();
            listAttachment.Add(myCard.ToAttachment());
            listAttachment.Add(myCard2.ToAttachment());
            listAttachment.Add(myCard3.ToAttachment());
            return listAttachment;
        }

        private async Task CallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }
    }
}