using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EIS_Acquisition_Utilllity.Pages.ParEIS_MSX
{
    /// <summary>
    /// Interaktionslogik für ParEIS_MSX_Calibration_Page.xaml
    /// </summary>
    public partial class ParEIS_MSX_Calibration_Page : Page
    {
        DeltaElektronika Supply = new DeltaElektronika();
        public static string Mode = "Current";


        List<double> voltages = new List<double>();
        List<double> currents = new List<double>();
        List<List<double>> ChannelPoints = new List<List<double>>();

        List<double> Gains = new List<double>();
        List<double> Offsets = new List<double>();


        public ParEIS_MSX_Calibration_Page()
        {
            InitializeComponent();

            CB_Mode.SelectionChanged += (s, e) => { Mode = ((ComboBoxItem)CB_Mode.SelectedItem).Content.ToString(); };
        }

        // ################ Source ################

        private async void DE_Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            DE_Connect_Btn.IsEnabled = false;

            string IP = TB_DE_IP.Text;
            UInt16 cmd_port = Convert.ToUInt16(TB_DE_Port.Text);

            if (await (Supply.Start(IP, cmd_port)))
            {
                DE_Connect_Btn.IsEnabled = false;
                DE_Disconnect_Btn.IsEnabled = true;
                DE_Enable_Btn.IsEnabled = true;
                DE_Disable_Btn.IsEnabled = true;
            }
            else
            {
                DE_Connect_Btn.IsEnabled = true;
            }
        }

        private void DE_Disconnect_Btn_Click(object sender, RoutedEventArgs e)
        {
            Supply.Stop();

            DE_Connect_Btn.IsEnabled = true;
            DE_Disconnect_Btn.IsEnabled = false;

            DE_Enable_Btn.IsEnabled = false;
            DE_Disable_Btn.IsEnabled = false;
        }

        private void DE_Enable_Btn_Click(object sender, RoutedEventArgs e)
        {
            Supply.SetVoltage(1.1);
            Supply.SetCurrent(0.5);

            Supply.OutputEnable();
        }

        private void DE_Disable_Btn_Click(object sender, RoutedEventArgs e)
        {
            Supply.OutputDisable();

            Supply.SetVoltage(0.0);
            Supply.SetCurrent(0.0);
        }




        private void Cal_Start_Btn_Click(object sender, RoutedEventArgs e)
        {

            // Setup Supply
            double Voltage, Current;
            double.TryParse(TB_Voltage.Text, NumberStyles.Any, new CultureInfo("en-US"), out Voltage);
            double.TryParse(TB_Current.Text, NumberStyles.Any, new CultureInfo("en-US"), out Current);

            Supply.SetVoltage(Voltage);
            Supply.SetCurrent(Current);

            Supply.OutputEnable();


            // Setup Acquisition
            Acquisition_Device.Acq_Config.NrCards = 26;
            Acquisition_Device.Acq_Config.Fserial = 15_000_000;
            Acquisition_Device.Acq_Config.NrSamples = 2_000;
            Acquisition_Device.Acq_Config.Fsample = 500;
            Acquisition_Device.Acquisition_Setup();

            Acquisition_Device.Acquisition_START();
        }

        private void CalNew_Btn_Click(object sender, RoutedEventArgs e)
        {
            voltages.Clear();
            currents.Clear();
            ChannelPoints.Clear();

            for (int i = 0; i < 78; i++)
            {
                ChannelPoints.Add(new List<double>());
            }
        }

        private void ChooseCal_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == "Current")
            {
                // Setup Supply
                double Current;
                double.TryParse(TB_Current.Text, NumberStyles.Any, new CultureInfo("en-US"), out Current);
                currents.Add(Current);

                for (int i = 0; i < 74; i++)
                {
                    ChannelPoints[i].Add(ParEIS_M_SX.SortedChannels[i].Average());
                }
            }
            if (Mode == "Voltage")
            {
                double Voltage;
                double.TryParse(TB_Voltage.Text, NumberStyles.Any, new CultureInfo("en-US"), out Voltage);
                voltages.Add(Voltage);

                ChannelPoints[75].Add(ParEIS_M_SX.SortedChannels[75].Average());
            }
        }

        private void CalcCalib_Btn_Click(object sender, RoutedEventArgs e)
        {
            Gains.Clear();
            Offsets.Clear();

            if ((ChannelPoints.Count > 0) && (ChannelPoints[0].Count == 2) && (ChannelPoints[75].Count == 2))
            {
                // Current Channels
                for (int i = 0; i < 74; i++)
                {
                    Gains.Add((currents.Max() - currents.Min()) / (ChannelPoints[i].Max() - ChannelPoints[i].Min()));
                    Offsets.Add(currents.Min() - Gains[i] * ChannelPoints[i].Min());
                }

                // Empty Channel 75
                Gains.Add(0.0);
                Offsets.Add(0.0);

                // Voltage Channel
                Gains.Add((voltages.Max() - voltages.Min()) / (ChannelPoints[75].Max() - ChannelPoints[75].Min()));
                Offsets.Add(voltages.Min() - Gains[75] * ChannelPoints[75].Min());

                // Empty Channel 77 Shunt?
                Gains.Add(0.0);
                Offsets.Add(0.0);

                // Empty Channel 78 Temp?
                Gains.Add(0.0);
                Offsets.Add(0.0);
            }

            // Write Calibration File
            List<string> Lines = new List<string>();

            string header = "Channel;Factor;Offset";
            Lines.Add(header);
            for (int i = 0; i < Gains.Count; i++)
            {
                Lines.Add((i + 1) + ";" + Gains[i].ToString(new CultureInfo("en-US")) + ";" + Offsets[i].ToString(new CultureInfo("en-US")));
            }

            Microsoft.Win32.SaveFileDialog openFileDlg = new Microsoft.Win32.SaveFileDialog();
            openFileDlg.FileName = "ParEIS_Calib_LZ_"; // Default file name
            openFileDlg.DefaultExt = ".csv"; // Default file extension
            openFileDlg.Filter = "Data file (.csv)|*.csv"; // Filter files by extension

            // Process save file dialog box results
            if (openFileDlg.ShowDialog() == true)
            {
                //ParEIS_M_SX.Write_PHY_Data(openFileDlg.FileName);
                System.IO.File.WriteAllLines(openFileDlg.FileName, Lines);
            }
        }

        private void Load_Config_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".csv";
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Data files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDlg.ShowDialog() == true)
            {
                Console.WriteLine("Open File: " + openFileDlg.FileName);

                ParEIS_M_SX.ReadCalibData(openFileDlg.FileName);

                MainWindow.SetCalibration("Calibrated");
            }
        }
    }
}
