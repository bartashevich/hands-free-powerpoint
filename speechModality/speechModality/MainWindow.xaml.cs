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

namespace speechModality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SpeechMod _sm;
        public MainWindow()
        {
            InitializeComponent();

            _sm = new SpeechMod();
            _sm.Recognized += _sm_Recognized;
        }

        private void _sm_Recognized(object sender, SpeechEventArg e)
        {
            // result.Text = e.Text;
            // confidence.Text = e.Confidence+"";

            // if (e.Final) result.FontWeight = FontWeights.Bold;
            // else result.FontWeight = FontWeights.Normal;

            this.Dispatcher.Invoke(() =>
            {
                if (e.AssistantActive) circle.Fill = Brushes.Green;
                else circle.Fill = Brushes.Gray;
            });
        }

        private void button_a_on(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("status\",\"ASSISTANT_ACTIVE");
            Console.WriteLine("Test ASSISTANT_ACTIVE");
        }

        private void button_a_off(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("status\",\"ASSISTANT_INACTIVE");
            Console.WriteLine("Test ASSISTANT_INACTIVE");
        }

        private void button_next_slide(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"NEXT_SLIDE");
            Console.WriteLine("Test NEXT_SLIDE");
        }

        private void button_prev_slide(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"PREV_SLIDE");
            Console.WriteLine("Test PREV_SLIDE");
        }

        private void button_change(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("combo\",\"CHANGE");
            System.Threading.Thread.Sleep(1000);
            _sm.SendCommand("action\",\"CHANGE");
            Console.WriteLine("Test CHANGE");
        }

        private void button_suspend(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"SUSPEND");
            Console.WriteLine("Test SUSPEND");
        }

        private void button_calculator(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"CALCULATOR");
            Console.WriteLine("Test CALCULATOR");
        }

        private void button_open_help(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"OPEN_HELP");
            Console.WriteLine("Test OPEN_HELP");
        }

        private void button_close_help(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"CLOSE_HELP");
            Console.WriteLine("Test CLOSE_HELP");
        }

        private void button_read_slide(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"READ_SLIDE");
            Console.WriteLine("Test READ_SLIDE");
        }

        private void button_read_next(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("action\",\"READ_NEXT");
            Console.WriteLine("Test READ_NEXT");
        }
    }
}
