using IndexByte;
using LocalRT.radtor.local.config.tor;
using LocalRT.radtor.local.core;
using LocalRT.radtor.utils;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace LocalRT.radtor.local.config
{
    class ManagerTOR
    {
        private int torPORT { get; set; }
        public String rtOnionUrl { get; set; }
        private bool _TORok { get; set; }
        public string torPath { get; set; }
        public string TorExeFile { get; set; }
        public string TorConfigFile { get; set; }
        private ConfigTOR configTor { get; set; }
        public string workspaceRadtorPath { get; set; }
        private int HiddenServicePort { get; set; }
        public bool TOROk
        {
            get
            {
                return _TORok;
            }

            set
            {
                _TORok = value;
            }
        }
        public string LocalTorUrl
        {
            get
            {
                return rtOnionUrl;
            }

            set
            {
                rtOnionUrl = value;
            }
        }
        
        public string rtRegeditPath = RadTorUtil.getLocalRegeditPathByMAC();

        private EncripTOR encripTOR = new EncripTOR();

        private readonly string ONION = "shell";

        public ManagerTOR(string workspace)
        {
            workspaceRadtorPath = workspace;
        }

        /// <summary>
        ///      Valida si TOR esta instalado, lo instala y lo activa.
        /// </summary>
        internal void active(int port, int radtorPort)
        {
            this.torPORT = port;
            HiddenServicePort = radtorPort;
            RadTorLog.log("Activando TOR. HiddenServicePort :\t" + HiddenServicePort);
            RadTorLog.log("Activando TOR. torPORT :\t" + torPORT);
            //resetTOR();
            RadTorLog.log("Tor esta corriendo? :\t" + TOROk);
            if (!isTORinstall())
            {
                RadTorLog.log("TOR NO SE ENCUENTRA INSTALADO...");
                installTOR();
            }
            configTOR();
            RadTorLog.log("LEVANTANDO SERVICIO TOR");
            TOROk = runTOR(port);
        }

        private void configTOR()
        {
            configTor = new ConfigTOR(TorConfigFile);
            configTor.AvoidDiskWrites = "1";
            configTor.DataDirectory = workspaceRadtorPath;
            configTor.GeoIPFile = configTor.DataDirectory + @"\geoip";
            //configTor.ControlPort = string.Format("{0}", torPORT + 1);
            configTor.HiddenServiceDir = workspaceRadtorPath;
            configTor.HiddenServicePort = string.Format("80 127.0.0.1:{0}", HiddenServicePort);
            configTor.SocksListenAddress = string.Format("127.0.0.1:{0}", torPORT);
            configTor.SocksPort = string.Format("{0}", torPORT);
            configTor.save();
            RadTorLog.log("Archivo configuracion TOR CREADO.");
        }

        /// <summary>
        ///    Running TOR and waiting for her.
        /// </summary>
        private bool runTOR(int torPort)
        {
            RadTorLog.log("BOTAR TOR ANTES DE CORRER!");
            resetTOR();

            string workPathRADTOR = workspaceRadtorPath + @"\";
            string hostname = workPathRADTOR + "hostname";

            RadTorLog.log("RUN TOR >> workPathRADTOR :\t" + workPathRADTOR);
            RadTorLog.log("RUN TOR >> hostname :\t" + hostname);
            RadTorLog.log("RUN TOR >> TorExeFile :\t" + TorExeFile);
            #region EliminarConfigTor
            if (File.Exists(hostname))
            {
                try
                {
                    RadTorLog.log("RUN TOR >> Archivo HOST existe, elimando TOR!");
                    if (File.Exists(workPathRADTOR + "hostname")) File.Delete(workPathRADTOR + "hostname");
                    if (File.Exists(workPathRADTOR + "lock")) File.Delete(workPathRADTOR + "lock");
                    if (File.Exists(workPathRADTOR + "cached-certs")) File.Delete(workPathRADTOR + "cached-certs");
                    if (File.Exists(workPathRADTOR + "cached-microdesc-consensus")) File.Delete(workPathRADTOR + "cached-microdesc-consensus");
                    if (File.Exists(workPathRADTOR + "cached-microdescs.new")) File.Delete(workPathRADTOR + "cached-microdescs.new");
                    if (File.Exists(workPathRADTOR + "private_key")) File.Delete(workPathRADTOR + "private_key");
                    if (File.Exists(workPathRADTOR + "state")) File.Delete(workPathRADTOR + "state");
                    RadTorLog.log("Elimando existencia de TOR anteriormente instalado en " + workPathRADTOR);
                }
                catch (Exception e)
                {
                    RadTorLog.log("No se pudo eliminar archivos de configuracion " + e.Message);
                }
            }
            #endregion
            try
            {
                RadTorLog.log("RUN TOR >> SUBIENDO TOR");
                Process exeProcess = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.Arguments = "-f " + TorConfigFile; //+ " HashedControlPassword " + ConfigTOR.hashedControlPassword;
                startInfo.FileName = TorExeFile;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                exeProcess = Process.Start(startInfo);
                int patience = 0;
                while (!File.Exists(workPathRADTOR + "hostname"))
                {
                    RadTorLog.log("RUN TOR >> RUN TOR RUUUUUUUUN!!!");
                    patience++;
                    if (patience > 3)
                    {
                        //exeProcess.BeginOutputReadLine();
                        //string output = exeProcess.StandardOutput.ReadToEnd();
                        RadTorLog.log("ALGO ANDA MAL, INTENTARE DE NUEVO");
                        RegistryKey rtSubKey = Registry.CurrentUser.OpenSubKey(rtRegeditPath, true);
                        RegistryKey rtRegPath = rtSubKey.CreateSubKey("open");
                        rtRegPath.SetValue(ONION, "");
                        active(torPort, HiddenServicePort);
                    }
                    else
                    {
                        Thread.Sleep(30000);
                    }
                    /* Si alguna vez necesito captura user y password aqui es el lugar */
                }
                RadTorLog.log("YA ESTA EN EJECUCION; AHORA OCULTO ARCHIVOS DE CONFIGURACION");
                #region ocultarArchivosTOR
                //try { 


                //    DirectoryInfo workspaceInfo = new DirectoryInfo(workPathRADTOR);
                //    foreach (FileInfo filesTor in workspaceInfo.GetFiles())
                //    {
                //        File.SetAttributes(filesTor.FullName, FileAttributes.Hidden);
                //        RadTorLog.log(filesTor.FullName + "..Oculto");
                //    }
                //    RadTorLog.log("ARCHIVOS TOR OCULTOS!");
                //}
                //catch (Exception e)
                //{
                //    RadTorLog.log("NO SE PUDIERON OCULTAR ARCHIVOS TOR.." + e.Message);
                //}
                #endregion
                rtOnionUrl = readHostName(hostname);
                RadTorLog.log("HOST TOR " + rtOnionUrl);
                return true;
            }
            catch (Exception e)
            {
                RadTorLog.log("ERROR AL CORRER TOR " + e.StackTrace);
                return false;
            }
        }

        private string readHostName(string hostname)
        {
            string hostcontent;
            var fileStream = new FileStream(hostname, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                hostcontent = streamReader.ReadToEnd();
            }            
            return hostcontent.TrimEnd().TrimStart();
        }

        /// <summary>
        ///      Check if tor are running
        /// </summary>
        private void resetTOR()
        {
            System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
            TorExeFile = getTorExeFile();
            if (TorExeFile == null)
            {
                RadTorLog.log("TOR NO HA SIDO INSTALADO EN ESTA MAQUINA!!");
                //TODO: CHECK IS VALID QUESTION
                //installTOR();
            }
            else
            {
                Debug.WriteLine(TorExeFile);
                foreach (System.Diagnostics.Process process in processList)
                {
                    try
                    {
                        if (TorExeFile.ToUpper().Equals(process.MainModule.FileName.ToUpper()))
                        {
                            RadTorLog.log("TOR ESTA CORRIENDO!! " + process.MainModule.FileName);
                            process.Kill();
                            RadTorLog.log("TOR A SIDO finishid!");
                        }
                    }
                    catch { }
                }
                RadTorLog.log("TODAS LAS INSTANCIAS DE TOR HAN SIDO ELIMINADAS");
            }
        }


        /// <summary>
        ///     unzip tor and install, config
        /// </summary>
        private void installTOR()
        {
            RadTorLog.log("INSTALANDO TOR...");
            //CREA AMBIENTE DE CONFIGURACION
            Random r = new Random();
            //string appdata = ((new DirectoryInfo(LocalConfiguration.translateSystemVar("%APPDATA%"))).Parent).FullName;
            string appdata = LocalConfiguration.translateSystemVar("%APPDATA%");
            string appdataTor = RadTorUtil.returnRandomSubFolder(appdata);
            string subFolder = appdataTor.Split('\\')[appdataTor.Split('\\').Length - 1];
            string torc = subFolder + ".dll";

            //PARA PRUEBAS
            appdataTor += @"\" + subFolder + ".cache";

            RadTorLog.log("Install TOR>> subFolder : " + subFolder);
            RadTorLog.log("Install TOR>> torc : " + torc);
            RadTorLog.log("Install TOR>> appdataTor : " + appdataTor);

            if (Directory.Exists(torPath))
            {
                appdataTor += @"\" + subFolder + "_" + RadTorUtil.randomAlpha(8);
            }

            if (!Directory.Exists(appdataTor))
            {
                DirectoryInfo di = Directory.CreateDirectory(appdataTor);
                try
                {
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
                catch
                {
                }
            }
            TorConfigFile = appdataTor + @"\" + torc;
            //TODO: ESCOJER NOMBRE DECENTE
            TorExeFile = appdataTor + @"\" + RadTorUtil.theBestProcess();
            if (!File.Exists(TorExeFile))
            {
                TORByteContent torContentBytes = new TORByteContent();
                torContentBytes.torzip = appdataTor + @"\" + RadTorUtil.randomAlpha(10) + ".zip";
                torContentBytes.create();
                torPath = appdataTor;
                try
                {
                    RadTorLog.log("DESCOMPRIMIENDO TOR!!");
                    RadTorLog.log("TOR ZIP :\t" + torContentBytes.torzip);
                    RadTorLog.log("TOR APP PATH :\t" + appdataTor);
                    RadTorUtil.DeCompressFile(torContentBytes.torzip, appdataTor);
                    //ZipFile.ExtractToDirectory(torContentBytes.torzip, appdataTor);
                    File.Move(appdataTor + @"\tor.exe", TorExeFile);
                }
                catch (Exception e)
                {
                    RadTorLog.log("ERROR AL DESCOMPRIMIR TOR!!!!" + e);

                }
                File.Move(torContentBytes.torzip, torContentBytes.torzip.Replace(".zip", ".tmp"));

            }
            RadTorLog.log("TOR DESCOMPRIMIDO!!");
            //REGISTRAR EN REGEDIT
            RegistryKey rtSubKey = Registry.CurrentUser.OpenSubKey(rtRegeditPath, true);
            RegistryKey rtRegPath = rtSubKey.CreateSubKey("open");
            rtRegPath.SetValue(ONION, encripTOR.encript(TorConfigFile));
            RadTorLog.log("SE GUARDA RUTA EN REGEDIT EN : " + rtRegeditPath);
            RadTorLog.log("SE GUARDA RUTA EN REGEDIT EL VALOR : " + TorConfigFile);
        }


        private String getTorExeFile()
        {
            //LEER REGEDIT
            try
            {
                RegistryKey regCurrentUser = Registry.CurrentUser;
                regCurrentUser = regCurrentUser.OpenSubKey(rtRegeditPath + @"\open");
                torPath = encripTOR.decrypt(regCurrentUser.GetValue(ONION).ToString());
                TorConfigFile = torPath;
                string torPathExe = Path.GetDirectoryName(torPath);
                RadTorLog.log("TOR EXE SACADO DESDE REGEDIT " + torPathExe);
                //TODO: tomar exe copiar a un nombre que exista en los procesos corriendo, eliminar original y luego almacenar este en Regedit.
                return Directory.GetFiles(torPathExe, "*.exe", SearchOption.AllDirectories)[0];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     find if tor install.
        /// </summary>
        private bool isTORinstall()
        {
            //LEER REGEDIT
            try
            {
                RegistryKey pRegKey = Registry.CurrentUser;
                pRegKey = pRegKey.OpenSubKey(rtRegeditPath + @"\open");
                torPath = encripTOR.decrypt(pRegKey.GetValue(ONION).ToString());
                TorConfigFile = torPath;
                RadTorLog.log("TOR PATH :\t" + torPath);
                return (File.Exists(torPath));
            }
            catch
            {
                return false;
            }
        }

    }
}
