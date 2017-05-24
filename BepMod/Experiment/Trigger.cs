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
        public Entity entity;

        public int index;

        public float distance;

        public event TriggerEnterEventHandler TriggerEnter;
        public event TriggerExitEventHandler TriggerExit;

        private Vector3 _position;
        private float _radius;
        private String _name;

        private Action<Trigger> _enter;
        private Action<Trigger> _exit;

        public bool triggeredInside = false;

        public string NameFormat = "TRIGGER_{0}";

        public Trigger(
            Vector3 position,
            float radius = 10.0f,
            String name = "",
            Entity entity = null,
            Action<Trigger> enter = null,
            Action<Trigger> exit = null
        )
        {
            _position = position;
            _radius = radius;
            _name = name;

            _enter = enter;
            _exit = exit;

            if (entity == null)
            {
                entity = Game.Player.Character;
            }
            this.entity = entity;
        }

        public void Dispose()
        {
        }

        override public string ToString()
        {
            return String.Format(NameFormat, _name);
        }

        protected virtual void OnTriggerEnter(EventArgs e)
        {
            Log("Entered trigger: " + _name);
            if (debugLevel > 1)
            {
                ShowMessage("Entered trigger: " + ToString());
            }

            TriggerEnter?.Invoke(this, index, e);
        }

        protected virtual void OnTriggerExit(EventArgs e)
        {
            Log("Exited trigger: " + _name);
            if (debugLevel > 1)
            {
                ShowMessage("Exited trigger: " + ToString());
            }

            TriggerExit?.Invoke(this, index, e);
        }

        public virtual void DoTick()
        {
            distance = entity.Position.DistanceTo2D(_position);
            bool inside = distance < _radius;

            if (debugLevel > 2)
            {
                RenderCircleOnGround(
                    _position,
                    _radius,
                    inside ? Color.Green : Color.Red
                );
            }

            if (inside && !triggeredInside)
            {
                triggeredInside = true;
                OnTriggerEnter(EventArgs.Empty);
                _enter?.Invoke(this);
            }
            else if (!inside && triggeredInside)
            {
                triggeredInside = false;
                OnTriggerExit(EventArgs.Empty);
                _exit?.Invoke(this);
            }
        }
    }
}
