using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;

namespace PriceTagTagger
{
    internal class Cascade
    {
        [Category("Display"), DisplayName("(Name)")]
        [Description("Descriptive name for the current cascade settings")]
        public string Name { get; set; }

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
        public Color MarkersBorderColor { get; set; }

        [Category("Detector")]
        public float DetectorScaleFactor { get; set; }

        [Category("Detector")]
        public int DetectorMinNeighbors { get; set; }

        public override string ToString()
        {
            // TODO: Clean
            return string.IsNullOrEmpty(Name) ? (string.IsNullOrEmpty(CascadePath)?"undefined": 
                $"'{Path.GetFileName(CascadePath)}' (from {Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(CascadePath)))}/{Path.GetFileName(Path.GetDirectoryName(CascadePath))})"
            ) : Name;
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