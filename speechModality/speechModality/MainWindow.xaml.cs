﻿using System;
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

        private void button_test_1(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("shape\",\"SQUARE");
            Console.WriteLine("Test SQUARE");
        }

        private void button_test_2(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("shape\",\"TRIANGLE");
            Console.WriteLine("Test 2");
        }

        private void button_test_3(object sender, RoutedEventArgs e)
        {
            _sm.SendCommand("shape\",\"CIRCLE");
            Console.WriteLine("Test 3");
        }
    }
}
