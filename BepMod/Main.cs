using GTA;

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
            if (e.KeyCode == System.Windows.Forms.Keys.J)
            {
                UI.Notify("Hello, world!");
            }
        }
    }
}
