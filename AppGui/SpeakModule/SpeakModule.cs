using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Speech.Recognition;

namespace SpeakModule
{
    public class Speaking
    {
        private Tts tts = new Tts();

        static void Main(string[] args)
        {
            Console.WriteLine("Main speak");
        }

        public void Speak(String text)
        {
            Console.WriteLine(text);

            string str = "<speak version=\"1.0\"";
            str += " xmlns:ssml=\"http://www.w3.org/2001/10/synthesis\"";
            str += " xml:lang=\"pt-PT\">";
            str += text;
            str += "</speak>";

            tts.Speak(str, 0);
        }
    }
}
