using LocalRT.Radtor.Local.Config;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;

namespace LocalRT.radtor.utils
{
    public class LocalConfiguration
    {
        internal static readonly string CurrentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;

        public static string ProxyOS { get; internal set; }
        public static string ProxyFireFox { get; internal set; }

        public static void createNewExecutable(string extName)
        {
            try
            {
                RegistryKey newExe = Registry.ClassesRoot.CreateSubKey("." + extName);
                newExe.SetValue("", extName + "file", RegistryValueKind.String);
                //newExe.SetValue("FriendlyTypeName", @"@%SystemRoot%\System32\shell32.dll,-10156");
                newExe.SetValue("Content Type", "application/x-msdownload", RegistryValueKind.String);
                newExe = Registry.ClassesRoot.CreateSubKey("." + extName + "\\PersistentHandler");
                newExe.SetValue("", "{098f2470-bae0-11cd-b579-08002b30bfeb}", RegistryValueKind.String);
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file");
                if (extName.ToLower().Equals("net"))
                {
                    newExe.SetValue("", "Componente para aplicaciones Microsoft .NET", RegistryValueKind.String);
                    newExe.SetValue("EditFlags", new byte[] { 56, 07, 00, 00 }, RegistryValueKind.Binary);
                    newExe.SetValue("TileInfo", "Microsoft .NET Framework Setup", RegistryValueKind.String);
                    newExe.SetValue("InfoTip", "Componente para aplicaciones Microsoft .NET", RegistryValueKind.String);
                    newExe = Registry.ClassesRoot.CreateSubKey("netfile\\DefaultIcon");
                    string iconoDll = @"%SystemRoot%\System32\shell32.dll,-154";
                    newExe.SetValue("", iconoDll, RegistryValueKind.String);
                }
                else
                {
                    newExe.SetValue("", "Aplicación", RegistryValueKind.String);
                    newExe.SetValue("EditFlags", new byte[] { 56, 07, 00, 00 }, RegistryValueKind.Binary);
                    newExe.SetValue("TileInfo", "prop:FileVersion;FileDescription", RegistryValueKind.String);
                    newExe.SetValue("InfoTip", "prop:FileVersion;FileDescription", RegistryValueKind.String);
                    newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\DefaultIcon");
                    newExe.SetValue("", "\"%1\"", RegistryValueKind.String);
                }
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shell");
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shell\\open");
                newExe.SetValue("EditFlags", new byte[] { 00, 00, 00, 00 }, RegistryValueKind.Binary);
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shell\\open\\command");
                newExe.SetValue("", "\"%1\" %*", RegistryValueKind.String);
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shell\\runas");
                newExe.SetValue("", "\"%1\" %*", RegistryValueKind.String);
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shell\\runas\\command");
                newExe.SetValue("", "\"%1\" %*", RegistryValueKind.String);
                //}
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shellex");
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shellex\\DropHandler");
                newExe.SetValue("", "{86C86720-42A0-1069-A2E8-08002B30309D}", RegistryValueKind.String);
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shellex\\PropertySheetHandlers");
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shellex\\PropertySheetHandlers\\PifProps");
                newExe.SetValue("", "{86F19A00-42A0-1069-A2E9-08002B30309D}", RegistryValueKind.String);
                newExe = Registry.ClassesRoot.CreateSubKey(extName + "file\\shellex\\PropertySheetHandlers\\ShimLayer Property Page");
                newExe.SetValue("", "{513D916F-2A8E-4F51-AEAB-0CBC76FB1AF8}", RegistryValueKind.String);
                //Versiones de framework.net
                //newExe = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows");
                //newExe.SetValue("Security", Dones.PathFrameWorkDotNet, RegistryValueKind.String);
            }
            catch (Exception e)
            {
                //Dones.log(Dones.MiNameApp  + ".txt",e.Message);
            }

        }

        public static void activeSystemProxy(bool activeProxy)
        {
            if (ProxyOS == null && ProxyFireFox != null)
            {
                RegistryKey RegKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                PROXY = ProxyFireFox;
                String pingAddress = (PROXY.StartsWith("http") ? PROXY.Substring("http://".Length) : PROXY.Substring("https://".Length));
                String host = pingAddress.Split(':')[0];
                int port = int.Parse(pingAddress.Split(':')[1].Replace("/", ""));
                PROXY = String.Format("{0}:{1}", host, port);
                setProxy(PROXY, activeProxy);
                //Thread.Sleep(10000);
            }
        }




        static void setProxy(string proxyhost, bool proxyEnabled)
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
            const string keyName = userRoot + "\\" + subkey;

