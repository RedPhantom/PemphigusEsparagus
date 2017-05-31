using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PemphigusEsparagus
{
    class Program
    {
        const bool allinonefile = true;

        static void Main(string[] args)
        {
            int pnum = 1; // page number
            string totaldata = "";
        Start:

            //writeLog("Hello and welcome to pemphigus esparagus v1.0.");
            WebClient wc = new WebClient();
            if (!allinonefile)
            {
                totaldata = "";
            }
            bool startonnext = false;
            string msg = ""; // temp message to work with
            Regex regex = new Regex(Regex.Escape("o"));
            Message tmpmsg = new PemphigusEsparagus.Message();


            // download page, conv
            Console.WriteLine("HTTP Request is active for page {0}.", pnum);

            wc.Encoding = System.Text.Encoding.UTF8;
            string page = wc.DownloadString(getPageURL(pnum));
            Console.WriteLine("HTTP Request is complete.");
            // end download

            page = page.Replace("DMsg(MI", "eatshitynet"); // treat unwanted DMsg.
            Console.WriteLine("Started processing page {0}", pnum);

            Console.WriteLine(Regex.Matches(page, "DMsg").Count + " threads found.");
            int bob = Regex.Matches(Regex.Escape(page), Regex.Escape("DMsg")).Count;
            // for each message in the page
            for (int i = 0; i < bob; i++)
            {
                if (startonnext)
                {
                    Console.WriteLine("Started thread (msg {0} in page).", i);
                }
                if (i == 0) totaldata += "<thread>";

                int startIndex = page.IndexOf("DMsg(");
                int endIndex = page.Substring(startIndex).IndexOf(");");
                int len = endIndex - startIndex;
                msg = page.Substring(startIndex, endIndex);
                string msg2 = page.Substring(startIndex, 220);
                tmpmsg = Message.getData(getMsgId(msg));

                // writeLog(tmpmsg.msgId + "   " + tmpmsg.dateTime + "     " + tmpmsg.title + "     " + tmpmsg.author + "    " + tmpmsg.body);
                totaldata += Message.renderData(tmpmsg, Message.renderType.XML);
                Console.WriteLine("Msg {0} .", tmpmsg.msgId);
                if (msg2.Contains("DEndT"))
                {
                    Console.WriteLine("Ended thread."); // last message in thread
                    //tmpmsg.lastmsg = true;
                    startonnext = true;
                    totaldata += "</thread>";
                }
                else
                {
                    startonnext = false;
                    tmpmsg.lastmsg = false;
                }

                page = page.Replace(msg, ""); // remove the used-up message.
            }
            if (!allinonefile)
            {
                System.IO.File.WriteAllText("ynetdata-pg." + pnum + ".xml", totaldata);
                Console.WriteLine("Page {0} has been saved to disk.", pnum);
            }
            pnum++;
            if (pnum <= 36) goto Start;
            if (allinonefile)
            {
                System.IO.File.WriteAllText("compdata-pg." + pnum + ".xml", totaldata);
            }
        }

        public static string getPageURL(int pagenum)
        {
            return ("http://www.ynet.co.il/home/0,7340,L-2230-18160-" + pagenum + ",00.html");
        }

        public static long getMsgId(string msg)
        {
            long msgId = long.Parse(msg.Substring(getStartPoint(msg, "DMsg("), 8)); // msgid is 8 chars long.
            return msgId;
        }

        public static int getStartPoint(string data, string sp)
        {
            return data.IndexOf(sp) + sp.Length;
        }


    }
}
