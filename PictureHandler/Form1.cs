using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PictureHandler
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        private long elapsedTimeWorking;
        private const int indexsColorBlack = 0;

        public Form1()
        {
            InitializeComponent();
            LoadPicture("smile.jpg");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            DialogResult dialogResult = openFileDialog1.ShowDialog();
            LoadPicture(openFileDialog1.FileName);
        }

        private void LoadPicture(string filename)
        {
            try
            {
                bitmap = new Bitmap(Image.FromFile(filename));
                textBox1.Text = openFileDialog1.FileName;
                picture.Image = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                textBox1.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MedianFiltering(bitmap);
            elapsedTime.Text = elapsedTimeWorking.ToString();
        }

        private void MedianFiltering(Bitmap bm)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<byte> redColors = new List<byte>();
            List<byte> greenColors = new List<byte>();
            List<byte> blueColors = new List<byte>();

            // Applying Median Filtering.
            for (int i = 0; i <= bm.Width - 3; i++)
            {
                for (int j = 0; j <= bm.Height - 3; j++)
                {
                    for (int x = i; x <= i + 2; x++)
                    {
                        for (int y = j; y <= j + 2; y++)
                        {
                            var currentColor = bm.GetPixel(x, y);
                            redColors.Add(currentColor.R);
                            greenColors.Add(currentColor.G);
                            blueColors.Add(currentColor.B);
                        }
                    }
                    byte[] resultRedColours = redColors.ToArray();
                    byte[] resultGreenColours = greenColors.ToArray();
                    byte[] resultBlueColours = blueColors.ToArray();
                    redColors.Clear();
                    greenColors.Clear();
                    blueColors.Clear();
                    Array.Sort<byte>(resultRedColours);
                    Array.Sort<byte>(resultGreenColours);
                    Array.Sort<byte>(resultBlueColours);
                    bm.SetPixel(i + 1, j + 1, Color.FromArgb(
                        resultRedColours[4],
                        resultGreenColours[4],
                        resultBlueColours[4]));
                }
            }

            // Set black color for edge pixels.
            for (int x = 0; x < bm.Width; x++)
            {
                bm.SetPixel(x, 0, Color.FromArgb(
                    indexsColorBlack, indexsColorBlack, indexsColorBlack));
            }

            for (int x = 0; x < bm.Width; x++)
            {
                bm.SetPixel(x, bm.Height - 1, Color.FromArgb(
                    indexsColorBlack, indexsColorBlack, indexsColorBlack));
            }

            for (int y = 0; y < bm.Height; y++)
            {
                bm.SetPixel(0, y, Color.FromArgb(
                    indexsColorBlack, indexsColorBlack, indexsColorBlack));
            }

            for (int y = 0; y < bm.Height; y++)
            {
                bm.SetPixel(bm.Width - 1, y, Color.FromArgb(
                    indexsColorBlack, indexsColorBlack, indexsColorBlack));
            }

            picture.Image = bm;

            stopwatch.Stop();
            elapsedTimeWorking = stopwatch.ElapsedMilliseconds;
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
    }
}
