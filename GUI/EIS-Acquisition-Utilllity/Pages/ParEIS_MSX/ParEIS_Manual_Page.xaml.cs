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

namespace EIS_Acquisition_Utilllity.Pages.ParEIS_MSX
{
    /// <summary>
    /// Interaktionslogik für ParEIS_Manual_Page.xaml
    /// </summary>
    public partial class ParEIS_Manual_Page : Page
    {
        public ParEIS_Manual_Page()
        {
            InitializeComponent();

            // E-Load Config BTN
            MainWindow.e_load_win.E_Load_Close += () => { ELoad_Win_Btn.IsEnabled = true; };
            MainWindow.e_load_win.E_Load_Open += () => { ELoad_Win_Btn.IsEnabled = false; };

            // RAW Plot BTN
            MainWindow.msx_raw_win.Win_Open += () => { ShowRAW_Plot_Btn.IsEnabled = false; };
            MainWindow.msx_raw_win.Win_Close += () => { ShowRAW_Plot_Btn.IsEnabled = true; };
        }





        // E-Load Btn's

        private void ELoad_Win_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.e_load_win.Open();
        }

        private void Start_ELoad_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.Load_START();
        }

        private void Stop_ELoad_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.STOP();
        }





        // Acquisition Btn's

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





        // Plot Btn's

        private void ShowRAW_Plot_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.msx_raw_win.Open();
        }
    }
}
