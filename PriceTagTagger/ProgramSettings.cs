using Accord.Vision.Detection;
using System.ComponentModel;
using System.Drawing;

namespace PriceTagTagger
{
    internal class ProgramSettings
    {
        public ProgramSettings() {}

        [Category("Model")]
        public CascadeType CascadeMode { get; set; }

        [Category("Model")]
        public string CascadePath { get; set; }

        [Category("Input")]
        public string ImagePath { get; set; }

        [Category("Model")]
        public int HaarMinSize { get; set; }

        [Category("Model")]
        public ObjectDetectorSearchMode SearchMode { get; set; }

        [Category("Markers")]
        public int MarkersBorderSize { get; set; }

        [Category("Markers")]
        public Color MarkersBorderColor { get; set; }

    }

    public enum CascadeType
    {
        Face,Nose,Custom
    }
}