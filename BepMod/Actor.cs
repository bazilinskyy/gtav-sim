using System;
using System.Drawing;

using GTA;
using GTA.Math;
using GTA.Native;

using static BepMod.Util;

namespace BepMod
{
    // A delegate type for hooking up change notifications.
    public delegate void ActorInsideRadiusEventHandler(object sender, EventArgs e);
    public delegate void ActorOutsideRadiusEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Actors are pedestrians, optionally with a vehicle.</summary>
    /// <remarks>
    /// Available events: InsideRadius, OutsideRadius, 
    /// </remarks>
    class Actor : IDisposable {
        public float distance;

        public String Name;

        public float MinSpeed = 0.0f;

        public event ActorInsideRadiusEventHandler ActorInsideRadius;
        public event ActorOutsideRadiusEventHandler ActorOutsideRadius;

        public Vehicle vehicle;
        public Ped ped;

        public int vehicleHandle = -1;
        public int pedHandle = -1;

        public float triggerRadius;
        public bool triggeredInside = false;
        
        public Actor(
            Vector3 position,
            float heading,
            float radius = 0.0f,
            PedHash pedHash = default(PedHash),
            VehicleHash vehicleHash = default(VehicleHash),
            String name = ""
        ) {
            triggerRadius = radius;
            Name = name;

            if (vehicleHash != default(VehicleHash)) {
                vehicle = World.CreateVehicle(vehicleHash, position, heading);

                vehicle.PlaceOnGround();
                vehicle.IsInvincible = true;
                vehicle.CanBeVisiblyDamaged = false;
                vehicle.SetDoorBreakable(VehicleDoor.FrontLeftDoor, false);
                vehicle.SetDoorBreakable(VehicleDoor.FrontRightDoor, false);

                vehicleHandle = vehicle.Handle;

                vehiclePool.Add(vehicle.Handle);
            }

            if (pedHash != default(PedHash)) {
                ped = World.CreatePed(pedHash, position, heading);

                ped.CanBeDraggedOutOfVehicle = false;
                ped.IsInvincible = true;

                if (vehicle != null) {
                    ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                }

                pedHandle = ped.Handle;

                pedPool.Add(ped.Handle);
            }
        }

        public void Dispose() {
            Log("Actor.Dispose()");
            if (ped != null && ped.Exists()) {
                try { ped.Delete(); }
                catch { }
            }

            if (vehicle != null && vehicle.Exists()) {
                try { vehicle.Delete(); }
                catch { }
            }

            ped = null;
            vehicle = null;
        }

        override public string ToString()
        {
            return String.Format("ACTOR_{0}", Name);
        }

        public Vector3 Position {
            get { return (vehicle != null) ? (vehicle.Position) : (ped.Position); }
        }

        protected virtual void OnActorInsideRadius(EventArgs e) {
            Log("Actor inside radius: " + Name);
            if (debugLevel > 0) {
                ShowMessage("Actor inside radius: " + Name);
            }

            if (ActorInsideRadius != null) {
                ActorInsideRadius(this, e);
            }
        }

        protected virtual void OnActorOutsideRadius(EventArgs e) {
            Log("Actor outside radius: " + Name);
            if (debugLevel > 0)
            {
                ShowMessage("Actor outside radius: " + Name);
            }

            if (ActorOutsideRadius != null) {
                ActorOutsideRadius(this, e);
            }
        }

        public virtual void DoTick() {
            Vector3 playerPos = Game.Player.Character.Position;
            Vector3 actorPos = Position;

            distance = Position.DistanceTo(playerPos);
            bool inRange = distance < triggerRadius;

            if (vehicle != null && vehicle.Speed < MinSpeed) {
                vehicle.Speed = MinSpeed;
            }
            
            if (debugLevel > 2) {
                RenderCircleOnGround(
                    Position, 
                    triggerRadius, 
                    inRange ? Color.Green : Color.Red
                );
            }
            
            if (inRange && !triggeredInside) {
                triggeredInside = true;
                OnActorInsideRadius(EventArgs.Empty);
            } else if (!inRange && triggeredInside) {
                triggeredInside = false;
                OnActorOutsideRadius(EventArgs.Empty);
            }
        }
    }
}
