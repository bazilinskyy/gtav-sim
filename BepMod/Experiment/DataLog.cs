using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Diagnostics;

using GTA;
using GTA.Math;

using static BepMod.Util;

namespace BepMod.Experiment
{
    class DataLog
    {
        private string Name = "test";

        private int CurrentIndex = 0;
        private int CurrentTick = 0;
        private bool Running = false;

        private List<Entry> entries = new List<Entry>();

        private Entry PreviousEntry = new Entry();

        public Scenario ActiveScenario;
        public SmartEye smartEye;

        public DateTime StartTime;
        public StreamWriter Writer;
        public string BasePath = "BepModLog";
        public string DateLogNameFormat = "{0}--{1}.csv";
        public string DateLogDateTimeFormat = "yyyy-MM-dd--HH-mm-ss";

        private int ShowEntryStartIndex = 2;

        private string CSVHeader = String.Join(",",
            "Index",
            "Tick",
            "FrameNumber",
            "ElapsedMS",
            "Scenario",
            "ParticipantPosition.X",
            "ParticipantPosition.Y",
            "ParticipantPosition.Z",
            "ParticipantHeading",
            "ParticipantCameraRotation.X",
            "ParticipantCameraRotation.Z",
            "ParticipantSpeed",
            "GazeScreenCoords.X",
            "GazeScreenCoords.Y",
            "FilteredGazeScreenCoords.X",
            "FilteredGazeScreenCoords.Y",
            "GazeRayResult.DitHitAnything",
            "GazeRayResult.HitCoords.X",
            "GazeRayResult.HitCoords.Y",
            "GazeRayResult.HitCoords.Z",
            "GazeRayResult.DitHitEntity",
            "GazeRayResult.HitEntity.Handle",
            "GazingAtActor",
            "GazeActor",
            "GazeActor.Position.X",
            "GazeActor.Position.Y",
            "GazeActor.Position.Z",
            "ActiveTriggers",
            "ActorsInRange"
        );

        public static string EscapeCSV(string s)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        public struct Entry
        {
            public int Index;
            public int Tick;
            public Int64 ElapsedMS;

            public Scenario Scenario;

            // SmartEye Data
            public SmartEye.WorldIntersection WorldIntersection; // maybe? save all received info?
            public UInt64 FrameNumber;
            public Vector2 PreviousGazeScreenCoords;
            public Vector2 GazeScreenCoords;
            public Vector2 FilteredGazeScreenCoords;


            // GTA participant details
            public Vector3 ParticipantPosition;
            public float ParticipantHeading;
            public float ParticipantSpeed;
            public Vector3 ParticipantCameraRotation;


            // Ray from gaze
            public RaycastResult GazeRayResult;
            public Actor GazeActor;


            // Triggers
            public List<Trigger> Triggers;
            public List<Trigger> ActiveTriggers;


            // Actors
            public List<Actor> Actors;
            public List<Actor> ActorsInRange;


            public string ToCSV()
            {
                return String.Join(",",
                    Index,
                    Tick,
                    FrameNumber,
                    ElapsedMS,

                    Scenario,

                    ParticipantPosition.X,
                    ParticipantPosition.Y,
                    ParticipantPosition.Z,

                    ParticipantHeading,

                    ParticipantCameraRotation.X,
                    ParticipantCameraRotation.Z,

                    ParticipantSpeed,

                    GazeScreenCoords.X,
                    GazeScreenCoords.Y,
                    FilteredGazeScreenCoords.X,
                    FilteredGazeScreenCoords.Y,

                    GazeRayResult.DitHitAnything,
                    GazeRayResult.HitCoords.X,
                    GazeRayResult.HitCoords.Y,
                    GazeRayResult.HitCoords.Z,

                    GazeRayResult.DitHitEntity,
                    GazeRayResult.DitHitEntity
                        ? EscapeCSV(GazeRayResult.HitEntity.Handle.ToString()) :
                        "",

                    GazeActor != null,
                    GazeActor,
                    (GazeActor != null) ? GazeActor.Position.X.ToString() : "",
                    (GazeActor != null) ? GazeActor.Position.Y.ToString() : "",
                    (GazeActor != null) ? GazeActor.Position.Z.ToString() : "",

                    EscapeCSV(String.Join(",", ActiveTriggers)),
                    EscapeCSV(String.Join(",", ActorsInRange))
                );
            }
        }

