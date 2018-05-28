using System;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AppGui
{
    internal class InternalFuncions
    {
        /** VOLUME FUNCIONS **/
        // mute / unmute system, second argument is not needed
        //VolumeControl("mute",0);

        // lower by 20%
        //VolumeControl("change", -20);

        // increase by 20%
        //VolumeControl("change", 20);

        // set at 45%
        //VolumeControl("set", 45);

        /** MOVIE FUNCIONS **/
        // pause / play movie
        //MoviePausePlay();

        /** SLIDE FUNCTIONS **/
        // first slide
        //PowerPointControl("first", 0);

        // last slide
        //PowerPointControl("last", 0);

        // previous slide
        //PowerPointControl("previous", 0);

        // previous slide
        //PowerPointControl("previous", 0);

        // next slide
        //PowerPointControl("next", 0);

        // previous slide
        //PowerPointControl("previous", 0);

        // set slide number
        //PowerPointControl("set", 5);

        /**
             * Possibilidade de estar a preparar apresentação e criar uma apresentação automática
             **/

        /** WINDOWS CONTROL **/
        //ControlWindows("show", 0);

        //Thread.Sleep(3000);

        //ControlWindows("set", 7);

        /** Open browser page **/
        //OpenBrowser("http://www.ipma.pt");

        /** Open program (calculator) **/
        //OpenProgram("calc.exe");

        // Power Control (sleep/lock)
        //PowerControl("lock");


        [DllImport("user32.dll", SetLastError = true)]
        static extern bool LockWorkStation();

        public static void SendEmail(dynamic json)
        {
            Outlook.Application oApp = new Outlook.Application();
            Outlook._MailItem oMailItem = (Outlook._MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

            String destEmail = "";
            String subject = "";
            String body = "";

            for (int i = 1; i < json.recognized.Count; i++)
            {
                String operation = json.recognized[i].ToString();
                Console.WriteLine(operation);

                switch (operation)
                {
                    // destination addresses
                    case "JOANA":
                        destEmail = "joana@example.com";
                        break;
                    case "DENIS":
                        destEmail = "bartashevich@ua.pt";
                        break;
                    case "LEONARDO":
                        destEmail = "leonardooliveira@ua.pt";
                        break;

                    // excuses
                    case "LATE":
                        subject = "Estou atrasado";
                        body = "Olá, estou um pouco atrasado. Peço desculpa.";
                        break;
                    case "GOING":
                        subject = "Estou a caminho";
                        body = "Olá, estou a caminho.";
                        break;
                    case "CALL_LATER":
                        subject = "Ligo mais tarde";
                        body = "Olá, ligo-te daqui a pouco.";
                        break;
                }
            }

            oMailItem.To = destEmail;
            oMailItem.Subject = subject;
            oMailItem.Body = body;

            oMailItem.Display(true);
        }

        public static void PowerControl(String operation)
        {
            switch (operation)
            {
                case "sleep":
                    Application.SetSuspendState(PowerState.Suspend, true, true);
                    break;
                case "lock":
                    LockWorkStation();
                    break;
            }
        }

        public static void OpenProgram(String program)
        {
            Process p = Process.Start(program);
            p.WaitForInputIdle();
        }

        public static void OpenWeather(dynamic json)
        {
            if(json.recognized.Count == 2)
            {
                String operation = json.recognized[1].ToString();
                Console.WriteLine(operation);
                switch (operation)
                {
                    // destination addresses
                    case "TODAY":
                        System.Diagnostics.Process.Start("https://www.tempo.pt/aveiro-provincia.htm");
                        break;
                    case "TOMORROW":
                        System.Diagnostics.Process.Start("https://www.tempo.pt/aveiro-provincia.htm?d=amanha");
                        break;
                }
            }
        }

        public static void ControlWindows(string operation, int windowNumber)
        {
            switch (operation)
            {
                case "show":
                    SendKeys.SendWait("^%{TAB}");
                    break;
                case "set":
                    if (windowNumber > 0)
                    {
                        if (windowNumber == 1)
                        {
                            SendKeys.SendWait("{LEFT}");
                            Thread.Sleep(1000);
                            SendKeys.SendWait("{ENTER}");
                        }
                        else if (windowNumber == 2)
                        {
                            SendKeys.SendWait("{ENTER}");
                        }
                        else
                        {
                            for (int i = 0; i < (windowNumber - 2); i++)
                            {
                                SendKeys.SendWait("{RIGHT}");
                                Thread.Sleep(1000);
                            }
                            SendKeys.SendWait("{ENTER}");
                        }
                    }
                    break;
            }
        }


        // KEYBOARD EVENTS
        // https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.aspx

        //POWERPOINT SHORTCUTS
        // https://support.office.com/en-us/article/use-keyboard-shortcuts-to-deliver-powerpoint-presentations-1524ffce-bd2a-45f4-9a7f-f18b992b93a0?ui=en-US&rs=en-US&ad=US
        public static void PowerPointControl(String operation, int slideNumber)
        {
            switch (operation)
            {
                case "pointer":
                    SendKeys.SendWait("^(l)");
                    Console.WriteLine("pointer");
                    break;
                case "pen":
                    SendKeys.SendWait("^(i)");
                    Console.WriteLine("pen");
                    break;
                case "hide":
                    SendKeys.SendWait("(b)");
                    Console.WriteLine("hide");
                    break;
                case "first":
                    SendKeys.SendWait("{HOME}");
                    break;
                case "last":
                    SendKeys.SendWait("{END}");
                    break;
                case "next":
                    SendKeys.SendWait("{RIGHT}");
                    break;
                case "previous":
                    SendKeys.SendWait("{LEFT}");
                    break;
                case "set":
                    if (slideNumber >= 0)
                    {
                        SendKeys.SendWait(slideNumber + "{ENTER}");
                    }
                    break;
            }
        }

        // simulate "SPACE" key from the keyboard
        public static void MoviePausePlay()
        {
            SendKeys.SendWait(" ");
        }

        // http://www.nirsoft.net/utils/nircmd.html
        public static void VolumeControl(String volumeOperation, int quantity)
        {
            switch (volumeOperation)
            {
                case "mute":
                    Process.Start("nircmdterm.exe", "mutesysvolume 1");
                    break;
                case "unmute":
                    Process.Start("nircmdterm.exe", "mutesysvolume 0");
                    break;
                case "change":
                    Process.Start("nircmdterm.exe", "changesysvolume " + quantity);
                    break;
                case "set":
                    Process.Start("nircmdterm.exe", "setsysvolume " + quantity);
                    break;
            }
        }
    }
}