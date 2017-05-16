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
                    new Vector3(-1133.6f, -1510.5f, 4.0f),
                    new Vector3(-1154.5f, -1479.0f, 3.9f),
                    new Vector3(-1178.5f, -1443.3f, 3.9f),
                    new Vector3(-1175.3f, -1426.8f, 4.1f),
                    new Vector3(-1144.7f, -1400.7f, 4.8f),
                    new Vector3(-1093.6f, -1378.0f, 4.8f),
                    new Vector3(-1057.7f, -1320.7f, 5.1f),
                    new Vector3(-993.0f, -1260.9f, 5.3f)
            };

            startPosition = points[0]; startHeading = 315.0f;
            //startPosition = points[5]; startHeading = 30.0f;
            //startPosition = new Vector3(-1117.6f, -1532.5f, 3.9f); startHeading = 30.0f;
            //startPosition = points[6]; startHeading = 30.0f;
            //startPosition = points[7]; startHeading = 30.0f;
            // startPosition = points[10]; startHeading = 300.0f;
        }

        public override void PostRun() {
            Log("Scenario1.PostRun()");

            Participant p1 = AddParticipant(
                location: new Location(-1076.1f, -1599.3f, 4.0f, 210.0f),
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 1"
            );

            Participant p2 = AddParticipant(
                location: new Location(-1007.5f, -1636.3f, 4.0f, 58.1f),
                radius: 7.5f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 2"
            );

            Participant p3 = AddParticipant(
                location: new Location(-983.4f, -1537.3f, 4.6f, 112.0f),
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 3"
            );

            Participant p4 = AddParticipant(
                location: new Location(-1065.6f, -1471.1f, 4.6f, 120.2f),
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 4"
            );

            Participant p5 = AddParticipant(
                location: new Location(-1091.9f, -1557.8f, 3.9f, 23.0f),
                pedHash: PedHash.Brad,
                name: "Participant 5"
            );

            Participant p61 = AddParticipant(
                location: new Location(-1116.7f, -1468.5f, 4.5f, 127.5f),
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 6.1"
            );
            Participant p62 = AddParticipant(
                location: new Location(-1181.5f, -1449.4f, 3.9f, 210.7f),
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "Participant 6.2"
            );


            // Event 1
            Trigger t1 = AddTrigger(
                points[1], 
                radius: 7.5f, 
                name: "Trigger 1"
            );

            t1.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier rechtdoor");
                gpsSoundStraight.Play();

                p1.vehicle.Speed = 25.0f;
                p1.ped.Task.DriveTo(
                    p1.vehicle,
                    points[points.Length - 1],
                    5.0f,
                    25.0f,
                    (int)DrivingStyle.AvoidTrafficExtremely
                );
            };



            // Event 2
            Vehicle[] parked2 = {
                AddParkedVehicle(new Location(-1009.4f, -1639.4f, 4.0f, 63.4f)),
                AddParkedVehicle(new Location(-1014.5f, -1645.5f, 3.9f, 29.7f)),
                AddParkedVehicle(new Location(-1011.4f, -1643.2f, 3.9f, 50.6f)),
                AddParkedVehicle(new Location(-1006.1f, -1633.0f, 4.1f, 242.2f))
            };

            Trigger t2 = AddTrigger(
                new Vector3(-1014.4f, -1633.5f, 4.2f), 
                radius: 7.5f, 
                name: "Trigger 2"
            );

            t2.TriggerEnter += (sender, index, e) => {
                p2.vehicle.Speed = 4.0f;
                p2.ped.Task.DriveTo(
                    p2.vehicle,
                    points[points.Length - 1],
                    5.0f,
                    25.0f,
                    (int)DrivingStyle.Rushed
                );
            };



            // Event 3
            Trigger t3 = AddTrigger(
                points[3], 
                radius: 7.5f, 
                name: "Trigger 3"
            );

            t3.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier rechtdoor");
                gpsSoundStraight.Play();

                p3.vehicle.Speed = 15.0f;
                p3.ped.Task.DriveTo(
                    p3.vehicle,
                    points[0],
                    5.0f,
                    20.0f,
                    (int)DrivingStyle.Rushed
                );

                p4.ped.Task.DriveTo(
                    p4.vehicle,
                    points[0],
                    radius: 5.0f,
                    speed: 7.5f,
                    drivingstyle: (int)DrivingStyle.Rushed
                );
            };



            // Event 4
            Trigger t4 = AddTrigger(
                points[4],
                radius: 7.5f,
                name: "Trigger 4"
            );

            t4.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier linksaf", 3000);
                gpsSoundLeft.Play();

                p4.ped.Task.DriveTo(
                    p4.vehicle,
                    points[0],
                    radius: 5.0f,
                    speed: p4.distance / 2,
                    drivingstyle: (int)DrivingStyle.Rushed
                );
            };



            // Event 5
            Trigger t51 = AddTrigger(
                new Vector3(-1069.1f, -1530.1f, 4.5f),
                radius: 7.5f,
                name: "Trigger 5.1"
            );

            Trigger t52 = AddTrigger(
                points[5],
                radius: 7.5f,
                name: "Trigger 5.2"
            );

            t51.TriggerEnter += (sender, index, e) => {
                p5.ped.Task.FollowPointRoute(
                    new Vector3(-1104.9f, -1534.6f, 4.0f),
                    new Vector3(-1108.4f, -1527.8f, 6.4f)
                );

                trafficLightsColor = TrafficLightColor.GREEN;
            };

            t52.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                gpsSoundRight.Play();
            };



            // Event 6
            Trigger t61 = AddTrigger(
                new Vector3(-1117.6f, -1532.5f, 3.9f),
                radius: 7.5f,
                name: "Trigger 6.1"
            );

            Trigger t62 = AddTrigger(
                points[6],
                radius: 7.5f,
                name: "Trigger 6.2"
            );

            t61.TriggerEnter += (sender, index, e) => {
                trafficLightsColor = TrafficLightColor.YELLOW;

                p62.ped.Task.DriveTo(
                    p62.vehicle,
                    target: new Vector3(-1157.5f, -1484.1f, 4.0f),
                    radius: 1.0f,
                    speed: 5.0f,
                    drivingstyle: (int)DrivingStyle.Normal
                );
            };

            t61.TriggerExit += (sender, index, e) => {
                trafficLightsColor = TrafficLightColor.RED;
            };

            t62.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier rechtdoor", 3000);
                gpsSoundStraight.Play();

                p61.ped.Task.DriveTo(
                    p61.vehicle,
                    target: new Vector3(-1199.9f, -1543.2f, 4.0f),
                    radius: 1.0f,
                    speed: 10.0f,
                    drivingstyle: (int)DrivingStyle.IgnoreLights
                );

                p62.ped.Task.DriveTo(
                    p62.vehicle,
                    target: points[points.Length - 1],
                    radius: 1.0f,
                    speed: 7.5f,
                    drivingstyle: (int)DrivingStyle.IgnoreLights
                );
            };



            // Event 7
            Participant p71 = AddParticipant(
                new Location(-1166.0f, -1458.9f, 3.8f, 33.3f),
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Participant p72 = AddParticipant(
                new Location(-1176.7f, -1441.2f, 3.8f, 215.2f),
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Trigger t71 = AddTrigger(
                points[7],
                radius: 7.5f,
                name: "Trigger 7.1"
            );


            p71.ParticipantInsideRadius += (sender, e) => {
                p71.vehicle.OpenDoor(
                    VehicleDoor.FrontLeftDoor,
                    false,
                    false
                );
            };


            t71.TriggerEnter += (sender, index, e) => {
                p72.ped.Task.DriveTo(
                    p72.vehicle,
                    target: points[5],
                    radius: 5.0f,
                    speed: 5.0f
                );
            };



            // Event 8
            Participant p81 = AddParticipant(
                new Location(-1205.8f, -1448.1f, 3.9f, 304.8f),
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Trigger t81 = AddTrigger(
                points[8],
                radius: 7.5f,
                name: "Trigger 8.1"
            );

            t81.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                gpsSoundRight.Play();

                p81.vehicle.Speed = 5.0f;
                p81.ped.Task.DriveTo(
                    p81.vehicle,
                    target: points[points.Length - 1],
                    radius: 1.0f,
                    speed: 10.0f,
                    drivingstyle: (int)DrivingStyle.IgnoreLights
                );

                trafficLightsColor = TrafficLightColor.GREEN;
            };



            // Event 10
            Trigger t101 = AddTrigger(
                points[10],
                radius: 7.5f,
                name: "Trigger 10.1"
            );

            t101.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier rechtdoor", 3000);
                gpsSoundStraight.Play();

                trafficLightsColor = TrafficLightColor.RED;
            };



            // Event 11
            Trigger t111 = AddTrigger(
                points[11],
                radius: 7.5f,
                name: "Trigger 11.1"
            );

            t111.TriggerEnter += (sender, index, e) => {
                UI.ShowSubtitle("Ga hier linksaf", 3000);
                gpsSoundLeft.Play();

                trafficLightsColor = TrafficLightColor.RED;
            };


            //new Location(-1064.8f, -1339.7f, 4.9f, 345.7f)



            UI.ShowSubtitle("Scenario 1", 2000);
        }
    }
}