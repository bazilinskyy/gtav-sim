using static BepMod.Util;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BepMod
{
    class Scenario1 : Scenario {
        public Scenario1() {
            points = new Vector3[] {
                new Vector3(-1054.6f, -1680.5f, 4.1f),
                    new Vector3(-1040.4f, -1663.6f, 4.1f),
                    new Vector3(-1019.4f, -1641.4f, 4.1f),
                    new Vector3(-1009.7f, -1575.9f, 4.7f),
                    new Vector3(-1038.8f, -1532.3f, 4.7f),
                    new Vector3(-1092.9f, -1545.7f, 4.0f),
                    new Vector3(-1139.9f, -1500.8f, 4.0f),
                    new Vector3(-1154.5f, -1479.0f, 3.9f),
                    new Vector3(-1178.5f, -1443.3f, 3.9f),
                    new Vector3(-1175.3f, -1426.8f, 4.1f),
                    new Vector3(-1144.7f, -1400.7f, 4.8f),
                    new Vector3(-1093.6f, -1378.0f, 4.8f),
                    new Vector3(-1057.7f, -1320.7f, 5.1f),
                    new Vector3(-993.0f, -1260.9f, 5.3f)
            };

            startPosition = points[0];
            startHeading = 315.0f;
        }

        public override void PostRun() {
            Log("Scenario1.PostRun()");

            // Event 1
            Participant p1 = new Participant(
                location: new Location(-1076.1f, -1599.3f, 4.0f, 210.0f),
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 1"
            );

            participants.Add(p1);


            Trigger t1 = AddTrigger(points[1], radius: 7.5f, name: "Trigger 1");

            t1.TriggerEnter += (sender, index, e) => {
                if (debugLevel > 0) {
                    UI.Notify("Entered trigger 1");
                }

                p1.vehicle.Speed = 25.0f;
                p1.ped.Task.DriveTo(
                    p1.vehicle,
                    points[points.Length - 1],
                    5.0f,
                    25.0f,
                    DrivingStyle.AvoidTrafficExtremely.GetHashCode()
                );
            };

            t1.TriggerExit += (sender, index, e) => {
                if (debugLevel > 0) {
                    UI.Notify("Exited trigger 1");
                }
            };

            triggers.Add(t1);



            // Event 2
            Vehicle[] parked2 = {
                AddParkedVehicle(new Location(-1009.4f, -1639.4f, 4.0f, 63.4f)),
                AddParkedVehicle(new Location(-1014.5f, -1645.5f, 3.9f, 29.7f)),
                AddParkedVehicle(new Location(-1011.4f, -1643.2f, 3.9f, 50.6f)),
                AddParkedVehicle(new Location(-1006.1f, -1633.0f, 4.1f, 242.2f))
            };

            Participant p2 = AddParticipant(
                location: new Location(-1007.5f, -1636.3f, 4.0f, 58.1f),
                radius: 7.5f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 2"
            );

            p2.ParticipantInsideRadius += (sender, e) => {
                UI.Notify("In range of participant 2");
                p2.vehicle.Speed = 4.0f;
                p2.ped.Task.DriveTo(
                    p2.vehicle,
                    points[points.Length - 1],
                    5.0f,
                    25.0f,
                    DrivingStyle.Rushed.GetHashCode()
                );
            };


            UI.ShowSubtitle("Scenario 1", 2500);
        }
    }
}