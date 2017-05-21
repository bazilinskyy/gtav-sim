using System.Collections.Generic;
using System.Drawing;

using GTA;
using GTA.Math;
using GTA.Native;

using NativeUI;

using static BepMod.Util;
using System;
using System.Threading;

using System.Windows.Forms;

namespace BepMod {
    public class Main : Script {
        private Scenario scenario;
        private List<Scenario> scenarios = new List<Scenario>();
        private int scenariosCount = 0;
        public Entity lastRayHitEntity;

        private UIMenu menu;
        private MenuPool menuPool = new MenuPool();

        private bool renderPosition = false;
        private bool renderRay = false;
        private UIText positionMessage;

        private double degreesPerPixel;

        public Main() {
            Log("Main.Main()");

            positionMessage = new UIText("",
                new Point((int) System.Math.Round((double) UI.WIDTH / 2), 20),
                .8f,
                Color.Yellow,
                GTA.Font.ChaletComprimeCologne,
                true
            );


            scenarios.Add(new Scenario1());
            // scenarios.Add(new Scenario2());
            // scenarios.Add(new Scenario3());

            menu = new UIMenu("BepMod", "");

            foreach(Scenario scenario in scenarios) {
                scenariosCount++;

                UIMenu scenarioSubMenu = menuPool.AddSubMenu(
                    menu,
                    String.Format("Scenario {0}", scenariosCount.ToString())
                );

                scenarioSubMenu.AddItem(new UIMenuItem(
                    "Run",
                    ""
                ));

                int pointIndex = 0;
                int scenarioIndex = scenariosCount - 1;

                scenarioSubMenu.OnItemSelect += (UIMenu sender, UIMenuItem item, int index) => {
                    Log("Dingen: " + index.ToString());
                    if (index == 0) {
                        RunScenario(scenarioIndex);
                        menu.Visible = false;
                        scenarioSubMenu.Visible = false;
                    }
                };

                var pointsList = new List<dynamic> { };
                for (int i = 0; i < scenario.points.Length; i++) {
                    pointsList.Add(String.Format("Point {0}", i));
                }

                UIMenuListItem pointMenuListItem = new UIMenuListItem("Teleport", pointsList, 0);
                scenarioSubMenu.AddItem(pointMenuListItem);
                scenarioSubMenu.OnItemSelect += (sender, item, index) => {
                    if (index == 1) {
                        Vector3 position = scenario.points[pointIndex];
                        UI.Notify("Teleport to: " + position.ToString());
                        Game.Player.Character.Position = position;
                    }
                };

                scenarioSubMenu.OnListChange += (sender, item, index) => {
                    if (item == pointMenuListItem) {
                        pointIndex = index;
                    }
                };
            }
            Log("Loaded " + scenariosCount.ToString() + " scenario(s)");

            menu.AddItem(new UIMenuItem("Stop running scenario", ""));

            var debugLevels = new List<dynamic> {
                "None",
                "Info",
                "Verbose",
                "Full"
            };

            UIMenuListItem debugLevelMenuItem = new UIMenuListItem("Debug level", debugLevels, debugLevel);
            menu.AddItem(debugLevelMenuItem);

            menu.OnListChange += (sender, item, index) => {
                if (item == debugLevelMenuItem) {
                    debugLevel = index;
                    ClearMessages();
                }
            };
                
            menu.AddItem(new UIMenuCheckboxItem("Show location", false, ""));
            menu.AddItem(new UIMenuCheckboxItem("Show target object", false, ""));
            menu.AddItem(new UIMenuItem("Remove all vehicles", ""));
            menu.AddItem(new UIMenuItem("Remove all pedestrians", ""));

            menu.RefreshIndex();

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;

            menuPool.Add(menu);

            UI.Notify("For the Bicycle Experiment Program press ~b~B");

            menu.AddInstructionalButton(new InstructionalButton("B", "BepMod menu"));
            menu.AddInstructionalButton(new InstructionalButton("N", "Save location"));
            menu.AddInstructionalButton(new InstructionalButton("I", "Load scenario"));

            KeyDown += DoKeyDown;
            Tick += MainTick;
            Aborted += MainAborted;

            gpsSoundLeft.SoundLocation = audioBase + "LEFT.WAV";
            gpsSoundLeft.Load();

            gpsSoundRight.SoundLocation = audioBase + "RIGHT.WAV";
            gpsSoundRight.Load();

            gpsSoundStraight.SoundLocation = audioBase + "STRAIGHTAHEAD.WAV";
            gpsSoundStraight.Load();

            Game.Player.Character.IsInvincible = true;

            double diag = Math.Sqrt(UI.WIDTH * UI.WIDTH + UI.HEIGHT * UI.HEIGHT);
            degreesPerPixel = GameplayCamera.FieldOfView / diag;

            if (Game.IsScreenFadedOut) {
                Game.FadeScreenIn(0);
            }
        }

        private void MainAborted(object sender, EventArgs e)
        {
            Log("ABORTED");
            smartEye.Stop();
        }

        ~Main() {
            Log("Main.~Main()");
            smartEye.Stop();
            StopScenario();
            ClearVehiclePool();
            Log("Main.~Main(): shut down complete");
         }

