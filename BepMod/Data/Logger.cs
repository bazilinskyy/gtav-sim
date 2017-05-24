using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Diagnostics;

using GTA;
using GTA.Math;

using BepMod.Experiment;
using static BepMod.Util;
using System.Globalization;

namespace BepMod.Data
{
    class Logger
    {
        private string Name = "test";

        private int CurrentIndex = 0;
        private int CurrentTick = 0;
        private bool Running = false;

        private List<Entry> _entries = new List<Entry>();
        private List<Vector2> _coords = new List<Vector2>();
        private List<Measurement> _measurements = new List<Measurement>();
        private List<Measurement> _activeMeasurements = new List<Measurement>();

        private Entry PreviousEntry = new Entry();

        public Scenario ActiveScenario;
        public SmartEye smartEye;

        public DateTime StartTime;
        public string BasePath = "BepModLog";
        public string DateTimeFormat = "yyyy-MM-dd--HH-mm-ss";

        public string LogFileNameFormat = "{0}--{1}-log.csv";
        public string LogFileName;
        public StreamWriter LogWriter;

        public string MeasurementFileNameFormat = "{0}--{1}-measurements.csv";
        public string MeasurementFileName;
        public StreamWriter MeasurementsWriter;

        private int ShowEntryStartIndex = 2;

        private static NumberFormatInfo nfi = new NumberFormatInfo();

        public static string EscapeCSV(string s)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        public class Measurement
        {
            private Logger _logger;

            private bool _started;
            private bool _stopped;

            private Actor _startActor;
            private Actor _stopActor;

            public int StartTick;
            public int EndTick;
            public int ElapsedTick;

            public Int64 StartMS;
            public Int64 EndMS;
            public Int64 ElapsedMS;

            public UInt64 StartFrameNumber;
            public UInt64 EndFrameNumber;
            public UInt64 ElapsedFrameNumber;

            public string StartEvent = "";
            public string StartName = "";
            public string StopEvent = "";
            public string StopName = "";

            public bool Started { get => _started; }
            public bool Stopped { get => _stopped; }

            internal Actor StopActor { get => _stopActor; }
            internal Actor StartActor { get => _startActor; }

            public Measurement(Logger logger, string startEvent = "", string startName = "")
            {
                _logger = logger;
                StartEvent = startEvent;
                StartName = startName;
            }

            public Measurement Start(Trigger trigger)
            {
                return Start(trigger.ToString(), "ENTER");
            }

            public Measurement Start(string startName = "", string startEvent = "")
            {
                if (_stopped || _started) {
                    return this;
                }

                if (startEvent != "")
                {
                    StartEvent = startEvent;
                }

                if (startName != "")
                {
                    StartName = startName;
                }
                
                StartTick = _logger.CurrentTick;
                StartFrameNumber = _logger.smartEye.lastFrameNumber;
                StartMS = _logger.ActiveScenario.Stopwatch.ElapsedMilliseconds;

                _started = true;

                _logger._activeMeasurements.Add(this);

                return this;
            }

            public Measurement Stop(string stopName = "", string stopEvent = "")
            {
                if (_stopped || !_started) {
                    return this;
                }
                
                if (stopName != "")
                {
                    StopName = stopName;
                }

                if (stopEvent != "")
                {
                    StopEvent = stopEvent;
                }
                
                EndTick = _logger.CurrentTick;
                EndFrameNumber = _logger.smartEye.lastFrameNumber;
                EndMS = _logger.ActiveScenario.Stopwatch.ElapsedMilliseconds;

                ElapsedTick = EndTick - StartTick;
                ElapsedFrameNumber = EndFrameNumber - StartFrameNumber;
                ElapsedMS = EndMS - StartMS;

                _stopped = true;

                _logger.WriteMeasurement(this);

                return this;
            }

            public Measurement StartOnGazeAtActor(Actor actor) 
            {
                if (StartName == "")
                {
                    StartName = actor.ToString();
                }

                if (StartEvent == "")
                {
                    StartEvent = "GAZE";
                }

                _startActor = actor;

                return this;
            }

            public Measurement StopOnGazeAtActor(Actor actor)
            {
                if (StopName == "")
                {
                    StopName = actor.ToString();
                }

                if (StopEvent == "")
                {
                    StopEvent = "GAZE";
                }

                _stopActor = actor;

                return this;
            }

            public static string CSVHeader = String.Join(",",
                "StartName",
                "StartEvent",

                "StopName",
                "StopEvent",

                "Stopped",

                "ElapsedMS",
                "ElapsedTick",
                "ElapsedFrameNumber",

                "StartMS",
                "StartTick",
                "StartFrameNumber",

                "EndMS",
                "EndTick",
                "EndFrameNumber"
            );

            public string ToCSV()
            {
                return String.Join(",",
                    EscapeCSV(StartName),
                    EscapeCSV(StartEvent),

                    EscapeCSV(StopName),
                    EscapeCSV(StopEvent),

                    Stopped ? 1 : 0,

                    ElapsedMS,
                    ElapsedTick,
                    ElapsedFrameNumber,

                    StartMS,
                    StartTick,
                    StartFrameNumber,

                    EndMS,
                    EndTick,
                    EndFrameNumber
                );
            }
        }

