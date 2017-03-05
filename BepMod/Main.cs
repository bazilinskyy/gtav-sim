using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using System.Collections.Generic;

namespace BepMod
{
    public class Main : Script
    {
        private HashSet<int> _vehiclePool = new HashSet<int>();
        private bool _removeVehicles = false;
        private UIMenu _menu;
        private InstructionalButton _button;
        private MenuPool _menuPool = new MenuPool();
        private Vehicle _vehicle;
        private bool _hudHidden = false;

        public Main()
        {
            _menu = new UIMenu("BepMod", "~b~Bicycle Experiment Program");

            _menu.AddItem(new UIMenuItem("Spawn bicycle", "You know you want it!"));
            _menu.AddItem(new UIMenuItem("Load scenario", "Example scenario"));
            _menu.AddItem(new UIMenuCheckboxItem("Remove traffic", false, "All except for your bike"));
            _menu.AddItem(new UIMenuCheckboxItem("Hide radar and HUD", false, "For the full immersive experience"));
            _menu.RefreshIndex();

            _menu.OnItemSelect += _menu_OnItemSelect;
            _menu.OnCheckboxChange += _menu_OnCheckboxChange;

            _menuPool.Add(_menu);

            UI.Notify("For the Bicycle Experiment Program press ~b~B");

            _button = new InstructionalButton("B", "Bicycle Experiment Program");
            _menu.AddInstructionalButton(_button);

            KeyDown += Main_KeyDown;
            Tick += Main_Tick;
        }
        
        public void _menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (index == 0)
            {
                Vector3 position = Game.Player.Character.Position;
                float heading = Game.Player.Character.Heading;

                _vehicle = World.CreateVehicle(VehicleHash.Cruiser, position, heading);

                _vehiclePool.Add(_vehicle.Handle);
                _vehicle.PlaceOnGround();

                Game.Player.Character.SetIntoVehicle(_vehicle, VehicleSeat.Driver);

                UI.Notify("Drive safely!");
            }
            else if (index == 1)
            {
                Game.FadeScreenOut(2);

                Vector3 position = new Vector3(190f, -1019f, 29f);
                float heading = 70f;

                _vehiclePool.Clear();

                GameplayCamera.RelativeHeading = heading;
                _vehicle = World.CreateVehicle(VehicleHash.Cruiser, position, heading);

                _vehiclePool.Add(_vehicle.Handle);
                _vehicle.PlaceOnGround();

                Game.Player.Character.SetIntoVehicle(_vehicle, VehicleSeat.Driver);
                
                UI.Notify("Scenario loaded");
                _menu.Visible = false;
            }
        }
        
        private void _menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            int index = sender.MenuItems.IndexOf(checkboxItem);
            if (index == 2)
            {
                _removeVehicles = Checked;
                UI.Notify("Remove traffic: ~b~" + _removeVehicles.ToString());
            }
            else if (index == 3)
            {
                _hudHidden = Checked;
                UI.Notify("Hud hidden: ~b~" + _hudHidden.ToString());
            }
        }

        private void doRemoveVehicles()
        {
            foreach (Vehicle vehicle in World.GetAllVehicles())
            {
                if (!_vehiclePool.Contains(vehicle.Handle))
                {
                    vehicle.Delete();
                }
            }
        }

        private void Main_Tick(object sender, System.EventArgs e)
        {
            _menuPool.ProcessMenus();

            if (_removeVehicles)
            {
                doRemoveVehicles();
            }

            if (_hudHidden)
            {
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME, true);
            }
        }

        private void Main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.B)
            {
                _menu.Visible = !_menu.Visible;
            }
        }
    }
}
