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
        Bitmap bitmap;

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
            //ChangePicture();
            //Median
            MedianFiltering(bitmap);
        }

        private void ChangePicture()
        {
            Bitmap result = new Bitmap(bitmap);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if(checkBox1.Checked)
                    {
                        pixel = ChangeGrayScale(pixel);
                    }
                    result.SetPixel(x, y, pixel);
                }
            }
            picture.Image = result;
        }

        private Color ChangeGrayScale(Color pixel)
        {
            int avg = (pixel.R + pixel.G + pixel.B + 1) / 3;
            return Color.FromArgb(avg, avg, avg);
        }

        private void MedianFiltering(Bitmap bm)
        {
            var watch = Stopwatch.StartNew();
            List<byte> termsList = new List<byte>();

            byte[,] image = new byte[bm.Width, bm.Height];

            //Convert to Grayscale
            for (int i = 0; i < bm.Width; i++)
            {
                for (int j = 0; j < bm.Height; j++)
                {
                    var c = bm.GetPixel(i, j);
                    byte gray = (byte)(.333 * c.R + .333 * c.G + .333 * c.B);
                    image[i, j] = gray;
                }
            }

            //applying Median Filtering 
            for (int i = 0; i <= bm.Width - 3; i++)
                for (int j = 0; j <= bm.Height - 3; j++)
                {
                    for (int x = i; x <= i + 2; x++)
                        for (int y = j; y <= j + 2; y++)
                        {
                            termsList.Add(image[x, y]);
                        }
                    byte[] terms = termsList.ToArray();
                    termsList.Clear();
                    Array.Sort<byte>(terms);
                    Array.Reverse(terms);
                    byte color = terms[4];
                    bm.SetPixel(i + 1, j + 1, Color.FromArgb(color, color, color));
                }

            picture.Image = bm;

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
        }
    }
}
