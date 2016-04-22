using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PimpMyGame
{
    public static class FileOperations
    {
        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        /// <value>
        /// The application data directory.
        /// </value>
        public static string AppDataDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "LS" + Environment.UserName.GetHashCode().ToString("X"));
            }
        }

        public static string RootFolder
        {
            get
            {
                return AppDataDirectory + @"\PimpMyGame\";
            }
        }

        public static string MediaFolder
        {
            get
            {
                return RootFolder + @"Media\";
            }
        }

        public static void CreateCustomFolders()
        {
            if (!Directory.Exists(RootFolder))
            {
                Directory.CreateDirectory(RootFolder);
            }
            if (!Directory.Exists(MediaFolder))
            {
                Directory.CreateDirectory(MediaFolder);
            }
        }
    }
}
