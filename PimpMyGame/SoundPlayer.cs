namespace PimpMyGame
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;

    using LeagueSharp;
    using LeagueSharp.SDK.Utils;

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public class SoundPlayer
    {
        private string _path;

        private string _fileName;

        public SoundPlayer(byte[] resource, string saveName)
        {
            _fileName = saveName;
            _path = FileOperations.MediaFolder + this._fileName;
            this.CopyToDisk(resource, this._path);
            DelayAction.Add(
                600,
                () =>
                    {
                        this.CreateLaunchScript();

                        this.LaunchScript();
                    });
        }

        private void CopyToDisk(byte[] resource, string file)
        {
            if (File.Exists(file)) return;
            File.WriteAllBytes(file, resource);
        }

        private void CreateLaunchScript()
        {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(FileOperations.MediaFolder + "playsound.vbs")))
            {
                sw.WriteLine("Set Sound = CreateObject(\"WMPlayer.OCX.7\")");
                sw.WriteLine("Sound.URL = \"" + this._path + "\"");
                sw.WriteLine("Sound.Controls.play");
                sw.WriteLine("do while Sound.currentmedia.duration = 0");
                sw.WriteLine("wscript.sleep 100");
                sw.WriteLine("loop");
                sw.WriteLine("wscript.sleep int(Sound.currentmedia.duration)*1000");
            }
        }
        private void LaunchScript()
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd");
            psi.WorkingDirectory = FileOperations.MediaFolder;
            psi.Arguments = "/c start \"\" /min cscript.exe //B //Nologo playsound.vbs";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi);
        }
    }
}