using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using GTA;
using GTA.Math;
using GTA.Native;

using NativeUI;

using BepMod.Experiment;
using static BepMod.Util;

namespace BepMod.Data
{
    public class Main : Script
    {
        private Scenario ActiveScenario;
        private List<Scenario> scenarios = new List<Scenario>();
        private int scenariosCount = 0;
        public Entity lastRayHitEntity;

        private UIMenu menu;
        private MenuPool menuPool = new MenuPool();

        private Logger _logger;
        private SmartEye _smartEye = new SmartEye();

        public Main()
        {
            Log("Main.Main()");

            _logger = new Logger(_smartEye);

            scenarios.Add(new Scenario1());
            // scenarios.Add(new Scenario2());

            menu = new UIMenu("BepMod", "");

            foreach (Scenario scenario in scenarios)
            {
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

                scenarioSubMenu.OnItemSelect += (UIMenu sender, UIMenuItem item, int index) =>
                {
                    Log("Dingen: " + index.ToString());
                    if (index == 0)
                    {
                        RunScenario(scenarioIndex);
                        menu.Visible = false;
                        scenarioSubMenu.Visible = false;
                    }
                };

                var pointsList = new List<dynamic> { };
                for (int i = 0; i < scenario.Points.Length; i++)
                {
                    pointsList.Add(String.Format("Point {0}", i));
                }

                UIMenuListItem pointMenuListItem = new UIMenuListItem("Teleport", pointsList, 0);
                scenarioSubMenu.AddItem(pointMenuListItem);
                scenarioSubMenu.OnItemSelect += (sender, item, index) =>
                {
                    if (index == 1)
                    {
                        Vector3 position = scenario.Points[pointIndex];
                        UI.Notify("Teleport to: " + position.ToString());
                        Game.Player.Character.Position = position;
                    }
                };

                scenarioSubMenu.OnListChange += (sender, item, index) =>
                {
                    if (item == pointMenuListItem)
                    {
                        pointIndex = index;
                    }
                };
            }
            Log("Loaded " + scenariosCount.ToString() + " scenario(s)");

            menu.AddItem(new UIMenuItem("Stop running scenario", ""));

            var debugLevels = new List<dynamic> {
                "None",
                "Gaze",
                "Info",
                "Verbose",
                "Full"
            };

            UIMenuListItem debugLevelMenuItem = new UIMenuListItem("Debug level", debugLevels, debugLevel);
            menu.AddItem(debugLevelMenuItem);

            menu.OnListChange += (sender, item, index) =>
            {
                if (item == debugLevelMenuItem)
                {
                    debugLevel = index;
                    ClearMessages();
                }
            };

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

            Game.Player.Character.IsInvincible = true;

            if (Game.IsScreenFadedOut)
            {
                Game.FadeScreenIn(0);
            }
        }

        private void MainAborted(object sender, EventArgs e)
        {
            Log("ABORTED");
            _logger.Stop();
            _smartEye.Stop();
        }

        ~Main()
        {
            Log("Main.~Main()");
            _smartEye.Stop();
            StopScenario();
            ClearVehiclePool();
            Log("Main.~Main(): shut down complete");
        }

        private void DoKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.B)
            {
                menu.Visible = !menu.Visible;
            }
            else if (e.KeyCode == Keys.I)
            {
                RunScenario(0);
            }
            else if (e.KeyCode == Keys.K)
            {
                StopScenario();
            }
            else if (e.KeyCode == Keys.N)
            {
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
            }
            else if (e.KeyCode == Keys.Z)
            {
                //Mayhem();
            }
            else if (e.KeyCode == Keys.L)
            {
                _smartEye.Stop();
            }
            else if (e.KeyCode == Keys.O)
            {
                _smartEye.Start();
            }
            else if (e.KeyCode == Keys.OemPeriod)
            {
                if (debugLevel >= 4)
                {
                    debugLevel = 0;
                }
                else
                {
                    debugLevel++;
                }
                ClearMessages();
            }
        }

        public void Mayhem()
        {
            Vector3 p = Game.Player.Character.Position;
            float h = Game.Player.Character.Heading;

            Vehicle v1 = World.CreateVehicle(VehicleHash.Maverick, p + new Vector3(0, 0, 60f), h);
            v1.Speed = 10f;

            vehiclePool.Add(v1.Handle);
        }

        public void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            Log("Menu_OnItemSelect (" + index + ") " + selectedItem.Text);

            if (index == scenariosCount)
            {
                StopScenario();
            }
        }

        private void StopScenario()
        {
            Log("Main.StopScenario()");
            if (ActiveScenario != null)
            {
                Log("scenario != null");
                ActiveScenario.Stop();
                ActiveScenario = null;
            }
        }

        private void RunScenario(int index)
        {
            Log("Main.RunScenario()");
            Game.FadeScreenOut(0);
            StopScenario();
            ActiveScenario = scenarios[index];
            ActiveScenario.Run(_logger, _smartEye);
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            int index = sender.MenuItems.IndexOf(checkboxItem);
        }

        bool ticked = false;

        private void MainTick(object sender, EventArgs e)
        {
            menuPool.ProcessMenus();

            if (ticked == false)
            {
                ticked = true;
                try
                {
                    _smartEye.Start();
                    RunScenario(0);
                }
                catch { }
            }

            if (ActiveScenario != null)
            {
                ActiveScenario.DoTick();
            }

            _smartEye.DoTick();
            _logger.DoTick();
            Util.DoTick();

            if (debugLevel > 1)
            {
                RenderCircleOnGround(
                    Game.Player.Character.Position,
                    5.0f,
                    System.Drawing.Color.FromArgb(90, System.Drawing.Color.Purple)
                );

                RenderCircleOnGround(
                    Game.Player.Character.Position,
                    7.5f,
                    System.Drawing.Color.FromArgb(60, System.Drawing.Color.Purple)
                );

                RenderCircleOnGround(
                    Game.Player.Character.Position,
                    10.0f,
                    System.Drawing.Color.FromArgb(40, System.Drawing.Color.Purple)
                );
            }
        }
    }
}