        public Entry BuildEntry()
        {
            Entry e = new Entry();

            e.Index = ++CurrentIndex;
            e.Tick = CurrentTick;
            e.ElapsedMS = ActiveScenario.stopwatch.ElapsedMilliseconds;

            e.Scenario = ActiveScenario;

            e.WorldIntersection = smartEye.lastClosestWorldIntersection;
            e.FrameNumber = smartEye.lastFrameNumber;

            e.PreviousGazeScreenCoords = PreviousEntry.GazeScreenCoords;
            e.GazeScreenCoords = GetScreenCoords(smartEye.lastClosestWorldIntersection);
            e.FilteredGazeScreenCoords = GetFilteredScreenCoords(e.PreviousGazeScreenCoords, e.GazeScreenCoords);

            e.ParticipantPosition = Game.Player.Character.Position;
            e.ParticipantSpeed = ActiveScenario.vehicle.Speed;
            e.ParticipantHeading = Game.Player.Character.Heading;
            e.ParticipantCameraRotation = GameplayCamera.Rotation;

            e.GazeRayResult = GetGazeRay(e.FilteredGazeScreenCoords);

            if (e.GazeRayResult.DitHitEntity)
            {
                e.GazeActor = ActiveScenario.FindActorByEntity(
                    e.GazeRayResult.HitEntity);
            }

            e.Triggers = ActiveScenario.triggers;
            e.ActiveTriggers = ActiveScenario.GetActiveTriggers();

            e.Actors = ActiveScenario.actors;
            e.ActorsInRange = ActiveScenario.GetActorsInRange();

            return e;
        }

        public void WriteEntry(Entry e)
        {
            Writer.WriteLine(e.ToCSV());
        }

        public void ShowEntry(Entry e)
        {
            int i = ShowEntryStartIndex;

            string gazeObject = "";
            SmartEye.WorldIntersection wi = e.WorldIntersection;
            if (wi.ObjectPoint != null)
            {
                gazeObject = wi.ObjectPoint.ToString();
            }

            ShowMessage("Entry (" + e.Index + ")", i++);
            ShowMessage("- Tick: " + e.Tick, i++);
            ShowMessage("- FrameNumber: " + e.FrameNumber, i++);
            ShowMessage("- ElapsedMS: " + e.ElapsedMS, i++);
            ShowMessage("- Scenario: " + e.Scenario, i++);
            ShowMessage("- ParticipantPosition: " + e.ParticipantPosition, i++);
            ShowMessage("- ParticipantHeading: " + e.ParticipantHeading, i++);
            ShowMessage("- ParticipantSpeed: " + e.ParticipantSpeed, i++);
            ShowMessage("- ParticipantCameraRotation: " + e.ParticipantCameraRotation, i++);
            ShowMessage("- GazeObject: " + gazeObject, i++);
            ShowMessage("- PreviousGazeScreenCoords: " + e.PreviousGazeScreenCoords, i++);
            ShowMessage("- GazeScreenCoords: " + e.GazeScreenCoords, i++);
            ShowMessage("- FilteredGazeScreenCoords: " + e.FilteredGazeScreenCoords, i++);
            ShowMessage("- GazeRayResult Hit: " + e.GazeRayResult.DitHitAnything, i++);
            ShowMessage("- GazeRayResult Entity: " + e.GazeRayResult.HitEntity, i++);
            ShowMessage("- GazeActor: " + e.GazeActor, i++);
            if (e.GazeActor != null)
            {
                ShowMessage("- GazeActor.Position: " + e.GazeActor.Position, i++);
            }
            else
            {
                ShowMessage("- GazeActor.Position: -", i++);
            }
            ShowMessage("- ActiveTriggers: " + String.Join(", ", e.ActiveTriggers), i++);
            ShowMessage("- ActorsInRange: " + String.Join(", ", e.ActorsInRange), i++);
        }

