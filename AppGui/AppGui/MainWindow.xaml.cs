using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private MainWindow main;

        private Timer actionTimer;
        private Timer comboTimer;
        private string comboString;

        public MainWindow()
        {
            InitializeComponent();

            main = this;


            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            //UpdateKinectStatus("active");

        }

        private void UpdateStatus(string status)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (status)
                {
                    case "KINECT_ACTIVE":
                        kinect_status.Fill = Brushes.Green;
                        break;
                    case "KINECT_INACTIVE":
                        kinect_status.Fill = Brushes.Gray;
                        break;
                    case "ASSISTANT_ACTIVE":
                        assistant_status.Fill = Brushes.Green;
                        break;
                    case "ASSISTANT_INACTIVE":
                        assistant_status.Fill = Brushes.Gray;
                        break;
                    case "MOUSE_ACTIVE":
                        mouse_status.Fill = Brushes.Green;
                        break;
                    case "MOUSE_ACTIVATING":
                        mouse_status.Fill = Brushes.Yellow;
                        break;
                    case "MOUSE_INACTIVE":
                        mouse_status.Fill = Brushes.Gray;
                        break;
                }
            });
        }

        private void UpdateAction(string action)
        {
            this.Dispatcher.Invoke(() =>
            {
                action_name.Content = "Action: " + action;

                // stop previous timer
                if (actionTimer != null)
                {
                    actionTimer.Stop();
                }

                if (action != "-")
                {
                    // set the timer to deactivate the kinect circle
                    actionTimer = new Timer(2 * 1000);
                    actionTimer.Elapsed += ResetActionTimer;
                    actionTimer.AutoReset = false;
                    actionTimer.Enabled = true;
                }
            });
        }

        private void UpdateCombo(string action)
        {
            if(action != "-") {
                if(comboString == null)
                {
                    comboString = action + " + ";
                }
                else
                {
                    comboString += action;
                }
            }

            this.Dispatcher.Invoke(() =>
            {
                if(action == "-")
                {
                    combo_name.Content = "Combo: " + action;
                }
                else
                {
                    combo_name.Content = "Combo: " + comboString;
                }

                // stop previous timer
                if (comboTimer != null)
                {
                    comboTimer.Stop();
                }

                if (action != "-")
                {
                    // set the timer to deactivate the kinect circle
                    comboTimer = new Timer(2 * 1000);
                    comboTimer.Elapsed += ResetComboTimer;
                    comboTimer.AutoReset = false;
                    comboTimer.Enabled = true;
                }
            });
        }

        private void ResetActionTimer(Object source, ElapsedEventArgs e)
        {
            UpdateAction("-");
        }

        private void ResetComboTimer(Object source, ElapsedEventArgs e)
        {
            UpdateCombo("-");
            comboString = null;
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            // first command
            String firstCommand = (string)json.recognized[0].ToString();
            Console.WriteLine(firstCommand);
            String secondCommand;

            // example of json.recognized: ["OPEN_EMAIL", "DENIS", "GOING"]

            switch (firstCommand)
            {
                case "status":
                    secondCommand = (string)json.recognized[1].ToString();
                    UpdateStatus(secondCommand);
                    Console.WriteLine(secondCommand);
                    break;
                case "action":
                    secondCommand = (string)json.recognized[1].ToString();
                    UpdateAction(secondCommand);
                    Console.WriteLine(secondCommand);
                    break;
                case "combo":
                    secondCommand = (string)json.recognized[1].ToString();
                    UpdateCombo(secondCommand);
                    Console.WriteLine(secondCommand);
                    break;
                case "OPEN_EMAIL":
                    InternalFuncions.SendEmail(json);
                    break;
                case "MUTE":
                    InternalFuncions.VolumeControl("mute", 0);
                    break;
                case "UNMUTE":
                    InternalFuncions.VolumeControl("unmute", 0);
                    break;
                case "VOLUME_UP":
                    InternalFuncions.VolumeControl("change", 2000);
                    break;
                case "VOLUME_DOWN":
                    InternalFuncions.VolumeControl("change", -2000);
                    break;
                case "OPEN_WEATHER":
                    InternalFuncions.OpenWeather(json);
                    break;
                case "OPEN_CALCULATOR":
                    InternalFuncions.OpenProgram("calc.exe");
                    break;
                case "NEXT_SLIDE":
                    InternalFuncions.PowerPointControl("next", 0);
                    break;
                case "PREVIOUS_SLIDE":
                    InternalFuncions.PowerPointControl("previous", 0);
                    break;
                case "LASER_POINTER":
                    InternalFuncions.PowerPointControl("pointer", 0);
                    break;
                case "LASER_PEN":
                    InternalFuncions.PowerPointControl("pen", 0);
                    break;
                case "HIDE_SLIDE":
                    InternalFuncions.PowerPointControl("hide", 0);
                    break;
                case "SUSPEND":
                case "MAY_SUSPEND":
                    InternalFuncions.PowerControl("sleep");
                    break;
                case "PAUSE":
                    InternalFuncions.MoviePausePlay();
                    break;
                case "PLAY":
                    InternalFuncions.MoviePausePlay();
                    break;
            }
        }
    }
}
