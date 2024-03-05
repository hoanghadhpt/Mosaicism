using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Mosaicism
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = Environment.UserName.ToUpper();
            if(username.Contains("HAHM") || username.Contains("1859") || username.Contains("0265"))
            { 
            Process();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void Process()
        {
            string[] dirs = Directory.GetDirectories(textBox1.Text.Trim());
            foreach (string dir in dirs)
            {
                ProcessSingle(dir);
            }
            MessageBox.Show("Done");

        }

        private void ProcessSingle(string folder)
        {
            string[] searchTxt = Directory.GetFiles(folder + "/01_source", "*.txt");
            
            if (searchTxt.Length > 0 )
            {
                string folderName = new DirectoryInfo(folder).Name;
                // Lấy txt đầu tiên
                string txtData = File.ReadAllText(searchTxt[0], Encoding.GetEncoding("Windows-1252"));
                // Chuyển đổi ký tự đặc biệt
                txtData = txtData.Replace("\"", "&#34;");
                
                txtData = txtData.Replace("≥", "&#8805;");
                txtData = txtData.Replace("≤", "&#8804;");
                txtData = txtData.Replace(">", "&#62;");
                txtData = txtData.Replace("<", "&#60;");

                txtData = txtData.Replace("“", "&#8220;");
                txtData = txtData.Replace("”", "&#8221;");

                txtData = txtData.Replace("’", "&#8217;");
                txtData = txtData.Replace("‘", "&#8216;");

                StringBuilder stringBuilder = new StringBuilder();
                foreach (char ch in txtData)
                {
                    if (ch >= ' ')
                    {
                        stringBuilder.Append("&#");
                        stringBuilder.Append(((Decimal)ch).ToString());
                        stringBuilder.Append(";");
                    }
                    else
                        stringBuilder.Append(ch);
                }
                txtData = stringBuilder.ToString();

                txtData = WindowFormat(txtData);
                txtData = AddTags(txtData);
                txtData = CleaningText(txtData);
                // MessageBox.Show(txtData);
                File.WriteAllText(folder + "/02_spi/" + folderName + ".xml", txtData);
            }
        }

        private string CleaningText(string txt)
        {
            txt = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<document>\r\n<text>" + txt + "</text>\r\n</document>";
            txt = txt.Replace("<p indent=\"3\"></p>", "");
            txt = txt.Replace(" </p>", "</p>");
            txt = txt.Replace("<p indent=\"3\"> ", " ");

            txt = txt.Replace("\t", " ");
            while (txt.Contains("  "))
            {
                txt = txt.Replace("  ", " ");
            }

            return txt;

        }

        private string AddTags(string txt)
        {
            if (!txt.Contains("\r")){
                string[] paragraphs = txt.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paragraphs.Length; i++)
                {
                    paragraphs[i] = paragraphs[i].Trim();
                }
                string markedText = string.Join("</p>\n<p class=\"indent\">", paragraphs);
                markedText = $"<p class=\"indent\">{markedText}</p>";
                txt = markedText;

            }
            else if (txt.StartsWith(" \r\n"))
            {
                string[] paragraphs = txt.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paragraphs.Length; i++)
                {
                    paragraphs[i] = paragraphs[i].Trim();
                }
                string markedText = string.Join("</p>\r\n<p class=\"indent\">", paragraphs);
                markedText = $"<p class=\"indent\">{markedText}</p>";
                txt = markedText;
            }
            else
            {
                string pattern = @"\r\n\r\n";
                string replacement = "</p>\r\n\r\n<p indent=\"3\">";
                string result = Regex.Replace(txt, pattern, replacement);

                // Thêm <p indent="3"> vào đầu chuỗi
                result = "\r\n<p class=\"indent\">" + result;

                // Thêm </p> vào cuối chuỗi
                result += "</p>";

                txt = result;

            }


            return txt;
        }

        private string WindowFormat(string txt) {

            txt.Replace("\r", "[@R]");
            txt.Replace("\n", "[@N]");
            txt.Replace("[@R][@N]", "\r\n");
            txt.Replace("[@R]", "\r\n");
            txt.Replace("[@N]", "\r\n");

            return txt;
        }
    }
}
