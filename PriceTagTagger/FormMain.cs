using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PriceTagTagger
{
    public partial class FormMain : Form
    {
        private bool _processAgain;
        private List<Cascade> _cascades;
        private string _image;
        private int _selected = 0;

        public FormMain()
        {
            InitializeComponent();

            _cascades = new List<Cascade>();
            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            propertyGridSettings.SelectedObject = (_cascades == null || _cascades.Count == 0)?null:_cascades[_selected];
            ProcessCurrentImage();
            UpdateGUI();
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
                try
                {
                    pictureBoxViewer.Image = Image.FromFile(_image);
                    toolStripStatusLabel1.Text = $"Processing \'{_image}\'";
                    toolStripProgressBarLoading.Value = 20;
                    backgroundWorkerLoadImage.RunWorkerAsync();
                }
                catch { }
            }
            else
                _processAgain = true;
        }

        private void backgroundWorkerLoadImage_DoWork(object sender, DoWorkEventArgs e)
        {
            var image = new UMat(_image, ImreadModes.Color); //UMat version
            var detected = new List<Tuple<Rectangle, Cascade>>();

            for (var i = 0; i < _cascades.Count; i++)
            {
                var c = _cascades[i];

                backgroundWorkerLoadImage.ReportProgress((int) Map(i, 0, _cascades.Count, 30, 80));

                if (!c.Enabled)
                    continue;

                //try
                {
                    var detector = new CascadeClassifier(c.CascadePath);

                    Rectangle[] detectedObjects;

                    using (var ugray = new UMat())
                    {
                        CvInvoke.CvtColor(image, ugray, ColorConversion.Bgr2Gray);

                        //normalizes brightness and increases contrast of the image
                        CvInvoke.EqualizeHist(ugray, ugray);

                        detectedObjects = detector.DetectMultiScale(ugray, c.DetectorScaleFactor,
                            c.DetectorMinNeighbors, c.DetectorMinSize, c.DetectorMaxSize);
                    }

                    detected.AddRange(detectedObjects.Select(r => Tuple.Create(r, c)));
                }
                //catch{ }
            }

            // Apply ZOrder
            detected.Sort(ZOrderComparer);

            foreach (var d in detected)
            {
                CvInvoke.Rectangle(image, d.Item1, new Bgr(d.Item2.MarkersBorderColor).MCvScalar,
                    d.Item2.MarkersBorderSize);
            }

            e.Result = new Bitmap(image.Bitmap);
            backgroundWorkerLoadImage.ReportProgress(90);
        }

        private static int ZOrderComparer(Tuple<Rectangle, Cascade> x, Tuple<Rectangle, Cascade> y)
        {
            return x.Item2.ZOrder.CompareTo(y.Item2.ZOrder);
        }

        long Map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        private void backgroundWorkerLoadImage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBoxViewer.Image = (Image) e.Result;

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
            _cascades.Add(new Cascade());
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            comboBoxSelectedCascade.SelectedValueChanged -= comboBoxSelectedCascade_SelectedValueChanged;
            comboBoxSelectedCascade.Items.Clear();

            var en = true;

            if (_cascades == null || _cascades.Count == 0)
                en = false;
            else
                comboBoxSelectedCascade.Items.AddRange(_cascades.ToArray());

            comboBoxSelectedCascade.Enabled = en;
            linkLabelDuplicateSelected.Enabled = en;
            duplicateSelectedToolStripMenuItem.Enabled = en;
            removeSelectedToolStripMenuItem.Enabled = en;

            loadnextToolStripMenuItem.Enabled = !string.IsNullOrEmpty(_image) && File.Exists(_image);

            comboBoxSelectedCascade.SelectedValueChanged += comboBoxSelectedCascade_SelectedValueChanged;

            if (comboBoxSelectedCascade.Items.Count > 0) 
            comboBoxSelectedCascade.SelectedIndex = _selected;
        }

        private void loadnextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessNextImage();
        }

        private void comboBoxSelectedCascade_SelectedValueChanged(object sender, EventArgs e)
        {
            _selected = comboBoxSelectedCascade.SelectedIndex;
            propertyGridSettings.SelectedObject = comboBoxSelectedCascade.SelectedItem;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Innovation Garage AS");
        }

        private void duplicateCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DuplicateCurrentCascade();
            UpdateGUI();
        }

        private void DuplicateCurrentCascade()
        {
            _cascades.Add((_cascades == null || _cascades.Count == 0) ? new Cascade(): _cascades[_selected].Clone());
            _selected = _cascades.Count - 1;
            _cascades[_selected].Name = string.IsNullOrEmpty(_cascades[_selected].ToString()) ? string.Empty: _cascades[_selected] + " (copy)";
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewCascadeFromFile();
        }

        private void AddNewCascadeFromFile()
        {
            var cascadeFile = new CascadeFileNameEditor().EditValue(null, "") as string;
            if (string.IsNullOrEmpty(cascadeFile) && !File.Exists(cascadeFile)) return;

            DuplicateCurrentCascade();
            _cascades[_selected].CascadePath = cascadeFile;
            UpdateGUI();
        }

        private void linkLabelDuplicateSelected_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DuplicateCurrentCascade();
            UpdateGUI();
        }

        private void linkLabelAddCascade_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AddNewCascadeFromFile();
        }

        private void removeSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cascades.RemoveAt(_selected);

            if (_cascades.Count == 0)
            {
                propertyGridSettings.SelectedObject = null;
            }

            _selected += _selected > 0 ? -1 : 0;
            UpdateGUI();
        }

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new SaveFileDialog { Filter = "XML File|*.xml" };

            if (d.ShowDialog() == DialogResult.OK)
            {
                Serialization(_cascades, d.FileName);
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog { Filter = "XML File|*.xml" };

            if (d.ShowDialog() == DialogResult.OK)
            {
                _cascades = Deserialize(d.FileName);
            }

            UpdateCurrent();
        }


        // From: https://stackoverflow.com/questions/5005900/how-to-serialize-listt
        public List<Cascade> Deserialize(string a_fileName)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<Cascade>));

            TextReader reader = new StreamReader(a_fileName);

            object obj = deserializer.Deserialize(reader);

            reader.Close();

            return (List<Cascade>)obj;
        }

        public void Serialization(List<Cascade> a_stations, string a_fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Cascade>));

            using (var stream = File.OpenWrite(a_fileName))
            {
                serializer.Serialize(stream, a_stations);
            }
        }



        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void appendCascadeSetToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
