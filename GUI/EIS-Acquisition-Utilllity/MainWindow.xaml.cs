using EIS_Acquisition_Utilllity.Pages;
using EIS_Acquisition_Utilllity.Pages.ParEIS_MSX;
using EIS_Acquisition_Utilllity.Pages.TestBench;
using OpenTK.Graphics.OpenGL;
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

namespace EIS_Acquisition_Utilllity
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //For status bar update
        public delegate void StatusUpdateEvent(string s);
        public static event StatusUpdateEvent StatusUpdate;
        public static event StatusUpdateEvent ConnectionUpdate;
        public static event StatusUpdateEvent CalibrationUpdate;

        List<Button> buttons;


        public PythonSettingsWindow py_window = new PythonSettingsWindow();
        public static E_Load_Window e_load_win = new E_Load_Window();

        ConnectionPage conn_page = new ConnectionPage();
        TB_Config_Page tb_config_page = new TB_Config_Page();
        TB_Spectrum_Page tb_spect_page = new TB_Spectrum_Page();
        TB_Calibration_Page tb_cal_page = new TB_Calibration_Page();


        // Test Bench test windows
        public static TB_RAW_Values_Window raw_win = new TB_RAW_Values_Window();
        public static TB_Channels_Window ch_win = new TB_Channels_Window();
        public static TB_PHY_Values_Window phy_win = new TB_PHY_Values_Window();
        public static TB_Nyqu_Window nyqu_win = new TB_Nyqu_Window();

        // ParEIS Model S/X Elements
        public static ParEIS_MSX_RAW_Window msx_raw_win = new ParEIS_MSX_RAW_Window();
        public static ParEIS_MSX_PHY_Window msx_phy_win = new ParEIS_MSX_PHY_Window();
        public static ParEIS_Manual_Page msx_manual = new ParEIS_Manual_Page();
        public static ParEIS_MSX_Spectrum_Page msx_spectrum = new ParEIS_MSX_Spectrum_Page();
        public static ParEIS_MSX_NYQU_Window msx_nyqu_win = new ParEIS_MSX_NYQU_Window();
        public static ParEIS_MSX_Calibration_Page msx_cal = new ParEIS_MSX_Calibration_Page();





        public MainWindow()
        {
            InitializeComponent();

            buttons = new List<Button>() { Connect_Btn, TB_Conig_Btn, TB_Spectrum_Btn, TB_Calibration_Btn, MSX_Conig_Btn, MSX_Spectrum_Btn, MSX_Calibration_Btn };

            Main_Frame.Content = conn_page;

            //Acquisition_Device.ACC_DATA_IN += () => raw_win.Show();
            //Acquisition_Device.ACC_DATA_IN += () => ch_win.Show();
            //Acquisition_Device.ACC_DATA_IN += () => phy_win.Show();

            py_window.Closing += (s, e) => { Python_Btn.IsEnabled = true; };

            StatusUpdate += (s) => { LB_Status.Content = s; };
            ConnectionUpdate += (s) => { LB_Connecion_Status.Content = s; };
            CalibrationUpdate += (s) => { LB_Calibration_Status.Content = s; };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Python_Btn_Click(object sender, RoutedEventArgs e)
        {
            Python_Btn.IsEnabled = false;
            py_window.Show();
        }

        public static void SetStatus(string msg)
        {
            StatusUpdate?.Invoke(msg);
        }

        public static void SetConnection(string msg)
        {
            ConnectionUpdate?.Invoke(msg);
        }

        public static void SetCalibration(string msg)
        {
            CalibrationUpdate?.Invoke(msg);
        }




        // Connection

        private void Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            Main_Frame.Content = conn_page;

            foreach (var b in buttons)
                b.IsEnabled = true;

            Connect_Btn.IsEnabled = false;
        }



        // ############### TEST BENCH ###############

        private void TB_Conig_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Handle event subscription
            Acquisition_Device.ACC_DATA_IN -= Test_Bench.DecodeData;
            Acquisition_Device.ACC_DATA_IN -= ParEIS_M_SX.DecodeData;
            Acquisition_Device.ACC_DATA_IN += Test_Bench.DecodeData;

            Main_Frame.Content = tb_config_page;

            foreach (var b in buttons)
                b.IsEnabled = true;

            TB_Conig_Btn.IsEnabled = false;
        }

        private void TB_Spectrum_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Handle event subscription
            Acquisition_Device.ACC_DATA_IN -= Test_Bench.DecodeData;
            Acquisition_Device.ACC_DATA_IN -= ParEIS_M_SX.DecodeData;
            Acquisition_Device.ACC_DATA_IN += Test_Bench.DecodeData;

            Main_Frame.Content = tb_spect_page;

            foreach (var b in buttons)
                b.IsEnabled = true;

            TB_Spectrum_Btn.IsEnabled = false;
        }

        private void TB_Calibration_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Handle event subscription
            Acquisition_Device.ACC_DATA_IN -= Test_Bench.DecodeData;
            Acquisition_Device.ACC_DATA_IN -= ParEIS_M_SX.DecodeData;
            Acquisition_Device.ACC_DATA_IN += Test_Bench.DecodeData;

            Main_Frame.Content = tb_cal_page;

            foreach (var b in buttons)
                b.IsEnabled = true;

            TB_Calibration_Btn.IsEnabled = false;
        }







        // ############### ParEIS Model S/X ###############

        private void MSX_Conig_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Handle event subscription
            Acquisition_Device.ACC_DATA_IN -= Test_Bench.DecodeData;
            Acquisition_Device.ACC_DATA_IN -= ParEIS_M_SX.DecodeData;
            Acquisition_Device.ACC_DATA_IN += ParEIS_M_SX.DecodeData;

            Main_Frame.Content = msx_manual;

            foreach (var b in buttons)
                b.IsEnabled = true;

            MSX_Conig_Btn.IsEnabled = false;
        }

        private void MSX_Spectrum_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Handle event subscription
            Acquisition_Device.ACC_DATA_IN -= Test_Bench.DecodeData;
            Acquisition_Device.ACC_DATA_IN -= ParEIS_M_SX.DecodeData;
            Acquisition_Device.ACC_DATA_IN += ParEIS_M_SX.DecodeData;

            Main_Frame.Content = msx_spectrum;

            foreach (var b in buttons)
                b.IsEnabled = true;

            MSX_Spectrum_Btn.IsEnabled = false;
        }

        private void MSX_Calibration_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Handle event subscription
            Acquisition_Device.ACC_DATA_IN -= Test_Bench.DecodeData;
            Acquisition_Device.ACC_DATA_IN -= ParEIS_M_SX.DecodeData;
            Acquisition_Device.ACC_DATA_IN += ParEIS_M_SX.DecodeData;

            Main_Frame.Content = msx_cal;

            foreach (var b in buttons)
                b.IsEnabled = true;

            MSX_Calibration_Btn.IsEnabled = false;
        }
    }
}
