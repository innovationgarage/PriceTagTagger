using Accord.Vision.Detection;

namespace PriceTagTagger
{
    internal class ProgramSettings
    {
        public ProgramSettings()
        {
        }

        public CascadeType CascadeMode { get; set; }

        public string CascadePath { get; set; }

        public string ImagePath { get; set; }

        public int HaarMinSize { get; set; }

        public ObjectDetectorSearchMode SearchMode { get; set; }

    }

    public enum CascadeType
    {
        Face,Nose,Custom
    }
}