using static BepMod.Util;
using System;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BepMod
{
    // A delegate type for hooking up change notifications.
    public delegate void ParticipantInsideRadiusEventHandler(object sender, EventArgs e);
    public delegate void ParticipantOutsideRadiusEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Participants are pedestrians, optionally with a vehicle.</summary>
    /// <remarks>
    /// Available events: InsideRadius, OutsideRadius, 
    /// </remarks>
    class Participant : IDisposable {
        public bool debug = false;

        public float distance;
        public bool renderDistance = false;

        public String participantName;

        public float MinSpeed = 0.0f;

        public event ParticipantInsideRadiusEventHandler ParticipantInsideRadius;
        public event ParticipantOutsideRadiusEventHandler ParticipantOutsideRadius;

        public Vehicle vehicle;

        public Ped ped;

        public float triggerRadius;
        public bool triggeredInside = false;
        
        public Participant(
            Location location,
            float radius = 0.0f,
            PedHash pedHash = default(PedHash),
            VehicleHash vehicleHash = default(VehicleHash),
            String name = ""
        ) {
            triggerRadius = radius;
            participantName = name;

            if (vehicleHash != default(VehicleHash)) {
                vehicle = World.CreateVehicle(vehicleHash, location.position, location.heading);

                vehicle.PlaceOnGround();
                vehicle.IsInvincible = true;
                vehicle.CanBeVisiblyDamaged = false;
                vehicle.SetDoorBreakable(VehicleDoor.FrontLeftDoor, false);
                vehicle.SetDoorBreakable(VehicleDoor.FrontRightDoor, false);

                vehiclePool.Add(vehicle.Handle);
            }

            if (pedHash != default(PedHash)) {
                ped = World.CreatePed(pedHash, location.position, location.heading);

                ped.CanBeDraggedOutOfVehicle = false;
                ped.IsInvincible = true;

                if (vehicle != null) {
                    ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                }

                pedPool.Add(ped.Handle);
            }
        }

        public void Dispose() {
            Log("Participant.Dispose()");
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

        public Vector3 Position {
            get { return (vehicle != null) ? (vehicle.Position) : (ped.Position); }
        }

        protected virtual void OnParticipantInsideRadius(EventArgs e) {
            Log("Participant inside radius: " + participantName);
            if (debugLevel > 0) {
                UI.Notify("Participant inside radius: " + participantName);
            }

            if (ParticipantInsideRadius != null) {
                ParticipantInsideRadius(this, e);
            }
        }

        protected virtual void OnParticipantOutsideRadius(EventArgs e) {
            Log("Participant outside radius: " + participantName);
            if (debugLevel > 0)
            {
                UI.Notify("Participant outside radius: " + participantName);
            }

            if (ParticipantOutsideRadius != null) {
                ParticipantOutsideRadius(this, e);
            }
        }

        public virtual void DoTick() {
            Vector3 playerPos = Game.Player.Character.Position;
            Vector3 participantPos = Position;

            distance = Position.DistanceTo(playerPos);
            bool inRange = distance < triggerRadius;

            if (renderDistance == true) {
                ShowMessage(participantName + " distance: " + distance.ToString("0.00"), 4);
            }

            if (vehicle != null && vehicle.Speed < MinSpeed) {
                vehicle.Speed = MinSpeed;
            }
            
            if (debug == true || debugLevel > 1) {
                RenderCircleOnGround(
                    Position, 
                    triggerRadius, 
                    inRange ? Color.Green : Color.Red, 
                    participantName
                );
            }
            
            if (inRange && !triggeredInside) {
                triggeredInside = true;
                OnParticipantInsideRadius(EventArgs.Empty);
            } else if (!inRange && triggeredInside) {
                triggeredInside = false;
                OnParticipantOutsideRadius(EventArgs.Empty);
            }
        }
    }
}