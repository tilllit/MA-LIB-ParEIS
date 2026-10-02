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

namespace EIS_Acquisition_Utilllity.Pages.ParEIS_MSX
{
    /// <summary>
    /// Interaktionslogik für ParEIS_MSX_PYH_Window.xaml
    /// </summary>
    public partial class ParEIS_MSX_PHY_Window : Window
    {

        int scale_cnt = 2;
        List<double> scale_values = new List<double>() { 0.5, 1, 1.5, 2 };

        public ParEIS_MSX_PHY_Window()
        {
            InitializeComponent();

            ParEIS_M_SX.PHY_DATA_IN += UpdatePlots;

            R1.Height = new GridLength(1, GridUnitType.Star);
            R2.Height = new GridLength(1, GridUnitType.Star);
            R3.Height = new GridLength(0);
            R4.Height = new GridLength(0);
        }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void Open()
        {
            this.Show();
            //Win_Open?.Invoke();
        }


        private void Size_Btn_Click(object sender, RoutedEventArgs e)
        {
            scale_cnt++;
            if (scale_cnt == scale_values.Count)
                scale_cnt = 0;

            Plot1_1.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_1.Refresh();
            Plot1_2.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_2.Refresh();
            Plot1_3.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_3.Refresh();
            Plot2_1.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot2_1.Refresh();
        }

        private void Scale_Btn_Click(object sender, RoutedEventArgs e)
        {
            Plot1_1.Plot.Axes.AutoScale();
            Plot1_1.Refresh();
            Plot1_2.Plot.Axes.AutoScale();
            Plot1_2.Refresh();
            Plot1_3.Plot.Axes.AutoScale();
            Plot1_3.Refresh();
            Plot2_1.Plot.Axes.AutoScale();
            Plot2_1.Refresh();
        }



        public void UpdatePlots()
        {
            // Voltage plot
            Plot1_1.Reset();
            var sig = Plot1_1.Plot.Add.Signal(ParEIS_M_SX.Voltage);
            sig.Color = ScottPlot.Colors.DarkOrange;
            sig.LineWidth = 3;
            Plot1_1.Plot.YLabel("[V]");
            Plot1_1.Plot.Axes.AutoScale();

            if (ParEIS_M_SX.Voltage.Max() < 0)
                Plot1_1.Plot.Axes.SetLimitsY(ParEIS_M_SX.Voltage.Min() * 1.1, 0);
            else
                Plot1_1.Plot.Axes.SetLimitsY(0, ParEIS_M_SX.Voltage.Max() * 1.1);

            Plot1_1.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_1.Plot.Title("LZ Voltage");
            Plot1_1.Refresh();


            // Shunt plot
            Plot1_2.Reset();
            var sig2 = Plot1_2.Plot.Add.Signal(ParEIS_M_SX.Shunt);
            sig2.Color = ScottPlot.Colors.DarkGreen;
            Plot1_2.Plot.YLabel("[A]");
            Plot1_2.Plot.Axes.AutoScale();

            Plot1_2.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_2.Plot.Title("LZ Current");
            Plot1_2.Refresh();


            // Temp plot
            Plot1_3.Reset();
            var sig3 = Plot1_3.Plot.Add.Signal(ParEIS_M_SX.Temperature);
            sig3.Color = ScottPlot.Colors.DarkViolet;
            Plot1_3.Plot.YLabel("[°C]");
            Plot1_3.Plot.Axes.AutoScale();

            Plot1_3.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_3.Plot.Title("NTC Temperature");
            Plot1_3.Refresh();


            // Current plot
            Plot2_1.Reset();
            for (int i = 0; i < ParEIS_M_SX.Currents.Count; i++)
                Plot2_1.Plot.Add.Signal(ParEIS_M_SX.Currents[i]);
            
            Plot2_1.Plot.YLabel("[A]");
            Plot2_1.Plot.Axes.AutoScale();

            Plot2_1.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot2_1.Plot.Title("Cell currents");
            Plot2_1.Refresh();



            this.Show();
        }






        private void Single_Btn_Click(object sender, RoutedEventArgs e)
        {
            Single_Btn.IsEnabled = false;
            ALL_Btn.IsEnabled = true;
            Current_Btn.IsEnabled = true;

            R1.Height = new GridLength(1, GridUnitType.Star);
            R2.Height = new GridLength(1, GridUnitType.Star);
            R3.Height = new GridLength(0);
            R4.Height = new GridLength(0);
        }

        private void ALL_Btn_Click(object sender, RoutedEventArgs e)
        {
            Single_Btn.IsEnabled = true;
            ALL_Btn.IsEnabled = false;
            Current_Btn.IsEnabled = true;

            R1.Height = new GridLength(0);
            R2.Height = new GridLength(0);
            R3.Height = new GridLength(1, GridUnitType.Star);
            R4.Height = new GridLength(0);
        }

        private void Current_Btn_Click(object sender, RoutedEventArgs e)
        {
            Single_Btn.IsEnabled = true;
            ALL_Btn.IsEnabled = true;
            Current_Btn.IsEnabled = false;

            R1.Height = new GridLength(0);
            R2.Height = new GridLength(0);
            R3.Height = new GridLength(0);
            R4.Height = new GridLength(1, GridUnitType.Star);
        }

    }
}
