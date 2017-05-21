using GTA;
using GTA.Math;
using GTA.Native;

using static BepMod.Util;

namespace BepMod.Experiment
{
    class Scenario1 : Scenario
    {
        public Scenario1()
        {
            Name = "SCENARIO_1";

            Points = new Vector3[] {
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

            StartPosition = Points[0]; StartHeading = 315.0f;
            //StartPosition = Points[2]; StartHeading = 270.0f;
            //StartPosition = Points[5]; StartHeading = 30.0f;
            //StartPosition = new Vector3(-1117.6f, -1532.5f, 3.9f); StartHeading = 30.0f;
            //StartPosition = Points[6]; StartHeading = 30.0f;
            //StartPosition = Points[7]; StartHeading = 30.0f;
            //StartPosition = Points[10]; StartHeading = 300.0f;
        }

        public override void PostRun()
        {
            Log("Scenario1.PostRun()");

            Actor p1 = AddActor(
                position: new Vector3(-1076.1f, -1599.3f, 4.0f),
                heading: 210.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "1"
            );

            Actor p2 = AddActor(
                position: new Vector3(-1007.5f, -1636.3f, 4.0f),
                heading: 58.1f,
                radius: 7.5f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "2"
            );

            Actor p3 = AddActor(
                position: new Vector3(-983.4f, -1537.3f, 4.6f),
                heading: 112.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "3"
            );

            Actor p4 = AddActor(
                position: new Vector3(-1065.6f, -1471.1f, 4.6f),
                heading: 120.2f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "4"
            );

            Actor p5 = AddActor(
                position: new Vector3(-1091.9f, -1557.8f, 3.9f),
                heading: 23.0f,
                pedHash: PedHash.Brad,
                name: "5"
            );

            Actor p61 = AddActor(
                position: new Vector3(-1116.7f, -1468.5f, 4.5f),
                heading: 127.5f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "6_1"
            );
            Actor p62 = AddActor(
                position: new Vector3(-1181.5f, -1449.4f, 3.9f),
                heading: 210.7f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "6_2"
            );


            // Event 1
            Trigger t1 = AddTrigger(
                Points[1],
                radius: 7.5f,
                name: "1"
            );

            t1.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtdoor");
                gpsSoundStraight.Play();

                p1.vehicle.Speed = 25.0f;
                p1.ped.Task.DriveTo(
                    p1.vehicle,
                    Points[Points.Length - 1],
                    5.0f,
                    25.0f,
                    (int)DrivingStyle.AvoidTrafficExtremely
                );
            };



            // Event 2
            Vehicle[] parked2 = {
                AddParkedVehicle(new Vector3(-1009.4f, -1639.4f, 4.0f), 63.4f),
                AddParkedVehicle(new Vector3(-1014.5f, -1645.5f, 3.9f), 29.7f),
                AddParkedVehicle(new Vector3(-1011.4f, -1643.2f, 3.9f), 50.6f),
                AddParkedVehicle(new Vector3(-1006.1f, -1633.0f, 4.1f), 242.2f)
            };

            Trigger t2 = AddTrigger(
                new Vector3(-1014.4f, -1633.5f, 4.2f),
                radius: 7.5f,
                name: "2"
            );

            t2.TriggerEnter += (sender, index, e) =>
            {
                p2.vehicle.Speed = 4.0f;
                p2.ped.Task.DriveTo(
                    p2.vehicle,
                    Points[Points.Length - 1],
                    5.0f,
                    25.0f,
                    (int)DrivingStyle.Rushed
                );
            };



            // Event 3
            Trigger t3 = AddTrigger(
                Points[3],
                radius: 7.5f,
                name: "3"
            );

            t3.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtdoor");
                gpsSoundStraight.Play();

                p3.vehicle.Speed = 15.0f;
                p3.ped.Task.DriveTo(
                    p3.vehicle,
                    Points[0],
                    5.0f,
                    20.0f,
                    (int)DrivingStyle.Rushed
                );

                p4.ped.Task.DriveTo(
                    p4.vehicle,
                    Points[0],
                    radius: 5.0f,
                    speed: 7.5f,
                    drivingstyle: (int)DrivingStyle.Rushed
                );
            };



            // Event 4
            Trigger t4 = AddTrigger(
                Points[4],
                radius: 7.5f,
                name: "4"
            );

