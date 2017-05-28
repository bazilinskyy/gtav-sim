using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using GTA;
using GTA.Math;
using GTA.Native;

namespace BepMod
{
    enum TrafficLightColor { Green = 0, Red, Yellow, Auto }

    static class Util
    {
        public static int uiWidth = 1920;
        public static int uiHeight = 1200;

        public static int debugLevel = 0;

        public static String[] messages = new String[30];

        public static UIText[] messageHandles = new UIText[30];

        public static HashSet<int> vehiclePool = new HashSet<int>();
        public static HashSet<int> pedPool = new HashSet<int>();

        public static List<int> trafficSignalHashes = new List<int> {
            865627822,
            -655644382,
            862871082,
            1043035044
        };
        public static TrafficLightColor trafficLightsColor = TrafficLightColor.Auto;

        public static int LastShowMessageIndex = 0;

        static Util() { }

        public static void Log(string message, string name = "general")
        {
            try
            {
                using (StreamWriter w = File.AppendText(String.Format("bepmod_{0}.log", name)))
                {
                    w.WriteLine("[{0}] {1}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        message
                    );
                }
            }
            catch { }
        }

        public static void DoTick()
        {
            ShowMessages();

            if (trafficLightsColor != TrafficLightColor.Auto)
            {
                foreach (Prop prop in World.GetNearbyProps(Game.Player.Character.Position, 100.0f))
                {
                    if (trafficSignalHashes.Contains(prop.Model.Hash))
                    {
                        Function.Call(Hash.SET_ENTITY_TRAFFICLIGHT_OVERRIDE, prop, (int)trafficLightsColor);
                    }
                }
            }
        }

        public static UIText GetMessageHandle(int index)
        {
            if (messageHandles[index] == null)
            {
                messageHandles[index] = new UIText("",
                    new Point(10, 10 + 30 * index),
                    .75f,
                    Color.Yellow,
                    GTA.Font.ChaletComprimeCologne,
                    false,
                    true,
                    true
                );
            }
            return messageHandles[index];
        }

        public static void ClearMessages()
        {
            for (int i = 0; i < messages.Length; i++)
            {
                ShowMessage(null, i);
            }
        }

        public static void ShowMessage(string message = null, int index = -1)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (index < 0)
                {
                    index = LastShowMessageIndex;
                }

                if (index >= 0)
                {
                    LastShowMessageIndex = index;
                }

                messages[index] = string.IsNullOrEmpty(message) ? null : message;
            }
            else
            {
                messages[index] = null;
            }            
        }

        public static void ShowMessages()
        {
            for (int i = 0; i < messages.Length; i++)
            {
                String message = messages[i];

                if (!string.IsNullOrEmpty(message))
                {
                    UIText messageHandle = GetMessageHandle(i);

                    messageHandle.Caption = message;
                    messageHandle.Draw();
                }
            }
        }

        public static void ClearVehiclePool()
        {
            try
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (vehiclePool.Contains(vehicle.Handle) && vehicle.Exists())
                    {
                        try { vehicle.Delete(); } catch { }
                    }
                }
            }
            catch { }

            vehiclePool.Clear();
        }

        public static void ClearPedPool()
        {
            try
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (pedPool.Contains(ped.Handle) && ped.Exists())
                    {
                        try { ped.Delete(); } catch { }
                    }
                }
            }
            catch { }

            pedPool.Clear();
        }

        public static void DoRemoveVehicles()
        {
            try
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (!vehiclePool.Contains(vehicle.Handle) &&
                        (Game.Player.Character.CurrentVehicle == null ||
                            Game.Player.Character.CurrentVehicle.Handle != vehicle.Handle))
                    {
                        if (vehicle.Exists())
                        {
                            try { vehicle.Delete(); } catch { }
                        }
                    }
                }
            }
            catch { }
        }

        public static void DoRemovePeds()
        {
            try
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (!pedPool.Contains(ped.Handle))
                    {
                        if (ped.Exists())
                        {
                            try { ped.Delete(); } catch { }
                        }
                    }
                }
            }
            catch { }
        }

        public static Vector3 RotToDir(Vector3 Rot)
        {
            double retz = Rot.Z * 0.01745329;
            double retx = Rot.X * 0.01745329;
            double absx = Math.Abs(Math.Cos(retx));

            return new Vector3(
                (float)(-Math.Sin(retz) * absx),
                (float)(Math.Cos(retz) * absx),
                (float)(Math.Sin(retx))
            );
        }

        public static void RenderCircleOnGround(
            Vector3 position,
            float radius,
            Color color
        )
        {
            position.Z = World.GetGroundHeight(position) + 0.5f;

            World.DrawMarker(
                MarkerType.HorizontalCircleSkinny,
                position,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(1, 1, 1) * radius * 2,
                color
            );
        }
    }
}