        public struct Entry
        {
            public int Index;
            public int Tick;
            public Int64 ElapsedMS;

            public Scenario Scenario;

            // SmartEye Data
            public UInt64 FrameNumber;
            public Vector2 PreviousGazeScreenCoords;
            public bool LookingAtScreen;
            public Vector2 GazeScreenCoords;
            public Vector2 SmoothedGazeScreenCoords;


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


            public static string CSVHeader = String.Join(",",
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
                "LookingAtScreen",
                "GazeScreenCoords.X",
                "GazeScreenCoords.Y",
                "SmoothedGazeScreenCoords.X",
                "SmoothedGazeScreenCoords.Y",
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

            public string ToCSV()
            {
                return String.Join(",",
                    Index,
                    Tick,
                    FrameNumber,
                    ElapsedMS,

                    Scenario,

                    ParticipantPosition.X.ToString(nfi),
                    ParticipantPosition.Y.ToString(nfi),
                    ParticipantPosition.Z.ToString(nfi),

                    ParticipantHeading.ToString(nfi),

                    ParticipantCameraRotation.X.ToString(nfi),
                    ParticipantCameraRotation.Z.ToString(nfi),

                    ParticipantSpeed.ToString(nfi),

                    LookingAtScreen ? 1 : 0,
                    GazeScreenCoords.X.ToString(nfi),
                    GazeScreenCoords.Y.ToString(nfi),
                    SmoothedGazeScreenCoords.X.ToString(nfi),
                    SmoothedGazeScreenCoords.Y.ToString(nfi),

                    GazeRayResult.DitHitAnything ? 1 : 0,
                    GazeRayResult.HitCoords.X.ToString(nfi),
                    GazeRayResult.HitCoords.Y.ToString(nfi),
                    GazeRayResult.HitCoords.Z.ToString(nfi),

                    GazeRayResult.DitHitEntity ? 1 : 0,
                    GazeRayResult.DitHitEntity
                        ? EscapeCSV(GazeRayResult.HitEntity.Handle.ToString()) :
                        "",

                    GazeActor != null,
                    GazeActor,
                    (GazeActor != null) ? GazeActor.Position.X.ToString(nfi) : "",
                    (GazeActor != null) ? GazeActor.Position.Y.ToString(nfi) : "",
                    (GazeActor != null) ? GazeActor.Position.Z.ToString(nfi) : "",

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
            e.ElapsedMS = ActiveScenario.Stopwatch.ElapsedMilliseconds;

            e.Scenario = ActiveScenario;

            e.FrameNumber = smartEye.lastFrameNumber;

            e.LookingAtScreen = smartEye.LookingAtScreen;
            e.PreviousGazeScreenCoords = PreviousEntry.GazeScreenCoords;
            e.GazeScreenCoords = smartEye.Coords;
            e.SmoothedGazeScreenCoords = smartEye.SmoothedCoords;

            e.ParticipantPosition = Game.Player.Character.Position;
            e.ParticipantSpeed = ActiveScenario.vehicle.Speed;
            e.ParticipantHeading = Game.Player.Character.Heading;
            e.ParticipantCameraRotation = GameplayCamera.Rotation;

            e.GazeRayResult = GetGazeRay(e.SmoothedGazeScreenCoords);
            
            if (e.GazeRayResult.DitHitEntity)
            {
                Actor actor = ActiveScenario.FindActorByEntity(e.GazeRayResult.HitEntity);
                if (actor != null)
                {
                    e.GazeActor = actor;
                }
            }
            
            if (e.GazeActor == null)
            {
                e.GazeActor = GetGazeActor(e.SmoothedGazeScreenCoords, out _);
            }

            e.Triggers = ActiveScenario.triggers;
            e.ActiveTriggers = ActiveScenario.GetActiveTriggers();

            e.Actors = ActiveScenario.actors;
            e.ActorsInRange = ActiveScenario.GetActorsInRange();

            return e;
        }

        public void WriteEntry(Entry e)
        {
            LogWriter.WriteLine(e.ToCSV());
        }

        public Measurement CreateMeasurement(Actor actor)
        {
            return CreateMeasurement().StartOnGazeAtActor(actor);
        }

        public Measurement CreateMeasurement(Trigger trigger)
        {
            Measurement measurement = CreateMeasurement();

            measurement.StartEvent = "ENTER";
            measurement.StartName = trigger.ToString();

            return measurement;
        }

        public Measurement CreateMeasurement(string startEvent = "", string startName = "")
        {
            Measurement measurement = new Measurement(this, startEvent, startName);

            _measurements.Add(measurement);

            return measurement;
        }

        public void WriteMeasurement(Measurement measurement)
        {
            MeasurementsWriter.WriteLine(measurement.ToCSV());
        }

        public void ShowEntry(Entry e)
        {
            int i = ShowEntryStartIndex;

            ShowMessage("Entry (" + e.Index + ")", i++);
            ShowMessage("- Tick: " + e.Tick, i++);
            ShowMessage("- FrameNumber: " + e.FrameNumber, i++);
            ShowMessage("- ElapsedMS: " + e.ElapsedMS, i++);
            ShowMessage("- Scenario: " + e.Scenario, i++);
            ShowMessage("- ParticipantPosition: " + e.ParticipantPosition, i++);
            ShowMessage("- ParticipantHeading: " + e.ParticipantHeading, i++);
            ShowMessage("- ParticipantSpeed: " + e.ParticipantSpeed, i++);
            ShowMessage("- ParticipantCameraRotation: " + e.ParticipantCameraRotation, i++);
            ShowMessage("- PreviousGazeScreenCoords: " + e.PreviousGazeScreenCoords, i++);
            ShowMessage("- LookingAtScreen: " + e.LookingAtScreen, i++);
            ShowMessage("- GazeScreenCoords: " + e.GazeScreenCoords, i++);
            ShowMessage("- SmoothedGazeScreenCoords: " + e.SmoothedGazeScreenCoords, i++);
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
            UIRectangle es = new UIRectangle(
                new Point(
                    (int)(((e.SmoothedGazeScreenCoords.X + 1) / 2) * UI.WIDTH) - 8,
                    (int)(((e.SmoothedGazeScreenCoords.Y + 1) / 2) * UI.HEIGHT) - 8
                ),
                new Size(new Point(16, 16)),
                Color.FromArgb(150, Color.Red)
            );
            es.Draw();

            UIRectangle et = new UIRectangle(
                new Point(
                    (int)(((e.GazeScreenCoords.X + 1) / 2) * UI.WIDTH) - 5,
                    (int)(((e.GazeScreenCoords.Y + 1) / 2) * UI.HEIGHT) - 5
                ),
                new Size(new Point(10, 10)),
                Color.FromArgb(150, Color.Yellow)
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
            
            if (e.GazeActor != null)
            {
                World.DrawMarker(
                    MarkerType.DebugSphere,
                    e.GazeActor.Position,
                    Vector3.Zero,
                    Vector3.Zero,
                    new Vector3(1, 1, 1),
                    Color.FromArgb(127, Color.Red)
                );
            }
        }

        public Actor GetGazeActor(Vector2 screenCoords, out Vector3 hitCoords)
        {
            var radius = 2;
            var numPoints = 5;
            var angleStep = Math.PI * 0.2;
            var distStep = 0.05 / numPoints;
            var resultCoord = new Vector3();

            for (var i = 0; i < numPoints; i++)
            {
                var angle = i * angleStep;
                var dist = i * distStep;
                var offsetX = Math.Sin(angle) * dist;
                var offsetY = Math.Cos(angle) * dist;
                var coord = screenCoords + new Vector2((float)offsetX, (float)offsetY);
                Entity entity;
                var hitcoord = Gta5EyeTracking.Geometry.RaycastEverything(coord, out entity, radius);
                if (i == 0)
                {
                    resultCoord = hitcoord;
                }

                if ((entity != null)
                    && ((Gta5EyeTracking.ScriptHookExtensions.IsEntityAPed(entity)
                            && entity.Handle != Game.Player.Character.Handle)
                        || (Gta5EyeTracking.ScriptHookExtensions.IsEntityAVehicle(entity)
                            && !(Game.Player.Character.IsInVehicle()
                                && entity.Handle == Game.Player.Character.CurrentVehicle.Handle))))
                {
                    Actor actor = ActiveScenario.FindActorByEntity(entity);

                    if (actor != null)
                    {
                        hitCoords = resultCoord;
                        return actor;
                    }
                }
            }

            hitCoords = new Vector3();
            return null;
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

        public Logger(SmartEye smartEye)
        {
            nfi.NumberDecimalSeparator = ".";

            this.smartEye = smartEye;

            Directory.CreateDirectory(BasePath);
        }

        ~Logger()
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
            string formattedDateTime = StartTime.ToString(DateTimeFormat);

            LogFileName = Path.Combine(BasePath,
                String.Format(LogFileNameFormat, formattedDateTime, Name));
            LogWriter = File.CreateText(LogFileName);
            LogWriter.AutoFlush = true;
            LogWriter.WriteLine(Entry.CSVHeader);

            MeasurementFileName = Path.Combine(BasePath,
                String.Format(MeasurementFileNameFormat, formattedDateTime, Name));
            MeasurementsWriter = File.CreateText(MeasurementFileName);
            MeasurementsWriter.AutoFlush = true;
            MeasurementsWriter.WriteLine(Measurement.CSVHeader);

            ActiveScenario = activeScenario;
            Running = true;
        }

        public void Stop()
        {
            if (Running)
            {
                // Flush log and clean up
                LogWriter.Flush();
                LogWriter.Close();
                LogWriter = null;
                LogFileName = null;

                // write all remaining measurements and clean up
                _activeMeasurements.ForEach(WriteMeasurement);
                MeasurementsWriter.Flush();
                MeasurementsWriter.Close();
                MeasurementsWriter = null;
                MeasurementFileName = null;

                _activeMeasurements.Clear();
                _measurements.Clear();

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
                _entries.Add(e);
                _coords.Add(e.SmoothedGazeScreenCoords);

                WriteEntry(e);
                if (debugLevel > 0)
                {
                    ShowEntryGaze(e);
                }
                if (debugLevel > 3)
                {
                    ShowEntry(e);
                }
                PreviousEntry = e;

                StartMeasurements(e);
                StopMeasurements(e);

                // int i = 7;
                // ShowMessage(Measurement.CSVHeader, i++);
                // _measurements.ForEach(m => ShowMessage(m.ToCSV(), i++));

                //if (e.GazeRayResult.DitHitEntity)
                //{
                //    e.GazeRayResult.HitEntity.ApplyForceRelative(new Vector3(0, 0, 1));
                //    e.GazeRayResult.HitEntity.ApplyForce(GameplayCamera.Direction.Normalized);
                //}
            }
        }
        private List<Measurement> StartMeasurements(Entry e)
        {
            List<Measurement> started = _activeMeasurements.FindAll(
                m => (m.StartActor != null && e.GazeActor != null && e.GazeActor == m.StartActor));

            started.ForEach(m => m.Start());

            _activeMeasurements.AddRange(started);

            return started;
        }

        private List<Measurement> StopMeasurements(Entry e)
        {
            List<Measurement> stopped = _activeMeasurements.FindAll(
                m => (m.StopActor != null && e.GazeActor != null && e.GazeActor == m.StopActor));

            stopped.ForEach(m => m.Stop());

            _activeMeasurements.RemoveAll(m => stopped.Contains(m));

            return stopped;
        }
    }
}
