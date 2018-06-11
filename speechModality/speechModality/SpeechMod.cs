using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.Timers;

namespace speechModality
{
    public class SpeechMod
    {
        private SpeechRecognitionEngine sre;
        private Grammar gr;
        public event EventHandler<SpeechEventArg> Recognized;

        private Tts tts;

        Timer activeTimer;
        private Boolean assistantActive = false;

        Timer speakingTimer;
        private Boolean assistantSpeaking = false;

        private SemanticValue pendingSemantic = null;

        private int round = 0;

        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        public SpeechMod()
        {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION","speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            sre.LoadGrammar(gr);

            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;

            // load speech sintesizer
            tts = new Tts();

            // keep assistant active (refreshing the timeout)
            ActivateAssistant();

            //onRecognized(new SpeechEventArg() { Text = "MUTE", Confidence = 100, Final = true, AssistantActive = assistantActive });

            // send command
            // format {"recognized":["SHAPE","COLOR"]}
            /*string json = "{ \"recognized\": [\"MUTE\"] }";
            Console.WriteLine(json);
            var exNot = lce.ExtensionNotification("","", 100, json);
            mmic.Send(exNot);*/

            // introduce assistant
            //Speak("Olá, eu sou a Maria, a tua assistente pessoal. Tenho todo o gosto em ajudar-te com algumas tarefas no teu computador. Podes saber mais sobre mim dizendo: ajuda. Sou um pouco distraída, por isso sempre que quiseres chamar por mim diz: ó Maria!");
        }

        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false, AssistantActive = assistantActive });
        }


        private void OnActivationExpired(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Assistant deactivated.");
            assistantActive = false;
            SendCommand("status\",\"ASSISTANT_INACTIVE");

            // update GUI
            onRecognized(new SpeechEventArg());
        }

        private void OnSpeakingEnded(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Assistant stopped speaking.");
            assistantSpeaking = false;

        }

        private void Speak(String text, int seconds)
        {
            string str = "<speak version=\"1.0\"";
            str += " xmlns:ssml=\"http://www.w3.org/2001/10/synthesis\"";
            str += " xml:lang=\"pt-PT\">";
            str += text;
            str += "</speak>";

            tts.Speak(str, 0);
            
            // enable talking flag
            assistantSpeaking = true;
            Console.WriteLine("Assistant speaking.");

            speakingTimer = new Timer(seconds * 1000);
            speakingTimer.Elapsed += OnSpeakingEnded;
            speakingTimer.AutoReset = false;
            speakingTimer.Enabled = true;
        }

        private void Speak(String text)
        {
            Speak(text, 4);
        }

        private void RandomSpeak(String[] choices, int seconds)
        {
            Speak(choices[round++ % choices.Length], seconds);
        }

        private void RandomSpeak(String[] choices)
        {
            Speak(choices[round++ % choices.Length]);
        }

        private void ActivateAssistant()
        {
            if (activeTimer != null)
            {
                activeTimer.Stop();
            }

            if (!assistantActive)
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + "\\plim.wav");
                player.Play();
            }

            assistantActive = true;
            Console.WriteLine("Assistant activated.");

            SendCommand("status\",\"ASSISTANT_ACTIVE");

            onRecognized(new SpeechEventArg() { AssistantActive = assistantActive });

            // activate assistant for the next 15 seconds
            activeTimer = new Timer(15 * 1000);
            activeTimer.Elapsed += OnActivationExpired;
            activeTimer.AutoReset = false;
            activeTimer.Enabled = true;
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Action: " + e.Result.Semantics["action"].Value.ToString() + "; Confidence: " + e.Result.Confidence);

            // ignore while the assistant is speaking
            if (assistantSpeaking)
            {
                return;
            }

            // ignore low confidance levels
            if (e.Result.Confidence < 0.5)
            {
                return;
            }

            // listen to activation command
            if (e.Result.Semantics["action"].Value.ToString() == "ACTIVATE")
            {
                ActivateAssistant();
                pendingSemantic = null;
                return;
            }

            // only continue if the assistante is active
            if (!assistantActive)
            {
                pendingSemantic = null;
                return;
            }

            // keep assistant active (refreshing the timeout)
            ActivateAssistant();

            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true, AssistantActive = assistantActive });

            // if confidence is between 40% and 60%
            if (e.Result.Confidence <= 0.4)
            {
                Speak("Desculpa, não consegui entender.");
                pendingSemantic = null;
                return;
            }

            // help
            if (e.Result.Semantics["action"].Value.ToString() == "HELP")
            {
                RandomSpeak(new string[]{
                    "Experimenta dizer: Que horas são em Brasília?",
                    "Experimenta dizer: Será que vai chover amanhã?",
                    "Experimenta dizer: Quero fazer um cálculo.",
                    "Experimenta dizer: Preciso de silêncio.",
                    "Experimenta dizer: Está muito alto."
                }, 4);
                pendingSemantic = null;
                return;
            }

            // if confidence is between 60% and 80%, confirm explicitly
            if (e.Result.Confidence <= 0.80)
            {
                pendingSemantic = e.Result.Semantics;

                switch (e.Result.Semantics["action"].Value.ToString())
                {
                    case "OPEN_EMAIL":
                        Speak("Queres que abra o email?", 2);
                        return;
                    case "OPEN_WEATHER":
                        Speak("Queres saber a meteorologia?", 2);
                        return;
                    case "OPEN_CALCULATOR":
                        Speak("Queres que abra a calculadora?", 2);
                        return;
                    case "MUTE":
                        Speak("Queres que desligue o som?", 2);
                        return;
                    case "SUSPEND":
                    case "MAY_SUSPEND":
                        Speak("Queres que suspenda o sistema?", 2);
                        return;
                }

                // by default, do not confirm actions that would be a mess if envolved voice feedback, like asking to go to the next slide of a powerpoint
                pendingSemantic = null;
            }
            else
            {
                // allways confirm "DRASTIC" commands
                switch (e.Result.Semantics["action"].Value.ToString())
                {
                    case "SUSPEND":
                        pendingSemantic = e.Result.Semantics;
                        Speak("Tens a certeza queres que suspenda o sistema?", 4);
                        return;
                    case "MAY_SUSPEND":
                        pendingSemantic = e.Result.Semantics;
                        Speak("Posso suspender o sistema até voltares, queres?", 4);
                        return;
                }
            }

            // hold sematics from a previous command that is being confirmed or from the current command
            SemanticValue semanticValue = null;

            if (pendingSemantic != null)
            {
                //voice feedback for confirmation
                switch (e.Result.Semantics["action"].Value.ToString())
                {
                    case "YES":
                        RandomSpeak(new string[]{
                            "A caminho.",
                            "Estou a tratar disso.",
                            "Com certeza.",
                            "Dá-me um instante.",
                            "Ok. Os teus pedidos são ordens."
                        });
                        break;
                    case "NO":
                        pendingSemantic = null;
                        RandomSpeak(new string[]{
                            "Ok, desculpa, a minha audição já não é o que era.",
                            "Ok, desculpa, às vezes ando um pouco distraída."
                        });
                        return;

                }

                semanticValue = pendingSemantic;
                pendingSemantic = null;
            } else
            {
                // voice feedback for commands
                switch (e.Result.Semantics["action"].Value.ToString())
                {
                    /*case "OPEN_EMAIL":
                        if (e.Result.Semantics["action"].Value.ToString() == "OPEN_EMAIL")
                        {
                            if (e.Result.Semantics.ContainsKey("person") && e.Result.Semantics["person"].Value.ToString() == "ASSISTANT")
                            {
                                Speak("Teria todo o gosto em receber um email teu, mas podemos falar diretamente.");
                                return;
                            }

                            if (emailsOpened == 1)
                            {
                                Speak("Só a mim é que não envias emails. A caminho.");
                            }
                            else if (emailsOpened == 2)
                            {
                                Speak("Muitos emails envias tu! Estou a tratar disso.");
                            }
                            else
                            {
                                Speak("Com certeza. Estou a tratar do teu email.");
                            }
                        }
                        break;
                    case "OPEN_WEATHER":
                        if (e.Result.Semantics["when"].Value.ToString() == "TODAY")
                        {
                            Speak("Esta é a previsão do tempo para hoje.");
                        } else
                        {
                            Speak("Esta é a previsão do tempo para amanhã.");
                        }
                        break;*/
                    case "OPEN_CALCULATOR":
                        Speak("Matemática nunca foi o meu forte. Importas-te de ser tu a escrever?");
                        break;
                    case "TIME":
                        // vars
                        DateTime localDate = DateTime.Now;

                        if (e.Result.Semantics.ContainsKey("where"))
                        {
                            switch (e.Result.Semantics["where"].Value.ToString())
                            {
                                case "BRASILIA":
                                    localDate = localDate.AddHours(-4);
                                    break;
                                case "AVEIRO":
                                case "PORTO":
                                case "LISBON":
                                case "LONDON":
                                    // same timezone
                                    break;
                                case "MADRID":
                                case "PARIS":
                                case "BERLIM":
                                    localDate = localDate.AddHours(1);
                                    break;
                                case "MOSCOW":
                                    localDate = localDate.AddHours(2);
                                    break;
                                case "TOKYO":
                                    localDate = localDate.AddHours(8);
                                    break;
                            }
                        }

                        int Hour = Int32.Parse(localDate.ToString("%h"));
                        int Minute = Int32.Parse(localDate.ToString("%m"));
                        bool PMTime;
                        String speakTime = "";

                        if (localDate.ToString("tt") == "PM")
                        {
                            PMTime = true;
                        }
                        else
                        {
                            PMTime = false;
                        }

                        if (Hour == 1)
                        {
                            speakTime += "É uma hora";
                        }
                        else if (Hour == 2)
                        {
                            speakTime += "São duas horas";
                        }
                        else if (Hour == 12)
                        {
                            if (PMTime)
                            {
                                speakTime += "É meio dia";
                            }
                            else
                            {
                                speakTime += "É meia noite";
                            }
                        }
                        else
                        {
                            speakTime += "São " + Hour + " horas";
                        }

                        if (Minute > 0)
                        {
                            if (Minute == 1)
                            {
                                speakTime += " e um minuto";
                            }
                            else
                            {
                                speakTime += " e " + Minute + " minutos";
                            }
                        }

                        if(Hour != 12) {
                            if (PMTime)
                            {
                                if (Hour < 8)
                                {
                                    speakTime += " da tarde";
                                } else
                                {
                                    speakTime += " da noite";
                                }
                            }
                            else
                            {
                                speakTime += " da manhã";
                            }
                        }
                        if (e.Result.Semantics.ContainsKey("where"))
                        {
                            var text = e.Result.Text.Split(' ');

                            speakTime += " " + text[text.Length - 2] + " " + text[text.Length - 1];
                        }

                        SendCommand("action\",\"TIME");
                        Speak(speakTime);

                        return;
                }

                semanticValue = e.Result.Semantics;
            }

            /*if (semanticValue["action"].Value.ToString() == "OPEN_EMAIL")
            {
                emailsOpened++;
            }*/

            // if a command was recognized and the confirmation of a previous command was ignored by the user, disable it
            pendingSemantic = null;

            // stop here if it was a inner command
            if (semanticValue["action"].Value.ToString() == "YES" || semanticValue["action"].Value.ToString() == "NO")
            {
                return;
            }

            // send command
            foreach (var resultSemantic in semanticValue)
            {
                Console.WriteLine(resultSemantic.Value.Value.ToString());

                switch (resultSemantic.Value.Value.ToString())
                {
                    case "NEXT_SLIDE":
                        SendCommand("action\",\"NEXT_SLIDE");
                        break;
                    case "PREV_SLIDE":
                        SendCommand("action\",\"PREV_SLIDE");
                        break;
                    case "CHANGE_SLIDE":
                        SendCommand("combo\",\"CHANGE");
                        System.Threading.Thread.Sleep(1000);
                        SendCommand("action\",\"CHANGE");
                        break;
                    case "SUSPEND":
                        SendCommand("action\",\"SUSPEND");
                        break;
                    case "CALCULATOR":
                        SendCommand("action\",\"CALCULATOR");
                        break;
                    case "OPEN_HELP":
                        SendCommand("action\",\"OPEN_HELP");
                        break;
                    case "CLOSE_HELP":
                        SendCommand("action\",\"CLOSE_HELP");
                        break;
                    case "READ_SLIDE":
                        SendCommand("action\",\"READ_SLIDE");
                        break;
                    case "READ_NEXT":
                        SendCommand("action\",\"READ_NEXT");
                        break;
                }
                break;
            }

            // send command
            // format {"recognized":["SHAPE","COLOR"]}
            /*string json = "{ \"action\": [";
            foreach (var resultSemantic in semanticValue)
            {
                json+= "\"" + resultSemantic.Value.Value +"\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";

            Console.WriteLine(json);
            var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime+"", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration)+"",e.Result.Confidence, json);
            mmic.Send(exNot);*/


        }

        public void SendCommand(string command)
        {
            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            string json = "{ \"recognized\":[\"" + command + "\"] }";

            var exNot = lce.ExtensionNotification("", "", 100, json);
            mmic.Send(exNot);
            Console.WriteLine(command);
        }
    }
}
