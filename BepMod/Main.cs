using GTA;
using GTA.Math;
using GTA.Native;

namespace BepMod
{
    public class Main : Script
    {
        public Main()
        {
            KeyDown += Main_KeyDown;
        }
        
        private void Main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.L)
            {
                UI.ShowSubtitle(
                    "Position: " + Game.Player.Character.Position.ToString() + 
                    ", heading: " + Game.Player.Character.Heading.ToString());
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.J)
            {
                // 5 meters in front of the player
                Vector3 position = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 5, 0));

                // At 90 degrees to the players heading
                float heading = Game.Player.Character.Heading - 90;

                Vehicle vehicle = World.CreateVehicle(VehicleHash.TriBike3, position, heading);
                
                vehicle.PlaceOnGround();

                UI.ShowSubtitle("Your bicycle has arrived!");
            }
        }
    }
}
