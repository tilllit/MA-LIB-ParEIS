using ScottPlot.TickGenerators.TimeUnits;
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
    /// Interaktionslogik für E_Load_Window.xaml
    /// </summary>
    public partial class E_Load_Window : Window
    {
        public delegate void WinEvent();
        public event WinEvent E_Load_Close;
        public event WinEvent E_Load_Open;

        bool sys_lock = true;
        bool sys_lock2 = true;

        public static List<UInt16> LookUpTsize = new List<UInt16> { 1000, 500, 144, 72, 36, 18 };

        byte Load_offset = 0;

        public E_Load_Window()
        {
            InitializeComponent();

            foreach (UInt16 size in LookUpTsize)
            {
                DD_LookUp.Items.Add(size.ToString("0"));
            }
            DD_LookUp.SelectedIndex = 0;


            TB_FSYS.Text = "144.000.000"; // = "140.000.000";
            TB_FSYS2.Text = "144.000.000"; // = "140.000.000";

            DD_LookUp.SelectionChanged += (s, e) => { F2C_Btn_Click(null, null); };

            RB_ALL.Click += (s, e) => { Load_offset = 0; };
            RB_220nF.Click += (s, e) => { Load_offset = 1; };
            RB_10nF.Click += (s, e) => { Load_offset = 2; };
            RB_2nF.Click += (s, e) => { Load_offset = 3; };
        }

        // Prefent text input in textboxes -> numbers only
        private void Text_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && !(e.Text.Last() == '.') && !(e.Text.Last() == ',');
        }





        // Turn frequency to PSC and ARR
        private void F2C_Btn_Click(object sender, RoutedEventArgs e)
        {
            double FSYS, Freq;

            double.TryParse(TB_Freq.Text, out Freq);
            double.TryParse(TB_FSYS.Text, out FSYS);
            UInt16 LookUp = LookUpTsize[DD_LookUp.SelectedIndex];

            var res = ConvertToSettings(Freq, FSYS, LookUp);
            UInt16 ARR = res.Item1;
            UInt16 PSC = res.Item2;

            TB_PSC.Text = PSC.ToString("0");
            TB_ARR.Text = ARR.ToString("0");
        }


        // Turn PSC and ARR to frequency
        private void C2F_Btn_Click(object sender, RoutedEventArgs e)
        {
            double FSYS, PSC, ARR;

            double.TryParse(TB_ARR.Text, out ARR);
            double.TryParse(TB_PSC.Text, out PSC);
            double.TryParse(TB_FSYS.Text, out FSYS);
            UInt16 LookUp = LookUpTsize[DD_LookUp.SelectedIndex];

            double Freq = ConvertToFreq(ARR, PSC, FSYS, LookUp);

            TB_Freq.Text = Freq.ToString("0.00");
        }

        // Lock / unloch F_sys setting
        private void FSYS_Ulock_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (sys_lock)
            {
                FSYS_Ulock_Btn.Content = "Lock";
                TB_FSYS.IsReadOnly = false;
                TB_FSYS.IsHitTestVisible = true;
                TB_FSYS.Foreground = Brushes.Black;
                sys_lock = false;
            }
            else
            {
                FSYS_Ulock_Btn.Content = "Unlock";
                TB_FSYS.IsReadOnly = true;
                TB_FSYS.IsHitTestVisible = false;
                TB_FSYS.Foreground = Brushes.Gray;
                sys_lock = true;
            }
        }

        private void F2C_Btn2_Click(object sender, RoutedEventArgs e)
        {
            double FSYS, Freq;

            double.TryParse(TB_Freq2.Text, out Freq);
            double.TryParse(TB_FSYS2.Text, out FSYS);

            var res = ConvertToSettings(1 / Freq, FSYS);
            UInt16 ARR = res.Item1;
            UInt16 PSC = res.Item2;

            TB_PSC2.Text = PSC.ToString("0");
            TB_ARR2.Text = ARR.ToString("0");
        }

        private void C2F_Btn2_Click(object sender, RoutedEventArgs e)
        {
            double FSYS, PSC, ARR;

            double.TryParse(TB_ARR2.Text, out ARR);
            double.TryParse(TB_PSC2.Text, out PSC);
            double.TryParse(TB_FSYS2.Text, out FSYS);

            double Freq = ConvertToFreq(ARR, PSC, FSYS);

            TB_Freq2.Text = (1 / Freq).ToString("0.00");
        }

        private void FSYS_Ulock_Btn2_Click(object sender, RoutedEventArgs e)
        {
            if (sys_lock2)
            {
                FSYS_Ulock_Btn2.Content = "Lock";
                TB_FSYS2.IsReadOnly = false;
                TB_FSYS2.IsHitTestVisible = true;
                TB_FSYS2.Foreground = Brushes.Black;
                sys_lock2 = false;
            }
            else
            {
                FSYS_Ulock_Btn2.Content = "Unlock";
                TB_FSYS2.IsReadOnly = true;
                TB_FSYS2.IsHitTestVisible = false;
                TB_FSYS2.Foreground = Brushes.Gray;
                sys_lock2 = true;
            }
        }



        private void Send_Sine_Btn_Click(object sender, RoutedEventArgs e)
        {
            double Freq;
            byte Periods;
            UInt16 Offset, Amplitude, ARR, PSC;
            double.TryParse(TB_Freq.Text, out Freq);
            UInt16.TryParse(TB_OFF.Text, out Offset);
            UInt16.TryParse(TB_AMP.Text, out Amplitude);
            byte.TryParse(TB_PER.Text, out Periods);
            UInt16.TryParse(TB_ARR.Text, out ARR);
            UInt16.TryParse(TB_PSC.Text, out PSC);
            UInt16 LookUp = LookUpTsize[DD_LookUp.SelectedIndex];

            byte config = (byte)((byte)(0x20) + (byte)(DD_LookUp.SelectedIndex));

            Console.WriteLine("ARR: " + ARR + " PSC: " + PSC);

            if (Load_offset == 0)
            {
                Acquisition_Device.Load_Setup(config, PSC, ARR, Amplitude, Offset, Periods);
            }
            else
            {
                Acquisition_Device.Load_Setup(config, PSC, ARR, Amplitude, Offset, Periods, (UInt16)(0x2E0 + Load_offset));
            }

            this.Hide();
            E_Load_Close?.Invoke();
        }

        private void Send_Pulse_Btn_Click(object sender, RoutedEventArgs e)
        {
            uint AMP;
            UInt16 DAC, ARR, PSC;
            double sec;

            double.TryParse(TB_Freq2.Text, out sec);
            UInt16.TryParse(TB_ARR2.Text, out ARR);
            UInt16.TryParse(TB_PSC2.Text, out PSC);


            C2F_Btn2_Click(null, null);
            double freq = 0;
            double.TryParse(TB_Freq2.Text, out freq);

            if (RB_A.IsChecked == true)
            {
                uint.TryParse(TB_OFF2.Text, out AMP);
                //ELoad.PID_Pulse(Ampere: AMP, ARR, PSC);
            }
            else if (RB_DAC.IsChecked == true)
            {
                UInt16.TryParse(TB_OFF2.Text, out DAC);
                //ELoad.PID_Pulse(ADC_Val: ADC, ARR, PSC);
                //ELoad.Pulse(DAC, ARR, PSC);

                if (Load_offset == 0)
                {
                    Acquisition_Device.Load_Setup(0x40, PSC, ARR, 0, DAC, 1);
                }
                else
                {
                    Acquisition_Device.Load_Setup(0x40, PSC, ARR, 0, DAC, 1, (UInt16)(0x2E0+Load_offset));
                }

                    this.Hide();
                E_Load_Close?.Invoke();
            }
        }







        public static double ConvertToFreq(double ARR, double PSC, double FSYS, UInt16 LookUp = 1)
        {
            double freq = FSYS / (PSC + 1) / (ARR + 1) / LookUp;

            return freq;
        }

        public static Tuple<UInt16, UInt16> ConvertToSettings(double Freq, double FSYS, UInt16 LookUp = 1)
        {
            UInt16 arr = 0;
            UInt16 psc = 0;

            double divider = FSYS / Freq / LookUp;

            if (divider >= Math.Pow(2, 16) * Math.Pow(2, 16))
            {
                Console.WriteLine("To high!");
                return new Tuple<UInt16, UInt16>(arr, psc);
            }


            //if (FSYS / Math.Pow(2, 16) < Freq)
            //{
            //    psc = 0;
            //}


            UInt16 start = (UInt16)(Math.Floor(divider / Math.Pow(2, 16)) + 1);

            for (UInt16 i = 1; i < Math.Pow(2, 16); i++)
            {
                double ARR = divider / i;

                if (ARR % 1 < 0.001 && ARR < Math.Pow(2, 16))
                {
                    arr = Convert.ToUInt16(ARR - 1);
                    psc = (UInt16)(i - 1);
                    break;
                }
            }

            return new Tuple<UInt16, UInt16>(arr, psc);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            E_Load_Close?.Invoke();
        }

        public void Open()
        {
            this.Show();

            E_Load_Open?.Invoke();
        }
    }
}
