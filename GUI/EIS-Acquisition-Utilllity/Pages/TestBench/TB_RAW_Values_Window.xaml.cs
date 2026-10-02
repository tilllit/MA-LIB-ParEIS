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
    /// Interaktionslogik für TB_RAW_Values_Window.xaml
    /// </summary>
    public partial class TB_RAW_Values_Window : Window
    {


        int scale_cnt = 3;
        List<double> scale_values = new List<double>() { 0.5, 1, 1.5, 2 };



        public TB_RAW_Values_Window()
        {
            InitializeComponent();

            Test_Bench.RAW_DATA_IN += DecodeData;
        }






        public void DecodeData()
        {

            // Decode into channels here???

            RAW_Plot.Reset();

            for (int i = 0; i < Acquisition_Device.Channel_Data.Count; i++)
            {
                var sig = RAW_Plot.Plot.Add.Signal(Acquisition_Device.Channel_Data[i].ToArray());
                //var sig = RAW_Plot.Plot.Add.Signal(Acquisition_Device.RAW_Channel_Data[i].ToArray());
                sig.LegendText = "Ch. " + (i + 1);
            }

            RAW_Plot.Plot.Title("RAW Values");
            RAW_Plot.Plot.ShowLegend(Alignment.LowerCenter, ScottPlot.Orientation.Horizontal);

            RAW_Plot.Plot.ScaleFactor = scale_values[scale_cnt];
            RAW_Plot.Plot.Axes.AutoScale();
            RAW_Plot.Refresh();

            this.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Scale_Btn_Click(object sender, RoutedEventArgs e)
        {
            RAW_Plot.Plot.Axes.AutoScale();
            RAW_Plot.Refresh();
        }

        private void Size_Btn_Click(object sender, RoutedEventArgs e)
        {
            RAW_Plot.Plot.ScaleFactor = scale_values[scale_cnt];

            scale_cnt++;
            if (scale_cnt == scale_values.Count)
                scale_cnt = 0;

            RAW_Plot.Refresh();
        }





    }
}
