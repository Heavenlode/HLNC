using Godot;
using System;
using System.Collections.Generic;

namespace HLNC.Addons.Questing {

    public partial class QuestStep : GodotObject {
        public int Id;
        public string Name;
        public string Description;
        public Godot.Collections.Array<QuestTaskGroup> TaskGroups;
    }
}
