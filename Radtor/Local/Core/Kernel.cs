using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using LocalRT.radtor.utils;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using LocalRT.Radtor.Remote;
using LocalRT.Radtor.Local.Core;
using System.Threading;
using LocalRT.radtor.local.web;

namespace LocalRT.radtor.local.core
{
    class Kernel : AbstractExecuteCommand
    {
        private static string from;

        internal string spy(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();
            Hashtable spy = new Hashtable();
            string path = LocalConfiguration.translateSystemVar(radtorRq.args);

            if (path.Equals("/"))
            {
                spy.Add("Parent", "");

                List<Hashtable> drivers = new List<Hashtable>();
                List<Hashtable> folders = new List<Hashtable>();
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)
                {
                    Hashtable driversHash = new Hashtable();
                    driversHash.Add("Name", d.Name);
                    driversHash.Add("Type", "disk");
                    driversHash.Add("Path", d.Name.Replace("\\", "/"));
                    if (d.IsReady == true)
                    {
                        driversHash.Add("VolumeLabel", d.VolumeLabel);
                        driversHash.Add("DriveFormat", d.DriveFormat);
                        driversHash.Add("DriveType", d.DriveType);
                        driversHash.Add("TotalSize", RadTorUtil.ByteSize(d.TotalSize));
                        driversHash.Add("TotalFreeSpace", RadTorUtil.ByteSize(d.TotalFreeSpace));
                        driversHash.Add("AvailableFreeSpace", RadTorUtil.ByteSize(d.AvailableFreeSpace));
                        drivers.Add(driversHash);
                    }


                }
                Hashtable userprofile = new Hashtable();

                String userprofilePath = LocalConfiguration.translateSystemVar("%userprofile%");
                DirectoryInfo userprofileDirectory = new DirectoryInfo(userprofilePath);
                userprofile.Add("Username", LocalConfiguration.translateSystemVar("%USERNAME%"));
                userprofile.Add("Path", userprofilePath.Replace("\\", " /"));

                userprofile.Add("CreationTime", RadTorUtil.formatDateTime(userprofileDirectory.CreationTime));
                userprofile.Add("LastWriteTime", RadTorUtil.formatDateTime(userprofileDirectory.LastWriteTime));
                userprofile.Add("LastAccessTime", RadTorUtil.formatDateTime(userprofileDirectory.LastAccessTime));

                userprofile.Add("Type", "folders");
                userprofile.Add("SubfolderSize", userprofileDirectory.GetDirectories().Length);
                userprofile.Add("SubfilesSize", userprofileDirectory.GetFiles().Length);

                folders.Add(userprofile);
                spy.Add("Driver", drivers);
                spy.Add("Folders", folders);
            }
            else
            {

                path = path.Replace("/", "\\");
                List<Hashtable> foldersHash = new List<Hashtable>();
                List<Hashtable> filesHash = new List<Hashtable>();
                DirectoryInfo directory = new DirectoryInfo(path);


                if (directory.Parent != null)
                {
                    spy.Add("Parent", directory.Parent.FullName);
                    DirectoryInfo parentFolder = directory.Parent;
                    Hashtable folder = new Hashtable();
                    try
                    {
                        folder.Add("Name", "..");
                        folder.Add("Path", parentFolder.FullName.Replace("\\", " /"));
                        folder.Add("CreationTime", RadTorUtil.formatDateTime(parentFolder.CreationTime));
                        folder.Add("LastWriteTime", RadTorUtil.formatDateTime(parentFolder.LastWriteTime));
                        folder.Add("LastAccessTime", RadTorUtil.formatDateTime(parentFolder.LastAccessTime));
                        folder.Add("Type", "folders");
                        folder.Add("SubfolderSize", parentFolder.GetDirectories().Length);
                        folder.Add("SubfilesSize", parentFolder.GetFiles().Length);
                        folder.Add("Access", "OK");
                    }
                    catch (Exception e)
                    {
                        folder.Add("Access", "NOK:" + e.Message);
                    }
                    foldersHash.Add(folder);
                }

                DirectoryInfo[] subFolder = directory.GetDirectories();
                foreach (DirectoryInfo carpetaInfo in subFolder)
                {
                    Hashtable folder = new Hashtable();
                    try
                    {
                        folder.Add("Name", carpetaInfo.Name);
                        folder.Add("Path", carpetaInfo.FullName.Replace("\\", " /"));
                        folder.Add("CreationTime", RadTorUtil.formatDateTime(carpetaInfo.CreationTime));
                        folder.Add("LastWriteTime", RadTorUtil.formatDateTime(carpetaInfo.LastWriteTime));
                        folder.Add("LastAccessTime", RadTorUtil.formatDateTime(carpetaInfo.LastAccessTime));
                        folder.Add("Attributes", carpetaInfo.Attributes);
                        folder.Add("Type", "folders");
                        folder.Add("SubfolderSize", carpetaInfo.GetDirectories().Length);
                        folder.Add("SubfilesSize", carpetaInfo.GetFiles().Length);
                        folder.Add("Access", "OK");
                    }
                    catch (Exception e)
                    {
                        folder.Add("Access", "NOK:" + e.Message);
                    }
                    foldersHash.Add(folder);
                }

                FileInfo[] fileInfo = directory.GetFiles();

                foreach (FileInfo fileDetail in fileInfo)
                {
                    Hashtable fileHash = new Hashtable();
                    fileHash.Add("Name", fileDetail.Name);
                    fileHash.Add("Path", fileDetail.FullName.Replace("\\", "/"));
                    fileHash.Add("Size", RadTorUtil.ByteSize(fileDetail.Length));
                    fileHash.Add("Type", "file");
                    fileHash.Add("CreationTime", RadTorUtil.formatDateTime(fileDetail.CreationTime));
                    fileHash.Add("LastWriteTime", RadTorUtil.formatDateTime(fileDetail.LastWriteTime));
                    fileHash.Add("LastAccessTime", RadTorUtil.formatDateTime(fileDetail.LastAccessTime));
                    fileHash.Add("Attributes", fileDetail.Attributes);
                    filesHash.Add(fileHash);
                }

                spy.Add("Folders", foldersHash);
                spy.Add("Files", filesHash);
            }
            responseRT.jsonResponse = spy;
            return jsonCmdResponse();
        }

