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

            _cascades = cascades;
            Output = new List<Tuple<Rectangle, Cascade>>();
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

        public List<Tuple<Rectangle, Cascade>> Output { get; }

        private readonly List<string> _inputImages;

        private void backgroundWorkerProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            for (var currentImage = 0; currentImage < _inputImages.Count; currentImage++)
            {
                var _image = _inputImages[currentImage];

                var image = new UMat(_image, ImreadModes.Color); //UMat version

                for (var i = 0; i < _cascades.Count; i++)
                {
                    var c = _cascades[i];

                    //backgroundWorkerLoadImage.ReportProgress((int)Map(i, 0, _cascades.Count, 30, 80));

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

                        Output.AddRange(detectedObjects.Select(r => Tuple.Create(r, c)));
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
    }
}