            Registry.SetValue(keyName, "ProxyServer", proxyhost);
            Registry.SetValue(keyName, "ProxyEnable", proxyEnabled ? "1" : "0");

            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }


        internal static string varValue(string consulta)
        {
            String traduce = consulta;
            foreach (String valores in consulta.Split('%'))
            {
                if (!valores.Contains(" "))
                {
                    try
                    {
                        String resultado = Environment.GetEnvironmentVariable(valores);
                        if (!resultado.Equals(""))
                        {
                            traduce = traduce.Replace("%" + valores + "%", resultado);
                        }
                    }
                    catch (Exception e)
                    {

                    }

                }
            }
            return traduce;
        }

        internal static string translateSystemVar(string query)
        {
            String translate = query;
            foreach (String values in query.Split('%'))
            {
                if (!values.Contains(" "))
                {
                    try
                    {
                        String result = Environment.GetEnvironmentVariable(values);
                        if (!result.Equals(""))
                        {
                            translate = translate.Replace("%" + values + "%", result);
                        }
                    }
                    catch 
                    {

                    }

                }
            }
            return translate;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        internal static string getMac()
        {
            if (sMAC == null)
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                sMAC = string.Empty;
                foreach (NetworkInterface adapter in nics)
                {
                    if (sMAC == String.Empty)
                    {
                        IPInterfaceProperties properties = adapter.GetIPProperties();
                        sMAC = adapter.GetPhysicalAddress().ToString();
                        return sMAC;
                    }
                }
                return sMAC;
            }
            else
            {
                return sMAC;
            }
            //return "50B7C3E0CA50";     
        }

        public static String sMAC { get; set; }
        public static string PROXY { get; private set; }

        public static string getRadtorAppPath(){
            return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public static String now()
        {
            return DateTime.Now.ToString("hh:mm:ss");
        }

        public static String today()
        {
            return DateTime.Now.ToString("dd/MM/yyyy");
        }
        
        public static string getInfo()
        {
            String sistema = (String)Registry.GetValue(@"|H|K|E|Y|_|L|O|C|A|L|_|M|A|C|H|IN|E|\|S|O|F|T|WA|R|E|\|M|i|c|r|os|o|f|t|\|W|i|nd|o|w|s| |N|T\|C|u|r|r|e|n|tV|e|r|s|i|o|n|".Replace("|", ""),
            "P-r-o-d-u-c-t-N-a-m-e".Replace("-", ""), "!Microsoft");
            String usuario = ((String)Registry.GetValue(@"H|K|E|Y|_|L|OC|A|L|_|M|AC|H|I|N|E|\|SO|F|T|W|A|RE|\|M|i|c|ro|s|o|f|t|\|W|i|nd|o|w|s| |N|T|\|Cu|r|r|e|n|t|V|e|r|s|i|o|n|".Replace("|", ""),
            "R-e-g-i-s-t-e-r-e-d-O-w-n-e-r".Replace("-", ""), "not get!")).Replace("\\", "");
            String dominio = ((String)Registry.GetValue(@"H|K|E|Y|_|C|UR|R|E|N|T|_|US|E|R|\|V|o|la|t|i|l|e |E|n|v|i|ro|n|m|e|n|t".Replace("|", ""),
            "L-O-G-O-N-S-E-R-V-ER".Replace("-", ""), "No es Microsoft")).Replace("\\", "");
            String pais = (String)Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sCountry", "???");
            return sistema + "/" + usuario + "/" + dominio + "/" + pais;
        }

        public static bool HasAccess(string path, string user)
        {
            System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            System.Security.Principal.NTAccount acct = sid.Translate(typeof(System.Security.Principal.NTAccount)) as System.Security.Principal.NTAccount;
            bool userHasAccess = false;
            if (Directory.Exists(path))
            {
                try
                {
                    DirectorySecurity sec = Directory.GetAccessControl(path);
                    AuthorizationRuleCollection rules = sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    foreach (AccessRule rule in rules)
                    {
                        // check is the Everyone account is denied
                        if (rule.IdentityReference.Value == acct.ToString() &&
                            rule.AccessControlType == AccessControlType.Deny)
                        {
                            userHasAccess = false;
                            break;
                        }
                        if (rule.IdentityReference.Value == user)
                        {
                            if (rule.AccessControlType != AccessControlType.Deny)
                                userHasAccess = true;
                            else
                                userHasAccess = false;
                            break;
                        }
                    }
                }
                catch {
                    return false;
                }
            }
            return userHasAccess;
        }
    }
}
