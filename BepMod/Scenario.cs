using static BepMod.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

using GTA;
using GTA.Math;

using GTA.Native;

namespace BepMod {
    class Scenario {
        public Vector3[] points;
        public int waypoint;

        public List<Blip> blips = new List<Blip> { };

        public Vector3 startPosition;
        public float startHeading;

        public VehicleHash vehicleHash = VehicleHash.Cruiser;
        public Weather weather = Weather.ExtraSunny;

        bool running = false;

        public Vehicle vehicle;
        public List<Participant> participants = new List<Participant>();
        public List<Trigger> triggers = new List<Trigger>();

        public void Main() {
            Log("Scenario.Main()");
        }

        public Scenario() { }

        public void SetStart(Vector3 position, float heading) {
            startPosition = position;
            startHeading = heading;
        }

        private void ClearParticipants() {
            foreach(Participant participant in participants) {
                participant.Dispose();
            }

            participants.Clear();
        }

        public virtual void PreRun() { }
        public virtual void PostRun() { }

        public virtual void Run() {
            Log("Scenario.Run()");

            Stop();

            Game.Player.Character.Position = startPosition;
            Game.Player.Character.Heading = startHeading;

            waypoint = 0;
            running = true;

            foreach(Blip blip in World.GetActiveBlips()) {
                blip.Remove();
            }

            ClearParticipants();
            pedPool.Clear();
            vehiclePool.Clear();
            DoRemovePeds();
            DoRemoveVehicles();

            AddBlipsForPoints();

            for (int i = 0; i < points.Length; i++) {
                Vector3 point = points[i];
                Trigger pointTrigger = AddTrigger(point, name: "Waypoint " + i.ToString());
                
                pointTrigger.TriggerEnter += (sender, index, e) => {
                    if (debugLevel > 0) {
                        UI.Notify("Entered waypoint trigger " + index.ToString());
                    }
                    if (index < points.Length) {
                        SetWaypoint(Math.Min(points.Length - 1, index + 2));
                    } else {
                        ClearWaypoint();
                    }
                };

                pointTrigger.TriggerExit += (sender, index, e) => {
                    if (debugLevel > 0) {
                        UI.Notify("Exited waypoint trigger " + index.ToString());
                    }
                };
            }

            PreRun();

            vehicle = World.CreateVehicle(vehicleHash, startPosition, startHeading);

            vehiclePool.Add(vehicle.Handle);

            vehicle.PlaceOnGround();

            Game.Player.Character.Position = vehicle.Position;
            Game.Player.Character.Heading = vehicle.Heading;

            Game.Player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            Game.Player.Character.CanBeDraggedOutOfVehicle = false;
            Game.Player.Character.CanBeKnockedOffBike = false;
            Game.Player.Character.CanBeTargetted = false;
            Game.Player.Character.CanRagdoll = false;
            Game.Player.Character.DrownsInWater = false;

            GameplayCamera.RelativeHeading = 0.0f;

            World.Weather = weather;

            PostRun();

            UI.Notify("Scenario loaded");
        }

        public void SetWaypoint(int index) {
            Log("Set waypoint " + index.ToString());
            Vector3 waypoint = points[index];
            Function.Call(Hash.SET_NEW_WAYPOINT, waypoint.X, waypoint.Y);
        }

        public void ClearWaypoint() { }

        public void AddBlibForPoint(Vector3 point, int index) {
            Blip blip = World.CreateBlip(point);
            blip.Name = "Point " + index.ToString();
            blips.Add(blip);
        }

        public void DoTick() {
            if (running) {
                Game.Player.Character.Health = 100;

                DoRemoveVehicles();
                DoRemovePeds();

                foreach(Participant participant in participants) {
                    participant.DoTick();
                }

                foreach(Trigger trigger in triggers) {
                    trigger.DoTick();
                }
            }
        }

        public void Stop() {
            Log("Scenario.Stop()");

            running = false;

            ClearParticipants();
            ClearVehiclePool();
            ClearPedPool();
            RemoveOldVehicles();
        }

        public void RemoveOldVehicles() {
            try {
                int vehicleHashCode = vehicleHash.GetHashCode();

                foreach(Vehicle vehicle in World.GetAllVehicles()) {
                    if (vehicle.GetHashCode() == vehicleHashCode && vehicle.Exists()) {
                        try { vehicle.Delete(); } catch { }
                    }
                }
            } catch { }
        }

        public void AddBlipsForPoints() {
            for (int i = 0; i < points.Length; i++) {
                AddBlibForPoint(points[i], i);
            }
        }

        // scenario helpers
        public Vehicle AddParkedVehicle(float x, float y, float z, float heading, VehicleHash vehicleHash = VehicleHash.Prairie) {
            return AddParkedVehicle(new Location(x, y, z, heading), vehicleHash);
        }

        public Vehicle AddParkedVehicle(Location location, VehicleHash vehicleHash = VehicleHash.Prairie) {
            Vehicle vehicle = World.CreateVehicle(vehicleHash, location.position, location.heading);
            vehicle.PlaceOnGround();
            vehiclePool.Add(vehicle.Handle);
            return vehicle;
        }

        public Trigger AddTrigger(Vector3 position, float radius = 10.0f, String name = "") {
            Trigger trigger = new Trigger(position, radius, name);
            triggers.Add(trigger);
            trigger.index = triggers.IndexOf(trigger);
            return trigger;
        }

        public Participant AddParticipant(Location location, float radius = 5.0f,
            PedHash pedHash = default(PedHash), VehicleHash vehicleHash = default(VehicleHash), String name = ""
        ) {
            Participant participant = new Participant(location, radius, pedHash, vehicleHash, name);
            participants.Add(participant);
            return participant;
        }
    }
}