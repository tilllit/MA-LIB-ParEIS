using ScottPlot;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
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
using System.Windows.Forms;

namespace EIS_Acquisition_Utilllity.Pages.TestBench
{
    /// <summary>
    /// Interaktionslogik für TB_Spectrum_Page.xaml
    /// </summary>
    public partial class TB_Spectrum_Page : Page
    {
        public TB_Spectrum_Page()
        {
            InitializeComponent();

            LB_Fequencys.ItemsSource = Acquisition_Config.TB_Sweeps.ToList();

            Test_Bench.Sweep_Session_Done += () => { System.Windows.MessageBox.Show("Successfully finished Sweep Session!", "Success!", MessageBoxButton.OK); };
        }

        private async void Run_Btn_Click(object sender, RoutedEventArgs e)
        {
            var conf = Acquisition_Config.TB_Sweeps[LB_Fequencys.SelectedIndex];

            byte lut = 0;
            switch (conf.LUT)
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

            Acquisition_Device.Load_Setup((byte)(0x20 + lut), conf.PSC, conf.ARR, conf.DAC_Amplitude, conf.DAC_Offset, conf.Periods, (UInt16)(0x2E0 + conf.E_Load_Speed));

            await Task.Delay(200);

            Acquisition_Device.Acquisition_Setup(conf.Samples, conf.NumCards, conf.Fsample, conf.Fserial, conf.Name);

            await Task.Delay(200);

            Acquisition_Device.Acquisition_START();
        }


        private void RunSweep_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Run path selection here!
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Test_Bench.SweepSessionPath = dialog.SelectedPath;
            }
            else return;

            Test_Bench.Sweeps.Clear();

            var sel = LB_Fequencys.SelectedItems;
            foreach (Acquisition_Config.SIN_Config item in sel)
                Test_Bench.Sweeps.Add(item);

            Test_Bench.StartSweepSession();
        }


        private void Stop_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.STOP();
        }













        private void ShowPlot_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.raw_win.Show();
            MainWindow.ch_win.Show();
            MainWindow.phy_win.Show();
        }

        private void HidePlot_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.raw_win.Hide();
            MainWindow.ch_win.Hide();
            MainWindow.phy_win.Hide();
        }


        private void RAW_Write_Btn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog openFileDlg = new Microsoft.Win32.SaveFileDialog();
            openFileDlg.FileName = "Acquisition_RAW_DATA_" + Acquisition_Device.Acq_Config.Name; // Default file name
            openFileDlg.DefaultExt = ".csv"; // Default file extension
            openFileDlg.Filter = "Data file (.csv)|*.csv"; // Filter files by extension

            // Process save file dialog box results
            if (openFileDlg.ShowDialog() == true)
            {
                Test_Bench.Write_RAW_Data(openFileDlg.FileName);
            }
        }

        public void RAW_Read_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".csv";
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Data files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDlg.ShowDialog() == true)
            {
                Test_Bench.Read_RAW_Data(openFileDlg.FileName);
            }
        }

        private void PHY_Write_Btn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog openFileDlg = new Microsoft.Win32.SaveFileDialog();
            openFileDlg.FileName = "Acquisition_PHY_DATA_" + Acquisition_Device.Acq_Config.Name; // Default file name
            openFileDlg.DefaultExt = ".csv"; // Default file extension
            openFileDlg.Filter = "Data file (.csv)|*.csv"; // Filter files by extension

            // Process save file dialog box results
            if (openFileDlg.ShowDialog() == true)
            {
                Test_Bench.Write_PHY_Data(openFileDlg.FileName);
            }
        }

        private void PHY_Read_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".csv";
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Data files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDlg.ShowDialog() == true)
            {
                Test_Bench.Read_PHY_Data(openFileDlg.FileName);
            }
        }





        private void Nyqu_New_Btn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog openFileDlg = new Microsoft.Win32.SaveFileDialog();
            openFileDlg.FileName = "Nyquist_"; // Default file name
            openFileDlg.DefaultExt = ".csv"; // Default file extension
            openFileDlg.Filter = "Data file (.csv)|*.csv"; // Filter files by extension

            // Process save file dialog box results
            if (openFileDlg.ShowDialog() == true)
            {
                List<string> Lines = new List<string>();

                // Create Header
                string header = "Frequency";
                for (int i = 0; i < 5/*Acquisition_Device.Currents.Count*/; i++)
                    header += ";Cell " + (i + 1) + " [Z Impedance]";
                Lines.Add(header);

                System.IO.File.WriteAllLines(openFileDlg.FileName, Lines);
            }
            EIS_Processing.ImpedanceFilePath = openFileDlg.FileName;
        }

        private void Nyqu_Process_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".csv";
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Data files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDlg.ShowDialog() == true)
            {
                var conf = Acquisition_Config.TB_Sweeps[LB_Fequencys.SelectedIndex];
                EIS_Processing.Process_Single_Frequency(conf, 5, openFileDlg.FileName, EIS_Processing.ImpedanceFilePath);
            }
        }

        private void Nyqu_Load_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".csv";
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "Data files (*.csv)|*.csv|All files (*.*)|*.*";


            if (openFileDlg.ShowDialog() == true)
            {
                EIS_Processing.ImpedanceFilePath = openFileDlg.FileName;

                // Update plot here or whatever....
            }
        }

        private void Nyqu_Plot_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.nyqu_win.Show();
            MainWindow.nyqu_win.DecodeData();
        }


    }
}
