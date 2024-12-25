using Godot;
using Heavenlode.Responsibilities;
using HLNC;
using System;
using System.Collections.Generic;

namespace HLNC.Addons.Questing
{
    /// <summary>
    /// A QuestController is a node that controls the state of a quest for a player. This is to be attached to the player node.
    /// </summary>
    public partial class QuestController : NetworkNode3D
    {
        struct SubscriberObject
        {
            public Callable handler;
            public Callable cleanup;
        }

        /// <summary>
        /// Subscribers to the OnAttemptCompleteTask signal.
        /// The subscription key is a tuple of questId, stepId, taskId.
        /// </summary>
        private Dictionary<Tuple<int, byte, byte>, Dictionary<Node, SubscriberObject>> _attemptCompleteTaskSubscribers = [];

        /// <summary>
        /// Subscribers to the OnCompleteStep signal.
        /// The subscription key is a tuple of questId, stepId.
        /// </summary>
        private Dictionary<Tuple<int, byte>, Dictionary<Node, SubscriberObject>> _completeStepSubscribers = [];

        [Signal]
        private delegate void OnAttemptCompleteTaskEventHandler(int questId, int stepId, int taskId);

        [Signal]
        public delegate void OnCompleteStepEventHandler(int questId, int stepId);

        /// <summary>
        /// This array represents what steps have been completed for each respective quest.
        /// The array index indicates the quest ID. The value at each index is a bitfield of the completed steps.
        /// For example, if a quest has 3 steps, and steps 0 and 2 are completed, then the value at index 0 would be 0b101 (or 5 in decimal).
        /// </summary>
        [NetworkProperty(InterestMask = (long)Heavenlode.InterestLayers.Owner)]
        public long[] CompletedSteps { get; set; } = [];

        // The currently active quest step has active tasks.
        // Once when every task has been completed can the quest step proceed.
        [NetworkProperty(InterestMask = (long)Heavenlode.InterestLayers.Owner)]
        public byte[] CompletedTasks { get; set; } = [];

        public int GetQuestStep(int questId)
        {
            if (CompletedSteps.Length <= questId)
            {
                return 0;
            }
            var currentStep = 0;
            while ((CompletedSteps[questId] & (1 << currentStep)) != 0)
            {
                currentStep += 1;
            }
            return currentStep;
        }

        public void OnNetworkChangeCompletedSteps(Tick tick, byte[] oldValue, byte[] newValue)
        {
            // Find the differences between the old and new values.
            for (int i = 0; i < newValue.Length; i++)
            {
                if (oldValue.Length > i)
                {
                    if (oldValue[i] != newValue[i])
                    {
                        if (newValue[i] == 1)
                        {
                            GD.Print("Responsibility accepted!");
                        }
                        else
                        {
                            GD.Print("Responsibility step completed!");
                        }
                    }
                }
                else
                {
                    if (newValue[i] == 1)
                    {
                        GD.Print("Responsibility accepted!");
                    }
                }
            }
        }

        [NetworkFunction]
        public void AttemptCompleteTask(int questId, byte stepId, byte taskId)
        {
            if (NetworkRunner.Instance.IsServer)
            {
                EmitSignal("OnAttemptCompleteTask", questId, stepId, taskId);
            }
        }

        public void SubscribeAttemptCompleteTask(
            Node subscriber,
            int questId,
            byte stepId,
            byte taskId,
            Callable handler
        )
        {
            var key = Tuple.Create(questId, stepId, taskId);
            if (!_attemptCompleteTaskSubscribers.ContainsKey(key))
                _attemptCompleteTaskSubscribers[key] = new Dictionary<Node, SubscriberObject>();
            var callable = Callable.From((int incomingId, byte incomingStepId, byte incomingTaskId) =>
            {
                _taskControllerOnAttemptComplete(incomingId, incomingStepId, incomingTaskId, questId, stepId, taskId, handler);
            });
            Connect("OnAttemptCompleteTask", callable);
            var cleanup = Callable.From(() => UnsubscribeAttemptCompleteTask(subscriber, questId, stepId, taskId));
            subscriber.Connect("tree_exiting", cleanup);
            _attemptCompleteTaskSubscribers[key][subscriber] = new SubscriberObject { handler = callable, cleanup = cleanup };
        }
        public void UnsubscribeAttemptCompleteTask(
            Node subscriber,
            int questId,
            byte stepId,
            byte taskId
        )
        {
            var key = Tuple.Create(questId, stepId, taskId);
            if (!_attemptCompleteTaskSubscribers.ContainsKey(key))
                return;
            if (!_attemptCompleteTaskSubscribers[key].ContainsKey(subscriber))
                return;
            Disconnect("OnAttemptCompleteTask", _attemptCompleteTaskSubscribers[key][subscriber].handler);
            subscriber.Disconnect("tree_exiting", _attemptCompleteTaskSubscribers[key][subscriber].cleanup);
            _attemptCompleteTaskSubscribers[key].Remove(subscriber);
        }
        private void _taskControllerOnAttemptComplete(
            int incomingResponsibilityId,
            byte incomingStepId,
            byte incomingTaskId,
            int filterResponsibilityId,
            byte filterStepId,
            byte filterTaskId,
            Callable handler)
        {
            if (NetworkRunner.Instance.IsClient)
                return;

            if (incomingResponsibilityId != filterResponsibilityId || incomingStepId != filterStepId || incomingTaskId != filterTaskId)
                return;

            var success = handler.Call();
            if (!success.AsBool())
                return;

            ServerCompleteTask(filterResponsibilityId, filterStepId, filterTaskId);
        }

        /// <summary>
        /// This method is called (e.g. by the StepController) when it determines that the player has completed a step. May also be called directly by the Server elsewhere.
        /// </summary>
        /// <param name="questId">The quest that is being targeted</param>
        /// <param name="stepId">The step that is being completed. This sets the current step to stepId + 1</param>
        public void ServerCompleteTask(int questId, byte stepId, byte taskId)
        {
            if (NetworkRunner.Instance.IsClient)
            {
                return;
            }
            if (stepId + 1 > 63)
            {
                GD.PrintErr($"Step ID {stepId} is out of bounds for quest {questId}");
                return;
            }
            CompletedSteps[questId] = CompletedSteps[questId] | (1 << stepId);
        }
    }
}
