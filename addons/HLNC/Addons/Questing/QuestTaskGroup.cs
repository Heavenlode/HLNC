using Godot;
using System;
using System.Collections.Generic;

namespace HLNC.Addons.Questing {

    public partial class QuestTaskGroup : GodotObject {
        public bool Hidden;
        public Godot.Collections.Array<QuestTask> Tasks;
        
        public HashSet<int> Actions;
        public int NextStepId;
    }
}
