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

        public int index;

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
            if (TriggerEnter != null) {
                TriggerEnter(this, index, e);
            }
        }

        protected virtual void OnTriggerExit(EventArgs e) {
            if (TriggerExit != null) {
                TriggerExit(this, index, e);
            }
        }

        public virtual void DoTick() {
            Vector3 playerPos = Game.Player.Character.Position;

            bool inside = triggerPosition.DistanceTo(playerPos) < triggerRadius;

            if (debug == true || debugLevel > 1) {
                RenderCircleOnGround(
                    triggerPosition, 
                    triggerRadius, 
                    inside ? Color.Green : Color.Red, 
                    triggerName
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
