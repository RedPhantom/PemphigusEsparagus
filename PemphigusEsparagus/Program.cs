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


        static void Main(string[] args)
        {
            string title = "Pemphigus Esparagus Message Thief v1.0";
            Console.Title = title;

            bool allinonefile = true;
            int pnum = 1; // page number
            int numpages = 36; // total number of pages
            string totaldata = "";
            string datapath = ".";
            try
            {
                datapath = args[0];
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Path not specified. Writing to local directory.");
                datapath = ".";
            }

            Console.Write("Write all data to one file? (Y/n) ");
            switch (Console.ReadLine().ToLower())
            {
                case "n":
                    {
                        allinonefile = false;
                        Console.WriteLine("Writing all data to multiple files.");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Writing all data to single file.");
                        break;
                    }
            }
            
            try
            {
                System.IO.File.WriteAllText(datapath + "/sepdata" + pnum + ".xml", totaldata);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Illegal path. Writing to local directory.");
                throw;
            }
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
            float bob = Regex.Matches(Regex.Escape(page), Regex.Escape("DMsg")).Count;
            // for each message in the page
            for (float i = 0; i < bob; i++)
            {
            // cases in which the message is the first in the thread:
                if (startonnext)
                {
                    Console.WriteLine("Started thread (msg {0} in page).", i);
                    
                }
                if (i == 0)
                {
                    totaldata += "<thread>";
                    
                }
            // -------
                int startIndex = page.IndexOf("DMsg(");
                int endIndex = page.Substring(startIndex).IndexOf(");");
                int len = endIndex - startIndex;
                msg = page.Substring(startIndex, endIndex);
                string msg2 = page.Substring(startIndex, 220);
                tmpmsg = Message.getData(getMsgId(msg));
                

                if (startonnext) tmpmsg.firstmsg = true; 
                if (i == 0) tmpmsg.firstmsg = true;

                Console.WriteLine("Message conversion URL is " + Message.sendToServer(tmpmsg));

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
                Console.Title = title + " / progress: " + (i * 100 / bob).ToString() + "% page " + pnum + " out of " + numpages;

                page = page.Replace(msg, ""); // remove the used-up message.
            }
            if (!allinonefile)
            {

                System.IO.File.WriteAllText(datapath + "/sepdata" + pnum + ".xml", totaldata);
                Console.WriteLine("Page {0} has been saved to disk.", pnum);
            }
            pnum++;
            if (pnum <= numpages) goto Start;
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
