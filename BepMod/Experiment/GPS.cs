using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Media;

namespace BepMod.Experiment
{
    class GPS
    {
        public SoundPlayer SoundLeft = new SoundPlayer();
        public SoundPlayer SoundRight = new SoundPlayer();
        public SoundPlayer SoundStraight = new SoundPlayer();
        public string audioBase = ".\\scripts\\GPS\\Audio\\snoop_dogg\\";

        public GPS()
        {

            SoundLeft.SoundLocation = audioBase + "LEFT.WAV";
            SoundLeft.Load();

            SoundRight.SoundLocation = audioBase + "RIGHT.WAV";
            SoundRight.Load();

            SoundStraight.SoundLocation = audioBase + "STRAIGHTAHEAD.WAV";
            SoundStraight.Load();

        }
    }
}
