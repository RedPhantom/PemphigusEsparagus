using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PemphigusEsparagus
{
    class GlobalVar
    {
        
        public void setThreadId(int threadId)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\Dev\Pemphigus\temp.txt");
            file.WriteLine(threadId.ToString());
            file.Close();
        }

        public int getThreadId()
        {
            string text = System.IO.File.ReadAllText(@"D:\Dev\Pemphigus\temp.txt");
            return (int.Parse(text));
        }
    }
}
