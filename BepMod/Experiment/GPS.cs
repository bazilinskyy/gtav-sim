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
        public enum Sound
        {
            NUM_10,
            NUM_100,
            NUM_20,
            NUM_200,
            NUM_300,
            NUM_400,
            NUM_50,
            NUM_500,
            NUM_800,
            BING_BONG,
            CALCULATINROUTE,
            EXITFREEWAY,
            HIGHLIGHTEDROUTE,
            IN,
            JOINFREEWAY,
            KEEP,
            LEFT,
            RECALCULATING,
            RIGHT,
            STRAIGHTAHEAD,
            TONE,
            TURN,
            UHAVEARRIVED,
            UTURN,
            YARDS
        };

        public SoundPlayer SoundLeft = new SoundPlayer();
        public SoundPlayer SoundRight = new SoundPlayer();
        public SoundPlayer SoundStraight = new SoundPlayer();
        public SoundPlayer SoundUHaveArived = new SoundPlayer();

        public string audioBase = ".\\scripts\\GPS\\Audio\\snoop_dogg\\";

        public GPS()
        {
            SoundLeft.SoundLocation = audioBase + "LEFT.WAV";
            SoundLeft.Load();

            SoundRight.SoundLocation = audioBase + "RIGHT.WAV";
            SoundRight.Load();

            SoundStraight.SoundLocation = audioBase + "STRAIGHTAHEAD.WAV";
            SoundStraight.Load();

            SoundUHaveArived.SoundLocation = audioBase + "UHAVEARRIVED.WAV";
            SoundUHaveArived.Load();
        }
    }
}
