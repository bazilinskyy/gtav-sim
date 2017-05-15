using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;


namespace BepMod {
    enum TrafficLightColor { GREEN = 0, RED, YELLOW, AUTO }


    static class Util {
        public static int debugLevel = 3;

        public static String[] messages = new String[20];

        public static UIText[] messageHandles = new UIText[20];

        public static HashSet<int> vehiclePool = new HashSet<int>();
        public static HashSet<int> pedPool = new HashSet<int>();

        public static List<int> trafficSignalHashes = new List<int> {
            -655644382,
            862871082,
            1043035044
        };
        public static TrafficLightColor trafficLightsColor = TrafficLightColor.AUTO;

        public static System.Media.SoundPlayer gpsSoundLeft = new System.Media.SoundPlayer();
        public static System.Media.SoundPlayer gpsSoundRight = new System.Media.SoundPlayer();
        public static System.Media.SoundPlayer gpsSoundStraight = new System.Media.SoundPlayer();
        public static string audioBase = ".\\scripts\\GPS\\Audio\\snoop_dogg\\";

        static Util() { }

        public static void Log(string message, string name = "general") {
            using(StreamWriter w = File.AppendText(String.Format("bepmod_{0}.log", name))) {
                w.WriteLine("[{0}] {1}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    message
                );
            }
        }

        public static void DoTick() {
            ShowMessages();

            if (trafficLightsColor != TrafficLightColor.AUTO) {
                foreach (Prop prop in World.GetNearbyProps(Game.Player.Character.Position, 100.0f)) {
                    if (trafficSignalHashes.Contains(prop.Model.Hash)) {
                        Function.Call(Hash.SET_ENTITY_TRAFFICLIGHT_OVERRIDE, prop, (int)trafficLightsColor);
                    }
                }
            }
        }

        public static UIText GetMessageHandle(int index) {
            if (messageHandles[index] == null) {
                messageHandles[index] = new UIText("",
                    new Point((int) System.Math.Round((double) UI.WIDTH / 2), 60 + 20*index),
                    .5f,
                    Color.Yellow,
                    GTA.Font.ChaletComprimeCologne,
                    true
                );
            }
            return messageHandles[index];
        }

        public static void ShowMessage(string message = null, int index = 0) {
            messages[index] = message;
        }

        public static void ShowMessages() {
            for (int i = 0; i < messages.Length; i++) {
                String message = messages[i];
                UIText messageHandle = GetMessageHandle(i);

                if (message != null) {
                    messageHandle.Caption = message;
                }

                messageHandle.Draw();
            }
        }

        public static void ClearVehiclePool() {
            try {
                foreach(Vehicle vehicle in World.GetAllVehicles()) {
                    if (vehiclePool.Contains(vehicle.Handle) && vehicle.Exists()) {
                        try { vehicle.Delete(); } catch { }
                    }
                }
            } catch { }

            vehiclePool.Clear();
        }

        public static void ClearPedPool() {
            try {
                foreach(Ped ped in World.GetAllPeds()) {
                    if (pedPool.Contains(ped.Handle) && ped.Exists()) {
                        try { ped.Delete(); } catch { }
                    }
                }
            } catch { }

            pedPool.Clear();
        }

        public static void DoRemoveVehicles() {
            try {
                foreach(Vehicle vehicle in World.GetAllVehicles()) {
                    if (!vehiclePool.Contains(vehicle.Handle) &&
                        (Game.Player.Character.CurrentVehicle == null ||
                            Game.Player.Character.CurrentVehicle.Handle != vehicle.Handle)) {
                        if (vehicle.Exists()) {
                            try { vehicle.Delete(); } catch { }
                        }
                    }
                }
            } catch { }
        }

        public static void DoRemovePeds() {
            try {
                foreach(Ped ped in World.GetAllPeds()) {
                    if (!pedPool.Contains(ped.Handle)) {
                        if (ped.Exists()) {
                            try { ped.Delete(); } catch { }
                        }
                    }
                }
            } catch { }
        }

        public static Vector3 RotToDir(Vector3 Rot) {
            double z = Rot.Z;
            double retz = z * 0.01745329;
            double x = Rot.X;
            double retx = x * 0.01745329;
            double absx = System.Math.Abs(System.Math.Cos(retx));
            return new Vector3(
                (float)(-System.Math.Sin(retz) * absx),
                (float)(System.Math.Cos(retz) * absx),
                (float)(System.Math.Sin(retx))
            );
        }

        public static void RenderCircleOnGround(
            Vector3 position, 
            float radius, 
            Color color,
            String message = ""
        ) {
            position.Z = World.GetGroundHeight(position) + 0.5f;

            World.DrawMarker(
                MarkerType.HorizontalCircleSkinny,
                position,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1, 1, 1) * radius * 2,
                color
            );

            // if (message != "") {
            //     Vector3 camrot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 0);
            //     Scaleform scaleform = new Scaleform("PLAYER_NAME_01");

            //     scaleform.CallFunction("SET_PLAYER_NAME", "~s~" + message);
            //     scaleform.Render3D(position + new Vector3(0.0f, 0.0f, 2.5f),
            //         rotation: new Vector3(0.0f, (0.0f - camrot.Z), 0.0f), 
            //         scale: new Vector3(5.0f, 3.0f, 1.0f)
            //     );
            // }
        }
    }
}