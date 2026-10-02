using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaktionslogik für ParEIS_MSX_NYQU_Window.xaml
    /// </summary>
    public partial class ParEIS_MSX_NYQU_Window : Window
    {

        int scale_cnt = 3;
        List<double> scale_values = new List<double>() { 0.5, 1, 1.5, 2 };

        List<List<Complex>> impedances = new List<List<Complex>>();

        public ParEIS_MSX_NYQU_Window()
        {
            InitializeComponent();

            LV_Cells.SelectionChanged += (s, e) => { UpdateData(); };
        }



        public void DecodeData()
        {
            Read_Imp_Data();

            Nyqu_Plot.Reset();

            var cells = Enumerable.Range(1, impedances.Count).ToList();
            LV_Cells.ItemsSource = cells;

            for (int i = 0; i < impedances.Count; i++)
            {
                var sig = Nyqu_Plot.Plot.Add.Scatter(impedances[i].Select(z => z.Real * 1000).ToArray(), (impedances[i].Select(z => z.Imaginary*1000).ToArray()));
                //var sig = Nyqu_Plot.Plot.Add.Signal(Acquisition_Device.Channel_Data[i].ToArray());
                //var sig = RAW_Plot.Plot.Add.Signal(Acquisition_Device.RAW_Channel_Data[i].ToArray());
                sig.LegendText = "Cell " + (i + 1);
            }

            Nyqu_Plot.Plot.Title("Impedance (Nyquist)");
            Nyqu_Plot.Plot.ShowLegend(Alignment.LowerCenter, ScottPlot.Orientation.Horizontal);

            Nyqu_Plot.Plot.ScaleFactor = scale_values[scale_cnt];
            Nyqu_Plot.Plot.XLabel("Z' (Real) [mΩ]");
            Nyqu_Plot.Plot.YLabel("Z'' (Imag) [mΩ]");
            Nyqu_Plot.Plot.Axes.AutoScaler.InvertedY = true;
            Nyqu_Plot.Refresh();
        }

        public void UpdateData()
        {
            Nyqu_Plot.Reset();

            foreach (var item in LV_Cells.SelectedItems)
            {
                int index = LV_Cells.Items.IndexOf(item);
                var sig = Nyqu_Plot.Plot.Add.Scatter(impedances[index].Select(z => z.Real*1000).ToArray(), (impedances[index].Select(z => z.Imaginary*1000).ToArray()));
                sig.LegendText = "Cell " + (index + 1);
            }

            Nyqu_Plot.Plot.Title("Impedance (Nyquist)");
            Nyqu_Plot.Plot.ShowLegend(Alignment.LowerCenter, ScottPlot.Orientation.Horizontal);

            Nyqu_Plot.Plot.ScaleFactor = scale_values[scale_cnt];
            Nyqu_Plot.Plot.XLabel("Z' (Real) [mΩ]");
            Nyqu_Plot.Plot.YLabel("Z'' (Imag) [mΩ]");
            Nyqu_Plot.Plot.Axes.AutoScaler.InvertedY = true;
            Nyqu_Plot.Refresh();
        }


        public void Read_Imp_Data()
        {
            if (EIS_Processing.ImpedanceFilePath == "")
                return;

            string payload = System.IO.File.ReadAllText(EIS_Processing.ImpedanceFilePath);
            List<string> Lines = System.IO.File.ReadLines(EIS_Processing.ImpedanceFilePath).ToList();

            impedances.Clear();


            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i] = Lines[i].Replace("nan", "0");
                var s = Lines[i].Split(';');

                if (i == 0)
                {
                    for (int j = 0; j < s.Length - 1; j++)
                        impedances.Add(new List<Complex>());
                }

                else
                {
                    for (int j = 1; j < s.Length; j++)
                    {
                        var imp = ParsePythonComplex(s[j]);
                        impedances[j - 1].Add(imp);
                    }
                }
            }

        }




        public static Complex ParsePythonComplex(string value)
        {
            //Match match = Regex.Match(
            //    value,
            //    @"^\(\s*" +
            //    @"(?<real>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)" +
            //    @"\s*" +
            //    @"(?<imag>[+-](?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)" +
            //    @"j\s*\)$"
            //);

            //if (!match.Success)
            //{
            //    throw new FormatException(
            //        "Ungültige komplexe Zahl: " + value
            //    );
            //}

            //double real = double.Parse(
            //    match.Groups["real"].Value,
            //    CultureInfo.InvariantCulture
            //);

            //double imaginary = double.Parse(
            //    match.Groups["imag"].Value,
            //    CultureInfo.InvariantCulture
            //);

            //return new Complex(real, imaginary);


            value = value.Trim();

            // Optionale Klammern entfernen
            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                value = value.Substring(1, value.Length - 2).Trim();
            }

            // Nur Imaginärteil, z.B. "0j", "1.5j", "-2.3e-5j"
            Match imagOnly = Regex.Match(
                value,
                @"^(?<imag>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)j$"
            );

            if (imagOnly.Success)
            {
                double imaginary = double.Parse(
                    imagOnly.Groups["imag"].Value,
                    CultureInfo.InvariantCulture
                );

                return new Complex(0.0, imaginary);
            }

            // Realteil + Imaginärteil, z.B. "1.2-3.4j"
            Match complex = Regex.Match(
                value,
                @"^(?<real>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)" +
                @"\s*" +
                @"(?<imag>[+-](?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)j$"
            );

            if (complex.Success)
            {
                double real = double.Parse(
                    complex.Groups["real"].Value,
                    CultureInfo.InvariantCulture
                );

                double imaginary = double.Parse(
                    complex.Groups["imag"].Value,
                    CultureInfo.InvariantCulture
                );

                return new Complex(real, imaginary);
            }

            // Nur Realteil, z.B. "3.14"
            if (double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double realOnly))
            {
                return new Complex(realOnly, 0.0);
            }

            throw new FormatException(
                "Ungültige komplexe Zahl: " + value
            );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

    }




}
