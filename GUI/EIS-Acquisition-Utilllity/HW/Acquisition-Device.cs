using EIS_Acquisition_Utilllity;
using EIS_Acquisition_Utilllity.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using static Acquisition_Config;
using static Acquisition_Device;


public static class Acquisition_Device
{
    public struct ACQ_Config
    {
        public string Name;
        public UInt32 NrCards;
        public UInt32 NrSamples;
        public UInt32 Fsample;
        public UInt32 Fserial;
    }

    public struct ACQ_Calib
    {
        public double Factor { get; set; }
        public double Offset { get; set; }

        public ACQ_Calib(double factor, double offset)
        {
            Factor = factor;
            Offset = offset;
        }
    }

    public static ACQ_Config Acq_Config = new ACQ_Config();

    public delegate void AcquisitionDataEvent();
    public static event AcquisitionDataEvent ACC_DATA_IN;
    public static event AcquisitionDataEvent ACC_DATA_Ready;
    public static event AcquisitionDataEvent PHY_DATA_IN;
    public static event AcquisitionDataEvent PHY_DATA_Ready;
    //public static event AcquisitionDataEvent Sweep_Session_Done;
    public static void Fire_Data_In() { ACC_DATA_IN?.Invoke(); }
    public static void Fire_PHY_Data_In() { PHY_DATA_IN?.Invoke(); }
    public static void Fire_Data_Ready() { ACC_DATA_Ready?.Invoke(); }
    public static void Fire_PHY_Data_Ready() { PHY_DATA_Ready?.Invoke(); }


    static TCPClient TCP_CMD_Socket = null;
    static TCPClient TCP_DADA_Socket = null;

    static bool connected = false;

    // TCP data RX
    static List<byte> DataBuffer = new List<byte>();
    private static UInt32 RX_length = 0;

    // Decoded Channel data
    public static List<List<Int32>> Channel_Data = new List<List<Int32>>();         // Decoded with 2s complement
    public static List<List<UInt32>> RAW_Channel_Data = new List<List<UInt32>>();   // RAW values but undecoded




