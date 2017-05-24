using GTA;
using GTA.Math;
using GTA.Native;

using BepMod.Data;
using static BepMod.Util;

namespace BepMod.Experiment
{
    class Scenario1 : Scenario
    {
        public Scenario1()
        {
            Name = "1";

            Points = new Vector3[] {
                new Vector3(-1054.6f, -1680.5f, 4.1f),
                new Vector3(-1039.7f, -1664.3f, 4.1f),
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
            //StartPosition = Points[1]; StartHeading = 315.0f;
            //StartPosition = Points[2]; StartHeading = 322.0f;
            //StartPosition = Points[3]; StartHeading = 34.9f;
            //StartPosition = Points[4]; StartHeading = 273.0f;
            //StartPosition = Points[5]; StartHeading = 30.0f;
            // StartPosition = new Vector3(-1117.6f, -1532.5f, 3.9f); StartHeading = 30.0f;
            // StartPosition = Points[6]; StartHeading = 30.0f;
            //StartPosition = Points[7]; StartHeading = 30.0f;
            //StartPosition = Points[10]; StartHeading = 300.0f;
        }

        public override void Initialise()
        {
            Log("Scenario1.Initialise()");

            trafficLightsColor = TrafficLightColor.Green;
            
            #region Event 1
            Actor a1 = CreateActor(
                position: new Vector3(-1047.0f, -1641.7f, 4.4f),
                heading: 223.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Issi2,
                name: "1"
            );

            CreateTrigger(
                new Vector3(-1043.3f, -1668.2f, 4.1f),
                radius: 7.5f,
                name: "1",
                enter: t =>
                {
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a1);

                    UI.ShowSubtitle("Ga hier rechtdoor");
                    gps.SoundStraight.Play();

                    a1.vehicle.Speed = 5.0f;

                    a1.DriveTo(
                        target: Points[Points.Length - 1],
                        speed: 3.0f,
                        destinationRadius: 7.5f,
                        destinationReached: () => a1.Remove()
                    );
                }
            );
            #endregion

            #region Event 2
            Actor a2 = CreateActor(
                position: new Vector3(-1007.5f, -1636.3f, 4.0f),
                heading: 58.1f,
                radius: 7.5f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "2"
            );

            CreateParkedVehicle(new Vector3(-1009.4f, -1639.4f, 4.0f), 63.4f, VehicleHash.Camper);

            CreateTrigger(
                new Vector3(-1014.4f, -1633.5f, 4.2f),
                radius: 10.0f,
                name: "2",
                enter: t =>
                {
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a2);

                    a2.vehicle.Speed = 4.0f;
                    a2.DriveTo(
                        Points[Points.Length - 1],
                        drivingstyle: DrivingStyle.Rushed,
                        destinationReached: () => a2.Remove()
                    );
                }
            );
            #endregion

            #region Event 3
            Actor a3 = CreateActor(
                position: new Vector3(-997.1f, -1544.7f, 4.6f),
                heading: 123.9f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Bus,
                name: "3"
            );

            Trigger t3_1 = CreateTrigger(
                new Vector3(-1006.4f, -1620.1f, 4.5f),
                name: "3_1"
            );

            Trigger t3_2 = CreateTrigger(
                new Vector3(-1004.3f, -1608.0f, 4.7f),
                radius: 7.5f,
                name: "3_2"
            );

            t3_1.TriggerEnter += (sender, index, e) =>
            {
                // drive to t-junction line
                a3.ped.Task.DriveTo(
                    a3.vehicle,
                    new Vector3(-1009.7f, -1554.6f, 4.7f),
                    5.0f,
                    10.0f
                );
            };

            t3_2.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtdoor");
                gps.SoundStraight.Play();

                // actor 3 turn in front of participant
                a3.ped.Task.DriveTo(
                    a3.vehicle,
                    Points[1],
                    5.0f,
                    10.0f,
                    (int)DrivingStyle.Rushed
                );
            };
            #endregion

            #region Event 4
            Actor a4 = CreateActor(
                position: new Vector3(-1089.2f, -1470.0f, 4.7f),
                heading: 207.8f,
                pedHash: PedHash.Tanisha,
                vehicleHash: VehicleHash.CarbonRS,
                name: "4"
            );

            Trigger t4_1 = CreateTrigger(
                new Vector3(-1020.3f, -1557.6f, 4.8f),
                radius: 7.5f,
                name: "4_1"
            );

            Trigger t4_2 = CreateTrigger(
                Points[4],
                radius: 7.5f,
                name: "4_2"
            );

            t4_1.TriggerEnter += (sender, index, e) =>
            {
                // drive to t-junction
                a4.ped.Task.DriveTo(
                    a4.vehicle,
                    new Vector3(-1071.8f, -1496.2f, 4.6f),
                    radius: 5.0f,
                    speed: 12.5f
                );
            };

            t4_2.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier linksaf", 3000);
                gps.SoundLeft.Play();

