using System;
using System.Drawing;

using GTA;
using GTA.Math;

using static BepMod.Util;

namespace BepMod.Experiment
{
    // A delegate type for hooking up change notifications.
    public delegate void TriggerEnterEventHandler(object sender, int index, EventArgs e);
    public delegate void TriggerExitEventHandler(object sender, int index, EventArgs e);

    class Trigger : IDisposable
    {
        public int index;

        public float distance;

        public event TriggerEnterEventHandler TriggerEnter;
        public event TriggerExitEventHandler TriggerExit;

        public Vector3 triggerPosition;
        public float triggerRadius;
        public String triggerName;
        public bool triggeredInside = false;

        public string NameFormat = "TRIGGER_{0}";

        public Trigger(
            Vector3 position,
            float radius = 10.0f,
            String name = ""
        )
        {
            triggerPosition = position;
            triggerRadius = radius;
            triggerName = name;
        }

        public void Dispose()
        {
            Log("Trigger.Dispose()");
        }

        override public string ToString()
        {
            return String.Format(NameFormat, triggerName);
        }

        protected virtual void OnTriggerEnter(EventArgs e)
        {
            Log("Entered trigger: " + triggerName);
            if (debugLevel > 0)
            {
                ShowMessage("Entered trigger: " + triggerName);
            }

            TriggerEnter?.Invoke(this, index, e);
        }

        protected virtual void OnTriggerExit(EventArgs e)
        {
            Log("Exited trigger: " + triggerName);
            if (debugLevel > 0)
            {
                ShowMessage("Exited trigger: " + triggerName);
            }

            TriggerExit?.Invoke(this, index, e);
        }

        public virtual void DoTick()
        {
            Vector3 playerPos = Game.Player.Character.Position;

            distance = triggerPosition.DistanceTo(playerPos);
            bool inside = distance < triggerRadius;

            if (debugLevel > 2)
            {
                RenderCircleOnGround(
                    triggerPosition,
                    triggerRadius,
                    inside ? Color.Green : Color.Red
                );
            }

            if (inside && !triggeredInside)
            {
                triggeredInside = true;
                OnTriggerEnter(EventArgs.Empty);
            }
            else if (!inside && triggeredInside)
            {
                triggeredInside = false;
                OnTriggerExit(EventArgs.Empty);
            }
        }
    }
}
