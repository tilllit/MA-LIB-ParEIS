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
using System.Windows.Threading;
using EIS_Acquisition_Utilllity.Pages.TestBench;

namespace EIS_Acquisition_Utilllity.Pages
{
    /// <summary>
    /// Interaktionslogik für ConnectionPage.xaml
    /// </summary>
    public partial class ConnectionPage : Page
    {
        public delegate void ConsoleEvent(string s);
        public static event ConsoleEvent console_add;


        public ConnectionPage()
        {
            InitializeComponent();

            console_add += (s) => { LB_Console.Items.Insert(0, s); };
        }


        public static void AddToConsole(string payload)
        {
            console_add?.Invoke(payload);
        }


        private async void Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            Connect_Btn.IsEnabled = false;

            string IP = TB_IP.Text;
            UInt16 cmd_port = Convert.ToUInt16(TB_CMD_Port.Text);
            UInt16 data_port = Convert.ToUInt16(TB_DATA_Port.Text);

            if (await (Acquisition_Device.Connect(IP, cmd_port, data_port)))
            {
                Connect_Btn.IsEnabled = false;
                Disonnect_Btn.IsEnabled = true;
                Send_Btn.IsEnabled = true;
                MainWindow.SetConnection("Connected");
            }
            else
            {
                Connect_Btn.IsEnabled = true;
            }
        }

        private void Disonnect_Btn_Click(object sender, RoutedEventArgs e)
        {
            Acquisition_Device.Disconnect();

            Connect_Btn.IsEnabled = true;
            Disonnect_Btn.IsEnabled = false;
            Send_Btn.IsEnabled = false;
            MainWindow.SetConnection("Unconnected");
        }

        private void Send_Btn_Click(object sender, RoutedEventArgs e)
        {
            //string payload = TB_Send.Text;
            //Acquisition_Device.Send(payload);


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
    }
}
