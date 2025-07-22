using System;
using System.Diagnostics;
using System.Threading;

namespace LocalRT.radtor.utils
{
    class RadTorLog
    {
        public static void log(String lines)
        {
            DateTime now = DateTime.Now;
            try
            {
                String now_filename = now.ToString("yyyyMMdd");
                System.IO.StreamWriter file = new System.IO.StreamWriter("Radtor_" + now_filename + ".log", true);
                String rigthnow = now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                file.WriteLine(rigthnow + " >> " + lines);
                Debug.WriteLine(lines);
                file.Close();
            }
            catch (Exception e)
            {
                Thread.Sleep(10000);
                log(lines);
            }

        }
    }
}
