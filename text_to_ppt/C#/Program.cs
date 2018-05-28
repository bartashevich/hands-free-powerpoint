using System;
using System.Text;
using Spire.Presentation;
using System.IO;
using System.Collections.Generic;

namespace SaveTextfromPPT
{
    class Program
    {
        static void Main(string[] args)
        {
            List<List<string>> FileContent = GetContentFromPPTX("slides.pptx");
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < FileContent.Count; i++)
            {
                List<string> tmplist = FileContent[i];

                sb.Append("Page: " + i.ToString() + Environment.NewLine);

                for (int j = 0; j < tmplist.Count; j++)
                {
                    sb.Append(tmplist[j] + Environment.NewLine);
                }

                sb.Append("----------------------" + Environment.NewLine);
            }

            File.WriteAllText("result.txt", sb.ToString());

        }

        public static List<List<string>> GetContentFromPPTX(string filepath)
        {
            Presentation presentation = new Presentation(filepath, FileFormat.PPT);
            List<List<string>> FileContent = new List<List<string>>();

            foreach (ISlide slide in presentation.Slides)
            {
                List<string> page = new List<string>();

                foreach (IShape shape in slide.Shapes)
                {
                    if (shape is IAutoShape)
                    {
                        foreach (TextParagraph tp in (shape as IAutoShape).TextFrame.Paragraphs)
                        {
                            page.Add(tp.Text);
                        }
                    }

                }
                FileContent.Add(page);
            }

            for (int i = 0; i < FileContent.Count; i++)
            {
                List<string> tmplist = FileContent[i];

                int listSize = tmplist.Count;

                for (int j = 0; j < listSize; j++)
                {
                    // delete slide number from plain text
                    if (tmplist[j] == (i + 1).ToString() || tmplist[j] == String.Empty)
                    {
                        tmplist.RemoveAt(j);
                        listSize--;
                        j--;
                    }
                }
            }

            return FileContent;
        }
    }
}
