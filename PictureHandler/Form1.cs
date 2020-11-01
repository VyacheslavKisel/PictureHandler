using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PictureHandler
{
    public partial class Form1 : Form
    {
        private readonly Color _blackColor = Color.FromArgb(0, 0, 0);

        private Bitmap _bitmap;

        private int _width;
        private int _height;

        private long _elapsedTimeWorking;

        private const int ThreadsCount = 1;
        private static readonly Thread[] Threads = new Thread[ThreadsCount];

        // Initialize fake lock object.
        private readonly object _locker = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        #region MedianFilteringLogic

        private void MedianFiltering()
        {
            var stopwatch = Stopwatch.StartNew();

            var startHeight = 0;
            var heightSegment = _height / ThreadsCount;
            var finishHeight = heightSegment;
            var remainder = _height - heightSegment * ThreadsCount;

            for (var i = 0; i < ThreadsCount; i++)
            {
                // Add remainder to first segment.
                if (i == 0)
                {
                    finishHeight += remainder;
                }

                var startHeightCopy = startHeight;
                var finishHeightCopy = finishHeight;

                Threads[i] = new Thread(() => MedianFilteringThread(startHeightCopy, finishHeightCopy));
                Threads[i].Start();

                startHeight = finishHeight;
                finishHeight += heightSegment;
            }

            for (var i = 0; i < ThreadsCount; i++)
            {
                Threads[i].Join();
            }

            // Fill black colors to all pixels at the top/bottom/left/right on image.
            for (var index = 0; index < _width; index++)
            {
                _bitmap.SetPixel(index, 0, _blackColor);
            }

            for (var index = 0; index < _width; index++)
            {
                _bitmap.SetPixel(index, _height - 1, _blackColor);
            }

            for (var index = 0; index < _height; index++)
            {
                _bitmap.SetPixel(0, index, _blackColor);
            }

            for (var index = 0; index < _height; index++)
            {
                _bitmap.SetPixel(_width - 1, index, _blackColor);
            }

            stopwatch.Stop();
            _elapsedTimeWorking = stopwatch.ElapsedMilliseconds;

            // Replace processed image in the form.
            picture.Image = _bitmap;
        }

        private void MedianFilteringThread(int startHeight, int finalHeight)
        {
            var stopwatch = Stopwatch.StartNew();

            var redColors = new Collection<byte>();
            var greenColors = new Collection<byte>();
            var blueColors = new Collection<byte>();

            // For all image pixels.
            for (var i = 0; i <= _width - 3; i++)
            {
                for (var j = startHeight; j <= finalHeight - 3; j++)
                {
                    // Get 3x3 matrix.
                    for (var x = i; x <= i + 2; x++)
                    {
                        for (var y = j; y <= j + 2; y++)
                        {
                            Color currentColor;

                            lock (_locker)
                            {
                                currentColor = _bitmap.GetPixel(x, y);
                            }

                            redColors.Add(currentColor.R);
                            greenColors.Add(currentColor.G);
                            blueColors.Add(currentColor.B);
                        }
                    }

                    var resultRedColors = redColors.ToList();
                    var resultGreenColors = greenColors.ToList();
                    var resultBlueColors = blueColors.ToList();

                    redColors.Clear();
                    greenColors.Clear();
                    blueColors.Clear();

                    resultRedColors.Sort();
                    resultGreenColors.Sort();
                    resultBlueColors.Sort();

                    lock (_locker)
                    {
                        _bitmap.SetPixel(i + 1, j + 1, Color.FromArgb(
                            resultRedColors[4], resultGreenColors[4], resultBlueColors[4]));
                    }
                }
            }

            stopwatch.Stop();

            _elapsedTimeWorking = stopwatch.ElapsedMilliseconds;
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

                MedianFiltering();

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

                _width = _bitmap.Width;
                _height = _bitmap.Height;

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