                // cross the road
                a4.ped.Task.DriveTo(
                    a4.vehicle,
                    Points[0],
                    radius: 5.0f,
                    speed: a4.distance / 2,
                    drivingstyle: (int)DrivingStyle.Rushed
                );
            };
            #endregion

            #region Event 5
            Actor a5 = CreateActor(
                position: new Vector3(-1091.9f, -1557.8f, 3.9f),
                heading: 23.0f,
                pedHash: PedHash.Brad,
                name: "5"
            );

            Trigger t5_1 = CreateTrigger(
                new Vector3(-1069.1f, -1530.1f, 4.5f),
                radius: 7.5f,
                name: "5_1"
            );

            Trigger t5_2 = CreateTrigger(
                Points[5],
                radius: 7.5f,
                name: "5_2"
            );

            t5_1.TriggerEnter += (sender, index, e) =>
            {
                a5.ped.Task.FollowPointRoute(
                    new Vector3(-1104.9f, -1534.6f, 4.0f),
                    new Vector3(-1108.4f, -1527.8f, 6.4f)
                );

                trafficLightsColor = TrafficLightColor.Green;
            };

            t5_2.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                gps.SoundRight.Play();
            };
            #endregion

            #region Event 6
            // car from right
            Actor a6_1 = CreateActor(
                position: new Vector3(-1116.7f, -1468.5f, 4.5f),
                heading: 127.5f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.StingerGT,
                name: "6_1"
            );

            // oncoming bus at busstop
            Actor a6_2 = CreateActor(
                position: new Vector3(-1171.8f, -1467.0f, 4.3f),
                heading: 210.7f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Airbus,
                name: "6_2"
            );

            // traffic lights to red
            // drive oncoming bus to intersection
            CreateTrigger(
                new Vector3(-1117.6f, -1532.5f, 3.9f),
                radius: 7.5f,
                name: "6_1",
                enter: t =>
                {
                    trafficLightsColor = TrafficLightColor.Yellow;
                    SetTimeout(() => 
                    {
                        trafficLightsColor = TrafficLightColor.Red;

                        // drive bus to intersection
                        a6_2.DriveTo(
                            target: new Vector3(-1157.4f, -1484.9f, 4.4f),
                            speed: 5.0f
                        );
                    }, 3000);

                    a6_2.DriveTo(
                        target: new Vector3(-1157.5f, -1484.1f, 4.0f),
                        speed: 5.0f
                    );
                }
            );

            Logger.Measurement m6 = _logger.CreateMeasurement();

            CreateTrigger(
                Points[6],
                radius: 7.5f,
                name: "6_2",
                enter: t =>
                {
                    m6.Start(t);

                    UI.ShowSubtitle("Ga hier rechtdoor", 3000);
                    gps.SoundStraight.Play();

                    trafficLightsColor = TrafficLightColor.Red;

                    a6_1.DriveTo(
                        target: new Vector3(-1152.1f, -1525.0f, 3.9f),
                        speed: 10.0f,
                        drivingstyle: DrivingStyle.IgnoreLights,
                        destinationReached: () => a6_1.Remove()
                    );
                },
                exit: t => {
                    CheckRedLight();
                    m6.Stop("RAN_RED_LIGHT", _participantRanRedLight ? "1" : "0");
                }
            );

            CreateTrigger(
                new Vector3(-1161.0f, -1510.2f, 4.0f),
                radius: 7.5f,
                name: "6_3",
                entity: a6_1.vehicle,
                enter: t =>
                {
                    SetTimeout(() => trafficLightsColor = TrafficLightColor.Green, 3000);

                    a6_2.DriveTo(
                        target: new Vector3(-1089.3f, -1444.2f, 4.7f),
                        speed: 5.0f,
                        drivingstyle: DrivingStyle.IgnoreLights,
                        destinationReached: () => a6_2.Remove()
                    );
                }
            );
            #endregion
            
            #region Event 7
            Actor a7_1 = CreateActor(
                new Vector3(-1166.0f, -1458.9f, 3.8f),
                heading: 33.3f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "7_1"
            );

            Actor a7_2 = CreateActor(
                new Vector3(-1176.7f, -1441.2f, 3.8f),
                heading: 215.2f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "7_2"
            );

            // oncoming car
            CreateTrigger(
                Points[7],
                radius: 7.5f,
                name: "7_1",
                enter: t7_1 => a7_2.ped.Task.DriveTo(
                    a7_2.vehicle,
                    target: Points[5],
                    radius: 5.0f,
                    speed: 5.0f
                )
            );

            // open door
            CreateTrigger(
                new Vector3(-1168.0f, -1460.1f, 4.4f),
                radius: 7.5f,
                name: "7_2",
                enter: t7_2 => a7_1.vehicle.OpenDoor(
                    VehicleDoor.FrontLeftDoor,
                    false,
                    false
                )
            );
            #endregion
            
            #region Event 8
            Actor p81 = CreateActor(
                new Vector3(-1205.8f, -1448.1f, 3.9f),
                heading: 304.8f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie
            );

            Trigger t81 = CreateTrigger(
                Points[8],
                radius: 7.5f,
                name: "8_1"
            );

            t81.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                gps.SoundRight.Play();

                p81.vehicle.Speed = 5.0f;
                p81.ped.Task.DriveTo(
                    p81.vehicle,
                    target: Points[Points.Length - 1],
                    radius: 1.0f,
                    speed: 10.0f,
                    drivingstyle: (int)DrivingStyle.IgnoreLights
                );

                trafficLightsColor = TrafficLightColor.Green;
            };
            #endregion
            
            #region Event 9
            Trigger t9_1 = CreateTrigger(
                Points[10],
                radius: 7.5f,
                name: "9_1"
            );

            t9_1.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier rechtdoor", 3000);
                gps.SoundStraight.Play();

                trafficLightsColor = TrafficLightColor.Red;
            };
            #endregion
            
            #region Event 10
            Trigger t10_1 = CreateTrigger(
                Points[11],
                radius: 7.5f,
                name: "10_1"
            );

            t10_1.TriggerEnter += (sender, index, e) =>
            {
                UI.ShowSubtitle("Ga hier linksaf", 3000);
                gps.SoundLeft.Play();

                trafficLightsColor = TrafficLightColor.Red;
            };
            #endregion

            UI.ShowSubtitle("Scenario 1", 2000);
        }
    }
}