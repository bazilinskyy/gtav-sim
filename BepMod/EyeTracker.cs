using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using static BepMod.Util;

namespace BepMod
{
    class EyeTracker
    {
        public string status = "";

        public void DoTick()
        {
            ShowMessage(status, 1);
        }

        public EyeTrackerPacket lastPacket = new EyeTrackerPacket();
        public UInt32 lastFrameNumber;
        public WorldIntersection lastClosestWorldIntersection = new WorldIntersection(
            new Vector3(0, 0, 0),
            new Vector3(UI.WIDTH / 2, UI.HEIGHT / 2, 0),
            "No recorded intersection yet"
        );

        public int listenPort = 5001;
        public bool listening = false;
        public Thread listeningThread;

        public int GetOpenUdpPort()
        {
            var startingAtPort = 5001;
            var maxNumberOfPortsToCheck = 500;
            var range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);
            var portsInUse =
                from p in range
                join used in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
            on p equals used.Port
                select p;

            return range.Except(portsInUse).FirstOrDefault();
        }

        UdpClient socket;

        ~EyeTracker()
        {
            Log("~EyeTracker()");
            Stop();
        }

        public void Start()
        {
            Log("EyeTracker.Start()");
            status = "EyeTracker.Start()";

            if (listening == false)
            {
                Stop();

                this.listenPort = GetOpenUdpPort();

                listening = true;
                listeningThread = new Thread(new ThreadStart(PacketListener));
                listeningThread.IsBackground = true;
                listeningThread.Start();
            }
        }

        public void Stop()
        {
            Log("EyeTracker.Stop()");
            status = "EyeTracker.Stop()";

            if (listening == true)
            {
                listening = false;
            }

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public void PacketListener()
        {
            Log("PacketListener()");
            socket = null;

            try
            {
                socket = new UdpClient(listenPort);
            }
            catch (SocketException e)
            {
                Log("EyeTracker listener not initialized correctly: " + e.ToString());
                status = "EyeTracker listen failed";
            }

            if (socket != null)
            {
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
                status = "EyeTracker listening on " + listenPort.ToString();

                while (listening)
                {
                    try
                    {
                        byte[] data = socket.Receive(ref groupEP);

                        EyeTrackerPacket res = new EyeTrackerPacket(data);

                        lastPacket = res;

                        foreach (EyeTrackerSubPacket subPacket in res.SubPackets)
                        {
                            if (subPacket.Id == 1)
                            {
                                lastFrameNumber = subPacket.GetUInt32();
                            }
                            else if (subPacket.Id == 64)
                            {
                                lastClosestWorldIntersection = subPacket.GetWorldIntersection();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log("EyeTracker exception: " + e.ToString());
                    }
                }
            }

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public static byte[] GetData(byte[] value, int startIndex, int length)
        {
            byte[] ret = new byte[length];

            Array.Copy(value, startIndex, ret, 0, length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ret);
            }

            return ret;
        }

        public struct WorldIntersection
        {
            public Vector3 WorldPoint;
            public Vector3 ObjectPoint;
            public string Name;

            public WorldIntersection(Vector3 WorldPoint, Vector3 ObjectPoint, string Name)
            {
                this.WorldPoint = WorldPoint;
                this.ObjectPoint = ObjectPoint;
                this.Name = Name;
            }

            override public string ToString()
            {
                return String.Format(
                    "WI ({0}): {1}",
                    Name,
                    ObjectPoint.ToString()
                );
            }
        }

        public struct EyeTrackerSubPacket
        {
            public UInt16 Id;
            public UInt16 Length;
            public byte[] Data;

            public EyeTrackerSubPacket(byte[] value, int startIndex)
            {
                Id = BitConverter.ToUInt16(GetData(value, startIndex, 2), 0);
                Length = BitConverter.ToUInt16(GetData(value, startIndex + 2, 2), 0);
                Data = new byte[Length];
                Array.Copy(value, startIndex + 4, Data, 0, Length);
            }

            public UInt16 GetUInt16()
            {
                return BitConverter.ToUInt16(GetData(Data, 0, Length), 0);
            }

            public UInt32 GetUInt32()
            {
                return BitConverter.ToUInt32(GetData(Data, 0, Length), 0);
            }

            public UInt64 GetUInt64()
            {
                return BitConverter.ToUInt64(GetData(Data, 0, Length), 0);
            }

            public Double GetDouble()
            {
                return BitConverter.ToDouble(GetData(Data, 0, Length), 0);
            }

            public WorldIntersection GetWorldIntersection()
            {
                return new WorldIntersection(
                    new Vector3(
                        (float)BitConverter.ToDouble(GetData(Data, 2, 8), 0),
                        (float)BitConverter.ToDouble(GetData(Data, 10, 8), 0),
                        (float)BitConverter.ToDouble(GetData(Data, 18, 8), 0)
                    ),
                    new Vector3(
                        (float)BitConverter.ToDouble(GetData(Data, 26, 8), 0),
                        (float)BitConverter.ToDouble(GetData(Data, 34, 8), 0),
                        (float)BitConverter.ToDouble(GetData(Data, 42, 8), 0)
                    ),
                    Encoding.ASCII.GetString(Data, 50, Length - 50)
                );
            }
        }

        public struct EyeTrackerPacket
        {
            public string Id;
            public UInt16 Type;
            public UInt16 Length;

            public List<EyeTrackerSubPacket> SubPackets;

            public EyeTrackerPacket(byte[] value)
            {
                Id = Encoding.ASCII.GetString(value, 0, 4);
                Type = BitConverter.ToUInt16(GetData(value, 4, 2), 0);
                Length = BitConverter.ToUInt16(GetData(value, 6, 2), 0);
                SubPackets = new List<EyeTrackerSubPacket>();

                int pos = 8;

                while (pos < Length)
                {
                    EyeTrackerSubPacket subPacket = new EyeTrackerSubPacket(value, pos);
                    SubPackets.Add(subPacket);
                    pos += (4 + subPacket.Length);
                }
            }

            public EyeTrackerSubPacket GetSubPacket(UInt16 type)
            {
                foreach (EyeTrackerSubPacket subPacket in SubPackets)
                {
                    if (subPacket.Id == type)
                    {
                        return subPacket;
                    }
                }

                return new EyeTrackerSubPacket();
            }
        }
    }
}
