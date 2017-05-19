using static BepMod.Util;
using System;
using System.Drawing;
using GTA;
using GTA.Math;

namespace BepMod
{
    // A delegate type for hooking up change notifications.
    public delegate void TriggerEnterEventHandler(object sender, int index, EventArgs e);
    public delegate void TriggerExitEventHandler(object sender, int index, EventArgs e);

    class Trigger : IDisposable {
        public bool debug = false;

        public bool renderDistance = false;

        public int index;

        public float distance;

        public event TriggerEnterEventHandler TriggerEnter;
        public event TriggerExitEventHandler TriggerExit;

        public Vector3 triggerPosition;
        public float triggerRadius;
        public String triggerName;
        public bool triggeredInside = false;

        public Trigger(
            Vector3 position,
            float radius = 10.0f,
            String name = ""
        ) {
            triggerPosition = position;
            triggerRadius = radius;
            triggerName = name;
        }

        public void Dispose() {
            Log("Trigger.Dispose()");
        }
        
        protected virtual void OnTriggerEnter(EventArgs e) {
            Log("Entered trigger: " + triggerName);
            if (debugLevel > 0)
            {
                UI.Notify("Entered trigger: " + triggerName);
            }

            if (TriggerEnter != null) {
                TriggerEnter(this, index, e);
            }
        }

        protected virtual void OnTriggerExit(EventArgs e) {
            Log("Exited trigger: " + triggerName);
            if (debugLevel > 0) {
                UI.Notify("Exited trigger: " + triggerName);
            }

            if (TriggerExit != null) {
                TriggerExit(this, index, e);
            }
        }

        public virtual void DoTick() {
            Vector3 playerPos = Game.Player.Character.Position;

            distance = triggerPosition.DistanceTo(playerPos);
            bool inside = distance < triggerRadius;

            if (renderDistance == true) {
                ShowMessage(triggerName + " distance: " + distance.ToString("0.00"), 3);
            }

            if (debug == true || debugLevel > 2) {
                RenderCircleOnGround(
                    triggerPosition, 
                    triggerRadius, 
                    inside ? Color.Green : Color.Red
                );
            }

            if (inside && !triggeredInside) {
                triggeredInside = true;
                OnTriggerEnter(EventArgs.Empty);
            } else if (!inside && triggeredInside) {
                triggeredInside = false;
                OnTriggerExit(EventArgs.Empty);
            }
        }
    }
}
