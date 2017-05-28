using System;
using System.Collections.Generic;
using System.Diagnostics;

using GTA;
using GTA.Math;
using GTA.Native;

using BepMod.Data;
using static BepMod.Util;

namespace BepMod.Experiment
{
    abstract class Scenario
    {
        public static GPS gps = new GPS();

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

        public Stopwatch Stopwatch;

        protected Logger _logger;
        protected SmartEye _smartEye;

        protected bool _participantRanRedLight = false;

        public Scenario() { }
        
        public void SetStart(Vector3 position, float heading)
        {
            StartPosition = position;
            StartHeading = heading;
        }

        private void ClearActors()
        {
            foreach (Actor actor in actors)
            {
                actor.Dispose();
            }

            actors.Clear();
        }

        public virtual void PreRun() { }
        public virtual void Initialise() { }


        public virtual void Run(Logger logger, SmartEye smartEye)
        {
            Game.FadeScreenOut(500);

            Stopwatch = new Stopwatch();
            Stopwatch.Stop();

            _logger = logger;
            _smartEye = smartEye;

            Log("Scenario.Run()");

            Stop();

            Game.Player.Character.Position = StartPosition;
            Game.Player.Character.Heading = StartHeading;

            Waypoint = 0;
            Running = true;

            foreach (Blip blip in World.GetActiveBlips())
            {
                blip.Remove();
            }

            ClearActors();
            pedPool.Clear();
            vehiclePool.Clear();
            DoRemovePeds();
            DoRemoveVehicles();

            AddBlipsForPoints();

            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 point = Points[i];
                Trigger pointTrigger = CreateTrigger(
                    point,
                    name: i.ToString()
                );
                pointTrigger.NameFormat = "WAYPOINT_{0}";

                pointTrigger.TriggerEnter += (sender, index, e) =>
                {
                    if (index < Points.Length)
                    {
                        SetWaypoint(Math.Min(Points.Length - 1, index + 2));
                    }
                    else
                    {
                        ClearWaypoint();
                    }
                };
            }

            PreRun();

            vehicle = World.CreateVehicle(VehicleHash, StartPosition, StartHeading);

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
            Game.Player.Character.CanWearHelmet = false;
            Game.Player.Character.RemoveHelmet(true);
            Function.Call(Hash.REMOVE_ALL_PED_WEAPONS, Game.Player.Character.Handle);
            Game.Player.Character.CanSwitchWeapons = false;


            GameplayCamera.RelativeHeading = 0.0f;
            World.RenderingCamera.Direction = Points[1];

            World.Weather = Weather;

            Stopwatch.Start();
            _logger.Start(this);

            Initialise();

            vehicle.Speed = 0;

            SetTimeout(() => Game.FadeScreenIn(1500), 500);
        }

        override public string ToString()
        {
            return String.Format("SCENARIO_{0}", Name);
        }

        public bool CheckRedLight() {
            if (trafficLightsColor == TrafficLightColor.Red)
            {
                if (_participantRanRedLight == false)
                {
                    UI.ShowSubtitle("Rustig aan!", 3000);
                }

                _participantRanRedLight = true;
            }
            else if (_participantRanRedLight == true)
            {
                _participantRanRedLight = false;
            }

            return _participantRanRedLight;
        }

        public void SetWaypoint(int index)
        {
            Log("Set waypoint " + index.ToString());
            Vector3 waypoint = Points[index];
            Function.Call(Hash.SET_NEW_WAYPOINT, waypoint.X, waypoint.Y);
        }

        public void ClearWaypoint() { }

        public void AddBlibForPoint(Vector3 point, int index)
        {
            Blip blip = World.CreateBlip(point);
            blip.Name = "POINT_" + index.ToString();
            Blips.Add(blip);
        }

        public struct Delay
        {
            public bool Cancelled;

            public Action Callback;
            public Int64 Timeout;

            public Int64 StartTime;

            public Delay(Action callback, Int64 timeoutMs, Int64 elapsedMs)
            {
                Cancelled = false;
                Callback = callback;
                Timeout = timeoutMs;
                StartTime = elapsedMs;
            }

