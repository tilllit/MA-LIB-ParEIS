using ScottPlot;
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

namespace EIS_Acquisition_Utilllity.Pages.TestBench
{
    /// <summary>
    /// Interaktionslogik für TB_Channels_Window.xaml
    /// </summary>
    public partial class TB_Channels_Window : Window
    {
        List<ScottPlot.WPF.WpfPlot> plots;

        int scale_cnt = 3;
        List<double> scale_values = new List<double>() { 0.5, 1, 1.5, 2 };

        public TB_Channels_Window()
        {
            InitializeComponent();

            plots = new List<ScottPlot.WPF.WpfPlot>() { Plot1_1, Plot1_2, Plot1_3, 
                                                        Plot2_1, Plot2_2, Plot2_3, 
                                                        Plot3_1, Plot3_2, Plot3_3 };

            //Acquisition_Device.ACC_DATA_IN += DecodeData;
            Test_Bench.RAW_DATA_IN += DecodeData;
        }




        public void DecodeData()
        {
            try
            {
                // Decode into channels here???
                for (int i = 0; i < plots.Count; i++)
                {
                    plots[i].Reset();
                    plots[i].Plot.Add.Signal(Acquisition_Device.Channel_Data[i].ToArray());
                    plots[i].Plot.Title("Channel " + (i + 1));
                    plots[i].Plot.ScaleFactor = scale_values[scale_cnt];
                    plots[i].Plot.Axes.AutoScale();
                    plots[i].Refresh();

                    this.Show();
                }
            }
            catch{ }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }


        private void Scale_Btn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < plots.Count; i++)
            {
                plots[i].Plot.Axes.AutoScale();
                plots[i].Refresh();
            }
        }

        private void Size_Btn_Click(object sender, RoutedEventArgs e)
        {
            scale_cnt++;
            if (scale_cnt == scale_values.Count)
                scale_cnt = 0;

            for (int i = 0; i < plots.Count; i++)
            {
                plots[i].Plot.ScaleFactor = scale_values[scale_cnt];
                plots[i].Refresh();
            }
        }


    }
}
