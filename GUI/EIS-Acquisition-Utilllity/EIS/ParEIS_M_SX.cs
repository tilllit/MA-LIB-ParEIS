using EIS_Acquisition_Utilllity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test_Bench;



public class ParEIS_M_SX
{
    // Events
    public delegate void ParEIS_DataEvent();
    public static event ParEIS_DataEvent RAW_DATA_IN;
    public static event ParEIS_DataEvent PHY_DATA_IN;
    public static event ParEIS_DataEvent Sweep_Session_Done;

    // Config
    public static int NrCards = 26;
    public static int NrChannels = 3 * NrCards;

    public static List<int> ChannelPositions = new List<int>() 
    {
        // Explanation:
        // Cell 1-74 are in CH 1-74,
        // CH 75 is unused,
        // VBat: CH 76,
        // Shunt: CH 77,
        // NTC: CH 78

        // Top Side
         1,  3,  5, // Card 1
         7,  9, 11, // Card 2
        13, 15, 17, // Card 3
        19, 21, 23, // Card 4
        25, 27, 29, // Card 5
        31, 33, 35, // Card 6
        37, 39, 41, // Card 7
        43, 45, 47, // Card 8
        49, 51, 53, // Card 9
        55, 57, 59, // Card 10
        61, 63, 65, // Card 11
        67, 69, 71, // Card 12
        73, 74, 75, // Card 13

        // Bottom Side
         6,  4,  2, // Card 14
        12, 10,  8, // Card 15
        18, 16, 14, // Card 16
        24, 22, 20, // Card 17
        30, 28, 26, // Card 18
        36, 34, 32, // Card 19
        42, 40, 38, // Card 20
        48, 46, 44, // Card 21
        76, 78, 77, // AMP Card (Card 22)
        54, 52, 50, // Card 23
        60, 58, 56, // Card 24
        66, 64, 62, // Card 25
        72, 70, 68, // Card 26
    };


    // Calibration data
    public static List<Acquisition_Device.ACQ_Calib> Calib_Data = new List<Acquisition_Device.ACQ_Calib>();
    public static bool calib_loaded = false;


    // RAW Data storage
    public static List<List<Int32>> SortedChannels = new List<List<Int32>>();

    // PHY Data storage
    public static List<List<double>> Currents = new List<List<double>>();
    public static List<double> Voltage = new List<double>();
    public static List<double> Shunt = new List<double>();
    public static List<double> Temperature = new List<double>();


    // Sweep Session
    public static List<Acquisition_Config.SIN_Config> Sweeps = new List<Acquisition_Config.SIN_Config>();
    public static string SweepSessionPath = null;



    public static void DecodeData()
    {
        // first sort Acq Device channel data to ParEIS MSX channel data (sorting C1: CH1, CH3, CH5 ...)

        for (int i = 0; i < NrChannels; i++)
            SortedChannels.Add(new List<Int32>());

        for (int i = 0; i < NrChannels; i++)
        {
            SortedChannels[ChannelPositions[i] - 1] = Acquisition_Device.Channel_Data[i].ToList();
        }

        RAW_DATA_IN?.Invoke();

        Decode_PHY_Data();
    }


    public static void Decode_PHY_Data()
    {
        // Decode channel data into PHY data here


        // Decode Voltage
        Voltage.Clear();
        List<Int32> RAW_Voltage = SortedChannels[75].ToList();
        for (int i = 0; i < RAW_Voltage.Count; i++)
        {
            double v = 0.0;
            if (calib_loaded)
            {
                v = RAW_Voltage[i];
                v = Calib_Data[75].Factor * v + Calib_Data[75].Offset;
            }
            else
            {
                v = RAW_Voltage[i];
                v = v / (double)Math.Pow(2, 19);
                v = v * 4.096;
                v = v / 15.0;
                v = v * 4.55;       // Gain: Voltage Divider 
                v = v + 3.018;
            }
            Voltage.Add(v);
        }


        // Decode Shunt
        Shunt.Clear();
        List<Int32> RAW_Shunt = SortedChannels[76].ToList();
        for (int i = 0; i < RAW_Shunt.Count; i++)
        {
            double c = 0.0;
            if (calib_loaded)
            {
                c = RAW_Shunt[i];
                c = Calib_Data[76].Factor * c + Calib_Data[76].Offset;
            }
            else
            {
                c = RAW_Shunt[i];
                c = c / (double)Math.Pow(2, 19);
                c = c * 4.096;
                c = c / 15.0;
                c = c / 6.6;       // Gain: AMP Card
                c = c / 0.000375;

            }
            Shunt.Add(c);
        }


        // Decode Temperature
        Temperature.Clear();
        List<Int32> RAW_Temp = SortedChannels[77].ToList();
        for (int i = 0; i < RAW_Temp.Count; i++)
        {
            double t = 0.0;
            if (calib_loaded)
            {
                t = RAW_Temp[i];
                t = Calib_Data[77].Factor * t + Calib_Data[77].Offset;
            }
            else
            {
                t = RAW_Temp[i];
            }
            Temperature.Add(t);
        }


        // Decode Currents
        Currents.Clear();
        for (int i = 0; i < 74; i++)
            Currents.Add(new List<double>());

        for (int i = 0; i < 74; i++)
        {
            for (int j = 0; j < SortedChannels[i].Count; j++)
            {
                double c = 0.0;
                if (calib_loaded)
                {
                    c = SortedChannels[i][j];
                    c = Calib_Data[i].Factor * c + Calib_Data[i].Offset;
                }
                else
                {
                    c = SortedChannels[i][j];
                    c = c / (double)Math.Pow(2, 19);
                    c = c * 4.096;
                    c = c / 15.0;
                    c = c / 48;       // Gain: Nagelbrett
                    c = c / 0.0035; // 3.5mOhm //0.0005; // 0.5mOhm

                }
                Currents[i].Add(c);
            }
        }


        PHY_DATA_IN?.Invoke();
    }



