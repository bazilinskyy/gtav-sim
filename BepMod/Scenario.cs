using static BepMod.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using GTA;
using GTA.Math;

using GTA.Native;
using System.Diagnostics;

namespace BepMod {
    abstract class Scenario {
        public string Name;

        public Vector3[] Points;
        public int Waypoint;

        public List<Blip> Blips = new List<Blip> { };

        public Vector3 StartPosition;
        public float StartHeading;

        public VehicleHash VehicleHash = VehicleHash.Cruiser;
        public Weather Weather = Weather.ExtraSunny;

        bool Running = false;

        public Vehicle vehicle;
        public List<Actor> actors = new List<Actor>();
        public List<Trigger> triggers = new List<Trigger>();

        public DataLog Logger;
        public Stopwatch stopwatch;

        public void Main() {
            Log("Scenario.Main()");
        }

        public Scenario() { }

        public void SetStart(Vector3 position, float heading) {
            StartPosition = position;
            StartHeading = heading;
        }

        private void ClearActors() {
            foreach(Actor actor in actors) {
                actor.Dispose();
            }

            actors.Clear();
        }

        public virtual void PreRun() { }
        public virtual void PostRun() { }


        public virtual void Run(DataLog dataLog) {
            stopwatch = new Stopwatch();
            Logger = dataLog;
            
            Log("Scenario.Run()");

            Stop();

            Game.Player.Character.Position = StartPosition;
            Game.Player.Character.Heading = StartHeading;

            Waypoint = 0;
            Running = true;

            foreach(Blip blip in World.GetActiveBlips()) {
                blip.Remove();
            }

            ClearActors();
            pedPool.Clear();
            vehiclePool.Clear();
            DoRemovePeds();
            DoRemoveVehicles();

            AddBlipsForPoints();

            for (int i = 0; i < Points.Length; i++) {
                Vector3 point = Points[i];
                Trigger pointTrigger = AddTrigger(
                    point, 
                    name: i.ToString()
                );
                pointTrigger.NameFormat = "WAYPOINT_{0}";
                
                pointTrigger.TriggerEnter += (sender, index, e) => {
                    if (index < Points.Length) {
                        SetWaypoint(Math.Min(Points.Length - 1, index + 2));
                    } else {
                        ClearWaypoint();
                    }
                };
            }

            PreRun();

            vehicle = World.CreateVehicle(VehicleHash, StartPosition, StartHeading);

            vehiclePool.Add(vehicle.Handle);

            vehicle.MaxSpeed = 7.5f;

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
            World.RenderingCamera.Direction = Points[1];

            World.Weather = Weather;

            stopwatch.Start();
            Logger.Start(this);

            PostRun();

            UI.Notify("Scenario loaded");
        }

        override public string ToString()
        {
            return Name;
        }

        public void SetWaypoint(int index) {
            Log("Set waypoint " + index.ToString());
            Vector3 waypoint = Points[index];
            Function.Call(Hash.SET_NEW_WAYPOINT, waypoint.X, waypoint.Y);
        }

        public void ClearWaypoint() { }

        public void AddBlibForPoint(Vector3 point, int index) {
            Blip blip = World.CreateBlip(point);
            blip.Name = "Point " + index.ToString();
            Blips.Add(blip);
        }


        public void DoTick() {
            if (Running) {
                World.CurrentDayTime = new TimeSpan(12, 0, 0);

                Game.Player.Character.Health = 100;

                DoRemoveVehicles();
                DoRemovePeds();

                foreach(Actor actor in actors) {
                    actor.DoTick();
                }

                foreach(Trigger trigger in triggers) {
                    trigger.DoTick();
                }
            }
        }

        public void Stop() {
            Log("Scenario.Stop()");

            stopwatch.Stop();

            Logger.Stop();

            Running = false;

            ClearTriggers();
            ClearActors();
            ClearVehiclePool();
            ClearPedPool();
            RemoveOldVehicles();
        }

        private void ClearTriggers()
        {
            foreach (Trigger trigger in triggers)
            {
                trigger.Dispose();
            }

            triggers.Clear();
        }

        public void RemoveOldVehicles() {
            try {
                int vehicleHashCode = VehicleHash.GetHashCode();

                foreach(Vehicle vehicle in World.GetAllVehicles()) {
                    if (vehicle.GetHashCode() == vehicleHashCode && vehicle.Exists()) {
                        try { vehicle.Delete(); } catch { }
                    }
                }
            } catch { }
        }

        public void AddBlipsForPoints() {
            for (int i = 0; i < Points.Length; i++) {
                AddBlibForPoint(Points[i], i);
            }
        }

        // scenario helpers
        public Vehicle AddParkedVehicle(
            float x, float y, float z, float heading,
            VehicleHash vehicleHash = VehicleHash.Prairie
        ) {
            return AddParkedVehicle(new Vector3(x, y, z), heading, vehicleHash);
        }

        public Vehicle AddParkedVehicle(
            Vector3 position, float heading,
            VehicleHash vehicleHash = VehicleHash.Prairie
        ) {
            Vehicle vehicle = World.CreateVehicle(vehicleHash, position, heading);
            vehicle.PlaceOnGround();
            vehiclePool.Add(vehicle.Handle);
            return vehicle;
        }

        public Trigger AddTrigger(
            Vector3 position,
            float radius = 10.0f,
            String name = ""
        ) {
            Trigger trigger = new Trigger(position, radius, name);
            triggers.Add(trigger);
            trigger.index = triggers.IndexOf(trigger);
            return trigger;
        }

        public Actor AddActor(
            Vector3 position, float heading,
            float radius = 5.0f,
            PedHash pedHash = default(PedHash),
            VehicleHash vehicleHash = default(VehicleHash),
            String name = ""
        ) {
            Actor actor = new Actor(position, heading, radius, pedHash, vehicleHash, name);
            actors.Add(actor);
            return actor;
        }

        public Actor FindActorByEntity(Entity entity)
        {
            int handle = entity.Handle;
            return actors.Find(x => x.pedHandle == handle || x.vehicleHandle == handle);
        }


        // Log helpers
        public List<Trigger> GetActiveTriggers()
        {
            return triggers.FindAll(x => x.triggeredInside);
        }

        public List<Actor> GetActorsInRange()
        {
            return actors.FindAll(x => x.triggeredInside);
        }
    }
}