using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;

namespace PriceTagTagger
{
    public partial class FormBatchProcessing : Form
    {
        private List<Cascade> _cascades;

        public FormBatchProcessing(string workingFolder, List<Cascade> cascades)
        {
            InitializeComponent();
            labelCurrentFile.Text = "...";

            _cascades = cascades;
            Output = new List<CascadeMatch>();
            _inputImages = new List<string>();

            foreach(var img in Directory.GetFiles(workingFolder))
                switch(Path.GetExtension(img).ToLower())
                {
                    case ".jpg":
                    case ".png":
                    case ".gif":
                        _inputImages.Add(img);
                        break;
                }
            progressBarMain.Style = ProgressBarStyle.Marquee;
            backgroundWorkerProcess.RunWorkerAsync();
        }

        public List<CascadeMatch> Output { get; }

        private readonly List<string> _inputImages;

        private void backgroundWorkerProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            // TODO: Combine with main routin
            for (var currentImage = 0; currentImage < _inputImages.Count; currentImage++)
            {
                if (backgroundWorkerProcess.CancellationPending)
                    return;

                var _image = _inputImages[currentImage];
                backgroundWorkerProcess.ReportProgress((int)Utilities.Map(currentImage, 0, _inputImages.Count, 10, 100), _image);

                var image = new UMat(_image, ImreadModes.Color); //UMat version

                for (var i = 0; i < _cascades.Count; i++)
                {
                    var c = _cascades[i];
                    if (!c.Enabled)
                        continue;

                    try
                    {
                        var detector = new CascadeClassifier(c.CascadePath);

                        Rectangle[] detectedObjects;

                        using (var ugray = new UMat())
                        {
                            CvInvoke.CvtColor(image, ugray, ColorConversion.Bgr2Gray);

                            // Normalizes brightness and increases contrast of the image
                            CvInvoke.EqualizeHist(ugray, ugray);

                            detectedObjects = detector.DetectMultiScale(ugray, c.DetectorScaleFactor,
                                c.DetectorMinNeighbors, c.DetectorMinSize, c.DetectorMaxSize);
                        }

                        Output.AddRange(detectedObjects.Select(r => new CascadeMatch(r,c,_image)));
                    }
                    catch (Exception ex)
                    {
                        //backgroundWorkerLoadImage.ReportProgress(-1, "Error applying '" + c.Name + "': " + ex.Message);
                        //Thread.Sleep(2000);
                    }
                }
            }

            // Apply ZOrder
            Output.Sort(Utilities.ZOrderComparer);

            /*foreach (var d in detected)
            {
                CvInvoke.Rectangle(image, d.Item1, new Bgr(d.Item2.MarkersBorderColor).MCvScalar,
                    d.Item2.MarkersBorderSize);

                AdjustPosition(d.Item1, d.Item2.LabelPosition, out var p);

                CvInvoke.PutText(image, d.Item2.LabelText, p, FontFace.HersheyPlain, d.Item2.MarkersBorderSize * 0.5,
                    new Bgr(d.Item2.MarkersBorderColor).MCvScalar, d.Item2.MarkersBorderSize, LineType.AntiAlias);
            }

            e.Result = new Bitmap(image.Bitmap);
            backgroundWorkerLoadImage.ReportProgress(90);*/
        }

        private void backgroundWorkerProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarMain.Style = ProgressBarStyle.Continuous;
            progressBarMain.Value = e.ProgressPercentage;

            if (e.UserState is string s)
            {
                labelCurrentFile.Text = s;

                if (File.Exists(s))
                    pictureBoxPreview.Image = Image.FromFile(s);
            }
        }

        private void backgroundWorkerProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            backgroundWorkerProcess.CancelAsync();
            DialogResult = DialogResult.Cancel;
        }
    }

    public struct CascadeMatch  
    {
        public CascadeMatch(Rectangle rectangle, Cascade cascade, string imagePath)
        {
            Rectangle = rectangle;
            Cascade = cascade;
            ImagePath = imagePath;
        }

        public Rectangle Rectangle { get; }
        public Cascade Cascade { get; }
        public string ImagePath { get; }
    }
}
