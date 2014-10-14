using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Akumu.Antigate;

namespace SimpleExample
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private string ImageFilePath;

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (string.IsNullOrEmpty(apiKey.Text))
            {
                MessageBox.Show("Укажите API ключ");
                return;
            }

            if (string.IsNullOrEmpty(ImageFilePath))
            {
                MessageBox.Show("Файл не выбран");
                return;
            }

            AntiCaptcha anticap = new AntiCaptcha(apiKey.Text);

            anticap.CheckDelay = 10000;
            anticap.CheckRetryCount = 20;
            anticap.SlotRetry = 5;
            anticap.SlotRetryDelay = 800;

            // дополнительные API параметры (см: http://antigate.com/panel.php?action=api)
            anticap.Parameters.Set("min_len", "2");
            anticap.Parameters.Set("max_len", "10");

            if(checkBox1.Checked)
                anticap.Parameters.Set("is_russian", "1");

            textBox1.Invoke((MethodInvoker)delegate { textBox1.Text = "Ждем..."; });

            try
            {
                // отправляем файл и ждем ответа
                string answer = anticap.GetAnswer(ImageFilePath);

                string result = "Ответ не получен";
                if (answer != null)
                    result = answer;

                textBox1.Invoke((MethodInvoker)delegate { textBox1.Text = result; });
            }
            catch (AntigateErrorException aee)
            {
                // Antigate ответил одной из документированных в API ошибкой
                MessageBox.Show(aee.Message, "Antigate error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // возникло исключение
                MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif";

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ImageFilePath = ofd.FileName;
                    pictureBox1.ImageLocation = ImageFilePath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            worker.RunWorkerAsync();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button2.Enabled = true;
        }
    }
}
