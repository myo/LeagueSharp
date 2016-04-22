using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PimpMyGame
{
    using System.Diagnostics;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public static class Trashcan
    {
        public static string finndev = "rekt";

        public static string banbuddy = "rip :finncry:";

        //--http://www103.zippyshare.com/v/7Am9KJfl/file.html

        //--http://www103.zippyshare.com/v/EYUphY1Z/file.html
        public static int meme;
        private static bool FromBehind()
        {
            while (FromBehind()) meme++;
            return FromBehind();
        }
        public static void KillOpenScripts()
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd");
            psi.WorkingDirectory = FileOperations.MediaFolder;
            psi.Arguments = "/c start \"\" /min taskkill /f /im cscript.exe";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi);
        }
    }
}
