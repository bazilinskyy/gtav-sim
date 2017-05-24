using System;
using System.Drawing;

using GTA;
using GTA.Math;
using GTA.Native;

using static BepMod.Util;

namespace BepMod.Experiment
{
    // A delegate type for hooking up change notifications.
    public delegate void ActorInsideRadiusEventHandler(object sender, EventArgs e);
    public delegate void ActorOutsideRadiusEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Actors are pedestrians, optionally with a vehicle.</summary>
    /// <remarks>
    /// Available events: InsideRadius, OutsideRadius, 
    /// </remarks>
    class Actor : IDisposable
    {
        public float distance;

        public String _name;

        public float MinSpeed = 0.0f;

        public event ActorInsideRadiusEventHandler ActorInsideRadius;
        public event ActorOutsideRadiusEventHandler ActorOutsideRadius;

        public Vehicle vehicle;
        public Ped ped;

        public int vehicleHandle = -1;
        public int pedHandle = -1;

        public float _radius;
        public bool triggeredInside = false;

        private Vector3 _destination = new Vector3();
        private float _destinationRadius = 5.0f;
        private Action _destinationReached;

        private bool _removed = false;
        public bool IsRemoved { get => _removed; }

        public Actor(
            Vector3 position,
            float heading,
            float radius = 0.0f,
            PedHash pedHash = default(PedHash),
            VehicleHash vehicleHash = default(VehicleHash),
            String name = "",
            Action destinationReached = null
        )
        {
            _radius = radius;
            _name = name;
            _destinationReached = destinationReached;

            if (vehicleHash != default(VehicleHash))
            {
                vehicle = World.CreateVehicle(vehicleHash, position, heading);

                vehicle.PlaceOnGround();
                vehicle.IsInvincible = true;
                vehicle.CanBeVisiblyDamaged = false;

                vehicleHandle = vehicle.Handle;

                vehiclePool.Add(vehicle.Handle);
            }

            if (pedHash != default(PedHash))
            {
                ped = World.CreatePed(pedHash, position, heading);

                ped.CanBeDraggedOutOfVehicle = false;
                ped.IsInvincible = true;

                if (vehicle != null)
                {
                    ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                }

                pedHandle = ped.Handle;

                pedPool.Add(ped.Handle);
            }
        }

        public void Remove()
        {
            if (ped != null && ped.Exists())
            {
                try { ped.Delete(); }
                catch { }
            }

            if (vehicle != null && vehicle.Exists())
            {
                try { vehicle.Delete(); }
                catch { }
            }

            ped = null;
            vehicle = null;

            _removed = true;
        }

        public void Dispose()
        {
            Remove();
        }

        override public string ToString()
        {
            return String.Format("ACTOR_{0}", _name);
        }

        public Vector3 Position {
            get { return (vehicle != null) ? (vehicle.Position) : (ped.Position); }
        }

        public Vector3 Velocity {
            get { return (vehicle != null) ? (vehicle.Velocity) : (ped.Velocity); }
        }

        protected virtual void OnActorInsideRadius(EventArgs e)
        {
            Log("Actor inside radius: " + _name);
            if (debugLevel > 0)
            {
                ShowMessage("Actor inside radius: " + _name);
            }

            ActorInsideRadius?.Invoke(this, e);
        }

        protected virtual void OnActorOutsideRadius(EventArgs e)
        {
            Log("Actor outside radius: " + _name);
            if (debugLevel > 0)
            {
                ShowMessage("Actor outside radius: " + _name);
            }

            ActorOutsideRadius?.Invoke(this, e);
        }

        public virtual void DoTick()
        {
            Vector3 playerPos = Game.Player.Character.Position;
            Vector3 actorPos = Position;

            distance = Position.DistanceTo2D(playerPos);
            bool inRange = distance < _radius;

            if (vehicle != null && vehicle.Speed < MinSpeed)
            {
                vehicle.Speed = MinSpeed;
            }

            if (debugLevel > 1 && distance < 75.0f)
            {
                Vector2 sc;
                Gta5EyeTracking.Geometry.WorldToScreenRel_Native(Position, out sc);

                if (sc.X != 0 || sc.Y != 0)
                {
                    UIText label = new UIText(
                        String.Join("\n",
                            _name,
                            Position,
                            vehicle?.Speed
                        ),
                        new Point(
                            (int)(((sc.X + 1) / 2) * UI.WIDTH),
                            (int)(((sc.Y + 1) / 2) * UI.HEIGHT)
                        ),
                        0.5f,
                        font: GTA.Font.ChaletLondon,
                        color: Color.White,
                        shadow: false,
                        outline: true,
                        centered: true
                    );
                    label.Draw();
                }

            }

            if (debugLevel > 2)
            {
                RenderCircleOnGround(
                    Position,
                    _radius,
                    inRange ? Color.Green : Color.Red
                );
            }

            if (inRange && !triggeredInside)
            {
                triggeredInside = true;
                OnActorInsideRadius(EventArgs.Empty);
            }
            else if (!inRange && triggeredInside)
            {
                triggeredInside = false;
                OnActorOutsideRadius(EventArgs.Empty);
            }

            if (Position.DistanceTo2D(_destination) < _destinationRadius)
            {
                _destinationReached?.Invoke();
                _destination = new Vector3();
            }
        }

        public void DriveTo(
            Vector3 target, float speed = 5.0f,
            float radius = 1.0f, float destinationRadius = 10.0f,
            DrivingStyle drivingstyle = DrivingStyle.Normal,
            Action destinationReached = null
        )
        {
            _destination = target;
            _destinationRadius = destinationRadius;
            _destinationReached = destinationReached;

            ped.Task.DriveTo(
                vehicle, 
                target, 
                speed, 
                drivingstyle.GetHashCode()
            );
        }
    }
}