        internal string sayHi(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();
            Hashtable hi = new Hashtable();
            hi.Add("Iam", LocalConfiguration.getInfo());
            hi.Add("datetime", LocalConfiguration.today() + " " + LocalConfiguration.now());
            hi.Add("status", "SEND!");
            responseRT.jsonResponse = hi;
            responseRT.args = from;
            return jsonCmdResponse();
        }



        internal string delete(RTRequest radtorRq)
        {
            Hashtable deleTOR = new Hashtable();
            string path = radtorRq.args;
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            try
            {
                if (File.Exists(path))
                {
                    System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
                    FileInfo imagePath = new FileInfo(path);

                    foreach (System.Diagnostics.Process process in processList)
                    {
                        if (imagePath.Name.Equals(process.ProcessName))
                        {
                            process.Kill();
                        }
                    }
                    deleTOR.Add("Status", "deleted");
                    deleTOR.Add("File", radtorRq.args);
                    imagePath.Delete();
                }
                else
                {
                    ArrayList filesArray = new ArrayList();
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo directorio = new DirectoryInfo(path);
                        FileInfo[] fileToDeleteInfo = directorio.GetFiles();
                        foreach (FileInfo imageDetail in fileToDeleteInfo)
                        {
                            imageDetail.Delete();
                            filesArray.Add(imageDetail.FullName);
                        }
                        Directory.Delete(path);
                    }
                    deleTOR.Add("Status", "deleted");
                    deleTOR.Add("Files", filesArray);
                    deleTOR.Add("Directory", radtorRq.args);
                }
            }
            catch (Exception e)
            {
                deleTOR.Add("Status", "error");
                deleTOR.Add("Error", e.Message);
            }
            responseRT.jsonResponse = deleTOR;
            return jsonCmdResponse();
        }

        internal string process(RTRequest radtorRq)
        {
            string quickResponse;
            string comando = radtorRq.args.Split('?')[0];

            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            try
            {
                quickResponse = "NOK";
                System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();

                switch (comando.ToUpper().Trim())
                {
                    case "SHOW":
                        Hashtable processArray = new Hashtable();
                        quickResponse = "OK";
                        foreach (System.Diagnostics.Process process in processList)
                        {
                            String idTask = "";
                            Hashtable processHash = new Hashtable();
                            try
                            {
                                idTask = "P_" + process.Id;
                                processHash.Add("User", process.StartInfo.UserName);
                                processHash.Add("Name", process.ProcessName);
                                processHash.Add("Id", process.Id);
                                processHash.Add("Title", process.MainWindowTitle);
                                processHash.Add("Size", RadTorUtil.ByteSize(process.PagedMemorySize64));
                                try
                                {
                                    processHash.Add("FileName", process.MainModule.FileName);
                                }
                                catch
                                //(Win32Exception e)
                                {
                                    processHash.Add("FileName", "");
                                }
                                processHash.Add("Status", "ok");

                            }
                            catch (Exception e)
                            {
                                processHash.Add("Status", "Error:\t" + e.Message);
                            }
                            processArray.Add(idTask, processHash);

                        }
                        responseRT.jsonResponse = processArray;
                        break;
                    case "KILL":
                        string processName;
                        string par = radtorRq.args.Split('?')[1];
                        bool finishid = false;
                        Hashtable killProcess = new Hashtable();
                        foreach (System.Diagnostics.Process process in processList)
                        {
                            if (par.Equals(process.Id.ToString()))
                            {
                                processName = process.ProcessName;
                                process.Kill();
                                finishid = true;
                                killProcess.Add("Status", "ok");
                                killProcess.Add("ProcesoName", processName);
                            }
                            if (par.Equals(process.ProcessName))
                            {
                                processName = process.ProcessName;
                                process.Kill();
                                finishid = true;
                                killProcess.Add("Status", "ok");
                                killProcess.Add("ProcesoName", processName);
                            }
                        }
                        if (!finishid)
                        {
                            killProcess.Add("Status", "nok");
                            killProcess.Add("Error", "not found ");
                        }
                        responseRT.jsonResponse = killProcess;
                        break;
                    default:
                        quickResponse = " radtorRq.args " + radtorRq.args + " unknow";
                        Hashtable notfound = new Hashtable();
                        notfound.Add("Status", "nok");
                        notfound.Add("Error", quickResponse);
                        responseRT.jsonResponse = notfound;
                        break;
                }

            }
            catch (Exception err)
            {
                Hashtable error = new Hashtable();
                error.Add("Status", "nok");
                error.Add("Error", err.Message);
                responseRT.jsonResponse = error;
            }
            return jsonCmdResponse();
        }

        internal string copy(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            Hashtable copyInfo = new Hashtable();
            string sourceFileName = LocalConfiguration.translateSystemVar(radtorRq.args.Split(new char[] { '?' })[0].Trim());
            string destFileName = LocalConfiguration.translateSystemVar(radtorRq.args.Split(new char[] { '?' })[1].Trim());
            copyInfo.Add("SourceFileName", sourceFileName);
            copyInfo.Add("DestFileName", destFileName);
            try
            {
                System.IO.File.Copy(sourceFileName, destFileName);
                copyInfo.Add("Status", "ok");
            }
            catch (Exception exception)
            {
                copyInfo.Add("Status", "Error ** " + exception.Message + " ** Trace " + exception.StackTrace);
            }
            responseRT.jsonResponse = copyInfo;
            return jsonCmdResponse();
        }

        internal string download(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            Hashtable donwloadhash = new Hashtable();


            responseRT.jsonResponse = donwloadhash;
            return jsonCmdResponse();
        }

        internal string screenshot(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            Hashtable screenshotHash = new Hashtable();
            string lastScreenShot = "";
            FileInfo imageDetail;
            try
            {
                string imagePath = LocalConfiguration.translateSystemVar(radtorRq.args);
                Rectangle bounds = Screen.GetBounds(Point.Empty);
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    String imagePathFinal = imagePath.ToLower().Replace(".gif", "").Replace(".jpg", "").Replace(".png", "");
                    long sizeJpg, sizeGif, sizePng;
                    File.Delete(imagePathFinal + ".jpg");
                    bitmap.Save(imagePathFinal + ".jpg", ImageFormat.Png);
                    imageDetail = new FileInfo(imagePathFinal + ".jpg");
                    sizeJpg = imageDetail.Length;
                    File.Delete(imagePathFinal + ".png");
                    bitmap.Save(imagePathFinal + ".png", ImageFormat.Jpeg);
                    imageDetail = new FileInfo(imagePathFinal + ".png");
                    sizePng = imageDetail.Length;
                    File.Delete(imagePathFinal + ".gif");
                    bitmap.Save(imagePathFinal + ".gif", ImageFormat.Gif);
                    imageDetail = new FileInfo(imagePathFinal + ".gif");
                    sizeGif = imageDetail.Length;

                    if (sizeGif < sizeJpg && sizeGif < sizePng)
                    {
                        imageDetail = new FileInfo(imagePathFinal + ".gif");
                        imagePathFinal += ".gif";
                        File.Delete(imagePath + ".png");
                        File.Delete(imagePath + ".jpg");
                    }
                    if (sizeJpg < sizeGif && sizeJpg < sizePng)
                    {
                        imageDetail = new FileInfo(imagePathFinal + ".jpg");
                        imagePathFinal += ".jpg";
                        File.Delete(imagePath + ".png");
                        File.Delete(imagePath + ".gif");
                    }
                    if (sizePng < sizeGif && sizePng < sizeJpg)
                    {
                        imageDetail = new FileInfo(imagePathFinal + ".png");
                        imagePathFinal += ".png";
                        File.Delete(imagePath + ".gif");
                        File.Delete(imagePath + ".jpg");
                    }
                    lastScreenShot = imagePathFinal;
                }
                Image imagen = System.Drawing.Image.FromFile(lastScreenShot);
                String base64imagen = RadTorUtil.ImageToBase64String(imagen);
                String idImg = RadTorUtil.randomAlpha(10);
                Hashtable conceptosUI = new Hashtable();
                conceptosUI.Add("Status", "ok");
                conceptosUI.Add("Imagen64", base64imagen);
                conceptosUI.Add("Size", RadTorUtil.ByteSize(imageDetail.Length));
                conceptosUI.Add("ImagenLocal", lastScreenShot);
                responseRT.jsonResponse = conceptosUI;
                responseRT.args = radtorRq.args;
                responseRT.cmd = radtorRq.cmd;


                return jsonCmdResponse();
            }
            catch (Exception err)
            {
                Hashtable conceptosUI = new Hashtable();
                conceptosUI.Add("Status", "error : " + err.Message);
                responseRT.jsonResponse = conceptosUI;
            }
            responseRT.jsonResponse = screenshotHash;
            return jsonCmdResponse();
        }

        internal string readFile(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            Hashtable readFile = new Hashtable();


            responseRT.jsonResponse = readFile;
            return jsonCmdResponse();
        }

        internal string execute(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            Hashtable executeHash = new Hashtable();

            String appExe = radtorRq.args.Split('*')[0];
            appExe = LocalConfiguration.translateSystemVar(appExe);
            String @params = "";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            if (radtorRq.args.Split('*').Length >= 2)
            {
                @params = radtorRq.args.Split('*')[1];
                startInfo.Arguments = LocalConfiguration.translateSystemVar(@params);
            }

            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = appExe;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            executeHash.Add("App", appExe);
            executeHash.Add("Params", @params);

            try
            {
                Process exeProcess = Process.Start(startInfo);
                executeHash.Add("Status", "ok");
            }
            catch (Exception e)
            {
                executeHash.Add("Status", e.StackTrace);
            }

            responseRT.jsonResponse = executeHash;
            return jsonCmdResponse();
        }

        internal string show(RTRequest radtorRq)
        {
            Hashtable showHash = new Hashtable();
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();


            responseRT.jsonResponse = showHash;
            return jsonCmdResponse();
        }

        internal string showImg(RTRequest radtorRq)
        {
            responseRT.args = radtorRq.args;
            responseRT.cmd = radtorRq.cmd;
            responseRT.mac = LocalConfiguration.getMac();

            Hashtable showImg = new Hashtable();


            responseRT.jsonResponse = showImg;
            return jsonCmdResponse();
        }
    }
}

