﻿using System;
using System.Net;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PemphigusEsparagus
{
    public class Message
    {
        public long msgId { get; set; }
        public int threadId { get; set; }
        public string title { get; set; }
        public string dateTime { get; set; }
        public string author { get; set; }
        public string body { get; set; }
        public bool lastmsg { get; set; }
        public bool firstmsg { get; set; }

        public enum renderType { XML, CSV }

        private static readonly HttpClient client = new HttpClient();
        /*
         * 
         * 
         * Structure is:
         * msgId    msgTitle    dd/mm/yyyy hh:mm    author  body
         */
        /// <summary>
        /// Renders data in a csv-similar format, separating data by {||}. 
        /// </summary>
        /// <returns></returns>
        public static string renderData(Message msg, renderType render = renderType.CSV)
        {
            if (render == renderType.CSV)
            {
                string separator = "{|}";
                string data = msg.msgId + separator + msg.title + separator + msg.dateTime.ToString() + separator + msg.author + separator + msg.body;
                return data;
            }
            else if (render == renderType.XML)
            {
                string _xml = "";
                msg.body.Replace("\r\n", "<br>");
                msg.body.Replace("\n", "<br>");
                msg.body.Replace("\r", "<br>");
                msg.body.Replace("<", "");
                msg.body.Replace(">", "");

                _xml += "<msg id=\"" + msg.msgId + "\">";
                _xml += "<title>" + msg.title + "</title>";
                _xml += "<datetime>" + msg.dateTime + "</datetime>";
                _xml += "<author>" + msg.author + "</author>";
                _xml += "<body>" + msg.body + "</body></msg>";
                //if (msg.lastmsg) _xml += "</thread><thread>\n";
                return _xml;
            }
            return null;
        }

        public static Message getData(long msgId)
        {
            Message msg = new Message();
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;

            string rawMsg = wc.DownloadString("http://www.ynet.co.il/NRFrame/Ext/App/TDG/TDGDisplayMessage/0,9734,2230-18160-" + msgId + "-2229-1,00.html");
            //string _title = rawMsg.Substring(rawMsg.IndexOf("<font class=text18>"), rawMsg.IndexOf("</font><BR>") - rawMsg.IndexOf("<font class=text18>"));
            int _titlesp = getStartPoint(rawMsg, "<font class=text18>");
            int _titleep = getEndPoint(rawMsg, _titlesp, "</font><BR>");
            string _title = rawMsg.Substring(_titlesp, _titleep).Replace("&nbsp;", "").Replace("נושא :", "");
            rawMsg = cleanup("</font><BR>", rawMsg);
            rawMsg = cleanup("<font class=text18>", rawMsg);


            int _datesp = getStartPoint(rawMsg, "<font class=text12>");
            int _dateep = getEndPoint(rawMsg, _datesp, "</font><BR>");
            string _date = rawMsg.Substring(_datesp, _dateep).Replace("&nbsp;", "").Replace("תאריך :", "");
            //_date.Replace("<font class=text12>", "").Replace("</font>", "");
            rawMsg = cleanup("<font class=text12>", rawMsg);
            rawMsg = cleanup("</font><BR>", rawMsg);


            int _authorsp = getStartPoint(rawMsg, "<font class=text12>");
            int _authorep = getEndPoint(rawMsg, _authorsp, "</font><BR>");
            string _author = rawMsg.Substring(_authorsp, _authorep).Replace("&nbsp;", "").Replace("מחבר/ת :", "");
            rawMsg = cleanup("<font class=text12>", rawMsg);
            rawMsg = cleanup("</font><BR>", rawMsg);

            rawMsg = cleanup("</script>", rawMsg);
            int _bodysp = getStartPoint(rawMsg, "}\n	</script>");
            int _bodyep = getEndPoint(rawMsg, _bodysp, "<BR></div>");

            if (_bodyep < 0 || _bodysp < 0)
            {
                msg.body = "";
            }
            else
            {
                msg.body = rawMsg.Substring(_bodysp, _bodyep).Replace("&nbsp;", "").Replace("<BR>", "\n");
            }



            msg.title = _title;
            msg.dateTime = _date.Substring(0, 8) + " " + _date.Substring(8, 5);
            msg.author = _author;
            msg.msgId = msgId;
            // msg.body = _body;
            return msg;
        }

        public static string sendToServer(Message msg)
        {
            string author = System.Uri.EscapeDataString(msg.author);
            string datetime = System.Uri.EscapeDataString(msg.dateTime);
            string body = System.Uri.EscapeDataString(msg.body);
            string title = System.Uri.EscapeDataString(msg.title);
            string isfirst = msg.firstmsg.ToString().ToLower();
            string url = "http://10.0.0.7/pemphigus/putmessage.php?title=" + title + "&datetime=" + datetime + "&author=" + author + "&body=" + body + "&first=" + isfirst;
            return url;
        }
        public static int getStartPoint(string data, string sp)
        {
            return data.IndexOf(sp) + sp.Length;
        }

        public static int getEndPoint(string data, int sp, string ep)
        {
            return data.IndexOf(ep) - sp;
        }
        public static string cleanup(string str, string data)
        {
            return data.Remove(data.IndexOf(str), str.Length);

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

       /* public Message upload(Message msg, int threadId = 0)
        {
            int id = 0;

            if (msg.firstmsg)
            {
                // create new topic
                id = msg.postTopic(4, msg.body, msg.title).Result;
                msg.threadId = id;
                return (msg);
            } else
            {
                // create new reply

                msg.postReply(threadId, msg.body, msg.title);
                return (msg);
            }

        } */

        public async Task<int> postTopic(int forumId, string topicBody, string topicTitle)
        {
            var values = new Dictionary<string, string>
                {
                   { "action", "topic" },
                   { "body", topicBody },
                   { "title", topicTitle },
                   { "id", forumId.ToString() }
                };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("http://10.0.0.7/pemphigus/uploadData.php", content);

            var responseString = await response.Content.ReadAsStringAsync();
            return (int.Parse(responseString));
        }

        public async Task postReply(int topicId, string replyBody, string replyTitle)
        {
            var values = new Dictionary<string, string>
                {
                   { "action", "reply" },
                   { "body", replyBody },
                   { "title", replyTitle },
                   { "id", topicId.ToString() }
                };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("http://10.0.0.7/pemphigus/uploadData.php", content);
        }
    }

}