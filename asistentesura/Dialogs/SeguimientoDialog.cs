using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SimpleEchoBot;
using SimpleEchoBot.Models;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class SeguimientoDialog : IDialog<object>
    {
        string desiredSecurityNumber = string.Empty;

        public async Task StartAsync(IDialogContext context)
        {
            string userName = ConfigurationManager.AppSettings["UserName"].ToString();
            await context.PostAsync(String.Format("Oye {0} ¿Podrías ayudarme con tu número de seguridad social por favor?", userName));
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            desiredSecurityNumber = activity.Text;

            if (!String.IsNullOrEmpty(desiredSecurityNumber))
            {
                Activity replyToConversation = CardDesigner(activity, desiredSecurityNumber);
                await context.PostAsync(replyToConversation);
                context.Wait(HandleProcessInformation);
            }
            else
            {
                await context.PostAsync("Por favor necesito un número de 11 dígitos válido");
                context.Wait(MessageReceivedAsync);
            }          
        }

        private async Task HandleProcessInformation(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string processId = activity.Text;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load($"https://surastorage.blob.core.windows.net/queryfiles/consultaEspecifica09917449861.xml");
            XmlNodeList xProcesses = xDoc.GetElementsByTagName("infoGeneralTramite");
            string processResult = string.Empty;
            string processDetail = string.Empty;

            foreach (XmlNode item in xProcesses)
            {

                processResult = item["detalleTramite"]["descEstatus"].InnerText;
                processDetail = item["detalleTramite"]["detalleEstatus"].InnerText;
            }

            string finalProcessMessage = String.Format("El estado de tu trámite es {0}, el detalle es: {1}.", processResult, processDetail);
            await context.PostAsync(finalProcessMessage);
            await context.PostAsync("Escribe 'regresar' si deseas revisar otro trámite o escribe 'Inicio' para volver al principio");
            context.Wait(HandleDialogToRoot);
        }

        private async Task HandleDialogToRoot(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string selectedResponse = activity.Text.ToLower();

            switch (selectedResponse)
            {
                case "regresar":
                    Activity replyToConversation = CardDesigner(activity, desiredSecurityNumber);
                    await context.PostAsync(replyToConversation);
                    context.Wait(HandleProcessInformation);
                    break;
                case "inicio":
                    context.Call(new MainIndex(), CallBack);
                    break;
            }
        }

        private List<Process> GetExternalInformation(string nss)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load($"https://surastorage.blob.core.windows.net/queryfiles/consultaGeneral" + nss + ".xml");
            XmlNodeList xProcesses = xDoc.GetElementsByTagName("infoGeneralTramite");
            List<Process> processesList = new List<Process>();

            foreach (XmlNode item in xProcesses)
            {
                processesList.Add(new Process
                {
                    DescProceso = item["descProceso"].InnerText,
                    FolioServicio = item["folioServicio"].InnerText
                });
            }

            return processesList;
        }

        private Activity CardDesigner(Activity activity, string nss)
        {
            List<Process> processesList = GetExternalInformation(nss);

            Activity replyToConversation = activity.CreateReply();
            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.List;
            replyToConversation.Attachments = new List<Attachment>();
            HeroCard plCard = CreateNewCard(processesList);

            Attachment plAttachment = plCard.ToAttachment();
            replyToConversation.Attachments.Add(plAttachment);
            return replyToConversation;
        }

        private HeroCard CreateNewCard(List<Process> processesList)
        {
            List<CardAction> cardButtons = new List<CardAction>();

            foreach (Process cardContent in processesList)
            {
                CardAction plButton = new CardAction()
                {
                    Value = $"{cardContent.FolioServicio}",
                    Type = "imBack",
                    Title = $"{cardContent.DescProceso}"
                };

                cardButtons.Add(plButton);
            }

            HeroCard plCard = new HeroCard()
            {
                Title = "Trámites en proceso",
                Subtitle = "Encontré los siguientes procesos abiertos con tu NSS",
                Buttons = cardButtons
            };
            return plCard;
        }

        private async Task CallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Done("");
        }
    }
}