﻿using System.Windows.Forms;

namespace ABC
{
    public partial class FormSim : Form
    {
        private readonly FormGPS mf = null;

        //Form stuff
        public FormSim(Form callingForm)
        {
            mf = callingForm as FormGPS;
            InitializeComponent();
        }
    }
}