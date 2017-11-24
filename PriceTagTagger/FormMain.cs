using Accord.Vision.Detection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PriceTagTagger
{
    public partial class FormMain : Form
    {
        private HaarObjectDetector detector;
        private readonly ProgramSettings _settings;
        private bool _processAgain;

        public FormMain()
        {
            InitializeComponent();

            _settings = new ProgramSettings();
            //var cascade = new Accord.Vision.Detection.Cascades.NoseHaarCascade();

            _settings.HaarMinSize = 20;
            _settings.CascadePath = @"D:\Downloads\supermarket\NEW_ATTEMPT2\classifier_old_working_crappy\classifier\cascade.xml";
            _settings.SearchMode = ObjectDetectorSearchMode.Average;
            _settings.ImagePath = @"D:\Downloads\supermarket\input\tags_rema.jpg";
            _settings.CascadeMode = CascadeType.Custom;
            _settings.MarkersBorderColor = Color.Violet;
            _settings.MarkersBorderSize = 1;

            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            // Now, create a new Haar object detector with the cascade:
            detector = new HaarObjectDetector(GetCascade(), _settings.HaarMinSize, _settings.SearchMode);

            /*var descriptor = TypeDescriptor.GetProperties(propertyGrid1.SelectedObject.GetType())["SpouseName"];
            var attrib = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
            FieldInfo isReadOnly = attrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);*/

            var p = TypeDescriptor.GetAttributes(_settings.CascadePath);
            

            propertyGridSettings.SelectedObject = _settings;
            ProcessCurrentImage();
        }

        private HaarCascade GetCascade()
        {
            switch (_settings.CascadeMode)
            {
                case CascadeType.Face:
                    return new Accord.Vision.Detection.Cascades.FaceHaarCascade();
                case CascadeType.Nose:
                    return new Accord.Vision.Detection.Cascades.NoseHaarCascade();
            }

            return HaarCascade.FromXml(_settings.CascadePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Next image
            ProcessNextImage();
        }

        private void ProcessNextImage()
        {
            _settings.ImagePath = GetNextFile(_settings.ImagePath);
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
                toolStripStatusLabel1.Text = "Loading " + _settings.ImagePath;
                toolStripProgressBarLoading.Value = 70;
                backgroundWorkerLoadImage.RunWorkerAsync();
            }
            else
                _processAgain = true;
        }

        private void toolStripSplitButtonLoad_ButtonClick(object sender, EventArgs e)
        {
            ProcessNextImage();
        }

        private void backgroundWorkerLoadImage_DoWork(object sender, DoWorkEventArgs e)
        {
            // Note that we have specified that we do not want overlapping objects,
            // and that the minimum object an object can have is 50 pixels. Now, we
            // can use the detector to classify a new image. For instance, consider
            // the famous Lena picture:

            var bmp = Accord.Imaging.Image.FromFile(_settings.ImagePath);
            backgroundWorkerLoadImage.ReportProgress(80);

            // We have to call ProcessFrame to detect all rectangles containing the 
            // object we are interested in (which in this case, is the face of Lena):
            Rectangle[] rectangles = detector.ProcessFrame(bmp);
            backgroundWorkerLoadImage.ReportProgress(90);

            // The answer will be a single rectangle of dimensions
            // 
            //   {X = 126 Y = 112 Width = 59 Height = 59}
            // 
            // which indeed contains the only face in the picture.

            var g = Graphics.FromImage(bmp);

            g.DrawImage(bmp, 0, 0);
            foreach (var r in rectangles)
                g.DrawRectangle(new Pen(_settings.MarkersBorderColor, _settings.MarkersBorderSize), r);

            //g.Save();*/
            //g.FillRectangle(Brushes.Green, 20, 20, 100, 100);
            g.Save();

            //pictureBox1.Image = new Bitmap(bmp.Height, bmp.Width, g);
            e.Result = bmp;
        }

        private void backgroundWorkerLoadImage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Image = (Bitmap)e.Result;

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

        private void faceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void noseToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
            var d = new OpenFileDialog();
            if (d.ShowDialog() == DialogResult.OK)
            {
                _settings.ImagePath = d.FileName;
                UpdateCurrent();
            }
        }

        private void loadpreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
