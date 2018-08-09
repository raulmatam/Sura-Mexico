using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Models;
using System.Collections.Generic;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class TramitesDialog : IDialog<object>
    {
        List<Process> processesList = new List<Process>();

        public async Task StartAsync(IDialogContext context)
        {
            string userName = ConfigurationManager.AppSettings["UserName"].ToString();
            await context.PostAsync(String.Format("Oye {0} ¿Podrías ayudarme con tu número de seguridad social por favor?", userName));
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string desiredSecurityNumber = activity.Text;

            if (!String.IsNullOrEmpty(desiredSecurityNumber))
            {
                Activity replyToConversation = CardDesigner(activity, desiredSecurityNumber);
                await context.PostAsync(replyToConversation);
            }
            else
            {
                await context.PostAsync("Por favor necesito un número de 11 dígitos válido");
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task HandleProcessInformation(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string processId = activity.Text;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load($"https://surastorage.blob.core.windows.net/queryfiles/consultaEspecifica09917449861.xml");
            XmlNodeList xProcesses = xDoc.GetElementsByTagName("infoGeneralTramite");


            foreach (XmlNode item in xProcesses)
            {
                processesList.Add(new Process
                {
                    DescProceso = item["descEstatus"].InnerText,
                    FolioServicio = item["folioServicio"].InnerText
                });
            }

            //return processesList;

            //string finalResult =

            //context.PostAsync();

            //context.Wait(MessageReceivedAsync);
        }

        private List<Process> GetExternalInformation(string nss)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load($"https://surastorage.blob.core.windows.net/queryfiles/consultaGeneral" + nss +".xml");
            XmlNodeList xProcesses = xDoc.GetElementsByTagName("infoGeneralTramite");
            

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

        /*public string InvokeService(string NSSInputNumber)
        {
            HttpWebRequest request = CreateSOAPWebRequest();

            XmlDocument SOAPReqBody = new XmlDocument();
            SOAPReqBody.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:con=""http://consultatel.ws.tramites.afore.sura.mx.com/"">
                                   <soapenv:Header/>
                                   <soapenv:Body>
                                      <con:consultaTramiteRequest>
                                         <nss> 09917449861 </nss>
                                         <folioServicio></folioServicio>
                                         <origen> 1 </origen>
                                      </con:consultaTramiteRequest>
                                   </soapenv:Body>
                                </soapenv:Envelope> ");


            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            using (WebResponse Servicers = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Servicers.GetResponseStream()))
                {
                    string ServiceResult = rd.ReadToEnd();
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(ServiceResult);
                    string innerResult = xml.InnerText;
                    XmlDocument newXml = new XmlDocument();
                    newXml.LoadXml(innerResult);
                    XmlNodeList xList = newXml.SelectNodes("/NewDataSet/Table");
                }
            }

            return "done!";
        }*/

        /*private HttpWebRequest CreateSOAPWebRequest()
        {
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"https://187.237.94.57/ConsultaTramitesTelefonWS/RelacionamientoEficienteWSService?wsdl");
            Req.Headers.Add(@"SOAPAction:http://187.237.94.57/ConsultaTramitesTelefonWS");
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            Req.Method = "POST";
            return Req;
        }*/
    }
}