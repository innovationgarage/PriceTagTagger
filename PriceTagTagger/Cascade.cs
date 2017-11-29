using System.ComponentModel;
using System.Drawing;

namespace PriceTagTagger
{
    internal class Cascade
    {
        public Cascade() { }

        [Category("Display"),DisplayName("(Name)")]
        public string Name { get; set; }

        [Category("Model")]
        public string CascadePath { get; set; }

        /*[Category("Model")]
        public int ModelStages { get; internal set; }*/

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
    }
}