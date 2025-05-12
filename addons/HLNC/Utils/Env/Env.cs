using System;
using System.Collections.Generic;
using Godot;

namespace HLNC.Utils
{
    [Tool]
    public partial class Env : Node
    {
        public static Env Instance { get; private set; }
        private bool initialized = false;
        private Godot.Collections.Dictionary<string, string> env = new Godot.Collections.Dictionary<string, string>();

        public Godot.Collections.Dictionary<string, string> StartArgs = [];

        public enum ProjectSettingId
        {
            WORLD_DEFAULT_SCENE
        }

        public static Dictionary<ProjectSettingId, string> ProjectSettingKeys = new Dictionary<ProjectSettingId, string> {
            { ProjectSettingId.WORLD_DEFAULT_SCENE, "HLNC/world/default_scene" }
        };

        public override void _Ready()
        {
            foreach (var argument in OS.GetCmdlineArgs())
            {
                if (argument.Contains('='))
                {
                    var keyValuePair = argument.Split("=");
                    StartArgs[keyValuePair[0].TrimStart('-')] = keyValuePair[1];
                }
                else
                {
                    // Options without an argument will be present in the dictionary,
                    // with the value set to an empty string.
                    StartArgs[argument.TrimStart('-')] = "";
                }
            }

            InitialWorldScene = StartArgs.GetValueOrDefault("initialWorldScene", ProjectSettings.GetSetting(ProjectSettingKeys[ProjectSettingId.WORLD_DEFAULT_SCENE]).AsString());

            if (StartArgs.ContainsKey("worldId"))
            {
                InitialWorldId = new UUID(StartArgs["worldId"]);
            }
            else
            {
                InitialWorldId = UUID.Empty;
            }
        }

        public override void _EnterTree()
        {
            if (Instance != null)
            {
                QueueFree();
            }
            Instance = this;
        }

        public string GetValue(string valuename)
        {
            if (OS.HasEnvironment(valuename))
            {
                return OS.GetEnvironment(valuename);
            }

            Godot.Collections.Dictionary<string, string> env;

            if (HasServerFeatures)
            {
                env = Parse("res://.env.server.txt");
            }
            else
            {
                env = Parse("res://.env.client.txt");
            }

            if (env.ContainsKey(valuename))
            {
                return env[valuename];
            }

            return "";
        }

        public string InitialWorldScene { get; private set; }

        public UUID InitialWorldId { get; private set; }

        /// <inheritdoc/>

        public bool HasServerFeatures
        {
            get
            {
                if (OS.HasFeature("dedicated_server")) return true;
                return false;
            }
        }

        private Godot.Collections.Dictionary<string, string> Parse(string filename)
        {
            if (initialized) return env;

            if (!FileAccess.FileExists(filename))
            {
                return new Godot.Collections.Dictionary<string, string>();
            }

            var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
            while (!file.EofReached())
            {
                string line = file.GetLine();
                var o = line.Split("=");

                if (o.Length == 2)
                {
                    env[o[0]] = o[1].Trim('"');
                }
            }

            initialized = true;
            return env;
        }
    }
}