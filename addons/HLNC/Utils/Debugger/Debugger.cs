using Godot;

namespace HLNC.Utils
{
    public class TickLog
    {
        public string Message;
        public Debugger.DebugLevel Level;
    }

    [Tool]
    public partial class Debugger : Node
    {
        public static Debugger Instance { get; private set; }
        public static Debugger EditorInstance => Engine.GetSingleton("Debugger") as Debugger;

        public override void _EnterTree()
        {
            if (Engine.IsEditorHint())
            {
                Engine.RegisterSingleton("Debugger", this);
                return;
            }
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public enum DebugLevel
        {
            ERROR,
            WARN,
            INFO,
            VERBOSE,
        }

        public void Log(string msg, DebugLevel level = DebugLevel.INFO)
        {
            if (level > (DebugLevel)ProjectSettings.GetSetting("HLNC/config/log_level", 0).AsInt16())
            {
                return;
            }
            var platform = Env.Instance == null ? "Editor" : (Env.Instance.HasServerFeatures ? "Server" : "Client");
            var messageString = $"({level}) HLNC.{platform}: {msg}";
            if (level == DebugLevel.ERROR)
            {
                GD.PrintErr(messageString);
                // Also print stack trace
                GD.Print(new System.Exception().StackTrace);
            }
            else
            {
                GD.Print(messageString);
            }
        }
    }
}