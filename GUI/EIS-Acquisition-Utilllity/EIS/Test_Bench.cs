using EIS_Acquisition_Utilllity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;


public class Test_Bench
{
    // Events
    public delegate void TestBench_DataEvent();
    public static event TestBench_DataEvent RAW_DATA_IN;
    public static event TestBench_DataEvent PHY_DATA_IN;
    public static event TestBench_DataEvent Sweep_Session_Done;


    // Calibration data
    public static List<Acquisition_Device.ACQ_Calib> Calib_Data = new List<Acquisition_Device.ACQ_Calib>();
    public static bool calib_loaded = false;

    // PHY Data
    public static List<double> Voltages = new List<double>();
    public static List<List<double>> Currents = new List<List<double>>();
    public static List<double> ShuntCurrent = new List<double>();

    // Sweep Session
    public static List<Acquisition_Config.SIN_Config> Sweeps = new List<Acquisition_Config.SIN_Config>();
    public static string SweepSessionPath = null;




    public static void DecodeData()
    {
        RAW_DATA_IN?.Invoke();

        DecodeVoltage();
        DecodeCurrents();
        DecodeShunt();

        PHY_DATA_IN?.Invoke();
    }


    private static void DecodeVoltage()
    {
        var adc_volts = Acquisition_Device.Channel_Data[8];
        Voltages = new List<double>();
        for (int i = 0; i < adc_volts.Count; i++)
        {
            double v = adc_volts[i];

            if (calib_loaded)
                v = Calib_Data[8].Factor * v + Calib_Data[8].Offset;
            else
            {
                v = v / (double)Math.Pow(2, 19);
                v = v * 4.096;// 2.0;
                v = v / 15.0;
                v = v * 31.3;       // Gain: Voltage Divider 
            }
            Voltages.Add(v);
        }
    }

    private static void DecodeCurrents()
    {
        List<int> currIndex = new List<int>() { 7, 6, 2, 1, 0 };

        Currents.Clear();
        for (int i = 0; i < currIndex.Count; i++)
        {
            List<double> curr = new List<double>();

            for (int j = 0; j < Acquisition_Device.Channel_Data[currIndex[i]].Count; j++)
            {
                double c = Acquisition_Device.Channel_Data[currIndex[i]][j];

                if (calib_loaded)
                    c = Calib_Data[currIndex[i]].Factor * c + Calib_Data[currIndex[i]].Offset;
                else
                {
                    c = c / (double)Math.Pow(2, 19);
                    c = c * 4.096 / 2.0;
                    c = c / 15.0;       // Gain: ADC Card
                    c = c / 7.5;        // Gain: Voltage Divider
                    c = c / 0.0035;     // Shunt: ca. 3.5 mOhm
                }
                curr.Add(c);
            }

            Currents.Add(curr);
        }
    }

    private static void DecodeShunt()
    {
        ShuntCurrent.Clear();

        List<double> curr1 = new List<double>();
        for (int i = 0; i < Acquisition_Device.Channel_Data[3].Count; i++)
        {
            double c = Acquisition_Device.Channel_Data[3][i];

            if (calib_loaded)
                c = Calib_Data[3].Factor * c + Calib_Data[3].Offset;
            else
            {
                c = c / (double)Math.Pow(2, 19);
                c = c * 4.096 / 2;
                c = c / 15.0;       // Gain: ADC Card
                c = c / 1.68;//96.3;       // Gain: Voltage Divider
                c = c / 0.00075;//0.000025;   // Shunt: ca. 25 uOhm
            }
            curr1.Add(c);
            ShuntCurrent.Add(c);
        }

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

        //Acquisition_Device.Fire_Data_In();
        DecodeData();
    }

    public static void Write_RAW_Data(string File_path = null)
    {

        List<string> Lines = new List<string>();

        // Create Header
        //UInt32.TryParse(TB_NrCards.Text, out Acquisition_Device.Acq_Config.NrCards);
        string header = "Sample";
        for (int i = 0; i < Acquisition_Device.Acq_Config.NrCards * 3; i++)
            header += ";Channel" + (i + 1);
        Lines.Add(header);

        //UInt32.TryParse(TB_NrSamples.Text, out Acquisition_Device.Acq_Config.NrSamples);
        for (int i = 0; i < Acquisition_Device.Acq_Config.NrSamples; i++)
        {
            string line = (i + 1).ToString();

            for (int j = 0; j < Acquisition_Device.Acq_Config.NrCards * 3; j++)
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

        Voltages.Clear();
        Currents.Clear();

        for (int i = 0; i < Lines.Count; i++)
        {
            var s = Lines[i].Split(';');

            if (i == 0)
            {
                for (int j = 2; j < s.Length; j++)
                {
                    Currents.Add(new List<double>());
                }
            }
            else
            {
                for (int j = 1; j < s.Length; j++)
                {
                    if (j == 1)
                    {
                        double v = 0.0;
                        double.TryParse(s[j], NumberStyles.Float, CultureInfo.InvariantCulture, out v);
                        Voltages.Add(v);
                    }
                    else
                    {
                        double c = 0.0;
                        double.TryParse(s[j], NumberStyles.Float, CultureInfo.InvariantCulture, out c);
                        Currents[j - 2].Add(c);
                    }
                }
            }
        }

        //Acquisition_Device.Fire_PHY_Data_In();
        PHY_DATA_IN?.Invoke();
    }

    public static void Write_PHY_Data(string File_path = null)
    {

        List<string> Lines = new List<string>();

        // Create Header
        //UInt32.TryParse(TB_NrCards.Text, out Acquisition_Device.Acq_Config.NrCards);
        string header = "Sample";
        header += ";Voltage [V]";
        for (int i = 0; i < Currents.Count; i++)
            header += ";Cell " + (i + 1) + " [A]";
        Lines.Add(header);

        //UInt32.TryParse(TB_NrSamples.Text, out Acquisition_Device.Acq_Config.NrSamples);
        for (int i = 0; i < Acquisition_Device.Acq_Config.NrSamples; i++)
        {
            string line = (i + 1).ToString();                               // Sample Nr.
            line += ";" + Voltages[i].ToString(new CultureInfo("en-US"));        // Voltage

            for (int j = 0; j < Currents.Count; j++)
                line += ";" + Currents[j][i].ToString(new CultureInfo("en-US")); // Current of Cell[j]

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

