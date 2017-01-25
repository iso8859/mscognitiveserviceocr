using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ocr2
{
    // https://www.microsoft.com/cognitive-services/en-US/subscriptions
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap m_bmp;
        string m_file;
        OCR m_ocr;

        void FillWordList()
        {
            listBox1.Items.Clear();
            if (m_ocr!=null)
            {
                foreach (Region r in m_ocr.regions)
                {
                    foreach (Line line in r.lines)
                    {
                        foreach (Word word in line.words)
                        {
                            listBox1.Items.Add(word);
                        }
                    }
                }

            }
        }
        private void choisirImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_file = openFileDialog1.FileName;
                m_bmp = (Bitmap)Image.FromFile(m_file);
                Graphics graphics = this.CreateGraphics();
                m_bmp.SetResolution(graphics.DpiX, graphics.DpiY);
                panel1.AutoScrollMinSize = m_bmp.Size;
                panel1.Refresh();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (m_bmp != null)
            {
                Matrix translation = new Matrix();
                translation.Translate(-panel1.HorizontalScroll.Value, -panel1.VerticalScroll.Value);
                e.Graphics.Transform = translation;
                e.Graphics.DrawImageUnscaled(m_bmp, 0, 0);
                if (m_ocr!=null)
                {
                    Word selectedWord = listBox1.SelectedItem as Word;
                    Pen sPen = new Pen(Color.Red, 5);
                    foreach (Region r in m_ocr.regions)
                    {
                        e.Graphics.DrawRectangle(Pens.Yellow, r.rect);
                        foreach (Line line in r.lines)
                        {
                            e.Graphics.DrawRectangle(Pens.Blue, line.rect);
                            foreach (Word word in line.words)
                            {
                                Pen p = Pens.Red;
                                if (selectedWord == word)
                                    p = sPen;
                                e.Graphics.DrawRectangle(p, word.rect);
                            }
                        }
                    }
                }
            }
        }

        async void MakeRequest()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", System.Environment.GetEnvironmentVariable("Ocp-Apim-Subscription-Key"));

            // Request parameters
            queryString["language"] = "fr";
            queryString["detectOrientation"] = "true";
            var uri = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr?" + queryString;

            HttpResponseMessage response;

            // Request body
            byte[] byteData = System.IO.File.ReadAllBytes(m_file);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                try
                {
                    response = await client.PostAsync(uri, content);
                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                    {
                        var s = await response.Content.ReadAsStringAsync();
                        m_ocr = JsonConvert.DeserializeObject<OCR>(s);
                        System.IO.File.WriteAllText(System.IO.Path.ChangeExtension(m_file, ".json"), s);
                        panel1.Refresh();
                        FillWordList();
                    }
                    else
                    {
                        var s = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(s);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void oCRMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeRequest();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                m_ocr = JsonConvert.DeserializeObject<OCR>(System.IO.File.ReadAllText(openFileDialog2.FileName));
                panel1.Refresh();
                FillWordList();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Word selectedWord = listBox1.SelectedItem as Word;
            if (selectedWord!=null)
            {
//                if (Rectangle.Intersect(panel1.DisplayRectangle, selectedWord.rect) == Rectangle.Empty)
                {
                    panel1.HorizontalScroll.Value = selectedWord.rect.Left;
                    panel1.VerticalScroll.Value = selectedWord.rect.Top;
                }
            }
            panel1.Refresh();
        }
    }
}
