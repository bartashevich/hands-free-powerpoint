using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Win32;
using mmisharp;
using Newtonsoft.Json;
using SavePPTXtoText;
using SpeakModule;

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

        private double maxWindowWidth = 600;
        private double medWindowWidth = 200;
        private double minWindowWidth = 30;
        private double screenWidth;
        private double screenHeight;

        private string pptx_location = null;
        List<List<string>> FileContent = null;
        private int pptx_size = 0;
        private int pptx_current_index = 0;
        private int pptx_current_block = 0;

        private HelpWindow window = null;

        private Speaking sm;

        public MainWindow()
        {
            InitializeComponent();

            this.main = this;

            this.screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            this.Height = 60;
            this.Width = 600;

            this.Top = screenHeight - this.Height + 7;
            this.Left = screenWidth - this.Width + 7;

            this.Topmost = true;

            this.Show();

            this.sm = new Speaking();

            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            //UpdateKinectStatus("active");
        }

        void Button_Open_HelpWindow(object sender, RoutedEventArgs e)
        {
            Open_HelpWindow();
        }

        void Open_HelpWindow()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (window != null)
                {
                    Close_HelpWindow();
                }

                window = new HelpWindow();
                window.Topmost = true;
                window.Show();
            });
        }

        void Close_HelpWindow()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (window != null)
                {
                    window.Close();
                    window = null;
                }
            });
        }

        // upload file box
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                pptx_location = openFileDialog.FileName;
                FileContent = Saver.GetContentFromPPTX(pptx_location);
                pptx_size = FileContent.Count;
                pptx_current_index = 0;
                pptx_current_block = 0;

                sm.Speak("O ficheiro contem " + pptx_size + " diapositivos");
            }
        }

        // remove special characters
        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        // function to read slides
        private void Read_Slide(bool read_next)
        {
            if(pptx_location == null)
            {
                sm.Speak("Não foi especificado ficheiro de apresentação");
                return;
            }
            else if(pptx_size == 0)
            {
                sm.Speak("O ficheiro não contem diapositivos");
                return;
            }
            else if(pptx_current_index >= pptx_size || pptx_current_index < 0)
            {
                sm.Speak("Chegou ao final da apresentação");
                return;
            }

            List<string> slide = FileContent[pptx_current_index];
            string to_read = "";

            // read first sentence
            if (!read_next)
            {
                pptx_current_block = 0;
                to_read = slide[pptx_current_block++];
            }
            else
            {
                if (pptx_current_block >= slide.Count)
                {
                    sm.Speak("Não há mais nada para ler neste diapositivo");
                    return;
                }

                to_read = slide[pptx_current_block++];
            }

            try
            {
                sm.Speak((to_read));
            }
            catch (Exception)
            {
                Console.WriteLine("error");
            }
        }

        // update GUI statuses
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
                    case "VOLUME_ACTIVE":
                        volume_status.Fill = Brushes.Green;
                        break;
                    case "VOLUME_ACTIVATING":
                        volume_status.Fill = Brushes.Yellow;
                        break;
                    case "VOLUME_INACTIVE":
                        volume_status.Fill = Brushes.Gray;
                        break;
                }
            });
        }

        // execute actions
        private void ExecuteAction(string action)
        {
            switch (action)
            {
                case "READ_SLIDE":
                    Read_Slide(false);
                    break;
                case "READ_NEXT":
                    Read_Slide(true);
                    break;
                case "NEXT_SLIDE":
                    if (pptx_location == null)
                    {
                        sm.Speak("Não foi especificado ficheiro de apresentação");
                        return;
                    }

                    if (pptx_current_index + 1 >= pptx_size)
                    {
                        sm.Speak("Não há mais diapositivos");
                    }
                    else
                    {
                        pptx_current_index++;
                        pptx_current_block = 0;
                        InternalFuncions.PowerPointControl("next", 0);
                    }
                    break;
                case "PREV_SLIDE":
                    if(pptx_location == null)
                    {
                        sm.Speak("Não foi especificado ficheiro de apresentação");
                        return;
                    }

                    if (pptx_current_index <= 0)
                    {
                        sm.Speak("Não há mais diapositivos");
                    }
                    else
                    {
                        pptx_current_index--;
                        pptx_current_block = 0;
                        InternalFuncions.PowerPointControl("previous", 0);
                    }
                    break;
                case "OPEN_HELP":
                    Open_HelpWindow();
                    break;
                case "CLOSE_HELP":
                    Close_HelpWindow();
                    break;
                case "CALCULATOR":
                    InternalFuncions.OpenProgram("calc.exe");
                    break;
                case "LASER_POINTER":
                    InternalFuncions.PowerPointControl("pointer", 0);
                    break;
                case "LASER_PEN":
                    InternalFuncions.PowerPointControl("pen", 0);
                    break;
            }
        }

        // update GUI action
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

        // update GUI combo
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
                    ExecuteAction(secondCommand);
                    Console.WriteLine(secondCommand);
                    break;
                case "volume":
                    secondCommand = (string)json.recognized[1].ToString();
                    UpdateAction("Volume " + secondCommand);
                    UpdateVolume(secondCommand);
                    Console.WriteLine(secondCommand);
                    break;
                case "combo":
                    secondCommand = (string)json.recognized[1].ToString();
                    UpdateCombo(secondCommand);
                    Console.WriteLine(secondCommand);
                    break;
                /*case "OPEN_EMAIL":
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
                    break;*/
            }
        }

        private void UpdateVolume(string secondCommand)
        {
            if (secondCommand == "UP")
            {
                InternalFuncions.VolumeControl("change", 10);
                Console.WriteLine("VOL UP");
            }
            else if (secondCommand == "DOWN")
            {
                InternalFuncions.VolumeControl("change", -10);
            }
        }

        private void Button_Minimize_Window(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.Width >= this.maxWindowWidth)
                {
                    this.main.Width = this.medWindowWidth;
                    this.Left = this.screenWidth - this.Width + 7;
                }
                else if (this.Width >= this.medWindowWidth)
                {
                    this.Width = this.minWindowWidth;
                    this.Left = this.screenWidth - this.Width + 7;
                }
                else
                {
                    this.main.Width = this.maxWindowWidth;
                    this.Left = this.screenWidth - this.Width + 7;
                }
            });
        }
    }
}
