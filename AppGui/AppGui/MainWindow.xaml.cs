using System;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();


            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

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

            // example of json.recognized: ["OPEN_EMAIL", "DENIS", "GOING"]

            switch (firstCommand)
            {
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
