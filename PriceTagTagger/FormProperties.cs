using Emgu.CV;
using System.IO;
using System.Windows.Forms;

namespace PriceTagTagger
{
    public partial class FormProperties : Form
    {
        public FormProperties(string cascadePath)
        {
            InitializeComponent();
            Text = Path.GetFileName(cascadePath) + " Properties";
            propertyGridMain.SelectedObject = new CascadeClassifier(cascadePath);
        }

        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
