using ScottPlot;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EIS_Acquisition_Utilllity.Pages.TestBench
{
    /// <summary>
    /// Interaktionslogik für TB_PHY_Values_Window.xaml
    /// </summary>
    public partial class TB_PHY_Values_Window : Window
    {

        List<ScottPlot.WPF.WpfPlot> plots;

        int scale_cnt = 2;
        List<double> scale_values = new List<double>() { 0.5, 1, 1.5, 2 };


        public TB_PHY_Values_Window()
        {
            InitializeComponent();

            Test_Bench.PHY_DATA_IN += ShowData;


            plots = new List<ScottPlot.WPF.WpfPlot>() { Plot1_1, Plot1_2, Plot1_3,
                                                        Plot2_1, Plot2_2, Plot2_3,
                                                        Plot3_1, Plot3_2, Plot3_3 };


            R1.Height = new GridLength(1, GridUnitType.Star);
            R2.Height = new GridLength(1, GridUnitType.Star);
            R3.Height = new GridLength(1, GridUnitType.Star);
            R4.Height = new GridLength(0);
            R5.Height = new GridLength(0);
        }


        public void ShowData()
        {

            // Voltage plot
            Plot1_1.Reset();
            var sig = Plot1_1.Plot.Add.Signal(Test_Bench.Voltages);
            sig.Color = ScottPlot.Colors.DarkOrange;
            sig.LineWidth = 3;
            Plot1_1.Plot.YLabel("[V]");
            Plot1_1.Plot.Axes.AutoScale();

            if (Test_Bench.Voltages.Max() < 0)
                Plot1_1.Plot.Axes.SetLimitsY(Test_Bench.Voltages.Min() * 1.1, 0);
            else
                Plot1_1.Plot.Axes.SetLimitsY(0, Test_Bench.Voltages.Max() * 1.1);

            Plot1_1.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot1_1.Plot.Title("LZ Voltage");
            Plot1_1.Refresh();


            // Current plots
            var Cplots = new List<ScottPlot.WPF.WpfPlot>() { Plot1_2, Plot1_3, Plot2_1, Plot2_2, Plot2_3 };
            for (int i = 0; i < Test_Bench.Currents.Count; i++)
            {
                List<double> curr = Test_Bench.Currents[i].ToList();

                Cplots[i].Reset();
                Cplots[i].Plot.Add.Signal(curr);
                Cplots[i].Plot.YLabel("[A]");
                Cplots[i].Plot.Axes.AutoScale();
                Cplots[i].Plot.ScaleFactor = scale_values[scale_cnt];
                Cplots[i].Plot.Title("Current Cell " + (i + 1).ToString());
                Cplots[i].Refresh();
            }

            HandleCombinedPlot();
            HandleCurrentPlot();

            this.Show();
        }

        private void HandleCombinedPlot()
        {
            Plot_ALL.Reset();
            Plot_ALL.Plot.Title("Combined PHY Values");
            var AX = Plot_ALL.Plot.Axes.AddRightAxis();

            var sig2 = Plot_ALL.Plot.Add.Signal(Test_Bench.Voltages);
            sig2.LegendText = "Voltage";
            sig2.Color = ScottPlot.Colors.DarkOrange;
            sig2.Axes.YAxis = AX;
            sig2.LineWidth = 3;

            if (Test_Bench.Voltages.Max() < 0)
            {
                AX.Max = 0;
                AX.Min = Test_Bench.Voltages.Min() * 1.1;
            }
            else
            {
                AX.Min = 0;
                AX.Max = Test_Bench.Voltages.Min() * 1.1;
            }

            for (int i = 0; i < Test_Bench.Currents.Count; i++)
            {
                List<double> curr = Test_Bench.Currents[i].ToList();

                var sigC = Plot_ALL.Plot.Add.Signal(curr);
                sigC.Axes.YAxis = Plot_ALL.Plot.Axes.Left;
                sigC.LegendText = "Cell " + (i + 1);
            }

            //Plot_ALL.Plot.YLabel("[A]");
            AX.LabelText = "Cell Voltage [V]";
            Plot_ALL.Plot.Axes.Left.Label.Text = "Cell Currents [A]";
            Plot_ALL.Plot.Legend.Alignment = Alignment.LowerCenter;
            Plot_ALL.Plot.Legend.Orientation = ScottPlot.Orientation.Horizontal;
            Plot_ALL.Plot.Axes.AutoScaleX();
            Plot_ALL.Plot.Axes.AutoScaleY();
            Plot_ALL.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot_ALL.Refresh();
        }


        private void HandleCurrentPlot()
        {
            Plot_Current.Reset();
            Plot_Current.Plot.Title("Current PHY Values");

            var shunt = Plot_Current.Plot.Add.Signal(Test_Bench.ShuntCurrent);
            shunt.LegendText = "Shunt";
            shunt.Color = ScottPlot.Colors.DarkOrange;
            shunt.LineWidth = 2;

            List<double> currSum = new List<double>();
            for (int i = 0; i < Test_Bench.Currents[0].Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < Test_Bench.Currents.Count; j++)
                {
                    sum += Test_Bench.Currents[j][i];
                }
                currSum.Add(sum);
            }

            var Csum = Plot_Current.Plot.Add.Signal(currSum);
            Csum.LegendText = "Current sum";
            Csum.Color = ScottPlot.Colors.Blue;
            Csum.LineWidth = 2;

            Plot_Current.Plot.Axes.Left.Label.Text = "Current [A]";
            Plot_Current.Plot.Legend.Alignment = Alignment.LowerCenter;
            Plot_Current.Plot.Legend.Orientation = ScottPlot.Orientation.Horizontal;
            Plot_Current.Plot.Axes.AutoScaleX();
            Plot_Current.Plot.Axes.AutoScaleY();
            Plot_Current.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot_Current.Refresh();
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

            Plot_ALL.Plot.Axes.AutoScale();
            Plot_ALL.Refresh();

            Plot_Current.Plot.Axes.AutoScale();
            Plot_Current.Refresh();
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

            Plot_ALL.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot_ALL.Refresh();

            Plot_Current.Plot.ScaleFactor = scale_values[scale_cnt];
            Plot_Current.Refresh();
        }

        private void Single_Btn_Click(object sender, RoutedEventArgs e)
        {
            Single_Btn.IsEnabled = false;
            ALL_Btn.IsEnabled = true;
            Current_Btn.IsEnabled = true;

            R1.Height = new GridLength(1, GridUnitType.Star);
            R2.Height = new GridLength(1, GridUnitType.Star);
            R3.Height = new GridLength(1, GridUnitType.Star);
            R4.Height = new GridLength(0);
            R5.Height = new GridLength(0);
        }

        private void ALL_Btn_Click(object sender, RoutedEventArgs e)
        {
            Single_Btn.IsEnabled = true;
            ALL_Btn.IsEnabled = false;
            Current_Btn.IsEnabled = true;

            R1.Height = new GridLength(0);
            R2.Height = new GridLength(0);
            R3.Height = new GridLength(0);
            R4.Height = new GridLength(1, GridUnitType.Star);
            R5.Height = new GridLength(0);
        }

        private void Current_Btn_Click(object sender, RoutedEventArgs e)
        {
            Single_Btn.IsEnabled = true;
            ALL_Btn.IsEnabled = true;
            Current_Btn.IsEnabled = false;

            R1.Height = new GridLength(0);
            R2.Height = new GridLength(0);
            R3.Height = new GridLength(0);
            R4.Height = new GridLength(0);
            R5.Height = new GridLength(1, GridUnitType.Star);
        }
    }
}
