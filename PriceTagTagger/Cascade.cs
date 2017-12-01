using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PriceTagTagger
{
    public class Cascade
    {
        public Cascade()
        {
            DetectorMinSize = new Size(10, 10);
            DetectorMaxSize = new Size(1000, 1000);
            MarkersBorderSize = 1;
            DetectorScaleFactor = 1.2F;
            DetectorMinNeighbors = 3;
            MarkersBorderColor = Color.Fuchsia;
            LabelPosition = ContentAlignment.TopLeft;
            Enabled = true;
        }

        [Category("Display")]
        [Description("Descriptive name for the current cascade settings")]
        public string Name { get; set; }

        [Category("Label")]
        public string LabelText { get; set; }

        [Category("Label")]
        public ContentAlignment LabelPosition { get; set; }

        [Category("Display")]
        [Description("Enable to see the results in the image of this cascade")]
        public bool Enabled { get; set; }

        [Category("Display")]
        [Description("Layer for drawing the output results")]
        public short ZOrder { get; set; }

        [Category("Model")]
        [Description("Path pointing to the Cascade XML file")]
        [EditorAttribute(typeof(CascadeFileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string CascadePath { get; set; }

        [Category("Detector")]
        public Size DetectorMinSize { get; set; }

        [Category("Detector")]
        public Size DetectorMaxSize { get; set; }

        [Category("Markers")]
        public int MarkersBorderSize { get; set; }

        [Category("Markers")]
        [XmlIgnore]
        public Color MarkersBorderColor { get; set; }

        [Category("Internal"), ReadOnly(true)]
        [XmlElement("MarkersBorderColor")]
        public int MarkersBorderColorInternal
        {
            get => MarkersBorderColor.ToArgb();
            set => MarkersBorderColor = Color.FromArgb(value);
        }

        [Category("Detector")]
        public float DetectorScaleFactor { get; set; }

        [Category("Detector")]
        public int DetectorMinNeighbors { get; set; }

        public override string ToString()
        {
            // TODO: Clean
            try
            {
                return string.IsNullOrEmpty(Name)
                    ? (string.IsNullOrEmpty(CascadePath)
                        ? string.Empty
                        : $"'{Path.GetFileName(CascadePath)}' from {Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(CascadePath)))}/{Path.GetFileName(Path.GetDirectoryName(CascadePath))}"
                    )
                    : Name;
            }
            catch { }

            return "Invalid cascade file";
        }

        public Cascade Clone()
        {
            return (Cascade) MemberwiseClone();
        }
    }

    internal class CascadeFileNameEditor : UITypeEditor
    {
        private readonly OpenFileDialog _ofd = new OpenFileDialog();

        public override UITypeEditorEditStyle GetEditStyle(
            ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(
            ITypeDescriptorContext context,
            IServiceProvider provider,
            object value)
        {
            _ofd.FileName = value.ToString();
            _ofd.Filter = "XML Cascade file|*.xml";

            return _ofd.ShowDialog() == DialogResult.OK ? _ofd.FileName : base.EditValue(context, provider, value);
        }
    }
}