﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PictureHandler
{
    public partial class Form1 : Form
    {
        private readonly Color _blackColor = Color.FromArgb(0, 0, 0);
        private Bitmap _bitmap;
        private long _elapsedTimeWorking;

        /// <summary>
        /// .ctor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        #region MedianFilteringLogic

        private void MedianFiltering(Bitmap bitmap)
        {
            var stopwatch = Stopwatch.StartNew();

            var redColors = new Collection<byte>();
            var greenColors = new Collection<byte>();
            var blueColors = new Collection<byte>();

            // Applying median filtering.
            for (var i = 0; i <= bitmap.Width - 3; i++)
            {
                for (var j = 0; j <= bitmap.Height - 3; j++)
                {
                    for (var x = i; x <= i + 2; x++)
                    {
                        for (var y = j; y <= j + 2; y++)
                        {
                            var currentColor = bitmap.GetPixel(x, y);

                            redColors.Add(currentColor.R);
                            greenColors.Add(currentColor.G);
                            blueColors.Add(currentColor.B);
                        }
                    }

                    var resultRedColors = redColors.ToArray();
                    var resultGreenColors = greenColors.ToArray();
                    var resultBlueColors = blueColors.ToArray();

                    redColors.Clear();
                    greenColors.Clear();
                    blueColors.Clear();

                    Array.Sort(resultRedColors);
                    Array.Sort(resultGreenColors);
                    Array.Sort(resultBlueColors);

                    bitmap.SetPixel(i + 1, j + 1,
                        Color.FromArgb(resultRedColors[4], resultGreenColors[4], resultBlueColors[4]));
                }
            }

            // TODO: Duplicate code.
            //Set black color for edge pixels.
            for (var index = 0; index < bitmap.Width; index++)
            {
                bitmap.SetPixel(index, 0, _blackColor);
            }

            for (var index = 0; index < bitmap.Width; index++)
            {
                bitmap.SetPixel(index, bitmap.Height - 1, _blackColor);
            }

            for (var index = 0; index < bitmap.Height; index++)
            {
                bitmap.SetPixel(0, index, _blackColor);
            }

            for (var index = 0; index < bitmap.Height; index++)
            {
                bitmap.SetPixel(bitmap.Width - 1, index, _blackColor);
            }

            stopwatch.Stop();
            _elapsedTimeWorking = stopwatch.ElapsedMilliseconds;

            picture.Image = bitmap;
        }

        #endregion

        #region FormsEvents

        /// <summary>
        /// Event to upload image that will be processed.
        /// </summary>
        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Environment.UserName;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadPicture(openFileDialog1.FileName);
            }
        }

        /// <summary>
        /// Event to start processing the loaded image.
        /// </summary>
        private void Button2_Click(object sender, EventArgs eventArgs)
        {
            if (_bitmap != null)
            {
                errorLabel.Visible = false;

                MedianFiltering(_bitmap);

                elapsedTime.Text = _elapsedTimeWorking.ToString();
            }
            else
            {
                errorLabel.Visible = true;
            }
        }

        #endregion

        #region HelperMethods

        /// <summary>
        /// Method that uploads image that will be processed.
        /// </summary>
        /// <param name="imageName"> Name of image that will be processed. </param>
        private void LoadPicture(string imageName)
        {
            try
            {
                _bitmap = new Bitmap(Image.FromFile(imageName));

                textBox1.Text = openFileDialog1.FileName;

                picture.Image = _bitmap;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                textBox1.Text = string.Empty;
            }
        }

        #endregion
    }
}