    public static void Read_RAW_Data(string path)
    {
        string payload = System.IO.File.ReadAllText(path);
        List<string> Lines = System.IO.File.ReadLines(path).ToList();


        Acquisition_Device.Channel_Data.Clear();
        Acquisition_Device.Acq_Config.NrSamples = (UInt16)(Lines.Count - 1);

        for (int i = 0; i < Lines.Count; i++)
        {
            var s = Lines[i].Split(';');

            if (i == 0)
            {
                Acquisition_Device.Acq_Config.NrCards = ((uint)(s.Length) - 1) / 3;
                for (int j = 1; j < s.Length; j++)
                {
                    Acquisition_Device.Channel_Data.Add(new List<int>());
                }
            }
            else
            {
                for (int j = 1; j < s.Length; j++)
                {
                    int v = 0;
                    int.TryParse(s[j], out v);
                    Acquisition_Device.Channel_Data[j - 1].Add(v);
                }
            }
        }

        DecodeData();
    }

    public static void Write_RAW_Data(string File_path = null)
    {
        if (Acquisition_Device.Channel_Data.Count == 0)
            return;

        List<string> Lines = new List<string>();

        // Create Header
        string header = "Sample";
        for (int i = 0; i < Acquisition_Device.Channel_Data.Count; i++)
            header += ";Channel" + (i + 1);
        Lines.Add(header);

        //UInt32.TryParse(TB_NrSamples.Text, out Acquisition_Device.Acq_Config.NrSamples);
        for (int i = 0; i < Acquisition_Device.Channel_Data[0].Count; i++)
        {
            string line = (i + 1).ToString();

            for (int j = 0; j < Acquisition_Device.Channel_Data.Count; j++)
                line += ";" + Acquisition_Device.Channel_Data[j][i].ToString();

            Lines.Add(line);
        }


        if (File_path == null)
        {
            string path = SweepSessionPath + "\\RAW";
            string name = "Acquisition_RAW_DATA_" + Acquisition_Device.Acq_Config.Name + ".csv";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File_path = Path.Combine(path, name);
        }

        System.IO.File.WriteAllLines(File_path, Lines);
    }

