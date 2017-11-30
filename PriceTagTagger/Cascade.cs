using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace PriceTagTagger
{
    internal class Cascade
    {
        [Category("Display"), DisplayName("(Name)")]
        public string Name { get; set; }

        [Category("Model")]
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
                $"{Path.GetFileName(CascadePath)} in {Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(CascadePath)))}/{Path.GetFileName(Path.GetDirectoryName(CascadePath))}"
            ) : Name;
        }
    }
}