        public void ShowEntryGaze(Entry e)
        {
            UIRectangle et = new UIRectangle(
                new Point(
                    (int)(((e.FilteredGazeScreenCoords.X + 1) / 2) * UI.WIDTH),
                    (int)(((e.FilteredGazeScreenCoords.Y + 1) / 2) * UI.HEIGHT)
                ),
                new Size(new Point(15, 15)),
                Color.FromArgb(127, Color.Yellow)
            );
            et.Draw();

            World.DrawMarker(
                MarkerType.DebugSphere,
                e.GazeRayResult.HitCoords,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1, 1, 1),
                Color.FromArgb(127, Color.White)
            );
        }

        public Vector2 GetScreenCoords(SmartEye.WorldIntersection worldIntersection)
        {
            return GetScreenCoords(new Vector2(
                worldIntersection.ObjectPoint.X,
                worldIntersection.ObjectPoint.Y
            ));
        }

        public Vector2 GetScreenCoords(Vector2 P)
        {
            return new Vector2(
                P.X / 1920,
                P.Y / 1200
            ) * 2 - new Vector2(1, 1);
        }

        public Vector2 GetFilteredScreenCoords(Vector2 source, Vector2 target)
        {
            return Vector2.Lerp(
                source,
                target,
                0.3f
            );
        }

        public RaycastResult GetGazeRay(Vector2 screenCoords)
        {
            Vector3 camPoint;
            Vector3 farPoint;
            Gta5EyeTracking.Geometry.ScreenRelToWorld(screenCoords, out camPoint, out farPoint);

            Vector3 direction = farPoint - camPoint;

            RaycastResult ray = World.Raycast(
                source: camPoint,
                direction: direction,
                maxDistance: 200f,
                options: IntersectOptions.Everything,
                ignoreEntity: Game.Player.Character
            );

            return ray;
        }

        public DataLog(SmartEye smartEye)
        {
            this.smartEye = smartEye;

            Directory.CreateDirectory(BasePath);
        }

        ~DataLog()
        {
            Stop();
        }

        public void Start(Scenario activeScenario)
        {
            if (Running)
            {
                Stop();
            }

            StartTime = DateTime.Now;

            Writer = File.CreateText(
                Path.Combine(BasePath,
                    String.Format(DateLogNameFormat,
                        StartTime.ToString(DateLogDateTimeFormat),
                        Name
                    )));

            Writer.AutoFlush = true;

            Writer.WriteLine(CSVHeader);

            ActiveScenario = activeScenario;
            Running = true;
        }

        public void Stop()
        {
            if (Running)
            {
                // Flush log and clean up
                Writer.Flush();
                Writer.Close();
                Writer = null;

                Running = false;

                ClearMessages();
            }
        }

        // GTA tick to do GTA related operations in
        // Write log entry if running
        public void DoTick()
        {
            CurrentTick++;
            if (Running)
            {
                Entry e = BuildEntry();
                WriteEntry(e);
                if (debugLevel > 0)
                {
                    ShowEntry(e);
                }
                if (debugLevel > 1)
                {
                    ShowEntryGaze(e);
                }
                PreviousEntry = e;

                //if (e.GazeRayResult.DitHitEntity)
                //{
                //    e.GazeRayResult.HitEntity.ApplyForceRelative(new Vector3(0, 0, 1));
                //    e.GazeRayResult.HitEntity.ApplyForce(GameplayCamera.Direction.Normalized);
                //}
            }
        }
    }
}
