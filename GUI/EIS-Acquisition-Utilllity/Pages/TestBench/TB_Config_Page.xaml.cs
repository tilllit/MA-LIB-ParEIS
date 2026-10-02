using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace EIS_Acquisition_Utilllity.Pages.TestBench
{
    /// <summary>
    /// Interaktionslogik für TB_Config_Page.xaml
    /// </summary>
    public partial class TB_Config_Page : Page
    {

        public TB_Config_Page()
        {
            InitializeComponent();

            MainWindow.e_load_win.E_Load_Close += () => { ELoad_Win_Btn.IsEnabled = true; };
            MainWindow.e_load_win.E_Load_Open += () => { ELoad_Win_Btn.IsEnabled = false; };
        }




        private void Send_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.Acquisition_Setup();
            Acquisition_Device.Acquisition_START();
        }

        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.Acquisition_START();
        }

        private void ACC_Setup_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!UInt32.TryParse(TB_NrSamples.Text, out Acquisition_Device.Acq_Config.NrSamples))
                return;
            if (!UInt32.TryParse(TB_NrCards.Text, out Acquisition_Device.Acq_Config.NrCards))
                return;
            if (!UInt32.TryParse(TB_Fsample.Text, out Acquisition_Device.Acq_Config.Fsample))
                return;
            if (!UInt32.TryParse(TB_Fserial.Text, out Acquisition_Device.Acq_Config.Fserial))
                return;

            Acquisition_Device.Acquisition_Setup();
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
            openFileDlg.FileName = "Acquisition_DATA_"; // Default file name
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
                Console.WriteLine("Open File: " + openFileDlg.FileName);

                Test_Bench.Read_RAW_Data(openFileDlg.FileName);
            }
        }

        private void LockUnlock_Btn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)LockUnlock_Btn.Content == "Unlock")
            {
                LockUnlock_Btn.Content = "Lock";
                TB_NrCards.IsEnabled = true;
                TB_Fserial.IsEnabled = true;
            }
            else if ((string)LockUnlock_Btn.Content == "Lock")
            {
                LockUnlock_Btn.Content = "Unlock";
                TB_NrCards.IsEnabled = false;
                TB_Fserial.IsEnabled = false;
            }
        }

        private void Setup_ELoad_Btn_Click(object sender, RoutedEventArgs e)
        {
            //Acquisition_Device.Send("ELOAD-Setup");
            //Acquisition_Device.Load_Setup(0x20, 0, 14399, 40, 300, 10);
        }

        private void Start_ELoad_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.Load_START();
        }

        private void Stop_ELoad_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.STOP();
        }

        private void ELoad_Win_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.e_load_win.Open();
        }
    }
}
