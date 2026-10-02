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
using System.Windows.Shapes;

namespace EIS_Acquisition_Utilllity.Pages
{
    /// <summary>
    /// Interaktionslogik für PythonSettingsWindow.xaml
    /// </summary>
    public partial class PythonSettingsWindow : Window
    {
        public static bool errShow = false;
        public static string errMsg = "";

        public PythonSettingsWindow()
        {
            InitializeComponent();

            Python.RX += RX_Data;
            Python.Finished += (s) => { MessageBox.Show("Py Script done!", "Finished successfully!", MessageBoxButton.OK); };
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }


        public void addToConsole(string payload, UInt16 range = 0)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Console_LB.Items.Insert(0, payload);

                if (Console_LB.Items.Count > range && range != 0)
                {
                    Console_LB.Items.RemoveAt(Console_LB.Items.Count - 1);
                }
            }));
        }

        private void RX_Data(string payload)
        {
            try
            {
                if (payload != null)
                {
                    //Console.WriteLine(e.Data.ToString());
                    addToConsole(payload, 50);
                }
            }
            catch { return; }
        }














        private void Installation_Btn_Click(object sender, RoutedEventArgs e)
        {
            bool res = Python.Installed();
            Install_LB.Content = res.ToString();
            if (res)
            {
                Install_LB.FontWeight = FontWeights.Bold;
                Install_LB.Foreground = Brushes.LimeGreen;
            }
            else
            {
                Install_LB.FontWeight = FontWeights.Bold;
                Install_LB.Foreground = Brushes.Red;
            }
        }

        private void Version_Btn_Click(object sender, RoutedEventArgs e)
        {
            Version_LB.Content = Python.GetVersion();
        }

        private void Requ_Btn_Click(object sender, RoutedEventArgs e)
        {
            Python.debug = true;
            string defaultPath = AppDomain.CurrentDomain.BaseDirectory;
            //string path = System.IO.Path.Combine(defaultPath, "Python", "Test.py");
            string path = System.IO.Path.Combine(defaultPath, "Python\\Install.py");
            Python.Run(path);
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            Console_LB.Items.Clear();
        }
    }
}