    // ### Absolut basic functionalitys ###
    public static async Task<bool> Connect(string Address = "192.168.1.11", UInt16 CMD_Port = 5001, UInt16 DATA_Port = 5003)
    {
        // Ping Host and Port
        if (!(await Task.Run(() => IsPortOpen(Address, CMD_Port, 5000).Result)) || !(await Task.Run(() => IsPortOpen(Address, DATA_Port, 5000).Result)))
        {
            MessageBox.Show("No connection to Device!", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        TCP_CMD_Socket = new TCPClient();
        TCP_CMD_Socket.create(Address, CMD_Port);
        TCP_CMD_Socket.TCP_RX_Callback += TCP_CMD_Receive;

        TCP_DADA_Socket = new TCPClient();
        TCP_DADA_Socket.create(Address, DATA_Port);
        TCP_DADA_Socket.TCP_RX_Callback += TCP_DATA_Receive;

        connected = true;
        return true;
    }

    public static void Disconnect()
    {
        if (!connected)
            return;

        try
        {
            connected = false;

            TCP_CMD_Socket.Close();
            TCP_CMD_Socket.TCP_RX_Callback -= TCP_CMD_Receive;

            TCP_DADA_Socket.Close();
            TCP_DADA_Socket.TCP_RX_Callback -= TCP_DATA_Receive;
        }
        catch
        {
            TCP_CMD_Socket = null;
            TCP_DADA_Socket = null;
        }
    }

    public static void Send(string payload)
    {
        if (!connected)
            return;

        TCP_CMD_Socket.send(payload);
    }





    public static void Acquisition_START()
    {
        if (!connected)
            return;

        DataBuffer.Clear();

        TCP_CMD_Socket.send("START");
    }

    public static void Acquisition_Setup(UInt32 NrSamples = 0, UInt32 NrCards = 0, UInt32 Fsample = 0, UInt32 Fserial = 0, string Name = "")
    {
        if (Name == "")
            Name = Acq_Config.Name;
        if (NrSamples == 0)
            NrSamples = Acq_Config.NrSamples;
        if (NrCards == 0)
            NrCards = Acq_Config.NrCards;
        if (Fsample == 0)
            Fsample = Acq_Config.Fsample;
        if (Fserial == 0)
            Fserial = Acq_Config.Fserial;

        if (!connected)
            return;

        Acq_Config.Name = Name;
        Acq_Config.NrSamples = NrSamples;
        Acq_Config.NrCards = NrCards;

        RX_length = NrCards * NrSamples * 8;

        //TCP_CMD_Socket.send("ACC-Setup 1000000 100 70");
        TCP_CMD_Socket.send("ACC-Setup " + Fserial.ToString() + " " + Fsample.ToString() + " " + NrSamples.ToString() + ";");
    }

    public static void Load_START()
    {
        if (!connected)
            return;

        TCP_CMD_Socket.send("ELOAD-Start");
    }

    public static void Load_Setup(byte Config, UInt16 PSC, UInt16 ARR, UInt16 Amplitude, UInt16 Offset, byte Periods, UInt16 ID = 0x80)
    {
        if (!connected)
            return;

        byte DAC_Amplitude = (byte)(Math.Round(Amplitude / 20.0));
        byte DAC_Offset = (byte)(Math.Round(Offset / 20.0));

        TCP_CMD_Socket.send("ELOAD-Setup " + ID.ToString() + " " + Config.ToString() + " " + PSC.ToString() + " " + ARR.ToString() 
                                            + " " + DAC_Amplitude.ToString() + " " + DAC_Offset.ToString() + " " + Periods.ToString() + ";" );
    }

    public static void STOP()
    {
        Test_Bench.CancelSweepSession();
        ParEIS_M_SX.CancelSweepSession();

        if (!connected)
            return;

        TCP_CMD_Socket.send("STOP");
    }


    public static void TCP_CMD_Receive(object sender, EventArgs e)
    {
        byte[] buff = (byte[])sender;
        string payload = "";

        try
        {
            payload = Encoding.ASCII.GetString(buff);
        }
        catch { payload = null; }

        if (payload != null)
        {
            if (payload.Contains("Data-Out:"))
            {
                var s = payload.Split(' ');
                //RX_length = Convert.ToUInt32(s[1]);

                // Awaiting data
                //Console.WriteLine("!!!!  FINISH - RX: " + DataBuffer.Count + " Byte  !!!!");
            }
            else if (payload.Contains("ACC-Finished!"))
            {
                // Awaiting data
            }
            else if(payload.Contains("Data-Transmit-Finished!"))
            {
                // Awaiting data
                //Console.WriteLine("!!!!  FINISH - RX: " + DataBuffer.Count + " Byte  !!!!");
            }

        }

        ConnectionPage.AddToConsole("Port 5001: " + payload);

    }


    public static void TCP_DATA_Receive(object sender, EventArgs e)
    {
        byte[] buff = (byte[])sender;

        //Console.WriteLine("RX " + buff.Length + " Byte");
        ConnectionPage.AddToConsole("RX " + buff.Length + " Byte");

        DataBuffer.AddRange(buff);

        if (DataBuffer.Count == RX_length && DataBuffer.Count > 0)
        {
            ConnectionPage.AddToConsole("\n!!!!  FINISH - RX: " + DataBuffer.Count + " Byte  !!!! \n");
            RX_length = 0;

            // Call event here...
            //ACC_DATA_IN?.Invoke();

            DecodeChannelData();
        }
    }


    private static void DecodeChannelData()
    {

        // Convert bytes to uint64_t
        UInt32 NrValues = (UInt32)(DataBuffer.Count / 8);
        List<UInt64> RAW_Data = new List<UInt64>();
        for (int i = 0; i < NrValues; i++)
        {
            ulong value =
                ((ulong)DataBuffer[i*8 + 0]) |
                ((ulong)DataBuffer[i*8 + 1] << 8) |
                ((ulong)DataBuffer[i*8 + 2] << 16) |
                ((ulong)DataBuffer[i*8 + 3] << 24) |
                ((ulong)DataBuffer[i*8 + 4] << 32) |
                ((ulong)DataBuffer[i*8 + 5] << 40) |
                ((ulong)DataBuffer[i*8 + 6] << 48) |
                ((ulong)DataBuffer[i*8 + 7] << 56);
            RAW_Data.Add(value);
        }

        // Create fitting storage lists
        Channel_Data.Clear();
        RAW_Channel_Data.Clear();
        for (int i = 0; i < Acq_Config.NrCards * 3; i++)
        {
            Channel_Data.Add(new List<Int32>());
            RAW_Channel_Data.Add(new List<UInt32>());
        }

        // Extract values and sort into channels
        for (int i = 0; i < Acq_Config.NrSamples; i++)
        {
            for (int j = 0; j < Acq_Config.NrCards; j++)
            {

                UInt64 RAW_Val = RAW_Data[i * (int)Acq_Config.NrCards + j];


                UInt32 val3 = (UInt32)((RAW_Val >> 44) & 0xFFFFF);
                UInt32 val2 = (UInt32)((RAW_Val >> 24) & 0xFFFFF);
                UInt32 val1 = (UInt32)((RAW_Val >> 4) & 0xFFFFF);

                Channel_Data[j * 3 + 0].Add(SignExtend20(val1));
                Channel_Data[j * 3 + 1].Add(SignExtend20(val2));
                Channel_Data[j * 3 + 2].Add(SignExtend20(val3));

                RAW_Channel_Data[j * 3 + 0].Add(val1);
                RAW_Channel_Data[j * 3 + 1].Add(val2);
                RAW_Channel_Data[j * 3 + 2].Add(val3);
            }
        }

        ConnectionPage.AddToConsole("Decoding finished!");

        ACC_DATA_IN?.Invoke();
    }






    //private static Int32 SignExtend20(UInt32 x)
    //{
    //    x &= 0x000FFFFFu;

    //    if ((x & 0x80000u) != 0)
    //        x |= 0xFFF00000u;

    //    return unchecked((Int32)x);
    //}


    private static Int32 SignExtend20(UInt32 x)
    {
        Int32 y = 0;
        x &= 0x000FFFFFu;

        if ((x & 0x80000u) != 0)
        {
            x = (0x000FFFFF ^ x) - 1;
            y = (Int32)(-x);
        }
        //x |= 0xFFF00000u;
        //y = (Int32)(-(~x+1));
        else
        {
            y = (Int32)x;
        }


            //return unchecked((Int32)x);
            return y;
    }



    // this works to check for open port
    private static async Task<bool> IsPortOpen(string host, int port, int timeoutMs = 2000)
    {
        try
        {
            var TCPclient = new TcpClient();
            var connectTask = TCPclient.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeoutMs);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            // Wenn ConnectTask fertig ist UND erfolgreich war:
            if (completedTask == connectTask)
            {
                // Abwarten, ob dabei wirklich eine Verbindung zustande kam (durch await)
                await connectTask;
                return true;
            }
            else
            {
                // Timeout
                return false;
            }
        }
        catch (SocketException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }


}

