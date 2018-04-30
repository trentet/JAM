﻿using HtmlAgilityPack;
using MailKit;
using MimeKit;
using System;
using System.Linq;

namespace JobAlertManagerGUI.Model
{
    public class Email
    {
        private readonly string[] _trimStartList = { "\r", "\n", " ", "-", "\t", "&nbsp;" };
        private readonly string[] _replaceBodyList = { "\r\n", "\r", "\n", "\t", "--", "- -", "&nbsp;" };
        private string _preview;

        public UniqueId Id { get; set; }
        public IMessageSummary MessageSummary { get; set; }
        public string Preview { get => _preview; }
        public string FileName { get => Id + ".eml";  }

        public Email(IMessageSummary summary)
        {
            Id = summary.UniqueId;
            MessageSummary = summary;
            SetPreview(null);
        }

        public Email(IMessageSummary summary, MimeMessage message)
        {
            Id = summary.UniqueId;
            MessageSummary = summary;
            SetPreview(message);
        }

        public void SetPreview(MimeMessage message)
        {
            if (MessageSummary.TextBody != null && message == null)
            {
                _preview = MessageSummary.PreviewText;
            }
            else if (MessageSummary.HtmlBody != null && message.HtmlBody != null)
            {
                _preview = GetPreview(message);
            }
            else
            {
                _preview = "Unable to load preview...";
            }

            _preview = TrimPreview(_preview);
        }

        public string GetPreview(MimeMessage message)
        {
            try
            {
                var previewText = "";
                if (message.TextBody != null)
                {
                    var messageBody = message.Body;

                    if (messageBody.GetType() == typeof(MultipartAlternative))
                    {
                        previewText = ((MultipartAlternative)messageBody).TextBody;
                    }
                    else if (messageBody.GetType() == typeof(Multipart))
                    {
                        var multipartMsg = (Multipart)messageBody;
                        if (multipartMsg[0].GetType() == typeof(TextPart))
                        {
                            previewText = ((TextPart)multipartMsg[0]).Text;
                        }
                        else
                        {
                            var type = multipartMsg[0].GetType();
                            previewText = "Unsupported message type detected (" + type + ")...";
                            return previewText;
                        }
                    }
                    else if (messageBody.GetType() == typeof(MultipartRelated))
                    {
                        var multipartMsg = (MultipartRelated)messageBody;
                        if (multipartMsg[0].GetType() == typeof(MultipartAlternative))
                        {
                            previewText = ((MultipartAlternative)multipartMsg[0]).TextBody;
                        }
                        else
                        {
                            var type = multipartMsg[0].GetType();
                            previewText = "Unsupported message type detected (" + type + ")...";
                            return previewText;
                        }
                    }
                    else if (messageBody.GetType() == typeof(TextPart))
                    {
                        previewText = ((TextPart)messageBody).Text;
                    }
                    else
                    {
                        var type = messageBody.GetType();
                        previewText = "Unsupported message type detected (" + type + ")...";
                        return previewText;
                    }
                }
                else if (message.HtmlBody != null)
                {
                    var htmlMsg = message.Body;
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
                        return "HTML Body type detected. Unsupported message type (" + type + ")...";
                    }
                    previewText = GetBodyTextFromHtml(previewText);
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
            while (_trimStartList.Contains(previewText.Substring(0, 1)) || _trimStartList.Contains(previewText.Substring(0, 2)))
            {
                foreach (string trimFilter in _trimStartList)
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

            foreach (string bodyFilter in _replaceBodyList)
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

        private static string GetBodyTextFromHtml(string htmlString)
        {
            var bodyText = "Unable to retrieve message preview...";
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);
            foreach (var body in doc.DocumentNode.SelectNodes("//body"))
            {
                bodyText = body.InnerText;
                break;
            }
            return bodyText;
        }
    }
}
