using System.IO;
using UnityEngine;

namespace Ariadne
{
    public enum ShowHitbox
    {
        None,
        Show,
        VerboseLogs,
        Verbose,
    }

    public class GlobalSettings
    {
        public static string DefaultLogFolder = Path.GetDirectoryName(
            System.Reflection.Assembly.GetCallingAssembly().Location).ToString();

        public ShowHitbox ShowHitBoxes = ShowHitbox.VerboseLogs;

        public bool TrackHitboxes = true;

        public bool DebugAB = true;

        public bool LoggingActive = false;

        public string LogFolder = DefaultLogFolder;

        public int LoggingIntervalMS = 1000;
    }

    public class SaveSettings
    {
        public string saveId;
    }
}
