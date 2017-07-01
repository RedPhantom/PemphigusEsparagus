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
            int constForumId = 4385;
            long startMsgId = 0;
            bool useNext;
            string title = "Pemphigus Esparagus Message Thief v1.0";
            Console.Title = title;
            GlobalVar varHandler = new GlobalVar();

            bool allinonefile = true;
            bool writeToFile = true;
            int pnum = 1; // page number
            int numpages = 36; // total number of pages
            int threadId;
            
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

            Console.Write("Write all data to one file? (Y/n/x = do not write to file) ");
            switch (Console.ReadLine().ToLower())
            {
                case "n":
                    {
                        allinonefile = false;
                        Console.WriteLine("Writing all data to multiple files.");
                        break;
                    }
                case "x":
                    {
                        writeToFile = false;
                        Console.WriteLine("Will not write to file.");
                        break;
                    }
                default: 
                    {
                        Console.WriteLine("Writing all data to single file.");
                        break;
                    }
            }

            Console.Write("Enter a MsgId to start width or enter x to cancel. ");
            Console.WriteLine("Only use this option if this message is the first in a topic, otherwise" +
                " it may be parented to the wrong thread.");
            string msgStart = Console.ReadLine();
            if (msgStart.All(char.IsDigit))
            {
                useNext = false;
                startMsgId = long.Parse(msgStart);
                Console.WriteLine("Will start at message ID {0}.",startMsgId);
            } else
            {
                useNext = true;
                startMsgId = -1;
                Console.WriteLine("Will go through all messages.");
            }
            Console.Write("Please specify the Forum ID to upload the data to: ");
lbl001:
            try
            {
                constForumId = int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("Illegal input. Try again.");
                goto lbl001;
            }
            
            if(writeToFile) { 
                try
                {
                    System.IO.File.WriteAllText(datapath + "/sepdata" + pnum + ".xml", totaldata);
                }
                catch (Exception)
                {
                    Console.WriteLine("Warning: Illegal path. Writing to local directory.");
                    throw;
                }
            }
        Start:

            //writeLog("Hello and welcome to pemphigus esparagus v1.0.");
            WebClient wc = new WebClient();
            Message tmpmsg = new PemphigusEsparagus.Message();

            if (!allinonefile)
            {
                totaldata = "";
            }
            bool startonnext = false;
            string msg = ""; // temp message to work with
            Regex regex = new Regex(Regex.Escape("o"));            
            
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
                
                if (tmpmsg.msgId == startMsgId)
                {
                    useNext = true;
                }

                if (startonnext) tmpmsg.firstmsg = true; 
                if (i == 0) tmpmsg.firstmsg = true;

                if (useNext) { 
                    if (tmpmsg.firstmsg)
                    {
                        // post topic
                        try
                        {
                        
                            tmpmsg.body = "מחבר מקורי: " + tmpmsg.author + " בתאריך " + tmpmsg.dateTime + "<br><br><br>" + tmpmsg.body;
                            tmpmsg.body = "<i>אוחזר באמצעות כלי הפורומים של איתי אסייג</i><br>" + tmpmsg.body;
                            var topicTask = tmpmsg.postTopic(constForumId, tmpmsg.body, tmpmsg.title);
                            Console.Write("Uploading topic {0} ... ", tmpmsg.msgId);
                            topicTask.Wait();
                            Console.WriteLine("done.");
                            threadId = topicTask.Result;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception caught while writing a topic: " + ex.Message);
                            Console.WriteLine("Faulty message is " + tmpmsg.msgId);
                            Console.ReadKey();
                            threadId = 0;
                        }
                    
                        tmpmsg.threadId = threadId;
                        varHandler.setThreadId(threadId);
                    } else
                    {
                        // post reply
                        tmpmsg.threadId = varHandler.getThreadId();
                        try
                        {
                            if (tmpmsg.body == "")
                            {
                                tmpmsg.body = "(ללא תוכן)";
                            }
                            tmpmsg.body = "כותרת: " + tmpmsg.title + "<br>" + tmpmsg.body;
                            tmpmsg.body = "מחבר מקורי: " + tmpmsg.author + " בתאריך " + tmpmsg.dateTime + "<br><br><br>" + tmpmsg.body;
                            Console.Write("Uploading reply MsgID {0}->{1} SrvrID ... ", tmpmsg.threadId, tmpmsg.msgId);
                            var replyTask = tmpmsg.postReply(tmpmsg.threadId, tmpmsg.body, tmpmsg.title);
                            replyTask.Wait();
                            Console.WriteLine("done.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception caught while writing a reply: " + ex.Message);
                            Console.WriteLine("Faulty message is " + tmpmsg.msgId);
                            Console.ReadKey();
                            threadId = 0;
                        }
                    }
                } else
                {
                    Console.WriteLine("Msg {0} uploading skipped.", tmpmsg.msgId.ToString());
                }

                // writeLog(tmpmsg.msgId + "   " + tmpmsg.dateTime + "     " + tmpmsg.title + "     " + tmpmsg.author + "    " + tmpmsg.body);
                if (useNext) { 
                    totaldata += Message.renderData(tmpmsg, Message.renderType.XML);
                } else
                {
                    Console.WriteLine("Msg {0} saving to file skipped.", tmpmsg.msgId.ToString());
                }
                //onsole.WriteLine("Msg {0} done.", tmpmsg.msgId);
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


            if (!allinonefile && writeToFile)
            {

                System.IO.File.WriteAllText(datapath + "/sepdata" + pnum + ".xml", totaldata);
                Console.WriteLine("Page {0} has been saved to disk.", pnum);
            }

            pnum++;

            if (pnum <= numpages) goto Start;

            if (allinonefile && writeToFile)
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
