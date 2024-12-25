using System;
using Godot;

namespace HLNC.Addons.Questing {
    public partial class QuestTask : GodotObject {
        public string Description;
        public Func<QuestController, int, bool> EvaluateAction;
    }
}
