using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PriceTagTagger
{
    public partial class FormMain : Form
    {
        private bool _processAgain;
        private readonly List<Cascade> _cascades;
        private string _image;
        private int _selected = 0;

        public FormMain()
        {
            InitializeComponent();
            selectedCascade.GetBackColor += SelectedCascade_GetBackColor;
            selectedCascade.GetForeColor += SelectedCascade_GetForeColor;

            _cascades = new List<Cascade>();
            UpdateCurrent();
        }

        private Color SelectedCascade_GetForeColor(Qodex.CustomCheckedListBox listbox, DrawItemEventArgs e)
        {
            if(e.Index != -1)
                return GetOptimizedContrastFromColor(_cascades[e.Index].MarkersBorderColor);
            return Color.Black;
        }

        public Color GetOptimizedContrastFromColor(Color c)
        {
            return 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B < 0.5 ? Color.White : Color.Black;
        }

        private Color SelectedCascade_GetBackColor(Qodex.CustomCheckedListBox listbox, DrawItemEventArgs e)
        {
            if (e.Index != -1)
                return _cascades[e.Index].MarkersBorderColor;
            return Color.White;
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

        private static string GetNextFile(string currentImage)
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

                    detected.AddRange(detectedObjects.Select(r => Tuple.Create(r, c)));
                }
                catch (Exception ex)
                {
                    backgroundWorkerLoadImage.ReportProgress(-1, "Error applying '" + c.Name + "': " + ex.Message);
                    Thread.Sleep(2000);
                }
            }

            // Apply ZOrder
            detected.Sort(ZOrderComparer);

            foreach (var d in detected)
            {
                CvInvoke.Rectangle(image, d.Item1, new Bgr(d.Item2.MarkersBorderColor).MCvScalar,
                    d.Item2.MarkersBorderSize);

                AdjustPosition(d.Item1, d.Item2.LabelPosition, out var p);

                CvInvoke.PutText(image, d.Item2.LabelText, p, FontFace.HersheyPlain,d.Item2.MarkersBorderSize*0.5,
                    new Bgr(d.Item2.MarkersBorderColor).MCvScalar, d.Item2.MarkersBorderSize, LineType.AntiAlias);
            }

            e.Result = new Bitmap(image.Bitmap);
            backgroundWorkerLoadImage.ReportProgress(90);
        }

        private void AdjustPosition(Rectangle rect, ContentAlignment contentAlignment, out Point point)
        {
            switch (contentAlignment)
            {
                case ContentAlignment.TopLeft:point = new Point(rect.Left, rect.Top); break;
                case ContentAlignment.TopCenter:point = new Point((rect.Left + rect.Right) / 2, rect.Top); break;
                case ContentAlignment.TopRight:point = new Point(rect.Right, rect.Top); break;
                case ContentAlignment.MiddleLeft:point = new Point(rect.Left, (rect.Top + rect.Bottom) / 2); break;
                case ContentAlignment.MiddleCenter:point = new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2); break;
                case ContentAlignment.MiddleRight:point = new Point(rect.Right, (rect.Top + rect.Bottom) / 2); break;
                case ContentAlignment.BottomLeft:point = new Point(rect.Left, rect.Bottom); break;
                case ContentAlignment.BottomCenter:point = new Point((rect.Left + rect.Right) / 2, rect.Bottom); break;
                case ContentAlignment.BottomRight:point = new Point(rect.Right, rect.Bottom); break;
                default:throw new ArgumentOutOfRangeException(nameof(contentAlignment), contentAlignment, null);
            }
        }

        private static int ZOrderComparer(Tuple<Rectangle, Cascade> x, Tuple<Rectangle, Cascade> y)
        {
            return x.Item2.ZOrder.CompareTo(y.Item2.ZOrder);
        }

        private static long Map(long x, long in_min, long in_max, long out_min, long out_max)
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
            if (e.ProgressPercentage >= 0)
                toolStripProgressBarLoading.Value = e.ProgressPercentage;
            else
                toolStripStatusLabel1.Text = e.UserState as string;
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
            OpenImage();
        }

        private void OpenImage()
        {
            var d = new OpenFileDialog { Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png" };
            if (d.ShowDialog() == DialogResult.OK)
            {
                _image = d.FileName;
                UpdateCurrent();
            }
        }

        private void UpdateGUI()
        {
            selectedCascade.SelectedValueChanged -= selectedCascade_SelectedValueChanged;
            selectedCascade.ItemCheck -= selectedCascade_ItemCheck;
            selectedCascade.Items.Clear();

            var en = true;

            if (_cascades == null || _cascades.Count == 0)
                en = false;
            else
                foreach (var c in _cascades)
                    selectedCascade.Items.Add(c, c.Enabled);

            selectedCascade.Enabled = en;
            linkLabelDuplicateSelected.Enabled = en;
            duplicateSelectedToolStripMenuItem.Enabled = en;
            removeSelectedToolStripMenuItem.Enabled = en;
            appendCascadeSetToolStripMenuItem.Enabled = en;
            saveConfigurationToolStripMenuItem.Enabled = en;

            var imEn = !string.IsNullOrEmpty(_image) && File.Exists(_image);
            loadnextToolStripMenuItem.Enabled = imEn;
            processAgainToolStripMenuItem.Enabled = imEn;
            linkLabelOpenImage.Visible = !imEn;

            selectedCascade.SelectedValueChanged += selectedCascade_SelectedValueChanged;
            selectedCascade.ItemCheck += selectedCascade_ItemCheck;

            if (selectedCascade.Items.Count > 0) 
            selectedCascade.SelectedIndex = _selected;
        }

        private void loadnextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessNextImage();
        }

        private void selectedCascade_SelectedValueChanged(object sender, EventArgs e)
        {
            _selected = selectedCascade.SelectedIndex;
            propertyGridSettings.SelectedObject = selectedCascade.SelectedItem;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Innovation Garage AS", "About");
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
            //_cascades[_selected].Name = _cascades[_selected].ToString() + " (copy)";
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
            try
            {
                var d = new SaveFileDialog {Filter = "XML File|*.xml"};

                if (d.ShowDialog() == DialogResult.OK)
                    Serialization(_cascades, d.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save cascade set", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddOrAppendCascadeFromFile();
        }

        private void AddOrAppendCascadeFromFile(bool append = false)
        {
            try
            {
                var d = new OpenFileDialog { Filter = "XML File|*.xml" };

                if (d.ShowDialog() == DialogResult.OK)
                {
                    if (!append)
                        _cascades.Clear();
                    _cascades.AddRange(Deserialize(d.FileName));
                }

                UpdateCurrent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open cascade set", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // From: https://stackoverflow.com/questions/5005900/how-to-serialize-listt
        public List<Cascade> Deserialize(string a_fileName)
        {
            var deserializer = new XmlSerializer(typeof(List<Cascade>));
            var reader = new StreamReader(a_fileName);
            var obj = deserializer.Deserialize(reader);
            reader.Close();
            return (List<Cascade>) obj;
        }

        public void Serialization(List<Cascade> a_stations, string a_fileName)
        {
            var serializer = new XmlSerializer(typeof(List<Cascade>));

            using (var stream = File.OpenWrite(a_fileName))
            {
                serializer.Serialize(stream, a_stations);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_cascades.Count != 0 && MessageBox.Show("Are you sure?", "Clear current cascades",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _cascades.Clear();
            UpdateCurrent();
        }

        private void appendCascadeSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddOrAppendCascadeFromFile(true);
        }

        private void linkLabelOpenImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenImage();
        }

        private void processAgainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessCurrentImage();
        }

        private void selectedCascade_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _cascades[e.Index].Enabled = e.NewValue == CheckState.Checked;
            UpdateCurrent();
        }
    }
}