            t4.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier linksaf", 3000);
                gpsSoundLeft.Play();

                p4.ped.Task.DriveTo(
                    p4.vehicle,
                    Points[0],
                    radius: 5.0f,
                    speed: p4.distance / 2,
                    drivingstyle: (int)DrivingStyle.Rushed
                );
            };



            // Event 5
            Trigger t51 = AddTrigger(
                new Vector3(-1069.1f, -1530.1f, 4.5f),
                radius: 7.5f,
                name: "5_1"
            );

            Trigger t52 = AddTrigger(
                Points[5],
                radius: 7.5f,
                name: "5_2"
            );

            t51.TriggerEnter += (sender, index, e) =>
            {
                p5.ped.Task.FollowPointRoute(
                    new Vector3(-1104.9f, -1534.6f, 4.0f),
                    new Vector3(-1108.4f, -1527.8f, 6.4f)
                );

                trafficLightsColor = TrafficLightColor.GREEN;
            };

            t52.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                gpsSoundRight.Play();
            };



            // Event 6
            Trigger t61 = AddTrigger(
                new Vector3(-1117.6f, -1532.5f, 3.9f),
                radius: 7.5f,
                name: "6_1"
            );

            Trigger t62 = AddTrigger(
                Points[6],
                radius: 7.5f,
                name: "6_2"
            );

            t61.TriggerEnter += (sender, index, e) =>
            {
                trafficLightsColor = TrafficLightColor.YELLOW;

                p62.ped.Task.DriveTo(
                    p62.vehicle,
                    target: new Vector3(-1157.5f, -1484.1f, 4.0f),
                    radius: 1.0f,
                    speed: 5.0f,
                    drivingstyle: (int)DrivingStyle.Normal
                );
            };

            t61.TriggerExit += (sender, index, e) =>
            {
                trafficLightsColor = TrafficLightColor.RED;
            };

            t62.TriggerEnter += (sender, index, e) =>
            {
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
                    target: Points[Points.Length - 1],
                    radius: 1.0f,
                    speed: 7.5f,
                    drivingstyle: (int)DrivingStyle.IgnoreLights
                );
            };



            // Event 7
            Actor p71 = AddActor(
                new Vector3(-1166.0f, -1458.9f, 3.8f),
                heading: 33.3f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Actor p72 = AddActor(
                new Vector3(-1176.7f, -1441.2f, 3.8f),
                heading: 215.2f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Trigger t71 = AddTrigger(
                Points[7],
                radius: 7.5f,
                name: "7_1"
            );


            p71.ActorInsideRadius += (sender, e) =>
            {
                p71.vehicle.OpenDoor(
                    VehicleDoor.FrontLeftDoor,
                    false,
                    false
                );
            };


            t71.TriggerEnter += (sender, index, e) =>
            {
                p72.ped.Task.DriveTo(
                    p72.vehicle,
                    target: Points[5],
                    radius: 5.0f,
                    speed: 5.0f
                );
            };



            // Event 8
            Actor p81 = AddActor(
                new Vector3(-1205.8f, -1448.1f, 3.9f),
                heading: 304.8f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Trigger t81 = AddTrigger(
                Points[8],
                radius: 7.5f,
                name: "8_1"
            );

            t81.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                gpsSoundRight.Play();

                p81.vehicle.Speed = 5.0f;
                p81.ped.Task.DriveTo(
                    p81.vehicle,
                    target: Points[Points.Length - 1],
                    radius: 1.0f,
                    speed: 10.0f,
                    drivingstyle: (int)DrivingStyle.IgnoreLights
                );

                trafficLightsColor = TrafficLightColor.GREEN;
            };



            // Event 10
            Trigger t101 = AddTrigger(
                Points[10],
                radius: 7.5f,
                name: "10_1"
            );

            t101.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtdoor", 3000);
                gpsSoundStraight.Play();

                trafficLightsColor = TrafficLightColor.RED;
            };



            // Event 11
            Trigger t111 = AddTrigger(
                Points[11],
                radius: 7.5f,
                name: "11_1"
            );

            t111.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier linksaf", 3000);
                gpsSoundLeft.Play();

                trafficLightsColor = TrafficLightColor.RED;
            };



            UI.ShowSubtitle("Scenario 1", 2000);
        }
    }
}