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
    /// Interaktionslogik für ParEIS_MSX_RAW_Window.xaml
    /// </summary>
    public partial class ParEIS_MSX_RAW_Window : Window
    {

        public delegate void WinEvent();
        public event WinEvent Win_Close;
        public event WinEvent Win_Open;

        public ParEIS_MSX_RAW_Window()
        {
            InitializeComponent();

            R1.Height = new GridLength(1, GridUnitType.Star);
            R2.Height = new GridLength(0);

            ParEIS_M_SX.RAW_DATA_IN += UpdateUI;

            LV_Channels.SelectionChanged += (s, e) => { UpdateChannelsPlot(); };
            LV_Cards.SelectionChanged += (s, e) => { UpdateCardsPlot(); };
        }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            Win_Close?.Invoke();
        }

        public void Open()
        {
            this.Show();
            Win_Open?.Invoke();
        }



        private void Channel_Btn_Click(object sender, RoutedEventArgs e)
        {
            Channel_Btn.IsEnabled = false;
            Cards_Btn.IsEnabled = true;

            R1.Height = new GridLength(1, GridUnitType.Star);
            R2.Height = new GridLength(0);
        }

        private void Cards_Btn_Click(object sender, RoutedEventArgs e)
        {
            Channel_Btn.IsEnabled = true;
            Cards_Btn.IsEnabled = false;

            R1.Height = new GridLength(0);
            R2.Height = new GridLength(1, GridUnitType.Star);
        }

        private void SelectAll_Btn_Click(object sender, RoutedEventArgs e)
        {
            LV_Channels.SelectAll();
            LV_Cards.SelectAll();
        }


        private void UpdateUI()
        {
            // Channels Plot
            var channels = Enumerable.Range(1, 78).ToList();
            LV_Channels.ItemsSource = channels;
            LV_Channels.SelectAll();
            UpdateChannelsPlot();

            // Cards Plot
            var cards = Enumerable.Range(1, (Acquisition_Device.Channel_Data.Count / 3)).ToList();
            LV_Cards.ItemsSource = cards;
            LV_Cards.SelectAll();
            UpdateCardsPlot();

            this.Open();
        }

        private void UpdateChannelsPlot()
        {
            Channel_Plot.Reset();

            foreach (var item in LV_Channels.SelectedItems)
            {
                int index = LV_Channels.Items.IndexOf(item);
                Channel_Plot.Plot.Add.Signal(ParEIS_M_SX.SortedChannels[index]);
            }

            Channel_Plot.Plot.Title("Sorted RAW Channels");
            Channel_Plot.Plot.YLabel("RAW [cnt]");
            //Channel_Plot.Plot.Axes.AutoScale();
            Channel_Plot.Plot.ScaleFactor = 2;
            Channel_Plot.Refresh();
        }

        private void UpdateCardsPlot()
        {
            Cards_Plot.Reset();

            foreach (var item in LV_Cards.SelectedItems)
            {
                int index = LV_Cards.Items.IndexOf(item);
                Cards_Plot.Plot.Add.Signal(Acquisition_Device.Channel_Data[index * 3 + 0]);
                Cards_Plot.Plot.Add.Signal(Acquisition_Device.Channel_Data[index * 3 + 1]);
                Cards_Plot.Plot.Add.Signal(Acquisition_Device.Channel_Data[index * 3 + 2]);
            }

            Cards_Plot.Plot.Title("Cards RAW Channels");
            Cards_Plot.Plot.YLabel("RAW [cnt]");
            //Cards_Plot.Plot.Axes.AutoScale();
            Cards_Plot.Plot.ScaleFactor = 2;
            Cards_Plot.Refresh();
        }


    }
}