        private void DoKeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyCode == System.Windows.Forms.Keys.B) {
                menu.Visible = !menu.Visible;
            } else if (e.KeyCode == System.Windows.Forms.Keys.I) {
                RunScenario(0);
            } else if (e.KeyCode == System.Windows.Forms.Keys.N) {
                Vector3 position = Game.Player.Character.Position;
                float heading = Game.Player.Character.Heading;

                String location = String.Format(
                    "new Vector3({0}f, {1}f, {2}f), {3}f",
                    position.X.ToString("0.0"),
                    position.Y.ToString("0.0"),
                    position.Z.ToString("0.0"),
                    heading.ToString("0.0")
                );

                UI.Notify("Clipboard:\n" + location);

                Log(location, "location");

                // http://stackoverflow.com/questions/3546016/how-to-copy-data-to-clipboard-in-c-sharp
                Thread thread = new Thread(() => Clipboard.SetText(location));
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join(); //Wait for the thread to end
            } else if (e.KeyCode == System.Windows.Forms.Keys.Z) {
                Mayhem();
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.L)
            {
                smartEye.Stop();
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.O)
            {
                smartEye.Start();
            }
        }

        public void Mayhem() {
            Vector3 p = Game.Player.Character.Position;
            float h = Game.Player.Character.Heading;
            
            Vehicle v1 = World.CreateVehicle(VehicleHash.Maverick, p + new Vector3(0, 0, 60f), h);
            v1.Speed = 10f;

            vehiclePool.Add(v1.Handle);
        }

        public void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index) {
            Log("Menu_OnItemSelect (" + index + ") " + selectedItem.Text);

            if (index == scenariosCount) {
                StopScenario();
            } else if (index == (scenariosCount + 4)) {
                ClearVehiclePool();
                DoRemoveVehicles();
            } else if (index == (scenariosCount + 5)) {
                ClearPedPool();
                DoRemovePeds();
            }
        }

        private void StopScenario() {
            Log("Main.StopScenario()");
            if (scenario != null) {
                Log("scenario != null");
                scenario.Stop();
                scenario = null;
            }
        }

        private void RunScenario(int index) {
            Log("Main.RunScenario()");
            StopScenario();
            scenario = scenarios[index];
            scenario.Run();
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked) {
            int index = sender.MenuItems.IndexOf(checkboxItem);

            if (index == (scenariosCount + 2)) {
                renderPosition = Checked;
            } else if (index == (scenariosCount + 3)) {
                renderRay = Checked;
            }
        }

        bool ticked = false;
        private void MainTick(object sender, EventArgs e) {
            if (ticked == false) {
                ticked = true;
                try {
                    RunScenario(0);
                } catch {}
            }

            smartEye.DoTick();

            if (renderPosition) {
                Vector3 position = Game.Player.Character.Position;
                float heading = Game.Player.Character.Heading;

                positionMessage.Caption = String.Format(
                    "{0}, {1}, {2}, {3}",
                    position.X.ToString("0.0"),
                    position.Y.ToString("0.0"),
                    position.Z.ToString("0.0"),
                    heading.ToString("0.0")
                );

                positionMessage.Draw();
            } else if (positionMessage != null && positionMessage.Caption != "") {
                positionMessage.Caption = "";
                positionMessage.Draw();
            }

            SmartEye.WorldIntersection wi = smartEye.lastClosestWorldIntersection;
            ShowMessage(wi.ToString(), 15);

            Vector3 rc = wi.ObjectPoint;

            Vector2 rcp = new Vector2(rc.X, rc.Y);

            Vector2 p = new Vector2(
                rcp.X / UI.WIDTH, 
                rcp.Y / UI.HEIGHT
            ) * 2 - new Vector2(1, 1);

            Vector3 camPoint;
            Vector3 farPoint;
            Gta5EyeTracking.Geometry.ScreenRelToWorld(p, out camPoint, out farPoint);            
            
            Vector3 direction = farPoint - camPoint;

            RaycastResult ray = World.Raycast(
                source: camPoint,
                direction: direction,
                maxDistance: 200f,
                options: IntersectOptions.Everything,
                ignoreEntity: Game.Player.Character
            );

            lastRayHitEntity = ray.HitEntity;

            if (debugLevel >= 1)
            {
                ShowMessage("Frame: " + smartEye.lastFrameNumber, 2);
                ShowMessage("DitHitEntity: " + ray.DitHitEntity.ToString(), 3);
                ShowMessage("DitHitAnything: " + ray.DitHitAnything.ToString(), 4);
                ShowMessage("HitCoords: " + ray.HitCoords.ToString(), 5);
                ShowMessage("SurfaceNormal: " + ray.SurfaceNormal.ToString(), 6);

                if (debugLevel > 1)
                {
                    UIRectangle et = new UIRectangle(
                        new Point((int)rcp.X, (int)rcp.Y),
                        new Size(new Point(15, 15)),
                        Color.FromArgb(127, Color.Yellow)
                    );
                    et.Draw();

                    World.DrawMarker(
                        MarkerType.DebugSphere,
                        ray.HitCoords,
                        Vector3.Zero,
                        Vector3.Zero,
                        new Vector3(1, 1, 1),
                        Color.FromArgb(127, Color.White)
                    );
                }

                if (ray.HitEntity != null)
                {
                    ShowMessage("HitEntity: " + ray.HitEntity.Handle.ToString(), 8);

                    if (scenario != null)
                    {
                        Actor hitActor = scenario.FindActorByEntity(ray.HitEntity);

                        if (hitActor != null)
                        {
                            ShowMessage("HitActor: " + hitActor.Name, 9);
                        }
                        else
                        {
                            ShowMessage("HitActor: -", 9);
                        }
                    }
                }
                else
                {
                    ShowMessage("HitEntity: -", 8);
                }
            }

            menuPool.ProcessMenus();

            Util.DoTick();
            if (scenario != null) {
                scenario.DoTick();
            }
        }
    }
}