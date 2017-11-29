using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PriceTagTagger
{
    public partial class FormMain : Form
    {
        //private HaarObjectDetector detector;
        private bool _processAgain;
        private List<Cascade> _cascades;
        private string _image;

        public FormMain()
        {
            InitializeComponent();

            _cascades = new List<Cascade>
            {
                new Cascade
                {
                    CascadePath = @"D:\Downloads\supermarket\NEW_ATTEMPT2\classifier_old_working_crappy\classifier\cascade.xml",
                    MarkersBorderColor = Color.Violet,
                    MarkersBorderSize = 1,
                    DetectorMinNeighbors = 3,
                    DetectorMaxSize = new Size(1000, 1000),
                    DetectorMinSize = new Size(10, 10),
                    DetectorScaleFactor = 1.2F
                }
            };

            _image = @"D:\Downloads\supermarket\input\tags_rema.jpg";

            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            propertyGridSettings.SelectedObject = _cascades[0];
            ProcessCurrentImage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Next image
            ProcessNextImage();
        }

        private void ProcessNextImage()
        {
            _image = GetNextFile(_image);
            ProcessCurrentImage();
        }

        private string GetNextFile(string currentImage)
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(currentImage));

            var next = false;

            foreach (var f in files)
            {
                if (next)
                    return f;

                if (f == currentImage)
                    next = true;
            }
            return files[0];

        }

        private void ProcessCurrentImage()
        {
            if (!backgroundWorkerLoadImage.IsBusy)
            {
                pictureBoxViewer.Image = Image.FromFile(_image);
                toolStripStatusLabel1.Text = "Analyzing " + _image;
                toolStripProgressBarLoading.Value = 70;
                backgroundWorkerLoadImage.RunWorkerAsync();
            }
            else
                _processAgain = true;
        }

        private void backgroundWorkerLoadImage_DoWork(object sender, DoWorkEventArgs e)
        {
            var detector = new CascadeClassifier(_cascades[0].CascadePath);
            //var image = new UMat(_settings.ImagePath, ImreadModes.Color); //UMat version
            //var bmp = Accord.Imaging.Image.FromFile(_settings.ImagePath);

            var image = new UMat(_image, ImreadModes.Color); //UMat version
            backgroundWorkerLoadImage.ReportProgress(80);

            Rectangle[] detectedObjects;

            using (var ugray = new UMat())
            {
                CvInvoke.CvtColor(image, ugray, ColorConversion.Bgr2Gray);

                //normalizes brightness and increases contrast of the image
                CvInvoke.EqualizeHist(ugray, ugray);
                 
                detectedObjects = detector.DetectMultiScale(ugray, _cascades[0].DetectorScaleFactor, 
                    _cascades[0].DetectorMinNeighbors, _cascades[0].DetectorMinSize, _cascades[0].DetectorMaxSize);
            }

            foreach(var d in detectedObjects)
            {
                CvInvoke.Rectangle(image, d, new Bgr(_cascades[0].MarkersBorderColor).MCvScalar, _cascades[0].MarkersBorderSize);
            }

            e.Result = image.Bitmap;
                backgroundWorkerLoadImage.ReportProgress(90);
        }

        private void backgroundWorkerLoadImage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBoxViewer.Image = (Bitmap)e.Result;

            if (_processAgain)
            {
                _processAgain = false;
                backgroundWorkerLoadImage.RunWorkerAsync();
            }
            else
            {
                toolStripProgressBarLoading.Value = 100;
                toolStripStatusLabel1.Text = "Ready";
                timerClear.Start();
            }
        }

        private void timerClear_Tick(object sender, EventArgs e)
        {
            toolStripProgressBarLoading.Value = 0;
            timerClear.Stop();
        }

        private void backgroundWorkerLoadImage_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBarLoading.Value = e.ProgressPercentage;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadnextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessNextImage();
        }

        private void propertyGridSettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            UpdateCurrent();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog() { Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png" };
            if (d.ShowDialog() == DialogResult.OK)
            {
                _image = d.FileName;
                UpdateCurrent();
            }
        }

        private void emptyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cascades = new List<Cascade>();
        }

        private void opencascadeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var d = new OpenFileDialog() { Filter = "XML Cascade definition | *.xml" };
            if (d.ShowDialog() == DialogResult.OK)
            {
                _cascades[0].CascadePath = d.FileName;
                UpdateCurrent();
            }
        }
    }
}
