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
        private int scenarioIndex = 0;

        private UIMenu menu;
        private MenuPool menuPool = new MenuPool();

        private bool renderPosition = false;
        private bool renderRay = false;
        private UIText positionMessage;

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

                    scenarioSubMenu.OnItemSelect += (UIMenu sender, UIMenuItem item, int index) => {
                        if (index == 0) {
                            RunScenario(scenarioIndex - 1);
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
                menu.AddItem(new UIMenuItem("Stop running scenario", ""));

                var debugLevels = new List<dynamic> {
                    "None",
                    "Info",
                    "Verbose",
                    "Full"
                };

                UIMenuListItem debugLevelMenuItem = new UIMenuListItem("Debug level", debugLevels, 0);
                menu.AddItem(debugLevelMenuItem);

                menu.OnListChange += (sender, item, index) => {
                    if (item == debugLevelMenuItem) {
                        debugLevel = index;
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

                if (Game.IsScreenFadedOut) {
                    Game.FadeScreenIn(0);
                }
            }

            ~Main() {
                Log("Main.~Main()");
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
                    "new Location({0}f, {1}f, {2}f, {3}f)",
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
            }
        }

        public void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index) {
            scenarioIndex = index;

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
            Log("Main.RunScenario1()");
            Game.FadeScreenOut(100);
            Wait(100);
            StopScenario();
            scenario = scenarios[index];
            scenario.Run();
            Wait(100);
            Game.FadeScreenIn(100);
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked) {
            int index = sender.MenuItems.IndexOf(checkboxItem);

            if (index == (scenariosCount + 2)) {
                renderPosition = Checked;
            } else if (index == (scenariosCount + 3)) {
                renderRay = Checked;
            }
        }

        private void MainTick(object sender, EventArgs e) {
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

            if (renderRay) {
                RaycastResult ray = World.RaycastCapsule(GameplayCamera.Position,
                    GameplayCamera.Position + RotToDir(GameplayCamera.Rotation) * 1000.0f,
                    20.0f,
                    IntersectOptions.Everything);

                if (ray.HitEntity != null) {
                    Ped ped = new Ped(ray.HitEntity.Handle);

                    int uiWidth = (int) System.Math.Round((double) UI.WIDTH);
                    int uiHeight = (int) System.Math.Round((double) UI.HEIGHT);
                    int containerWidth = 400;
                    int containerHeight = 110;

                    UIContainer container = new UIContainer(
                        new Point(uiWidth - containerWidth, uiHeight - containerHeight),
                        new Size(new Point(containerWidth, containerHeight))
                    );

                    container.Items.Add(new UIText("Handle: " + ped.Handle.ToString(), new Point(20, 12), 0.3f, Color.Yellow));
                    container.Items.Add(new UIText("Hash: " + ped.Model.GetHashCode().ToString(), new Point(20, 24), 0.3f, Color.Yellow));
                    container.Items.Add(new UIText("Heading: " + ped.Heading.ToString(), new Point(20, 36), 0.3f, Color.Yellow));
                    container.Items.Add(new UIText("Rotation: " + ped.Rotation.ToString(), new Point(20, 48), 0.3f, Color.Yellow));
                    container.Items.Add(new UIText("Position: " + ped.Position.ToString(), new Point(20, 60), 0.3f, Color.Yellow));
                    container.Items.Add(new UIText("Velocity: " + ped.Velocity.ToString(), new Point(20, 72), 0.3f, Color.Yellow));
                    container.Items.Add(new UIText("Dimentions: " + ped.Model.GetDimensions().ToString(), new Point(20, 84), 0.3f, Color.Yellow));

                    container.Draw();
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