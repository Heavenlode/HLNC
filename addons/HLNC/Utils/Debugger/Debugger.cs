using Godot;

namespace HLNC.Utils {
    internal static class Debugger {
        public enum DebugLevel {
            ERROR,
            WARN,
            INFO,
            VERBOSE,
        }
        public static void Log(string msg, DebugLevel level = DebugLevel.INFO) {
            if (level > (DebugLevel)ProjectSettings.GetSetting("HLNC/config/log_level", 0).AsInt16()) {
                return;
            }
            var messageString = $"({level}) HLNC.{(Env.Instance.HasServerFeatures ? "Server" : "Client")}: {msg}";
            if (level == DebugLevel.ERROR) {
                GD.PrintErr(messageString);
            } else {
                GD.Print(messageString);
            }
        }
    }
}