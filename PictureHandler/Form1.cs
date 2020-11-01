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

        // TODO:
        private int _width;


        private long _elapsedTimeWorking;

        private const int ThreadsCount = 8;
        private static readonly Thread[] Threads = new Thread[ThreadsCount];

        private readonly object _locker = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        #region MedianFilteringLogic

        private void MedianFilteringThread(ServiceInfoMedianFiltering obj)
        {
            var startHeight =  obj.InitialHeight;
            var finalHeight = obj.FiniteHeight;

            var redColors = new Collection<byte>();
            var greenColors = new Collection<byte>();
            var blueColors = new Collection<byte>();

            // For all image pixels.
            for(var i = 0; i <= _width - 3; i++)
            {
                for (var j = startHeight; j <= finalHeight - 3; j++)
                {
                    // Get 3x3 matrix.
                    for (var x = i; x <= i + 2; x++)
                    {
                        for (var y = j; y <= j + 2; y++)
                        {
                            Color currentColor;
                            lock(_locker)
                            {
                                currentColor = _bitmap.GetPixel(x, y);
                            }

                            redColors.Add(currentColor.R);
                            greenColors.Add(currentColor.G);
                            blueColors.Add(currentColor.B);
                        }
                    }

                    var resultRedColors = redColors.ToArray();
                    redColors.Clear();
                    Array.Sort(resultRedColors);

                    var resultGreenColors = greenColors.ToArray();
                    greenColors.Clear();
                    Array.Sort(resultGreenColors);

                    var resultBlueColors = blueColors.ToArray();
                    blueColors.Clear();
                    Array.Sort(resultBlueColors);

                    lock(_locker)
                    {
                        _bitmap.SetPixel(i + 1, j + 1, Color.FromArgb(
                        resultRedColors[4], resultGreenColors[4], resultBlueColors[4]));
                    }                   
                }
            }
        }

        private void MedianFiltering(int amountThreads)
        {
            var stopwatch = Stopwatch.StartNew();

            var height = _bitmap.Height;
            var startHeight = 0;
            var heightSegment = height / amountThreads;
            var finishHeight = heightSegment;
            var remainder = _bitmap.Height - heightSegment * amountThreads;

            for (var i = 0; i < amountThreads; i++)
            {
                // TODO:
                if (i == 0)
                {
                    finishHeight += remainder;
                }

                var service = new ServiceInfoMedianFiltering(startHeight, finishHeight);

                //ThreadPool.QueueUserWorkItem(obj => MedianFilteringThread(new ServiceInfoMedianFiltering(startHeight, finishHeight)));
                Threads[i] = new Thread(() => MedianFilteringThread(service));
                Threads[i].Start();

                startHeight = finishHeight;
                finishHeight += heightSegment;
            }

            for (var i = 0; i < amountThreads; i++)
            {
                Threads[i].Join();
            }

            for (var index = 0; index < _bitmap.Width; index++)
            {
                _bitmap.SetPixel(index, 0, _blackColor);
            }

            for (var index = 0; index < _bitmap.Width; index++)
            {
                _bitmap.SetPixel(index, _bitmap.Height - 1, _blackColor);
            }

            for (var index = 0; index < _bitmap.Height; index++)
            {
                _bitmap.SetPixel(0, index, _blackColor);
            }

            for (var index = 0; index < _bitmap.Height; index++)
            {
                _bitmap.SetPixel(_bitmap.Width - 1, index, _blackColor);
            }

            stopwatch.Stop();
            _elapsedTimeWorking = stopwatch.ElapsedMilliseconds;

            picture.Image = _bitmap;
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

                MedianFiltering(ThreadsCount);

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

                // TODO:
                _width = _bitmap.Width;

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