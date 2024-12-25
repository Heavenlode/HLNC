using Godot;
using System;
using System.Collections.Generic;

namespace HLNC.Addons.Questing {
    public abstract partial class Quest : GodotObject
    {
        public string Name;
        public Godot.Collections.Dictionary<int, QuestStep> Steps;
    }
}
