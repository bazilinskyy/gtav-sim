using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GTA;
using GTA.Math;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using static BepMod.Util;

namespace BepMod
{
    class SmartEye
    {
        public string status = "";

        public void DoTick()
        {
            if (!string.IsNullOrEmpty(status))
            {
                Log("SmartEye: " + status);
                if (debugLevel > 0)
                {
                    ShowMessage(status, 1);
                }
                status = "";
            }
        }

        public Packet lastPacket = new Packet();
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

        ~SmartEye()
        {
            Log("~SmartEye()");
            Stop();
        }

        public void Start()
        {
            Log("SmartEye.Start()");
            status = "SmartEye.Start()";

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
            Log("SmartEye.Stop()");
            status = "SmartEye.Stop()";

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
            socket = null;

            try
            {
                socket = new UdpClient(listenPort);
            }
            catch (SocketException e)
            {
                Log("SmartEye listener not initialized correctly: " + e.ToString());
                status = "SmartEye listen failed";
            }

            if (socket != null)
            {
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
                status = "SmartEye listening on " + listenPort.ToString();

                while (listening)
                {
                    try
                    {
                        byte[] data = socket.Receive(ref groupEP);

                        Packet res = new Packet(data);

                        lastPacket = res;

                        foreach (SubPacket subPacket in res.SubPackets)
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
                        Log("SmartEye exception: " + e.ToString());
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
                    "WorldIntersection {0}",
                    ObjectPoint.ToString()
                );
            }
        }

        public struct SubPacket
        {
            public UInt16 Id;
            public UInt16 Length;
            public byte[] Data;

            public SubPacket(byte[] value, int startIndex)
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
                //UInt16 size = BitConverter.ToUInt16(GetData(Data, 50, 2), 0);

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
                    ""
                //Encoding.ASCII.GetString(Data, 50, size)
                );
            }
        }

        public struct Packet
        {
            public string Id;
            public UInt16 Type;
            public UInt16 Length;

            public List<SubPacket> SubPackets;

            public Packet(byte[] value)
            {
                Id = Encoding.ASCII.GetString(value, 0, 4);
                Type = BitConverter.ToUInt16(GetData(value, 4, 2), 0);
                Length = BitConverter.ToUInt16(GetData(value, 6, 2), 0);
                SubPackets = new List<SubPacket>();

                int pos = 8;

                while (pos < Length)
                {
                    SubPacket subPacket = new SubPacket(value, pos);
                    SubPackets.Add(subPacket);
                    pos += (4 + subPacket.Length);
                }
            }

            public SubPacket GetSubPacket(UInt16 type)
            {
                foreach (SubPacket subPacket in SubPackets)
                {
                    if (subPacket.Id == type)
                    {
                        return subPacket;
                    }
                }

                return new SubPacket();
            }
        }
    }
}
