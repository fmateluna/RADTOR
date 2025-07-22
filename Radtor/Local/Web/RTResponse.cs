using System;
using System.Collections;

namespace LocalRT.radtor.local.web
{
    public class RTResponse
    {
        public string mac { get; set; }
        public string args { get; set; }

        public string cmd{ get; set; }

        private string date = DateTime.Now.ToString("dd/MM/yyyy h:mm tt");

        public string Date
        {
            get { return date; }
            set { date = value; }
        }
        public Hashtable jsonResponse { get; set; }
    }
}