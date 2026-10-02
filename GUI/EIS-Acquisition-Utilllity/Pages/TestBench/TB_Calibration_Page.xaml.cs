using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EIS_Acquisition_Utilllity.Pages.TestBench
{
    /// <summary>
    /// Interaktionslogik für TB_Calibration_Page.xaml
    /// </summary>
    public partial class TB_Calibration_Page : Page
    {
        DM3068 Multimeter = new DM3068();
        DeltaElektronika Supply = new DeltaElektronika();

        public static string Mode = "Voltage";

        System.Timers.Timer MM_Polling_Timer = new System.Timers.Timer();

        public static List<double> M_Voltage = new List<double>();


        public TB_Calibration_Page()
        {
            InitializeComponent();

            Multimeter.NewData += FeedPlot;

            MM_Polling_Timer.Elapsed += PollValue;
            MM_Polling_Timer.Interval = 1000;
            MM_Polling_Timer.Enabled = false;

            TB_Avg_Measure.MouseDoubleClick += (s, e) => { Clipboard.SetText(TB_Avg_Measure.Text); };
            TB_Avg_Channel.MouseDoubleClick += (s, e) => { Clipboard.SetText(TB_Avg_Channel.Text); };

            CB_Mode.SelectionChanged += (s, e) => { Mode = ((ComboBoxItem)CB_Mode.SelectedItem).Content.ToString(); };
        }



        private void PollValue(object sender, ElapsedEventArgs e)
        {
            //Multimeter.GetVoltageDC();
            //Multimeter.GetCurrentDC();

            if (Mode == "Voltage")
            {
                Multimeter.GetVoltageDC();
            }
            else if (Mode == "Current")
            {
                Multimeter.GetCurrentDC();
            }
        }

        private void FeedPlot(double val)
        {
            M_Voltage.Add(val);

            Calib_Plot.Reset();
            Calib_Plot.Plot.Add.Signal(M_Voltage);
            Calib_Plot.Plot.Axes.AutoScale();
            Calib_Plot.Plot.Title("Calibration Voltage");
            Calib_Plot.Plot.YLabel("Voltage [V]");
            Calib_Plot.Plot.XLabel("Samples");
            Calib_Plot.Plot.ScaleFactor = 2;
            Calib_Plot.Refresh();
        }

        private async void Cal_Finish()
        {
            MM_Polling_Timer.Enabled = false;
            Acquisition_Device.ACC_DATA_IN -= Cal_Finish;

            await Task.Delay(500);

            // Shutdown supply
            Supply.OutputDisable();
            Supply.SetVoltage(0.0);
            Supply.SetCurrent(0.0);

            // Do average data ....
            var CH_avg = Acquisition_Device.Channel_Data[CB_Cannel.SelectedIndex].Average();
            var M_avg = M_Voltage.Average();

            TB_Avg_Channel.Text = CH_avg.ToString();//.ToString(new CultureInfo("en-US"));
            TB_Avg_Measure.Text = M_avg.ToString();//.ToString(new CultureInfo("en-US"));
        }



        private async void MM_Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            MM_Connect_Btn.IsEnabled = false;

            string IP = TB_MM_IP.Text;
            UInt16 cmd_port = Convert.ToUInt16(TB_MM_Port.Text);

            if (await(Multimeter.Start(IP, cmd_port)))
            {
                MM_Connect_Btn.IsEnabled = false;
                MM_Disconnect_Btn.IsEnabled = true;
                MM_SinglePoll_Btn.IsEnabled = true;
            }
            else
            {
                MM_Connect_Btn.IsEnabled = true;
            }
        }

        private void MM_Disconnect_Btn_Click(object sender, RoutedEventArgs e)
        {
            Multimeter.Stop();

            MM_Connect_Btn.IsEnabled = true;
            MM_Disconnect_Btn.IsEnabled = false;
            MM_SinglePoll_Btn.IsEnabled = false;
        }

        private void MM_SinglePoll_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == "Voltage")
            {
                Multimeter.GetVoltageDC();
            }
            else if (Mode == "Current")
            {
                Multimeter.GetCurrentDC();
            }
        }





        private async void DE_Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            DE_Connect_Btn.IsEnabled = false;

            string IP = TB_DE_IP.Text;
            UInt16 cmd_port = Convert.ToUInt16(TB_DE_Port.Text);

            if (await(Supply.Start(IP, cmd_port)))
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
            Supply.SetVoltage(3.4);
            Supply.SetCurrent(1.9);

            Supply.OutputEnable();
        }

        private void DE_Disable_Btn_Click(object sender, RoutedEventArgs e)
        {
            Supply.OutputDisable();

            Supply.SetVoltage(0.0);
            Supply.SetCurrent(0.0);
        }

        private async void Cal_Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Cleanup Plot and Data
            M_Voltage.Clear();
            Calib_Plot.Reset();

            // Setup Supply
            double Voltage, Current;
            double.TryParse(TB_Voltage.Text, NumberStyles.Any, new CultureInfo("en-US"), out Voltage);
            double.TryParse(TB_Current.Text, NumberStyles.Any, new CultureInfo("en-US"), out Current);

            Supply.SetVoltage(Voltage);
            Supply.SetCurrent(Current);

            Supply.OutputEnable();


            // Setup Acquisition
            Acquisition_Device.Acq_Config.NrCards = 3;
            Acquisition_Device.Acq_Config.Fserial = 25_000_000;
            Acquisition_Device.Acq_Config.NrSamples = 2_000;
            Acquisition_Device.Acq_Config.Fsample = 100;
            Acquisition_Device.Acquisition_Setup();

            Acquisition_Device.ACC_DATA_IN += Cal_Finish;

            Acquisition_Device.Acquisition_START();

            await Task.Delay(100);

            // Start Multimeter
            MM_Polling_Timer.Enabled = true;
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

                Test_Bench.ReadCalibData(openFileDlg.FileName);

                MainWindow.SetCalibration("Calibrated");
            }
        }
    }
}
