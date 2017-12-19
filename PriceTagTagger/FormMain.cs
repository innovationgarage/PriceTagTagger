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
using WPFFolderBrowser;

namespace PriceTagTagger
{
    public partial class FormMain : Form
    {
        private bool _processAgain;
        private readonly List<Cascade> _cascades;
        private readonly OpenFileDialog openImageDialog, openSetDialog;
        private readonly SaveFileDialog saveImageDialog, saveSetDialog, saveExportDialog;
        private string _image;
        private int _selected = 0;

        public FormMain()
        {
            InitializeComponent();
            selectedCascade.GetBackColor += SelectedCascade_GetBackColor;
            selectedCascade.GetForeColor += SelectedCascade_GetForeColor;

            _cascades = new List<Cascade>();

            // Load and save
            openImageDialog = new OpenFileDialog { Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png" };
            saveImageDialog = new SaveFileDialog { Filter = "PNG image|*.png" };
            saveSetDialog = new SaveFileDialog { Filter = "XML file|*.xml" };
            saveExportDialog = new SaveFileDialog { Filter = "CSV file|*.csv" };
            openSetDialog = new OpenFileDialog { Filter = "XML file|*.xml" };

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
            _image = Utilities.GetNextFile(_image);
            ProcessCurrentImage();
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
            var detected = new List<CascadeMatch>();

            for (var i = 0; i < _cascades.Count; i++)
            {
                var c = _cascades[i];

                backgroundWorkerLoadImage.ReportProgress((int) Utilities.Map(i, 0, _cascades.Count, 30, 80));

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

                    detected.AddRange(detectedObjects.Select(r => new  CascadeMatch(r, c, _image)));
                }
                catch (Exception ex)
                {
                    backgroundWorkerLoadImage.ReportProgress(-1, "Error applying '" + c.Name + "': " + ex.Message);
                    Thread.Sleep(2000);
                }
            }

            // Apply ZOrder
            detected.Sort(Utilities.ZOrderComparer);

            foreach (var d in detected)
            {
                CvInvoke.Rectangle(image, d.Rectangle, new Bgr(d.Cascade.MarkersBorderColor).MCvScalar,
                    d.Cascade.MarkersBorderSize);

                AdjustPosition(d.Rectangle, d.Cascade.LabelPosition, out var p);

                CvInvoke.PutText(image, d.Cascade.LabelText, p, FontFace.HersheyPlain,d.Cascade.MarkersBorderSize*0.5,
                    new Bgr(d.Cascade.MarkersBorderColor).MCvScalar, d.Cascade.MarkersBorderSize, LineType.AntiAlias);
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

            UpdateGUI();
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
            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {
                _image = openImageDialog.FileName;
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

            propertiesToolStripMenuItem.Enabled = en;
            selectedCascade.Enabled = en;
            linkLabelDuplicateSelected.Enabled = en;
            duplicateSelectedToolStripMenuItem.Enabled = en;
            removeSelectedToolStripMenuItem.Enabled = en;
            appendCascadeSetToolStripMenuItem.Enabled = en;
            saveConfigurationToolStripMenuItem.Enabled = en;

            var imEn = !string.IsNullOrEmpty(_image) && File.Exists(_image);
            loadnextToolStripMenuItem.Enabled = imEn;
            saveToolStripMenuItem.Enabled = imEn;
            processAgainToolStripMenuItem.Enabled = imEn;
            linkLabelOpenImage.Visible = !imEn;

            selectedCascade.SelectedValueChanged += selectedCascade_SelectedValueChanged;
            selectedCascade.ItemCheck += selectedCascade_ItemCheck;

            if (selectedCascade.Items.Count > 0) 
            selectedCascade.SelectedIndex = _selected;

            abortRunToolStripMenuItem.Enabled = backgroundWorkerLoadImage.IsBusy;
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
            MessageBox.Show("Innovation Garage AS" + Environment.NewLine + "v0.1 December 2017", "About");
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
                if (saveSetDialog.ShowDialog() == DialogResult.OK)
                    Serialization(_cascades, saveSetDialog.FileName);
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
                if (openSetDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!append)
                        _cascades.Clear();
                    _cascades.AddRange(Deserialize(openSetDialog.FileName));
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

        private void abortRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            backgroundWorkerLoadImage.CancelAsync();
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProperties(_cascades[selectedCascade.SelectedIndex].CascadePath).ShowDialog();
        }

        private void generateTextOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new WPFFolderBrowserDialog { Title = "Folder with the images to process" };

            if (d.ShowDialog().Value)
            {
                if (saveExportDialog.ShowDialog() == DialogResult.OK)
                {
                    var f = new FormBatchProcessing(d.FileName, _cascades);
                    if (f.ShowDialog()== DialogResult.OK)
                    {
                        var csv = new List<string> {"filename,width,height,class,xmin,ymin,xmax,ymax"};

                        foreach (var c in f.Output)
                        {
                            var img = Image.FromFile(c.ImagePath);
                            csv.Add($"{Path.GetFileName(c.ImagePath)},{img.Width},{img.Height},{c.Cascade.Name},{c.Rectangle.Left},{c.Rectangle.Top},{c.Rectangle.Right},{c.Rectangle.Bottom}");
                        }

                        File.WriteAllLines(saveExportDialog.FileName, csv);
                    }
                }
            }
        }

        private void selectedCascade_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _cascades[e.Index].Enabled = e.NewValue == CheckState.Checked;
            UpdateCurrent();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (saveImageDialog.ShowDialog() == DialogResult.OK)
                    pictureBoxViewer.Image.Save(saveImageDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save the image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
