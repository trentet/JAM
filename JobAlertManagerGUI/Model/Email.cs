using HtmlAgilityPack;
using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlertManagerGUI.Model
{
    public class Email
    {
        private IMessageSummary messageSummary;
        private string[] TrimStartList = new string[] { "\r", "\n", " ", "-", "\t", "&nbsp;" };
        private string[] ReplaceBodyList = new string[] { "\r\n", "\r", "\n", "\t", "--", "- -", "&nbsp;" };

        public IMessageSummary MessageSummary { get => messageSummary; set => messageSummary = value; }
        public string Preview { get => GetPreview(MessageSummary); }

        public Email(IMessageSummary messageSummary)
        {
            MessageSummary = messageSummary;
        }

        public string GetPreview(IMessageSummary MessageSummary)
        {
            try
            {
                string previewText = "";
                if (MessageSummary.TextBody != null)
                {
                    var messageBody = AppConfig.CurrentIMap.Inbox.GetMessage(MessageSummary.UniqueId).Body;

                    if (messageBody.GetType() == typeof(MultipartAlternative))
                    {
                        previewText = ((MultipartAlternative)messageBody).TextBody;
                    }
                    else if (messageBody.GetType() == typeof(Multipart))
                    {
                        Multipart multipartMsg = (Multipart)messageBody;
                        if (multipartMsg[0].GetType() == typeof(TextPart))
                        {
                            previewText = ((TextPart)multipartMsg[0]).Text;
                        }
                        else
                        {
                            var type = multipartMsg[0].GetType();
                            return null;
                        }
                    }
                    else if (messageBody.GetType() == typeof(MultipartRelated))
                    {
                        MultipartRelated multipartMsg = (MultipartRelated)messageBody;
                        if (multipartMsg[0].GetType() == typeof(MultipartAlternative))
                        {
                            previewText = ((MultipartAlternative)multipartMsg[0]).TextBody;
                        }
                        else
                        {
                            var type = multipartMsg[0].GetType();
                            return null;
                        }
                    }
                    else if (messageBody.GetType() == typeof(TextPart))
                    {
                        previewText = ((TextPart)messageBody).Text;
                    }
                    else
                    {
                        var thing = messageBody.GetType();
                        previewText = "Unsupported message type detected...";
                        return previewText;
                    }
                }
                else if (MessageSummary.HtmlBody != null)
                {
                    var htmlMsg = AppConfig.CurrentIMap.Inbox.GetMessage(MessageSummary.UniqueId).Body;
                    if (htmlMsg.GetType() == typeof(TextPart))
                    {
                        previewText = ((TextPart)htmlMsg).Text;
                    }
                    else if (htmlMsg.GetType() == typeof(MultipartAlternative))
                    {
                        previewText = ((MultipartAlternative)htmlMsg).HtmlBody;
                    }
                    else
                    {
                        var type = htmlMsg.GetType();
                        return "HTML Body type detected. Unsupported message type...";
                    }
                    previewText = GetBodyTextFromHTML(previewText);
                }
                else
                {

                }

                previewText = TrimPreview(previewText);
                return previewText;
            }
            catch (MailKit.Net.Imap.ImapProtocolException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return "Error retrieving email preview...";
            }

        }

        private string TrimPreview(string previewText)
        {
            while (TrimStartList.Contains(previewText.Substring(0, 1)) || TrimStartList.Contains(previewText.Substring(0, 2)))
            {
                foreach (string trimFilter in TrimStartList)
                {
                    if (previewText.StartsWith(trimFilter))
                    {
                        previewText = previewText.TrimStart(trimFilter.ToCharArray());
                    }
                }
            }

            if (previewText.Length > 150)
            {
                previewText = previewText.Substring(0, 150);
            }

            foreach (string bodyFilter in ReplaceBodyList)
            {
                previewText = previewText.Replace(bodyFilter, " ");
            }

            int oldLength = previewText.Length;
            while (true)
            {
                previewText = previewText.Replace("  ", " ");
                if (oldLength == previewText.Length)
                {
                    break;
                }
                else
                {
                    oldLength = previewText.Length;
                }
            }

            if (previewText.Length > 47)
            {
                previewText = previewText.Substring(0, 47);
                previewText += "...";
            }

            return previewText;
        }

        private string GetBodyTextFromHTML(string htmlString)
        {
            string bodyText = "Unable to retrieve message preview...";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlString);
            foreach (HtmlNode body in doc.DocumentNode.SelectNodes("//body"))
            {
                bodyText = body.InnerText;
                break;
            }
            return bodyText;
        }
    }
}
