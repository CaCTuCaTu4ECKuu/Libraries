using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

using Akumu.Antigate;
using Akumu.Antigate.Tools;

namespace ReCaptchaExample
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = pageUrl.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Укажите URL");
                return;
            }

            using (WebClient client = new WebClient())
            {
                // загружаем страницу содержащую ReCaptcha
                string page = client.DownloadString(url);

                try
                {
                    // массив ссылок на js 
                    string[] js = ReCaptcha.GetObjectsUrlsOnPage(page);

                    if (js == null)
                    {
                        MessageBox.Show("ReCaptcha не обноружена на указанной странице");
                    }
                    else
                    {
                        string jsPage = client.DownloadString(js[0]);

                        // работаем с первым js на странице
                        ReCaptchaObject rco = ReCaptcha.GetObject(jsPage);

                        if (rco == null)
                            MessageBox.Show("ReCaptcha на странице содержит ошибку");
                        else
                        {
                            pictureBox1.Invoke((MethodInvoker)delegate { pictureBox1.ImageLocation = rco.ImageURL; });
                            textBox1.Invoke((MethodInvoker)delegate { textBox1.Text = rco.Challenge; });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, ex.GetType().ToString());
                }
            }

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button2.Enabled = true;
        }
    }
}
