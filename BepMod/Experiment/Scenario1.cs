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
                new Vector3(-1007.7f, -1267.7f, 5.6f)
            };

            // VehicleHash = VehicleHash.Airbus;
            //VehicleHash = VehicleHash.Tyrus;
            //VehicleHash = VehicleHash.CarbonRS;

            StartPosition = Points[0]; StartHeading = 315.0f;
            //StartPosition = Points[1]; StartHeading = 315.0f;
            //StartPosition = Points[2]; StartHeading = 322.0f;
            //StartPosition = Points[3]; StartHeading = 34.9f;
            //StartPosition = new Vector3(-1004.3f, -1608.0f, 4.7f); StartHeading = 0.0f;
            //StartPosition = Points[4]; StartHeading = 35.0f;
            //StartPosition = Points[5]; StartHeading = 30.0f;
            //StartPosition = new Vector3(-1117.6f, -1532.5f, 3.9f); StartHeading = 30.0f;
            //StartPosition = Points[6]; StartHeading = 30.0f;
            //StartPosition = Points[7]; StartHeading = 30.0f;
            //StartPosition = Points[8]; StartHeading = 30.0f;
            //StartPosition = Points[9]; StartHeading = 300.0f;
            //StartPosition = Points[10]; StartHeading = 300.0f;
            //StartPosition = new Vector3(-1089.3f, -1378.6f, 4.7f); StartHeading = 250f;
            //StartPosition = new Vector3(-1110.1f, -1382.3f, 4.8f); StartHeading = 290.2f;
            //StartPosition = Points[11]; StartHeading = 300.0f;
            //StartPosition = Points[12]; StartHeading = 330.0f;
            //StartPosition = new Vector3(-1064.8f, -1338.2f, 4.9f); StartHeading = 340.0f;
            //StartPosition = new Vector3(-1059.9f, -1324.3f, 5.1f); StartHeading = 340.0f;
        }

        public override void Initialise()
        {
            Log("Scenario1.Initialise()");

            trafficLightsColor = TrafficLightColor.Green;
            vehicle.MaxSpeed = 7.5f;

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
                        target: new Vector3(-1021.1f, -1454.7f, 4.5f),
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
                        new Vector3(-1029.2f, -1457.9f, 4.5f),
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
                name: "3_1",
                enter: t =>
                {
                    // drive to t-junction line
                    a3.ped.Task.DriveTo(
                        a3.vehicle,
                        new Vector3(-1009.7f, -1554.6f, 4.7f),
                        5.0f,
                        10.0f
                    );
                }
            );

            Trigger t3_2 = CreateTrigger(
                new Vector3(-1004.3f, -1608.0f, 4.7f),
                radius: 7.5f,
                name: "3_2",
                enter: t =>
                {
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a3);

                    UI.ShowSubtitle("Ga hier rechtdoor");
                    gps.SoundStraight.Play();

                    // actor 3 turn in front of participant
                    a3.DriveTo(
                        Points[1],
                        10.0f,
                        drivingstyle: DrivingStyle.Rushed,
                        destinationReached: () => a3.Remove()
                    );
                }
            );
            #endregion

            #region Event 4
            Actor a4 = CreateActor(
                position: new Vector3(-1089.2f, -1470.0f, 4.7f),
                heading: 207.8f,
                pedHash: PedHash.Tanisha,
                vehicleHash: VehicleHash.CarbonRS,
                name: "4"
            );

            CreateTrigger(
                new Vector3(-1020.3f, -1557.6f, 4.8f),
                radius: 7.5f,
                name: "4_1",
                enter: t =>
                {
                    // drive to t-junction
                    a4.ped.Task.DriveTo(
                        a4.vehicle,
                        new Vector3(-1071.8f, -1496.2f, 4.6f),
                        radius: 5.0f,
                        speed: 12.5f
                    );
                }
            );

            CreateTrigger(
                Points[4],
                radius: 7.5f,
                name: "4_2",
                enter: t =>
                {
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a4);

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
                }
            );
            #endregion

            #region Event 5
            Actor a5 = CreateActor(
                position: new Vector3(-1091.9f, -1557.8f, 3.9f),
                heading: 23.0f,
                pedHash: PedHash.Brad,
                name: "5"
            );

            CreateTrigger(
                new Vector3(-1069.1f, -1530.1f, 4.5f),
                radius: 7.5f,
                name: "5_1",
                enter: t =>
                {
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a5);

                    a5.ped.Task.FollowPointRoute(
                        new Vector3(-1104.9f, -1534.6f, 4.0f),
                        new Vector3(-1108.4f, -1527.8f, 6.4f)
                    );

                    trafficLightsColor = TrafficLightColor.Green;
                }
            );

            CreateTrigger(
                Points[5],
                radius: 7.5f,
                name: "5_2",
                enter: t =>
                {
                    UI.ShowSubtitle("Ga hier rechtsaf", 3000);
                    gps.SoundRight.Play();
                }
            );
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
                position: new Vector3(-1169.7f, -1469.9f, 4.4f),
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

            // car from right
            CreateTrigger(
                new Vector3(-1136.4f, -1505.7f, 4.0f),
                radius: 7.5f,
                name: "6_2",
                enter: t =>
                {
                    m6.Start(t).StopOnGazeAtActor(a6_2);

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
                    m6.Stop(_participantRanRedLight ? "1" : "0", "RAN_RED_LIGHT");
                }
            );

            // bus turns across intersection
            CreateTrigger(
                new Vector3(-1161.0f, -1510.2f, 4.0f),
                radius: 7.5f,
                name: "6_3",
                entity: a6_1.vehicle,
                enter: t =>
                {
                    SetTimeout(() => {
                        UI.ShowSubtitle("Ga hier rechtdoor", 3000);
                        gps.SoundStraight.Play();

                        trafficLightsColor = TrafficLightColor.Green;
                    }, 6000);

                    a6_2.DriveTo(
                        target: new Vector3(-1073.9f, -1476.2f, 4.6f),
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
                enter: t7_1 =>
                {
                    a7_2.ped.Task.DriveTo(
                        a7_2.vehicle,
                        target: Points[5],
                        radius: 5.0f,
                        speed: 5.0f
                    );
                }
            );

            // open door
            CreateTrigger(
                new Vector3(-1168.0f, -1460.1f, 4.4f),
                radius: 7.5f,
                name: "7_2",
                enter: t => {
                    trafficLightsColor = TrafficLightColor.Yellow;
                    SetTimeout(() => trafficLightsColor = TrafficLightColor.Red, 3000);

                    _logger.CreateMeasurement().Start(t).StopOnGazeAtActor(a7_2);

                    a7_1.vehicle.OpenDoor(
                        VehicleDoor.FrontLeftDoor,
                        false,
                        false
                    );
                }
            );
            #endregion

            #region Event 8
            Actor a8_1 = CreateActor(
                new Vector3(-1205.8f, -1448.1f, 3.9f),
                heading: 304.8f,
                radius: 5.0f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Prairie,
                name: "8_1"
            );

            Logger.Measurement m8 = _logger.CreateMeasurement();

            CreateTrigger(
                Points[8],
                radius: 7.5f,
                name: "8_1",
                enter: t =>
                {
                    trafficLightsColor = TrafficLightColor.Red;
                    m8.Start(t);

                    gps.SoundRight.Play();
                    UI.ShowSubtitle("Ga hier rechtsaf", 3000);

                    a8_1.vehicle.Speed = 5.0f;
                    a8_1.DriveTo(
                        target: new Vector3(-1086.4f, -1473.5f, 4.6f),
                        speed: 10.0f,
                        drivingstyle: DrivingStyle.IgnoreLights,
                        destinationReached: () => a8_1.Remove()
                    );
                },
                exit: t => {
                    CheckRedLight();
                    m8.Stop(_participantRanRedLight ? "1" : "0", "RAN_RED_LIGHT");
                }
            );

            CreateTrigger(
                new Vector3(-1166.2f, -1415.6f, 4.3f),
                name: "8_2",
                entity: a8_1.vehicle,
                enter: t => {
                    trafficLightsColor = TrafficLightColor.Green;
                }
            );
            #endregion

            #region Event 9
            Actor a9 = CreateActor(
                new Vector3(-1103.5f, -1441.3f, 4.7f), 29.7f,
                pedHash: PedHash.Beverly,
                vehicleHash: VehicleHash.Zentorno,
                name: "9"
            );

            CreateTrigger(
                new Vector3(-1154.9f, -1408.6f, 4.6f),
                name: "9_1",
                enter: t =>
                {
                    trafficLightsColor = TrafficLightColor.Yellow;
                    SetTimeout(() => trafficLightsColor = TrafficLightColor.Red, 3000);
                }
            );

            Logger.Measurement m9 = _logger.CreateMeasurement();

            CreateTrigger(
                new Vector3(-1149.5f, -1402.8f, 5.1f),
                radius: 7.5f,
                name: "9_2",
                enter: t =>
                {
                    SetTimeout(() =>
                    {
                        _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a9);

                        m9.Start(t);
                        a9.vehicle.Speed = 50.0f;
                        a9.DriveTo(
                            new Vector3(-1180.9f, -1206.3f, 6.0f),
                            80.0f,
                            drivingstyle: DrivingStyle.Rushed,
                            destinationReached: () => a9.Remove()
                        );
                    }, 1500);

                    trafficLightsColor = TrafficLightColor.Red;
                },
                exit: t => {
                    CheckRedLight();
                    m9.Stop(_participantRanRedLight ? "1" : "0", "RAN_RED_LIGHT");
                }
            );

            CreateTrigger(
                new Vector3(-1140.9f, -1375.7f, 4.7f),
                name: "9_3",
                entity: a9.vehicle,
                enter: t =>
                {
                    SetTimeout(() => trafficLightsColor = TrafficLightColor.Green, 2000);
                }
            );
            #endregion

            #region Event 10
            // actor from right
            Actor a10_1 = CreateActor(
                new Vector3(-1088.7f, -1442.6f, 4.7f), 357.4f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Verlierer2,
                name: "10_1"
            );

            // actor from left
            Actor a10_2 = CreateActor(
                new Vector3(-1094.0f, -1317.4f, 4.9f), 295.1f,
                pedHash: PedHash.Brad,
                vehicleHash: VehicleHash.Double,
                name: "10_2"
            );

            Logger.Measurement m10 = _logger.CreateMeasurement();

            CreateTrigger(
                new Vector3(-1110.1f, -1382.3f, 4.8f),
                radius: 7.5f,
                name: "10_1",
                enter: t =>
                {
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a10_1);
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a10_2);

                    trafficLightsColor = TrafficLightColor.Green;

                    a10_1.DriveTo(Points[Points.Length - 1], 10.0f,
                        destinationReached: () => a10_1.Remove());
                    a10_2.DriveTo(new Vector3(-1088.7f, -1442.6f, 4.7f), 35.0f);
                }
            );

            CreateTrigger(
                Points[11],
                radius: 7.5f,
                name: "10_2",
                enter: t =>
                {
                    m10.Start(t);

                    UI.ShowSubtitle("Ga hier linksaf", 3000);
                    gps.SoundLeft.Play();
                },
                exit: t => {
                    m10.Stop("WAITED", "0");
                }
            );

            CreateTrigger(
                new Vector3(-1078.8f, -1381.1f, 4.8f),
                name: "10_3",
                entity: a10_2.vehicle,
                exit: t =>
                {
                    trafficLightsColor = TrafficLightColor.Green;
                    m10.Stop("WAITED", "1");
                }
            );
            #endregion

            #region Event 11
            // vehicle evading police
            Actor a11_1 = CreateActor(
                new Vector3(-1172.2f, -1351.9f, 4.6f), 295.2f,
                pedHash: PedHash.Barry,
                vehicleHash: VehicleHash.Vacca,
                name: "11_1"
            );

            // chasing cop car
            Actor a11_2 = CreateActor(
                new Vector3(-1199.9f, -1364.8f, 4.1f), 292.7f,
                pedHash: PedHash.Barry,
                vehicleHash: VehicleHash.Police,
                name: "11_2"
            );

            CreateTrigger(
                new Vector3(-1064.8f, -1338.2f, 4.9f),
                radius: 7.5f,
                name: "11_1",
                enter: t =>
                {
                    trafficLightsColor = TrafficLightColor.Yellow;
                    SetTimeout(() => trafficLightsColor = TrafficLightColor.Red, 3000);
                }
            );

            Logger.Measurement m11 = _logger.CreateMeasurement();

            CreateTrigger(
                new Vector3(-1059.9f, -1324.3f, 5.1f),
                radius: 7.5f,
                name: "11_2",
                enter: t =>
                {
                    m11.Start(t);
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a11_1);
                    _logger.CreateMeasurement(t).Start().StopOnGazeAtActor(a11_2);

                    a11_1.vehicle.Speed = 70.0f;
                    a11_1.DriveTo(
                        new Vector3(-992.4f, -1138.6f, 1.7f),
                        80.0f,
                        drivingstyle: DrivingStyle.Rushed
                    );

                    a11_2.vehicle.Speed = 70.0f;
                    a11_2.ped.Task.VehicleChase(a11_1.ped);
                    a11_2.vehicle.SirenActive = true;
                    a11_2.vehicle.SoundHorn(10000);

                    SetTimeout(() => trafficLightsColor = TrafficLightColor.Green, 9000);
                },
                exit: t => {
                    CheckRedLight();
                    m11.Stop(_participantRanRedLight ? "1" : "0", "RAN_RED_LIGHT");
                }
            );

            CreateTrigger(
                new Vector3(-1037.9f, -1287.6f, 5.7f),
                name: "11_3",
                enter: t =>
                {
                    UI.ShowSubtitle("Ga hier rechtdoor");
                    gps.SoundStraight.Play();
                }
            );
            #endregion

            #region Event Finish
            CreateTrigger(
                new Vector3(-1009.8f, -1266.0f, 5.7f),
                25.0f,
                "FINISH",
                enter: t =>
                {
                    gps.SoundUHaveArived.Play();

                    // stop loggers etc
                    UI.ShowSubtitle("Thank you for your time");

                    UI.Notify("Log files:");
                    UI.Notify(_logger.LogFileName);
                    UI.Notify(_logger.MeasurementFileName);

                    _logger.Stop();

                    SetTimeout(() => Game.FadeScreenOut(2500), 2500);
                }
            );
            #endregion


            UI.ShowSubtitle("Scenario 1", 2000);
        }
    }
}