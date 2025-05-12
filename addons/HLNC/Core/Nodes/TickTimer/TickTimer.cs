using System;

namespace HLNC.Utilities
{
    public class TickTimer
    {
        private int startTick;
        private int endTick;

        public TickTimer(int startTick, int endTick)
        {
            this.startTick = startTick;
            this.endTick = endTick;
        }

        public static TickTimer CreateFromSeconds(int currentTick, float seconds)
        {
            return new TickTimer(currentTick, currentTick + (int)Math.Ceiling(seconds * NetRunner.TPS));
        }

        public bool IsDone(int currentTick)
        {
            return currentTick >= endTick;
        }
    }
}