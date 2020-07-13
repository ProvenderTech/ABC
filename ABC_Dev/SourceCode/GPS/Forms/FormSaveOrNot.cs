using System;
using System.Windows.Forms;

namespace ABC
{
    public partial class FormSaveOrNot : Form
    {
        //class variables
        //private readonly FormGPS mainForm = null;

        public FormSaveOrNot()
        {
            //get copy of the calling main form
            //mainForm = callingForm as FormGPS;

            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            //back to FormGPS
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ignore;
            Close();
        }
    }
}