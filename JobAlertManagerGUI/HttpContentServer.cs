using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LumiSoft.Net.Mime;

namespace JobAlertManagerGUI
{
    internal enum HttpContentServerState
    {
        Message,
        Data
    }

    internal class HttpContentServer
    {
        private static HttpListener server = new HttpListener();
        public const int buffersize = 1024;

        public static bool IsServing = false;
        public static string Prefix
        {
            get
            {
                return server.Prefixes.First();
            }
        }

        private static Thread sv_thread;
        private static int port = 0;
        private static bool stopserving = false;
        private static bool serverstarting = false;

        public static MimeEntity CurrentEntity = null;
        public static byte[] MediaBuffer = null;
        public static string ContentTypeString = null;

        public static HttpContentServerState State = HttpContentServerState.Message;

        public static bool Start()
        {
            if (IsServing || serverstarting)
                return true;
            serverstarting = true;
            Random rnd = new Random((int)DateTime.Now.Ticks);
            port = rnd.Next(10000, 60000);
            server.Prefixes.Add("http://localhost:" + port + "/");
            try
            {
                server.Start();
                sv_thread = new Thread(ServerThread);
                sv_thread.IsBackground = true;
                sv_thread.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Stop()
        {
            server.Stop();
            IsServing = false;
        }

        private static void ServerThread()
        {
            IsServing = true;
            serverstarting = false;
            HttpListenerContext ctx = null;
            while (!stopserving)
            {
                try
                {
                    ctx = server.GetContext();
                    if (ctx.Request.HttpMethod != "GET")
                    {
                        Return500(ctx);
                        continue;
                    }
                    HandlerRequest(ctx);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    Return500(ctx);
                }
            }
            IsServing = false;
        }

        private static void Return500(HttpListenerContext ctx)
        {
            HttpListenerResponse resp = ctx.Response;
            resp.StatusCode = 500;
        }

        private static bool hasRelatedPart(out MimeEntity parts)
        {
            parts = CurrentEntity.ParentEntity;
            while (parts != null && parts.ContentType != MediaType_enum.Multipart_related)
                parts = parts.ParentEntity;
            return parts != null;
        }

        private static void HandlerRequest(HttpListenerContext ctx)
        {
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;
            if (State == HttpContentServerState.Data)
            {
                string ct = ContentTypeString;
                if (ct.IndexOf(';') != -1)
                    ct = ct.Substring(0, ct.IndexOf(';'));
                resp.StatusCode = 200;
                resp.Headers.Add("Cache-Control: no-cache");
                resp.ContentType = ct.ToLower();
                resp.ContentLength64 = MediaBuffer.Length;
                SendData(resp, MediaBuffer);
            }
            else
                ServeMessage(ctx);
        }

        private static void ServeMessage(HttpListenerContext ctx)
        {
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;
            if (CurrentEntity == null)
            {
                resp.StatusCode = 404;
                resp.OutputStream.Close();
            }
            else
            {
                MimeEntity parts;
                //if (CurrentEntity.ParentEntity != null && CurrentEntity.ParentEntity.ContentType == MediaType_enum.Multipart_related)
                if (hasRelatedPart(out parts))
                {
                    string rawurl = req.RawUrl.ToString();
                    rawurl = rawurl.ToLower() == server.Prefixes.First().ToLower() ? null : rawurl.Substring(rawurl.LastIndexOf('/') + 1);
                    MimeEntity he = null;
                    if (!string.IsNullOrEmpty(rawurl))
                    {
                        foreach (MimeEntity ce in parts.ChildEntities)
                        {
                            if (ce.ContentID == "<" + rawurl + ">")
                            {
                                he = ce;
                                break;
                            }
                        }
                        if (he == null)
                        {
                            foreach (MimeEntity ce in parts.ChildEntities)
                            {
                                if (!string.IsNullOrEmpty(ce.ContentType_Name))
                                {
                                    he = ce;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        he = CurrentEntity;
                    if (he == null)
                    {
                        resp.StatusCode = 404;
                        resp.OutputStream.Close();
                    }
                    else
                    {
                        byte[] outbf = null;
                        string ct,cs;
                        Encoding enc = GetEncoding(he.ContentTypeString, out ct, out cs);
                        if (IsContentBlock(he))
                        {
                            if (ct.ToLower() == "text/html")
                                CheckHtml(he);
                            string txt = Regex.Replace(he.DataText, @"cid\:", m => { return Prefix; }, RegexOptions.IgnoreCase);
                            outbf = enc.GetBytes(txt);
                            resp.ContentEncoding = enc;
                        }
                        else
                            outbf = he.Data;
                        resp.StatusCode = 200;
                        resp.Headers.Add("Cache-Control: no-cache");
                        resp.ContentType = he.ContentTypeString; 
                        resp.ContentLength64 = outbf.Length;
                        SendData(resp, outbf);
                    }
                }
                else
                {
                    string ct,cs;
                    Encoding enc = GetEncoding(CurrentEntity.ContentTypeString, out ct, out cs);
                    if (ct.ToLower() == "text/html")
                    {
                        CheckHtml(CurrentEntity);
                    }
                    byte[] outbf = enc.GetBytes(CurrentEntity.DataText);
                    resp.StatusCode = 200;
                    resp.Headers.Add("Cache-Control: no-cache");

                    resp.ContentEncoding = enc; // Encoding.UTF8;
                    resp.ContentType = CurrentEntity.ContentTypeString;// ct.ToLower();
                    resp.ContentLength64 = outbf.Length;
                    SendData(resp, outbf);
                }
            }
        }

        private static Encoding GetEncoding(string ctstr, out string ct, out string cs)
        {
            ct = ctstr;
            cs = null;
            if (ct.IndexOf(';') != -1)
            {
                cs = ct.Substring(ct.IndexOf(';') + 1).Trim();
                int pos = cs.IndexOf("charset=");
                if (pos != -1)
                {
                    cs = cs.Substring(pos + "charset=".Length).Trim("\" ".ToCharArray());
                    if (cs.IndexOf(';') != -1)
                        cs = cs.Substring(0, cs.IndexOf(';'));
                }
                else
                    cs = null;
                ct = ct.Substring(0, ct.IndexOf(';'));
            }
            LumiSoft.Net.Core.NormalizeEncodings(ref cs);
            Encoding enc;
            if (cs != null)
            {
                try
                {
                    enc = Encoding.GetEncoding(cs);
                }
                catch
                {
                    enc = Encoding.UTF8;
                }
            }
            else
                enc = Encoding.UTF8;
            return enc;
        }

        private static void SendData(HttpListenerResponse resp, byte[] outbf)
        {
            int nbytes = 0;
            while (nbytes < outbf.Length)
            {
                int size = outbf.Length - nbytes > buffersize ? buffersize : outbf.Length - nbytes;
                resp.OutputStream.Write(outbf, nbytes, size);
                nbytes += size;
            }
            resp.OutputStream.Close();
        }

        private static bool IsContentBlock(MimeEntity entity)
        {
            return entity.ContentType == MediaType_enum.Text_html ||
                entity.ContentType == MediaType_enum.Text_plain ||
                entity.ContentType == MediaType_enum.Text_rtf ||
                entity.ContentType == MediaType_enum.Text_xml ||
                entity.ContentType == MediaType_enum.NotSpecified;
        }

        private static void CheckHtml(MimeEntity entity)
        {
            if (entity.DataText.Trim().Length == 0)
                entity.WrapDataText("<html>", "</html>");
            else
            {
                int ipos = 0;
                char c = entity.DataText[ipos];
                while (ipos < entity.DataText.Length - 1 && (c == ' ' || c == '\t' || c == '\r' || c == '\n'))
                    c = entity.DataText[++ipos];
                string dtstr = entity.DataText.Substring(0, "<!DOCTYPE".Length);
                if (dtstr.ToUpper() == "<!DOCTYPE")
                {
                    ipos = entity.DataText.IndexOf('>', ipos + "<!DOCTYPE".Length);
                    while (ipos < entity.DataText.Length - 1 && (c == ' ' || c == '\t' || c == '\r' || c == '\n'))
                        c = entity.DataText[++ipos];
                }
                if (ipos > 4 && ipos < entity.DataText.Length - 5)
                {
                    string hdr = entity.DataText.Substring(ipos, 4);
                    if (hdr.ToLower() == "<div")
                    {
                        entity.WrapDataText("<html>", "</html>");
                    }
                }
                else
                    entity.WrapDataText("<html>", "</html>");
            }
        }

    }
}