    public static void Read_PHY_Data(string path)
    {
        string payload = System.IO.File.ReadAllText(path);
        List<string> Lines = System.IO.File.ReadLines(path).ToList();


        Acquisition_Device.Channel_Data.Clear();
        Acquisition_Device.Acq_Config.NrSamples = (UInt16)(Lines.Count - 1);

        Voltage.Clear();
        Shunt.Clear();
        Temperature.Clear();
        Currents.Clear();

        for (int i = 0; i < 74; i++)
            Currents.Add(new List<double>());

        for (int i = 1; i < Lines.Count; i++)
        {
            var s = Lines[i].Split(';');

            double v = 0.0;
            double.TryParse(s[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v);
            Voltage.Add(v);

            double c = 0.0;
            for (int j = 0; j < 74; j++)
            {
                c = 0.0;
                double.TryParse(s[j+2], NumberStyles.Float, CultureInfo.InvariantCulture, out c);
                Currents[j].Add(c);
            }

            c = 0.0;
            double.TryParse(s[74 + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out c);
            Shunt.Add(c);

            double t = 0.0;
            double.TryParse(s[74 + 3], NumberStyles.Float, CultureInfo.InvariantCulture, out t);
            Temperature.Add(t);
        }

        PHY_DATA_IN?.Invoke();
    }

    public static void Write_PHY_Data(string File_path = null)
    {

        List<string> Lines = new List<string>();

        // Create Header
        string header = "Sample";
        header += ";Voltage [V]";

        for (int i = 0; i < Currents.Count; i++)
            header += ";Cell " + (i + 1) + " [A]";

        header += ";Shunt [A]";
        header += ";Temperature [°C]";
        Lines.Add(header);

        //UInt32.TryParse(TB_NrSamples.Text, out Acquisition_Device.Acq_Config.NrSamples);
        for (int i = 0; i < Acquisition_Device.Acq_Config.NrSamples; i++)
        {
            string line = (i + 1).ToString();                               // Sample Nr.
            line += ";" + Voltage[i].ToString(new CultureInfo("en-US"));    // Voltage

            for (int j = 0; j < Currents.Count; j++)
            {
                line += ";" + Currents[j][i].ToString(new CultureInfo("en-US")); // Current of Cell[j]
            }

            line += ";" + Shunt[i].ToString(new CultureInfo("en-US"));      // Shunt current
            line += ";" + Temperature[i].ToString(new CultureInfo("en-US"));// NTC temp.

            Lines.Add(line);
        }

        if (File_path == null)
        {
            string path = SweepSessionPath + "\\PHY";
            string name = "Acquisition_PHY_DATA_" + Acquisition_Device.Acq_Config.Name + ".csv";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File_path = Path.Combine(path, name);
        }

        System.IO.File.WriteAllLines(File_path, Lines);
    }

    public static void ReadCalibData(string path)
    {
        string payload = System.IO.File.ReadAllText(path);
        List<string> Lines = System.IO.File.ReadLines(path).ToList();


        Calib_Data.Clear();

        for (int i = 1; i < Lines.Count; i++)
        {
            var s = Lines[i].Split(';');

            double Factor, Offset = 0;

            double.TryParse(s[1], NumberStyles.Float, CultureInfo.InvariantCulture, out Factor);
            double.TryParse(s[2], NumberStyles.Float, CultureInfo.InvariantCulture, out Offset);
            Calib_Data.Add(new Acquisition_Device.ACQ_Calib(Factor, Offset));
        }

        calib_loaded = true;
    }


    // Process Sweep Session

    public static async void StartSweepSession()
    {
        if (Sweeps.Count == 0 || SweepSessionPath == null)
            return;


        // subscribe to events
        PHY_DATA_IN += SweepSessionRoutine;


        // Start first sweep
        var conf_new = Sweeps[0];

        byte lut = 0;
        switch (conf_new.LUT)
        {
            case 1000:
                lut = 0;
                break;
            case 500:
                lut = 1;
                break;
            case 144:
                lut = 2;
                break;
            case 72:
                lut = 3;
                break;
            case 36:
                lut = 4;
                break;
            case 18:
                lut = 5;
                break;
        }


        MainWindow.SetStatus("Running Sweep: " + conf_new.Name);
        Acquisition_Device.Load_Setup((byte)(0x20 + lut), conf_new.PSC, conf_new.ARR, conf_new.DAC_Amplitude, conf_new.DAC_Offset, conf_new.Periods, (UInt16)(0x2E0 + conf_new.E_Load_Speed));
        await Task.Delay(200);
        Acquisition_Device.Acquisition_Setup(conf_new.Samples, conf_new.NumCards, conf_new.Fsample, conf_new.Fserial, conf_new.Name);
        await Task.Delay(200);
        Acquisition_Device.Acquisition_START();
    }

    public static async void SweepSessionRoutine()
    {
        if (SweepSessionPath == null)
        {
            CancelSweepSession();
            return;
        }

        // ##### DEAL WITH RESULTS FROM LAST SWEEP #####
        Write_RAW_Data();
        Write_PHY_Data();

        // Take old config for relaxation -> then remove from list
        var conf_old = Sweeps[0];
        Sweeps.RemoveAt(0);

        // Session done -> no more sweep configs
        if (Sweeps.Count == 0)
        {
            CancelSweepSession();
            MainWindow.SetStatus("Finished Sweep Session!");
            Sweep_Session_Done?.Invoke();
            return;
        }

        // start relax timer
        for (int i = 0; i < conf_old.Srelax; i++)
        {
            if (Sweeps.Count == 0 || SweepSessionPath == null)
                return;

            MainWindow.SetStatus("Relaxation for: " + (conf_old.Srelax - i) + "[s] - Next: " + Sweeps[0].Name);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }



        // ##### DEAL WITH SETUP OF NEW SWEEP #####

        var conf_new = Sweeps[0];

        byte lut = 0;
        switch (conf_new.LUT)
        {
            case 1000:
                lut = 0;
                break;
            case 500:
                lut = 1;
                break;
            case 144:
                lut = 2;
                break;
            case 72:
                lut = 3;
                break;
            case 36:
                lut = 4;
                break;
            case 18:
                lut = 5;
                break;
        }


        MainWindow.SetStatus("Running Sweep: " + conf_new.Name);
        Acquisition_Device.Load_Setup((byte)(0x20 + lut), conf_new.PSC, conf_new.ARR, conf_new.DAC_Amplitude, conf_new.DAC_Offset, conf_new.Periods, (UInt16)(0x2E0 + conf_new.E_Load_Speed));
        await Task.Delay(200);
        Acquisition_Device.Acquisition_Setup(conf_new.Samples, conf_new.NumCards, conf_new.Fsample, conf_new.Fserial, conf_new.Name);
        await Task.Delay(200);
        Acquisition_Device.Acquisition_START();
    }

    public static void CancelSweepSession()
    {
        Sweeps.Clear();
        SweepSessionPath = null;
        MainWindow.SetStatus("Sweep Session CANCELED!");

        // Unsubscibe from all events here !!!
        PHY_DATA_IN -= SweepSessionRoutine;
    }


}

