using GTA.Math;

namespace BepMod {
    public class Location {
        public Vector3 position;
        public float heading;

        public Location(float x, float y, float z, float heading = 0.0f) {
            this.position = new Vector3(x, y, z);
            this.heading = heading;
        }

        public Location(Vector3 position, float heading = 0.0f) {
            this.position = position;
            this.heading = heading;
        }
    }
}
