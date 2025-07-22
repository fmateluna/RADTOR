using LocalRT.radtor.local.core;
using LocalRT.radtor.local.web;
using System.Collections;
using System.Web.Script.Serialization;

namespace LocalRT.Radtor.Local.Core
{
    class AbstractExecuteCommand
    {
        private JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        protected RTResponse responseRT = new RTResponse();
        protected Hashtable responseHash = new Hashtable();

        protected string jsonCmdResponse()
        {
            
            return javaScriptSerializer.Serialize(responseRT);
            //string json = javaScriptSerializer.Serialize(responseRT);
            //EncripTOR encrip = new EncripTOR();
            //return encrip.encript(json);
        }
    }
}