            public void Cancel()
            {
                Cancelled = true;
            }

            public bool IsExpired(Int64 elapsedMs)
            {
                return (elapsedMs - StartTime) > Timeout;
            }

            public void Call(Int64 elapsedMs)
            {
                if (!Cancelled && IsExpired(elapsedMs))
                {
                    Callback();
                }
            }
        }

        private List<Delay> delays = new List<Delay>();

        public Delay SetTimeout(Action callback, Int64 timeoutMs)
        {
            return SetTimeout(new Delay(callback, timeoutMs, Stopwatch.ElapsedMilliseconds));
        }

        public Delay SetTimeout(Delay delay)
        {
            delays.Add(delay);
            return delay;
        }

        public void DoTick()
        {
            if (Running)
            {
                Int64 elapsedMs = Stopwatch.ElapsedMilliseconds;
                World.CurrentDayTime = new TimeSpan(12, 0, 0);

                Game.Player.Character.Health = 100;
                Game.Player.Character.CanBeKnockedOffBike = false;

                if (!Game.Player.Character.IsInVehicle())
                {
                    Game.Player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                }

                //if (Game.Player.Character.LastVehicle.HasCollidedWithAnything)
                //{
                //    ShowMessage("oh noh ;(", 18);
                //    Game.Player.Character.LastVehicle.HasCollision = false;
                //}


                if (_participantRanRedLight) {
                    vehicle.Speed = 0.0f;
                    CheckRedLight();
                }

                DoRemoveVehicles();
                DoRemovePeds();

                foreach (Delay delay in delays)
                {
                    if (delay.IsExpired(elapsedMs))
                    {
                        delay.Call(elapsedMs);
                    }
                }
                
                delays.RemoveAll(x => x.Cancelled || x.IsExpired(elapsedMs));

                foreach (Actor actor in actors)
                {
                    actor.DoTick();
                }
                actors.RemoveAll(x => x.IsRemoved);

                foreach (Trigger trigger in triggers)
                {
                    trigger.DoTick();
                }
            }
        }

        public void Stop()
        {
            Log("Scenario.Stop()");

            Stopwatch.Stop();

            _logger.Stop();

            Running = false;

            delays.Clear();
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

        public void RemoveOldVehicles()
        {
            try
            {
                int vehicleHashCode = VehicleHash.GetHashCode();

                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (vehicle.GetHashCode() == vehicleHashCode && vehicle.Exists())
                    {
                        try { vehicle.Delete(); } catch { }
                    }
                }
            }
            catch { }
        }

        public void AddBlipsForPoints()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                AddBlibForPoint(Points[i], i);
            }
        }

        // scenario helpers
        public Vehicle AddParkedVehicle(
            float x, float y, float z, float heading,
            VehicleHash vehicleHash = VehicleHash.Prairie
        )
        {
            return CreateParkedVehicle(new Vector3(x, y, z), heading, vehicleHash);
        }

        public Vehicle CreateParkedVehicle(
            Vector3 position, float heading,
            VehicleHash vehicleHash = VehicleHash.Prairie
        )
        {
            Vehicle vehicle = World.CreateVehicle(vehicleHash, position, heading);
            vehicle.PlaceOnGround();
            vehiclePool.Add(vehicle.Handle);
            return vehicle;
        }

        public Trigger CreateTrigger(
            Vector3 position,
            float radius = 10.0f,
            String name = "",
            Entity entity = null,
            Action<Trigger> enter = null,
            Action<Trigger> exit = null
        )
        {
            Trigger trigger = new Trigger(position, radius, name, entity, enter, exit);
            triggers.Add(trigger);
            trigger.index = triggers.IndexOf(trigger);
            return trigger;
        }

        public Actor CreateActor(
            Vector3 position, float heading,
            float radius = 5.0f,
            PedHash pedHash = default(PedHash),
            VehicleHash vehicleHash = default(VehicleHash),
            String name = ""
        )
        {
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
