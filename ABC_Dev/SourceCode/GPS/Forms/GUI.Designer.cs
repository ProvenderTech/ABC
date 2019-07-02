//Please, if you use this, share the improvements

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ABC.Properties;
using Microsoft.Win32;
using System.Collections.Generic;

namespace ABC
{
    public partial class FormGPS
    {
        //ABLines directory
        public string ablinesDirectory;

        //colors for sections and field background
        private byte redSections, grnSections, bluSections;
        public byte redField, grnField, bluField;
        public byte flagColor = 0;

        //how many cm off line per big pixel
        public int lightbarCmPerPixel;

        //polygon mode for section drawing
        private bool isDrawPolygons;

        //Is it in 2D or 3D, metric or imperial, display lightbar, display grid etc
        public bool isIn3D = true, isMetric = true, isLightbarOn = true, isGridOn, isSideGuideLines = true;
        public bool isPureDisplayOn = true, isSkyOn = true, isBigAltitudeOn = false;

        //master Manual and Auto, 3 states possible
        public enum btnStates { Off, Auto, On }
        public btnStates manualBtnState = btnStates.Off;
        public btnStates autoBtnState = btnStates.Off;

        //section button states
        public enum manBtn { Off, Auto, On }

        private void LoadGUI()
        {
            //metric settings
            isMetric = Settings.Default.setMenu_isMetric;
            metricToolStrip.Checked = isMetric;

            if (isMetric)
            {
                lblSpeedUnits.Text = "kmh";
                metricToolStrip.Checked = true;
                imperialToolStrip.Checked = false;
                //lblFlowLeft.Text = "LPM"; lblFlowRight.Text = "LPM";
            }
            else
            {
                lblSpeedUnits.Text = "mph";
                metricToolStrip.Checked = false;
                imperialToolStrip.Checked = true;
                //lblFlowLeft.Text = "GPM"; lblFlowRight.Text = "GPM";

            }

            //load up colors
            redField = (Settings.Default.setF_FieldColorR);
            grnField = (Settings.Default.setF_FieldColorG);
            bluField = (Settings.Default.setF_FieldColorB);

            redSections = Settings.Default.setF_SectionColorR;
            grnSections = Settings.Default.setF_SectionColorG;
            bluSections = Settings.Default.setF_SectionColorB;


            //set up grid and lightbar
            isGridOn = Settings.Default.setMenu_isGridOn;
            gridToolStripMenuItem.Checked = isGridOn;

            //log NMEA 
            isLogNMEA = Settings.Default.setMenu_isLogNMEA;
            logNMEAMenuItem.Checked = isLogNMEA;

            isLightbarOn = Settings.Default.setMenu_isLightbarOn;
            lightbarToolStripMenuItem.Checked = isLightbarOn;

            isSideGuideLines = Settings.Default.setMenu_isSideGuideLines;
            sideGuideLines.Checked = isSideGuideLines;

            isPureDisplayOn = Settings.Default.setMenu_isPureOn;
            pursuitLineToolStripMenuItem.Checked = isPureDisplayOn;

            isSkyOn = Settings.Default.setMenu_isSkyOn;
            skyToolStripMenu.Checked = isSkyOn;

            simulatorOnToolStripMenuItem.Checked = Settings.Default.setMenu_isSimulatorOn;
            if (simulatorOnToolStripMenuItem.Checked)
            {
                panelSimControls.Visible = true;
                timerSim.Enabled = true;
            }
            else
            {
                panelSimControls.Visible = false;
                timerSim.Enabled = false;
            }

            LineUpManualBtns();
            tabControl1.SelectedIndex = 3;

            //default to come up in mini pnel, exit remembers 
            SwapBatmanPanels();
            SwapBatmanPanels();

        }

        //force all the buttons same according to two main buttons
        private void ManualAllBtnsUpdate()
        {
            ManualBtnUpdate(0, btnSection1Man);
            ManualBtnUpdate(1, btnSection2Man);
            ManualBtnUpdate(2, btnSection3Man);
            ManualBtnUpdate(3, btnSection4Man);
            ManualBtnUpdate(4, btnSection5Man);
            ManualBtnUpdate(5, btnSection6Man);
            ManualBtnUpdate(6, btnSection7Man);
            ManualBtnUpdate(7, btnSection8Man);
            ManualBtnUpdate(8, btnSection9Man);
            ManualBtnUpdate(9, btnSection10Man);
            ManualBtnUpdate(10, btnSection11Man);
            ManualBtnUpdate(11, btnSection12Man);
        }

        //hide the left panel
        public void SwapBatmanPanels()
        {
            if (Properties.Settings.Default.setDisplay_isLargePanel)
            {
                //Batman mini-panel shows
                panelSimControls.Left = 245;
                oglMain.Left = 240;
                oglMain.Width = Width - 240 - 102;
                btnpTiltDown.Left = 245;
                btnpTiltUp.Left = 245;
                btnZoomIn.Left = 245;
                btnZoomOut.Left = 245;
                btnZoomExtents.Left = 245;
                txtDistanceOffABLine.Left = (Width - 245-100)/2 + 185;
                panelBatman.Visible = true;
                tabControl1.Visible = false;
                Properties.Settings.Default.setDisplay_isLargePanel = false;
                Properties.Settings.Default.Save();
                LineUpManualBtns();
            }
            else
            {
                //Big tab will be visible
                panelSimControls.Left = 345;
                oglMain.Left = 340;
                oglMain.Width = Width - 340 - 102;
                btnpTiltDown.Left = 345;
                btnpTiltUp.Left = 345;
                btnZoomIn.Left = 345;
                btnZoomOut.Left = 345;
                btnZoomExtents.Left = 345;
                txtDistanceOffABLine.Left = (Width - 345 - 100)/2 + 287;
                txtDistanceOffABLine.Top = -1;
                tabControl1.SelectedIndex = 3;
                panelBatman.Visible = false;
                tabControl1.Visible = true;
                Properties.Settings.Default.setDisplay_isLargePanel = true;
                Properties.Settings.Default.Save();
                LineUpManualBtns();

            }
        }

        //line up section On Off Auto buttons based on how many there are
        public void LineUpManualBtns()
        {
            int top = 0;
            if (panelSimControls.Visible == true)
            {
                top = 190;
                if (vehicle.numOfSections > 8) top = 230;
            }
            else{
                top = 140;
                if (vehicle.numOfSections > 8) top = 180;
            }

            btnSection1Man.Top = Height - top;
            btnSection2Man.Top = Height - top;
            btnSection3Man.Top = Height - top;
            btnSection4Man.Top = Height - top;
            btnSection5Man.Top = Height - top;
            btnSection6Man.Top = Height - top;
            btnSection7Man.Top = Height - top;
            btnSection8Man.Top = Height - top;

            int first2Thirds;
            if (tabControl1.Visible) first2Thirds = (Width - 535) / 2 + 385;
            else first2Thirds = (Width-395) / 2 + 260;

            int even = 60;
            int offset = 7;

            switch (vehicle.numOfSections)
            {
                case 1:
                    btnSection1Man.Left = (first2Thirds) - 20;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = false;
                    btnSection3Man.Visible = false;
                    btnSection4Man.Visible = false;
                    btnSection5Man.Visible = false;
                    btnSection6Man.Visible = false;
                    btnSection7Man.Visible = false;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 2:
                    btnSection1Man.Left = (first2Thirds)+ offset - even;
                    btnSection2Man.Left = (first2Thirds)+ offset + 0;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = false;
                    btnSection4Man.Visible = false;
                    btnSection5Man.Visible = false;
                    btnSection6Man.Visible = false;
                    btnSection7Man.Visible = false;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 3:
                    btnSection1Man.Left = (first2Thirds) - 80;
                    btnSection2Man.Left = (first2Thirds) - 20;
                    btnSection3Man.Left = (first2Thirds) + 40;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = false;
                    btnSection5Man.Visible = false;
                    btnSection6Man.Visible = false;
                    btnSection7Man.Visible = false;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 4:
                    btnSection1Man.Left = (first2Thirds)+ offset - even*2;
                    btnSection2Man.Left = (first2Thirds)+ offset - even;
                    btnSection3Man.Left = (first2Thirds)+ offset + 0;
                    btnSection4Man.Left = (first2Thirds)+ offset + even;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = false;
                    btnSection6Man.Visible = false;
                    btnSection7Man.Visible = false;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 5:
                    btnSection1Man.Left = (first2Thirds) - 140;
                    btnSection2Man.Left = (first2Thirds) - 80;
                    btnSection3Man.Left = (first2Thirds) - 20;
                    btnSection4Man.Left = (first2Thirds) + 40;
                    btnSection5Man.Left = (first2Thirds) + 100;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = false;
                    btnSection7Man.Visible = false;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 6:
                    btnSection1Man.Left = (first2Thirds)+ offset - even*3;
                    btnSection2Man.Left = (first2Thirds)+ offset - even*2;
                    btnSection3Man.Left = (first2Thirds)+ offset - even;
                    btnSection4Man.Left = (first2Thirds)+ offset + 0;
                    btnSection5Man.Left = (first2Thirds)+ offset+ even;
                    btnSection6Man.Left = (first2Thirds) + offset + even*2;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = false;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 7:
                    btnSection1Man.Left = (first2Thirds) - 200;
                    btnSection2Man.Left = (first2Thirds) - 140;
                    btnSection3Man.Left = (first2Thirds) - 80;
                    btnSection4Man.Left = (first2Thirds) - 20;
                    btnSection5Man.Left = (first2Thirds) + 40;
                    btnSection6Man.Left = (first2Thirds) + 100;
                    btnSection7Man.Left = (first2Thirds) + 160;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = true;
                    btnSection8Man.Visible = false;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;

                    break;

                case 8:
                    btnSection1Man.Left = (first2Thirds)+ offset - even*4;           //390;
                    btnSection2Man.Left = (first2Thirds)+ offset - even*3;           //290;
                    btnSection3Man.Left = (first2Thirds)+ offset - even*2;           //190;
                    btnSection4Man.Left = (first2Thirds)+ offset - even;           //90;
                    btnSection5Man.Left = (first2Thirds)+ offset + 0;           //10;
                    btnSection6Man.Left = (first2Thirds)+ offset + even;           //110;
                    btnSection7Man.Left = (first2Thirds)+ offset + even*2;           //210;
                    btnSection8Man.Left = (first2Thirds)+ offset + even*3;           //310;
                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = true;
                    btnSection8Man.Visible = true;
                    btnSection9Man.Visible = false;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 9:
                    btnSection1Man.Top = Height - top;
                    btnSection2Man.Top = Height - top;
                    btnSection3Man.Top = Height - top + 42;
                    btnSection4Man.Top = Height - top;
                    btnSection5Man.Top = Height - top + 42;
                    btnSection6Man.Top = Height - top;
                    btnSection7Man.Top = Height - top + 42;
                    btnSection8Man.Top = Height - top;
                    btnSection9Man.Top = Height - top;

                    even = 80; offset = 14;
                    btnSection1Man.Left = (first2Thirds) + offset - even * 3;
                    btnSection2Man.Left = (first2Thirds) + offset - even * 2;
                    btnSection4Man.Left = (first2Thirds) + offset - even;
                    btnSection6Man.Left = (first2Thirds) + offset + 0;
                    btnSection8Man.Left = (first2Thirds) + offset + even;
                    btnSection9Man.Left = (first2Thirds) + offset + even * 2;

                    btnSection3Man.Left = (first2Thirds) - 100;
                    btnSection5Man.Left = (first2Thirds) - 20;
                    btnSection7Man.Left = (first2Thirds) + 60;

                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = true;
                    btnSection8Man.Visible = true;
                    btnSection9Man.Visible = true;
                    btnSection10Man.Visible = false;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 10:
                    btnSection1Man.Top = Height - top;
                    btnSection2Man.Top = Height - top + 42;
                    btnSection3Man.Top = Height - top;
                    btnSection4Man.Top = Height - top + 42;
                    btnSection5Man.Top = Height - top;
                    btnSection6Man.Top = Height - top + 42;
                    btnSection7Man.Top = Height - top;
                    btnSection8Man.Top = Height - top + 42;
                    btnSection9Man.Top = Height - top;
                    btnSection10Man.Top = Height - top + 42;


                    btnSection1Man.Left = (first2Thirds) - 200;
                    btnSection3Man.Left = (first2Thirds) - 120;
                    btnSection5Man.Left = (first2Thirds) - 40;
                    btnSection7Man.Left = (first2Thirds) + 40;
                    btnSection9Man.Left = (first2Thirds) + 120;

                    btnSection2Man.Left = (first2Thirds) - 160;
                    btnSection4Man.Left = (first2Thirds) - 80;  
                    btnSection6Man.Left = (first2Thirds) - 0; 
                    btnSection8Man.Left = (first2Thirds) + 80;  
                    btnSection10Man.Left = (first2Thirds) + 160;        


                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = true;
                    btnSection8Man.Visible = true;
                    btnSection9Man.Visible = true;
                    btnSection10Man.Visible = true;
                    btnSection11Man.Visible = false;
                    btnSection12Man.Visible = false;
                    break;

                case 11:
                    btnSection1Man.Top = Height - top;
                    btnSection2Man.Top = Height - top;
                    btnSection3Man.Top = Height - top + 42;
                    btnSection4Man.Top = Height - top;
                    btnSection5Man.Top = Height - top + 42;
                    btnSection6Man.Top = Height - top;
                    btnSection7Man.Top = Height - top + 42;
                    btnSection8Man.Top = Height - top;
                    btnSection9Man.Top = Height - top + 42;
                    btnSection10Man.Top = Height - top;
                    btnSection11Man.Top = Height - top;


                    btnSection1Man.Left = (first2Thirds) - 200;
                    btnSection2Man.Left = (first2Thirds) - 140;
                    btnSection4Man.Left = (first2Thirds) - 80;
                    btnSection6Man.Left = (first2Thirds) - 20;
                    btnSection8Man.Left = (first2Thirds) + 40;
                    btnSection10Man.Left = (first2Thirds) + 100;
                    btnSection11Man.Left = (first2Thirds) + 160;

                    btnSection3Man.Left = (first2Thirds) - 110;
                    btnSection5Man.Left = (first2Thirds) - 50;
                    btnSection7Man.Left = (first2Thirds) +10;
                    btnSection9Man.Left = (first2Thirds) + 70;


                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = true;
                    btnSection8Man.Visible = true;
                    btnSection9Man.Visible = true;
                    btnSection10Man.Visible = true;
                    btnSection11Man.Visible = true;
                    btnSection12Man.Visible = false;
                    break;

                case 12:
                    btnSection1Man.Top = Height - top;
                    btnSection2Man.Top = Height - top + 42;
                    btnSection3Man.Top = Height - top;
                    btnSection4Man.Top = Height - top + 42;
                    btnSection5Man.Top = Height - top;
                    btnSection6Man.Top = Height - top + 42;
                    btnSection7Man.Top = Height - top;
                    btnSection8Man.Top = Height - top + 42;
                    btnSection9Man.Top = Height - top;
                    btnSection10Man.Top = Height - top + 42;
                    btnSection11Man.Top = Height - top;
                    btnSection12Man.Top = Height - top + 42;

                    offset = -9; even = 70;
                    btnSection1Man.Left = (first2Thirds)  + offset - even*3;
                    btnSection3Man.Left = (first2Thirds)  + offset - even*2;
                    btnSection5Man.Left = (first2Thirds)  + offset - even;
                    btnSection7Man.Left = (first2Thirds)  + offset + 0;
                    btnSection9Man.Left = (first2Thirds)  + offset+ even;                     
                    btnSection11Man.Left = (first2Thirds) + offset + even * 2;

                    offset = 22;
                    btnSection2Man.Left = (first2Thirds)  + offset - even*3;
                    btnSection4Man.Left = (first2Thirds)  + offset - even*2;
                    btnSection6Man.Left = (first2Thirds)  + offset - even;
                    btnSection8Man.Left = (first2Thirds)  + offset + 0;
                    btnSection10Man.Left = (first2Thirds) + offset + even;
                    btnSection12Man.Left = (first2Thirds) + offset + even * 2;

                    btnSection1Man.Visible = true;
                    btnSection2Man.Visible = true;
                    btnSection3Man.Visible = true;
                    btnSection4Man.Visible = true;
                    btnSection5Man.Visible = true;
                    btnSection6Man.Visible = true;
                    btnSection7Man.Visible = true;
                    btnSection8Man.Visible = true;
                    btnSection9Man.Visible = true;
                    btnSection10Man.Visible = true;
                    btnSection11Man.Visible = true;
                    btnSection12Man.Visible = true;
                    break;
            }

            if (isJobStarted)
            {
                switch (vehicle.numOfSections)
                {
                    case 1:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = false;
                        btnSection3Man.Enabled = false;
                        btnSection4Man.Enabled = false;
                        btnSection5Man.Enabled = false;
                        btnSection6Man.Enabled = false;
                        btnSection7Man.Enabled = false;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 2:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = false;
                        btnSection4Man.Enabled = false;
                        btnSection5Man.Enabled = false;
                        btnSection6Man.Enabled = false;
                        btnSection7Man.Enabled = false;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 3:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = false;
                        btnSection5Man.Enabled = false;
                        btnSection6Man.Enabled = false;
                        btnSection7Man.Enabled = false;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 4:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = false;
                        btnSection6Man.Enabled = false;
                        btnSection7Man.Enabled = false;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 5:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = false;
                        btnSection7Man.Enabled = false;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 6:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = false;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 7:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = true;
                        btnSection8Man.Enabled = false;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 8:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = true;
                        btnSection8Man.Enabled = true;
                        btnSection9Man.Enabled = false;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 9:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = true;
                        btnSection8Man.Enabled = true;
                        btnSection9Man.Enabled = true;
                        btnSection10Man.Enabled = false;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 10:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = true;
                        btnSection8Man.Enabled = true;
                        btnSection9Man.Enabled = true;
                        btnSection10Man.Enabled = true;
                        btnSection11Man.Enabled = false;
                        btnSection12Man.Enabled = false;
                        break;

                    case 11:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = true;
                        btnSection8Man.Enabled = true;
                        btnSection9Man.Enabled = true;
                        btnSection10Man.Enabled = true;
                        btnSection11Man.Enabled = true;
                        btnSection12Man.Enabled = false;
                        break;

                    case 12:
                        btnSection1Man.Enabled = true;
                        btnSection2Man.Enabled = true;
                        btnSection3Man.Enabled = true;
                        btnSection4Man.Enabled = true;
                        btnSection5Man.Enabled = true;
                        btnSection6Man.Enabled = true;
                        btnSection7Man.Enabled = true;
                        btnSection8Man.Enabled = true;
                        btnSection9Man.Enabled = true;
                        btnSection10Man.Enabled = true;
                        btnSection11Man.Enabled = true;
                        btnSection12Man.Enabled = true;
                        break;
                }
            }
        }

        //update individual btn based on state after push
        private void ManualBtnUpdate(int sectNumber, Button btn)
        {
            switch (section[sectNumber].manBtnState)
            {
                case manBtn.Off:
                    section[sectNumber].manBtnState = manBtn.Auto;
                    btn.BackColor = Color.Lime;
                    break;

                case manBtn.Auto:
                    section[sectNumber].manBtnState = manBtn.On;
                    btn.BackColor = Color.Yellow;
                    break;

                case manBtn.On:
                    section[sectNumber].manBtnState = manBtn.Off;
                    btn.BackColor = Color.Red;
                    break;
            }
        }

        // Buttons //-----------------------------------------------------------------------
        private void btnStanley_Click(object sender, EventArgs e)
        {
            isStanleyUsed = !isStanleyUsed;
            if (isStanleyUsed) btnStanley.Text = "StanLee";
            else btnStanley.Text = "Pure P";
            Properties.Vehicle.Default.setVehicle_isStanleyUsed = isStanleyUsed;
            Properties.Vehicle.Default.Save();
        }

        private void btnStartStopNtrip_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.setNTRIP_isOn)
            {
                if (isNTRIP_RequiredOn)
                {
                    ShutDownNTRIP();
                    btnStartStopNtrip.Text = "Start";
                    lblWatch.Text = "Stopped";
                    lblNTRIPSeconds.Text = "Offline ";
                    isNTRIP_RequiredOn = false;
                }
                else
                {
                    isNTRIP_RequiredOn = true;
                    btnStartStopNtrip.Text = "Stop";
                    lblWatch.Text = "Waiting";
                }
            }
            else
            {
                TimedMessageBox(2000, "Turn ON Ntrip Client", " NTRIP Client Not Set Up");
            }
        }

        //Guidance types
        private void btnAutoSteer_Click(object sender, EventArgs e)
        {
            if (isAutoSteerBtnOn)
            {
                isAutoSteerBtnOn = false;
                btnAutoSteer.Image = Properties.Resources.AutoSteerOff;
            }
            else
            {
                if (ABLine.isABLineSet | ct.isContourBtnOn | curve.isCurveSet)
                {
                    isAutoSteerBtnOn = true;
                    btnAutoSteer.Image = Properties.Resources.AutoSteerOn;
                }
                else
                {
                    var form = new FormTimedMessage(2000,(gStr.gsNoGuidanceLines),(gStr.gsTurnOnContourOrABLine));
                    form.Show();
                }
            }
        }
        private void btnABLine_Click(object sender, EventArgs e)
        {
            //if contour is on, turn it off
            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }

            //new direction so reset where to put turn diagnostic
            btnContourPriority.Enabled = true;
            
            if (ABLine.isABLineSet)
            {
                ABLine.abHeading = glm.toRadians(AB0.heading);
                ABLine.refPoint1.easting = AB0.X;
                ABLine.refPoint1.northing = AB0.Y;
                ABLine.SetABLineByHeading();
            }

            using (var form = new FormABLine(this))
            {
                ABLine.isABLineBeingSet = true;
                txtDistanceOffABLine.Visible = true;

                var result = form.ShowDialog();

                //Comes back

                //if ABLine isn't set, turn off the YouTurn
                if (!ABLine.isABLineSet)
                {
                    ABLine.isABLineBeingSet = false;
                    txtDistanceOffABLine.Visible = false;

                    //change image to reflect on off
                    btnABLine.Image = Properties.Resources.ABLineOff;
                    ABLine.isBtnABLineOn = false;

                    ABLine.isABLineBeingSet = false;

                    if (isAutoSteerBtnOn) btnAutoSteer.PerformClick();
                    btnCurve.Enabled = true;

                }

                //ab line is made
                else
                {
                    //change image to reflect on off
                    btnABLine.Image = Properties.Resources.ABLineOn;
                    ABLine.isBtnABLineOn = true;

                    ABLine.isABLineBeingSet = false;
                    btnCurve.Enabled = false;
                }
            }
        }
        private void btnCurve_Click(object sender, EventArgs e)
        {
            //if contour is on, turn it off
            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }

            //check if window already exists
            Form fc = Application.OpenForms["FormABCurve"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            curve.isCurveBtnOn = !curve.isCurveBtnOn;
            btnCurve.Image = curve.isCurveBtnOn ? Properties.Resources.CurveOn : Properties.Resources.CurveOff;

            if (curve.isCurveBtnOn)
            {
                //kill the ABLine
                ABLine.DeleteAB();
                ABLine.tramPassEvery = 0;
                ABLine.passBasedOn = 0;

                //save the no ABLine;
                FileSaveABLine();
                ABLine.isABLineBeingSet = false;
                txtDistanceOffABLine.Visible = false;

                //change image to reflect on off
                btnABLine.Image = Properties.Resources.ABLineOff;
                ABLine.isBtnABLineOn = false;

                ABLine.isABLineBeingSet = false;
                //btnContour.Enabled = false;
                btnABLine.Enabled = false;

                if (isAutoSteerBtnOn) btnAutoSteer.PerformClick();
                btnContourPriority.Enabled = true;

                Form form = new FormABCurve(this);
                form.Show();
            }
            else
            {
                btnContour.Enabled = true;
                btnABLine.Enabled = true;
                curve.isOkToAddPoints = false;
                curve.isCurveSet = false;
                btnContourPriority.Enabled = false;
                //curve.ResetCurveLine();
            }
        }
        private void btnContour_Click(object sender, EventArgs e)
        {
            ct.isContourBtnOn = !ct.isContourBtnOn;
            btnContour.Image = ct.isContourBtnOn ? Properties.Resources.ContourOn : Properties.Resources.ContourOff;

            if (ct.isContourBtnOn)
            {
                btnContourPriority.Enabled = true;

                if (ct.isRightPriority)
                {
                    btnContourPriority.Image = Properties.Resources.ContourPriorityRight;
                }
                else
                {
                    btnContourPriority.Image = Properties.Resources.ContourPriorityLeft;
                }
            }

            else
            {
                btnContourPriority.Image = Properties.Resources.Snap2;
            }
        }
        private void btnContourPriority_Click(object sender, EventArgs e)
        {
            if (ct.isContourBtnOn)
            {

                ct.isRightPriority = !ct.isRightPriority;

                if (ct.isRightPriority)
                {
                    btnContourPriority.Image = Properties.Resources.ContourPriorityRight;
                }
                else
                {
                    btnContourPriority.Image = Properties.Resources.ContourPriorityLeft;
                }
            }
            else
            {
                if (ABLine.isABLineSet)
                {
                    ABLine.SnapABLine();

                    //DialogResult result3 = MessageBox.Show("Save AB Line Snap?",
                    //                            "Save or Not",
                    //                            MessageBoxButtons.YesNo,
                    //                            MessageBoxIcon.Question,
                    //                            MessageBoxDefaultButton.Button2);
                    //if (result3 == DialogResult.Yes)
                    {
                        FileSaveABLine();
                    }
                }
                else if (curve.isCurveSet)
                {
                    curve.SnapABCurve();
                }
                else
                {
                    var form = new FormTimedMessage(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrABLine));
                    form.Show();
                }
            }
        }

        //Snaps
        private void SnapSmallLeft()
        {
            if (!ct.isContourBtnOn)
            {
                if (ABLine.isABLineSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistanceSmall;

                    ABLine.MoveABLine(-dist);
                    FileSaveABLine();
                }
                else if (curve.isCurveSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistanceSmall;

                    curve.MoveABCurve(-dist);
                }
                else
                {
                    var form = new FormTimedMessage(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrABLine));
                    form.Show();
                }
            }
        }
        private void SnapSmallRight()
        {
            if (!ct.isContourBtnOn)
            {
                if (ABLine.isABLineSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistanceSmall;
                    ABLine.MoveABLine(dist);
                    FileSaveABLine();
                }
                else if (curve.isCurveSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistanceSmall;
                    curve.MoveABCurve(dist);
                }
                else
                {
                    var form = new FormTimedMessage(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrABLine));
                    form.Show();
                }
            }
        }
        private void SnapRight()
        {
            if (!ct.isContourBtnOn)
            {
                if (ABLine.isABLineSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistance;

                    ABLine.MoveABLine(dist);
                    FileSaveABLine();
                }
                else if (curve.isCurveSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistance;
                    curve.MoveABCurve(dist);
                }
                else
                {
                    var form = new FormTimedMessage(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrABLine));
                    form.Show();
                }
            }

        }
        private void SnapLeft()
        {
            if (!ct.isContourBtnOn)
            {
                if (ABLine.isABLineSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistance;

                    ABLine.MoveABLine(-dist);
                    FileSaveABLine();
                }
                else if (curve.isCurveSet)
                {
                    //snap distance is in cm
                    double dist = 0.01 * Properties.Settings.Default.setDisplay_snapDistance;

                    curve.MoveABCurve(-dist);
                }
                else
                {
                    var form = new FormTimedMessage(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrABLine));
                    form.Show();
                }
            }

        }

        private void btnSmallSnapLeft_Click(object sender, EventArgs e)
        { 
                SnapSmallLeft();
        }
        private void btnSmallSnapRight_Click(object sender, EventArgs e)
        {
            SnapSmallRight();
        }
        private void btnSnapRight_Click(object sender, EventArgs e)
        {
            SnapRight();
        }
        private void btnSnapLeft_Click(object sender, EventArgs e)
        {
            SnapLeft();
        }

        private void btnSmallLeft2_Click(object sender, EventArgs e)
        {
            SnapSmallLeft();
        }
        private void btnSmallRight2_Click(object sender, EventArgs e)
        {
            SnapSmallRight();
        }
        private void btnBigLeft2_Click(object sender, EventArgs e)
        {
            SnapLeft();
        }
        private void btnBigRight2_Click(object sender, EventArgs e)
        {
            SnapRight();
        }

        //Section Manual and Auto
        private void btnManualOffOn_Click(object sender, EventArgs e)
        {
            System.Media.SystemSounds.Asterisk.Play();

            switch (manualBtnState)
            {
                case btnStates.Off:
                    manualBtnState = btnStates.On;
                    btnManualOffOn.Image = Properties.Resources.ManualOn;

                    //if Auto is on, turn it off
                    autoBtnState = btnStates.Off;
                    btnSectionOffAutoOn.Image = Properties.Resources.SectionMasterOff;

                    //turn all the sections allowed and update to ON!! Auto changes to ON
                    for (int j = 0; j < vehicle.numOfSections; j++)
                    {
                        section[j].isAllowedOn = true;
                        section[j].manBtnState = manBtn.Auto;
                    }

                    ManualAllBtnsUpdate();
                    break;

                case btnStates.On:
                    manualBtnState = btnStates.Off;
                    btnManualOffOn.Image = Properties.Resources.ManualOff;

                    //turn section buttons all OFF or Auto if SectionAuto was on or off
                    for (int j = 0; j < vehicle.numOfSections; j++)
                    {
                        section[j].isAllowedOn = false;
                        section[j].manBtnState = manBtn.On;
                    }

                    //Update the button colors and text
                    ManualAllBtnsUpdate();
                    break;
            }
        }
        private void btnSectionOffAutoOn_Click(object sender, EventArgs e)
        {
            System.Media.SystemSounds.Exclamation.Play();

            switch (autoBtnState)
            {
                case btnStates.Off:

                    autoBtnState = btnStates.Auto;
                    btnSectionOffAutoOn.Image = Properties.Resources.SectionMasterOn;

                    //turn off manual if on
                    manualBtnState = btnStates.Off;
                    btnManualOffOn.Image = Properties.Resources.ManualOff;

                    //turn all the sections allowed and update to ON!! Auto changes to ON
                    for (int j = 0; j < vehicle.numOfSections; j++)
                    {
                        section[j].isAllowedOn = true;
                        section[j].manBtnState = manBtn.Off;
                    }

                    ManualAllBtnsUpdate();
                    break;

                case btnStates.Auto:
                    autoBtnState = btnStates.Off;

                    btnSectionOffAutoOn.Image = Properties.Resources.SectionMasterOff;

                    //turn section buttons all OFF or Auto if SectionAuto was on or off
                    for (int j = 0; j < vehicle.numOfSections; j++)
                    {
                        section[j].isAllowedOn = false;
                        section[j].manBtnState = manBtn.On;
                    }

                    //Update the button colors and text
                    ManualAllBtnsUpdate();
                    break;
            }
        }

        //individual buttons for sections
        private void btnSection1Man_Click(object sender, EventArgs e)
        {
            if (autoBtnState != btnStates.Auto)
            {
                //if auto is off just have on-off for choices of section buttons
                if (section[0].manBtnState == manBtn.Off) section[0].manBtnState = manBtn.Auto;
                ManualBtnUpdate(0, btnSection1Man);
                return;
            }

            ManualBtnUpdate(0, btnSection1Man);
        }
        private void btnSection2Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[1].manBtnState == manBtn.Off) section[1].manBtnState = manBtn.Auto;
                ManualBtnUpdate(1, btnSection2Man);
                return;
            }

            ManualBtnUpdate(1, btnSection2Man);
        }
        private void btnSection3Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[2].manBtnState == manBtn.Off) section[2].manBtnState = manBtn.Auto;
                ManualBtnUpdate(2, btnSection3Man);
                return;
            }

            ManualBtnUpdate(2, btnSection3Man);
        }
        private void btnSection4Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[3].manBtnState == manBtn.Off) section[3].manBtnState = manBtn.Auto;
                ManualBtnUpdate(3, btnSection4Man);
                return;
            }
            ManualBtnUpdate(3, btnSection4Man);
        }
        private void btnSection5Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[4].manBtnState == manBtn.Off) section[4].manBtnState = manBtn.Auto;
                ManualBtnUpdate(4, btnSection5Man);
                return;
            }

            ManualBtnUpdate(4, btnSection5Man);
        }
        private void btnSection6Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[5].manBtnState == manBtn.Off) section[5].manBtnState = manBtn.Auto;
                ManualBtnUpdate(5, btnSection6Man);
                return;
            }

            ManualBtnUpdate(5, btnSection6Man);
        }
        private void btnSection7Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[6].manBtnState == manBtn.Off) section[6].manBtnState = manBtn.Auto;
                ManualBtnUpdate(6, btnSection7Man);
                return;
            }

            ManualBtnUpdate(6, btnSection7Man);
        }
        private void btnSection8Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[7].manBtnState == manBtn.Off) section[7].manBtnState = manBtn.Auto;
                ManualBtnUpdate(7, btnSection8Man);
                return;
            }

            ManualBtnUpdate(7, btnSection8Man);
        }
        private void btnSection9Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[8].manBtnState == manBtn.Off) section[8].manBtnState = manBtn.Auto;
                ManualBtnUpdate(8, btnSection9Man);
                return;
            }

            ManualBtnUpdate(8, btnSection9Man);

        }
        private void btnSection10Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[9].manBtnState == manBtn.Off) section[9].manBtnState = manBtn.Auto;
                ManualBtnUpdate(9, btnSection10Man);
                return;
            }

            ManualBtnUpdate(9, btnSection10Man);

        }
        private void btnSection11Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[10].manBtnState == manBtn.Off) section[10].manBtnState = manBtn.Auto;
                ManualBtnUpdate(10, btnSection11Man);
                return;
            }

            ManualBtnUpdate(10, btnSection11Man);

        }
        private void btnSection12Man_Click(object sender, EventArgs e)
        {
            //if auto is off just have on-off for choices of section buttons
            if (autoBtnState != btnStates.Auto)
            {
                if (section[11].manBtnState == manBtn.Off) section[11].manBtnState = manBtn.Auto;
                ManualBtnUpdate(11, btnSection12Man);
                return;
            }

            ManualBtnUpdate(11, btnSection12Man);

        }

        private void btnGPSData_Click(object sender, EventArgs e)
        {
            Form form = new FormGPSData(this);
            form.Show();
        }
        private void btnFileExplorer_Click(object sender, EventArgs e)
        {
            Process.Start(fieldsDirectory + currentFieldDirectory);
        }

        private void toolStripZoomOut_Click(object sender, EventArgs e)
        {

            if (camera.zoomValue <= 20) camera.zoomValue += camera.zoomValue * 0.2;
            else camera.zoomValue += camera.zoomValue * 0.05;
            camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
            SetZoom();
        }
        private void toolStripZoomIn_Click(object sender, EventArgs e)
        {
            if (camera.zoomValue <= 20)
            { if ((camera.zoomValue -= camera.zoomValue * 0.2) < 6.0) camera.zoomValue = 6.0; }
            else { if ((camera.zoomValue -= camera.zoomValue * 0.05) < 6.0) camera.zoomValue = 6.0; }

            camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
            SetZoom();
        }

        //The zoom tilt buttons
        private void btnZoomIn_MouseDown(object sender, MouseEventArgs e)
        {
            if (camera.zoomValue <= 20) camera.zoomValue += camera.zoomValue * 0.1;
            else camera.zoomValue += camera.zoomValue * 0.05;
            if (camera.zoomValue > 220) camera.zoomValue = 220;
            camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
            SetZoom();
        }
        private void btnZoomOut_MouseDown(object sender, MouseEventArgs e)
        {
            if (camera.zoomValue <= 20)
            { if ((camera.zoomValue -= camera.zoomValue * 0.1) < 6.0) camera.zoomValue = 6.0; }
            else { if ((camera.zoomValue -= camera.zoomValue * 0.05) < 6.0) camera.zoomValue = 6.0; }
            camera.camSetDistance = camera.zoomValue * camera.zoomValue * -1;
            SetZoom();
        }
        private void btnpTiltUp_MouseDown(object sender, MouseEventArgs e)
        {
            camera.camPitch -= ((camera.camPitch * 0.02) - 1);
            if (camera.camPitch > 0) camera.camPitch = 0;
        }
        private void btnpTiltDown_MouseDown(object sender, MouseEventArgs e)
        {
            camera.camPitch += ((camera.camPitch * 0.02) - 1);
            if (camera.camPitch < -80) camera.camPitch = -80;
        }
        private void btnZoomExtents_Click(object sender, EventArgs e)
        {
            //if (isJobStarted)
            {
                if (camera.camSetDistance < -400) camera.camSetDistance = -250;
                else camera.camSetDistance = -5 * maxFieldDistance;
                if (camera.camSetDistance == 0) camera.camSetDistance = -2000;
                SetZoom();
            }
        }

        private void btnSmoothAB_Click(object sender, EventArgs e)
        {
            if (isJobStarted && curve.isCurveBtnOn)
            {
                using (var form = new FormSmoothAB(this))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK) { }
                }
            }

            else
            {
                TimedMessageBox(2000, gStr.gsFieldNotOpen, gStr.gsStartNewField);
            }
        }

        private void btnWebCam_Click(object sender, EventArgs e)
        {
            Form form = new FormWebCam();
            form.Show();
        }
        private void btnHelp_Click(object sender, EventArgs e)
        {
            Process.Start("http://ABC.gh-ortner.com/doku.php");
        }
        private void btnDeleteAllData_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                DialogResult result3 = MessageBox.Show("Delete All Contours and Sections?",
                    "Delete For sure?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result3 == DialogResult.Yes)
                {
                    FileCreateContour();
                    FileCreateSections();

                    //turn auto button off
                    autoBtnState = btnStates.Off;
                    btnSectionOffAutoOn.Image = Properties.Resources.SectionMasterOff;

                    //turn section buttons all OFF and zero square meters
                    for (int j = 0; j < MAXSECTIONS; j++)
                    {
                        section[j].isAllowedOn = false;
                        section[j].manBtnState = manBtn.On;
                    }

                    //turn manual button off
                    manualBtnState = btnStates.Off;
                    btnManualOffOn.Image = Properties.Resources.ManualOff;

                    //Update the button colors and text
                    ManualAllBtnsUpdate();

                    //enable disable manual buttons
                    LineUpManualBtns();

                    //clear the section lists
                    for (int j = 0; j < MAXSECTIONS; j++)
                    {
                        //clean out the lists
                        section[j].patchList?.Clear();
                        section[j].triangleList?.Clear();
                    }

                    //clear out the contour Lists
                    ct.StopContourLine(pivotAxlePos);
                    ct.ResetContour();
                    fd.workedAreaTotal = 0;
                }
                else TimedMessageBox(1500, "Nothing Deleted", "Action has been cancelled");
            }
        }
        // Menu Items ------------------------------------------------------------------

        //File drop down items
        private void loadVehicleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                var form = new FormTimedMessage(2000, gStr.gsFieldIsOpen, gStr.gsCloseFieldFirst);
                form.Show();
                return;
            }
            if (FileOpenVehicle())
            {
                using (var form = new FormSettings(this, 0))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK) { }
                }
                using (var form = new FormDisplaySettings(this))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK) { }
                }

                TimedMessageBox(4000, "Did you make changes to the vehicle?", "Be sure to save vehicle if you did.");
            }

        }
        private void saveVehicleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSaveVehicle();
        }
        private void fieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            JobNewOpenResume();
        }
        private void setWorkingDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                var form = new FormTimedMessage(2000, gStr.gsFieldIsOpen, gStr.gsCloseFieldFirst);
                form.Show();
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Currently: " + Settings.Default.setF_workingDirectory;

            if (Settings.Default.setF_workingDirectory == "Default") fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else fbd.SelectedPath = Settings.Default.setF_workingDirectory;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ABC",true);

                if (fbd.SelectedPath != Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                {
                    //save the user set directory in Registry
                    regKey.SetValue("Directory", fbd.SelectedPath);
                    regKey.Close();
                    Settings.Default.setF_workingDirectory = fbd.SelectedPath;
                    Settings.Default.Save();
                }
                else
                {
                    regKey.SetValue("Directory", "Default");
                    regKey.Close();
                    Settings.Default.setF_workingDirectory = "Default";
                    Settings.Default.Save();
                }

                //restart program
                MessageBox.Show(gStr.gsProgramExitAndRestart);
                Close();
            }
        }
        private void enterSimCoordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = DialogResult.Cancel;
            using (var form = new FormSimCoords(this))
            {
                result = form.ShowDialog();
            }

            if (result == DialogResult.OK)
            {
                MessageBox.Show("ABC will Exit", "Please Restart the Program");
                if (isJobStarted) JobClose();
                Application.Exit();
            }
        }

        //Help menu drop down items
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new Form_About())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK) { }
            }
        }

        //Options Drop down menu items
        private void resetALLToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                MessageBox.Show(gStr.gsCloseFieldFirst);
            }
            else
            {
                DialogResult result2 = MessageBox.Show("Really Reset Everything?", "Reset settings",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result2 == DialogResult.Yes)
                {
                    Settings.Default.Reset();
                    Settings.Default.Save();
                    Vehicle.Default.Reset();
                    Vehicle.Default.Save();
                    MessageBox.Show(gStr.gsProgramExitAndRestart);
                    Application.Exit();
                }
            }
        }
        private void logNMEAMenuItem_Click(object sender, EventArgs e)
        {
            isLogNMEA = !isLogNMEA;
            logNMEAMenuItem.Checked = isLogNMEA;
            Settings.Default.setMenu_isLogNMEA = isLogNMEA;
            Settings.Default.Save();
        }
        private void lightbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isLightbarOn = !isLightbarOn;
            lightbarToolStripMenuItem.Checked = isLightbarOn;
            Settings.Default.setMenu_isLightbarOn = isLightbarOn;
            Settings.Default.Save();
        }
        private void polygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isDrawPolygons = !isDrawPolygons;
            polygonsToolStripMenuItem.Checked = !polygonsToolStripMenuItem.Checked;
        }
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isGridOn = !isGridOn;
            gridToolStripMenuItem.Checked = isGridOn;
            Settings.Default.setMenu_isGridOn = isGridOn;
            Settings.Default.Save();
        }
        private void sideGuideLines_Click(object sender, EventArgs e)
        {
            isSideGuideLines = !isSideGuideLines;
            sideGuideLines.Checked = isSideGuideLines;
            Settings.Default.setMenu_isSideGuideLines = isSideGuideLines;
            Settings.Default.Save();
        }
        private void pursuitLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPureDisplayOn = !isPureDisplayOn;
            pursuitLineToolStripMenuItem.Checked = isPureDisplayOn;
            Settings.Default.setMenu_isPureOn = isPureDisplayOn;
            Settings.Default.Save();
        }
        private void metricToolStrip_Click(object sender, EventArgs e)
        {
            metricToolStrip.Checked = true;
            imperialToolStrip.Checked = false;
            isMetric = true;
            Settings.Default.setMenu_isMetric = isMetric;
            Settings.Default.Save();
            lblSpeedUnits.Text = "kmh";

        }
        private void skyToolStripMenu_Click(object sender, EventArgs e)
        {
            isSkyOn = !isSkyOn;
            skyToolStripMenu.Checked = isSkyOn;
            Settings.Default.setMenu_isSkyOn = isSkyOn;
            Settings.Default.Save();
        }
        private void imperialToolStrip_Click(object sender, EventArgs e)
        {
            metricToolStrip.Checked = false;
            imperialToolStrip.Checked = true;
            isMetric = false;
            Settings.Default.setMenu_isMetric = isMetric;
            Settings.Default.Save();
            lblSpeedUnits.Text = "mph";

        }
        private void simulatorOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (simulatorOnToolStripMenuItem.Checked)
            {
                panelSimControls.Visible = true;
                timerSim.Enabled = true;
            }
            else
            {
                panelSimControls.Visible = false;
                timerSim.Enabled = false;
            }

            Settings.Default.setMenu_isSimulatorOn = simulatorOnToolStripMenuItem.Checked;
            Settings.Default.Save();
            LineUpManualBtns();
        }

        //setting color off Options Menu
        private void sectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //color picker for sections
            ColorDialog colorDlg = new ColorDialog
            {
                FullOpen = true,
                AnyColor = true,
                SolidColorOnly = false,
                Color = Color.FromArgb(255, redSections, grnSections, bluSections)
            };

            if (colorDlg.ShowDialog() != DialogResult.OK) return;

            redSections = colorDlg.Color.R;
            if (redSections > 253) redSections = 253;
            grnSections = colorDlg.Color.G;
            if (grnSections > 253) grnSections = 253;
            bluSections = colorDlg.Color.B;
            if (bluSections > 253) bluSections = 253;

            Settings.Default.setF_SectionColorR = redSections;
            Settings.Default.setF_SectionColorG = grnSections;
            Settings.Default.setF_SectionColorB = bluSections;
            Settings.Default.Save();
        }
        private void fieldToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //color picker for fields

            ColorDialog colorDlg = new ColorDialog
            {
                FullOpen = true,
                AnyColor = true,
                SolidColorOnly = false,
                Color = Color.FromArgb(255, Settings.Default.setF_FieldColorR,
                Settings.Default.setF_FieldColorG, Settings.Default.setF_FieldColorB)
            };

            if (colorDlg.ShowDialog() != DialogResult.OK) return;

            redField = colorDlg.Color.R;
            if (redField > 253) redField = 253;
            grnField = colorDlg.Color.G;
            if (grnField > 253) grnField = 253;
            bluField = colorDlg.Color.B;
            if (bluField > 253) bluField = 253;

            Settings.Default.setF_FieldColorR = redField;
            Settings.Default.setF_FieldColorG = grnField;
            Settings.Default.setF_FieldColorB = bluField;
            Settings.Default.Save();
        }

        //Area button context menu items
        private void toolStripMenuAreaSide_Click(object sender, EventArgs e)
        {
            isAreaOnRight = !isAreaOnRight;
            Settings.Default.setMenu_isAreaRight = isAreaOnRight;
            Settings.Default.Save();
        }

        //taskbar buttons
        private void toolStripBtnSmoothABCurve_Click(object sender, EventArgs e)
        {
            if (isJobStarted && curve.isCurveBtnOn)
            {
                using (var form = new FormSmoothAB(this))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK) { }
                }
            }

            else
            {
                if (!isJobStarted)  TimedMessageBox(2000, gStr.gsFieldNotOpen, gStr.gsStartNewField);
                else TimedMessageBox(2000, gStr.gsCurveNotOn, gStr.gsCurveTurnOn);
            }
        }
        private void toolStripAreYouSure_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                DialogResult result3 = MessageBox.Show("Delete All Contours and Sections?",
                    "Delete For sure?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result3 == DialogResult.Yes)
                {
                    FileCreateContour();
                    FileCreateSections();

                    //turn auto button off
                    autoBtnState = btnStates.Off;
                    btnSectionOffAutoOn.Image = Properties.Resources.SectionMasterOff;

                    //turn section buttons all OFF and zero square meters
                    for (int j = 0; j < MAXSECTIONS; j++)
                    {
                        section[j].isAllowedOn = false;
                        section[j].manBtnState = manBtn.On;
                    }

                    //turn manual button off
                    manualBtnState = btnStates.Off;
                    btnManualOffOn.Image = Properties.Resources.ManualOff;

                    //Update the button colors and text
                    ManualAllBtnsUpdate();

                    //enable disable manual buttons
                    LineUpManualBtns();

                    //clear out the contour Lists
                    ct.StopContourLine(pivotAxlePos);
                    ct.ResetContour();
                    fd.workedAreaTotal = 0;

                    //clear the section lists
                    for (int j = 0; j < MAXSECTIONS; j++)
                    {
                        //clean out the lists
                        section[j].patchList?.Clear();
                        section[j].triangleList?.Clear();
                    }
                    patchSaveList?.Clear();

                }
                else TimedMessageBox(1500, "Nothing Deleted", "Action has been cancelled");
            }
        }

        private void toolStripBtnSnap_Click(object sender, EventArgs e)
        {
            if (ABLine.isABLineSet) ABLine.SnapABLine();
            else if (curve.isCurveSet) curve.SnapABCurve();
            else
            {
                var form = new FormTimedMessage(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrABLine));
                form.Show();
            }
        }
        
        private void treePlanterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fc = Application.OpenForms["FormTreePlant"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            //
            Form form = new FormTreePlant(this);
            form.Show();

        }


        private void toolstripAutoSteerConfig_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fc = Application.OpenForms["FormSteer"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            //
            Form form = new FormSteer(this);
            form.Show();
        }
        private void toolStripAutoSteerChart_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fcg = Application.OpenForms["FormSteerGraph"];

            if (fcg != null)
            {
                fcg.Focus();
                return;
            }

            //
            Form formG = new FormSteerGraph(this);
            formG.Show();
        }

        private void toolStripNTRIPConfig_Click(object sender, EventArgs e)
        {
            SettingsNTRIP();
        }
        private void deleteContourPathsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FileCreateContour();
            ct.stripList?.Clear();
            ct.ptList?.Clear();
            ct.ctList?.Clear();
            contourSaveList?.Clear();
        }


        private void toolstripVehicleConfig_Click(object sender, EventArgs e)
        {
            using (var form = new FormSettings(this, 0))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK) { }
            }
        }
        private void toolstripDisplayConfig_Click(object sender, EventArgs e)
        {
            using (var form = new FormDisplaySettings(this))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK) { }
            }
        }
        private void toolstripUSBPortsConfig_Click(object sender, EventArgs e)
        {
            SettingsCommunications();
        }
        private void toolstripUDPConfig_Click(object sender, EventArgs e)
        {
            SettingsUDP();
        }
        private void toolstripResetTrip_Click_1(object sender, EventArgs e)
        {
            fd.distanceUser = 0;
            fd.workedAreaTotalUser = 0;
        }
        private void toolstripVR_Click(object sender, EventArgs e)
        {
            if (!isJobStarted)
            {
                TimedMessageBox(1000, "No Field Open", "Please Start a Field");
                return;
            }
        }

        private void toolstripField_Click(object sender, EventArgs e)
        {
            JobNewOpenResume();
        }
        private void toolStripBtnHideTabs_Click(object sender, EventArgs e)
        {
            SwapBatmanPanels();
        }
        private void toolStripBatman_Click(object sender, EventArgs e)
        {
            SwapBatmanPanels();
        }

        //camera tool buttons
        private void CameraFollowingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            camera.camFollowing = true;
            camera.camPitch = -70;
        }
        private void CameraNorthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            camera.camFollowing = false;
        }
        private void CameraTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            camera.camFollowing = true;
            camera.camPitch = 0;
        }

        //Sim controls
        private void timerSim_Tick(object sender, EventArgs e)
        {
            //if a GPS is connected disable sim
            //if (!sp.IsOpen)
            {
                if (isAutoSteerBtnOn && (guidanceLineDistanceOff != 32000)) sim.DoSimTick(guidanceLineSteerAngle * 0.01);
                //else if (recPath.isDrivingRecordedPath) sim.DoSimTick(guidanceLineSteerAngle * 0.01);
                //else if (self.isSelfDriving) sim.DoSimTick(guidanceLineSteerAngle * 0.01);
                else sim.DoSimTick(sim.steerAngleScrollBar);
            }
        }
        private void hsbarSteerAngle_Scroll(object sender, ScrollEventArgs e)
        {
            sim.steerAngleScrollBar = (hsbarSteerAngle.Value - 300) * 0.1;
            btnResetSteerAngle.Text = sim.steerAngleScrollBar.ToString("N1");
        }
        private void hsbarStepDistance_Scroll(object sender, ScrollEventArgs e)
        {
            sim.stepDistance = ((double)(hsbarStepDistance.Value)) / 10.0 / (double)fixUpdateHz;
        }
        private void btnResetSteerAngle_Click(object sender, EventArgs e)
        {
            sim.steerAngleScrollBar = 0;
            hsbarSteerAngle.Value = 300;
            btnResetSteerAngle.Text = sim.steerAngleScrollBar.ToString("N1");
        }
        private void btnResetSim_Click(object sender, EventArgs e)
        {
            sim.latitude = Properties.Settings.Default.setGPS_Latitude;
            sim.longitude = Properties.Settings.Default.setGPS_Longitude;
        }

        #region Properties // ---------------------------------------------------------------------

        public string Zone { get { return Convert.ToString(pn.zone); } }
        public string FixNorthing { get { return Convert.ToString(Math.Round(pn.fix.northing + pn.utmNorth, 2)); } }
        public string FixEasting { get { return Convert.ToString(Math.Round(pn.fix.easting + pn.utmEast, 2)); } }
        public string Latitude { get { return Convert.ToString(Math.Round(pn.latitude, 7)); } }
        public string Longitude { get { return Convert.ToString(Math.Round(pn.longitude, 7)); } }

        public string SatsTracked { get { return Convert.ToString(pn.satellitesTracked); } }
        public string HDOP { get { return Convert.ToString(pn.hdop); } }
        public string NMEAHz { get { return Convert.ToString(fixUpdateHz); } }
        public string PassNumber { get { return Convert.ToString(ABLine.passNumber); } }
        public string Heading { get { return Convert.ToString(Math.Round(glm.toDegrees(fixHeading), 1)) + "\u00B0"; } }
        public string GPSHeading { get { return (Math.Round(glm.toDegrees(gpsHeading), 1)) + "\u00B0"; } }
        public string Status { get { if (pn.status == "A") return "Active"; else return "Void"; } }
        public string FixQuality
        {
            get
            {
                if (timerSim.Enabled)
                    return "Simulator";
                else if (pn.fixQuality == 0) return "Invalid";
                else if (pn.fixQuality == 1) return "GPS fix";
                else if (pn.fixQuality == 2) return "DGPS fix";
                else if (pn.fixQuality == 3) return "PPS fix";
                else if (pn.fixQuality == 4) return "RTK fix";
                else if (pn.fixQuality == 5) return "Flt RTK";
                else if (pn.fixQuality == 6) return "Estimate";
                else if (pn.fixQuality == 7) return "Man IP";
                else if (pn.fixQuality == 8) return "Simulator";
                else return "Unknown";
            }
        }

        public string GyroInDegrees
        {
            get
            {
                if (mc.gyroHeading != 9999)
                    return Math.Round(mc.gyroHeading * 0.0625, 1) + "\u00B0";
                else return "-";
            }
        }
        public string RollInDegrees
        {
            get
            {
                if (mc.rollRaw != 9999)
                    return Math.Round((mc.rollRaw - ahrs.rollZero) * 0.0625, 1) + "\u00B0";
                else return "-";
            }
        }
        public string PureSteerAngle { get { return ((double)(guidanceLineSteerAngle) * 0.01).ToString("N1") + "\u00B0"; } }
        public string ActualSteerAngle { get { return ((double)(actualSteerAngleDisp) * 0.01).ToString("N1") + "\u00B0"; } }

        public string FixHeading { get { return Math.Round(fixHeading, 4).ToString(); } }

        public string LookAhead { get { return ((int)(section[0].sectionLookAhead)).ToString(); } }
        public string StepFixNum { get { return (currentStepFix).ToString(); } }
        public string CurrentStepDistance { get { return Math.Round(distanceCurrentStepFix, 3).ToString(); } }
        public string TotalStepDistance { get { return Math.Round(fixStepDist, 3).ToString(); } }

        public string WorkSwitchValue { get { return mc.workSwitchValue.ToString(); } }
        public string AgeDiff { get { return pn.ageDiff.ToString(); } }

        //Metric and Imperial Properties
        public string SpeedMPH
        {
            get
            {
                double spd = 0;
                for (int c = 0; c < 10; c++) spd += avgSpeed[c];
                spd *= 0.0621371;
                return Convert.ToString(Math.Round(spd, 1));
            }
        }
        public string SpeedKPH
        {
            get
            {
                double spd = 0;
                for (int c = 0; c < 10; c++) spd += avgSpeed[c];
                spd *= 0.1;
                return Convert.ToString(Math.Round(spd, 1));
            }
        }

        public string XTE
        {
            get
            {
                //double spd = 0;
                //for (int c = 0; c < 20; c++) spd += avgXTE[c];
                //spd *= 0.1;
                //return ((int)(spd * 0.05) + " cm");
                return (crossTrackError/10 + " cm");
            }
        }
        public string InchXTE
        {
            get
            {
                //double spd = 0;
                //for (int c = 0; c < 20; c++) spd += avgXTE[c];
                //spd *= 0.1;
                //return ((int)(spd * 0.019685) + " in");
                return ((int)(crossTrackError/25.54) + " in");
            }
        }

        public string FixOffset { get { return (pn.fixOffset.easting.ToString("N2") + ", " + pn.fixOffset.northing.ToString("N2")); } }
        public string FixOffsetInch { get { return ((pn.fixOffset.easting*glm.m2in).ToString("N0")+ ", " + (pn.fixOffset.northing*glm.m2in).ToString("N0")); } }

        public string Altitude { get { return Convert.ToString(Math.Round(pn.altitude,1)); } }
        public string AltitudeFeet { get { return Convert.ToString((Math.Round((pn.altitude * 3.28084),1))); } }

        #endregion properties 

        //Timer triggers at 50 msec, 20 hz, and is THE clock of the whole program//
        private void tmrWatchdog_tick(object sender, EventArgs e)
        {
            //go see if data ready for draw and position updates
            tmrWatchdog.Enabled = false;

            //did we get a new fix position?
            if (ScanForNMEA())
            {

                if (threeSecondCounter++ >= fixUpdateHz*3)
                {
                    threeSecondCounter = 0;
                    threeSeconds++;
                }
                if (oneSecondCounter++ >= fixUpdateHz)
                {
                    oneSecondCounter = 0;
                    oneSecond++;
                }
                if (oneHalfSecondCounter++ >= fixUpdateHz/2)
                {
                    oneHalfSecondCounter = 0;
                    oneHalfSecond++;
                }
                if (oneFifthSecondCounter++ >= 0)
                {
                    oneFifthSecondCounter = 0;
                    oneFifthSecond++;
                }

                //every fifth second update  ///////////////////////////   FIFTH Fifth ////////////////////////////
                if (displayUpdateOneFifthCounter != oneFifthSecond)
                {
                    //reset the counter
                    displayUpdateOneFifthCounter = oneFifthSecond;

                    lblHeading.Text = Heading;

                    if (tabControl1.SelectedIndex == 3 && tabControl1.Visible)
                    {

                        if (guidanceLineDistanceOff == 32020 | guidanceLineDistanceOff == 32000)
                        {
                            lblSetpointSteerAngle2.Text = "Off  ";
                        }
                        else
                        {
                            lblSetpointSteerAngle2.Text = PureSteerAngle;
                        }

                        lblActualSteerAngle2.Text = ActualSteerAngle;
                        {

                            lblRoll.Text = RollInDegrees;
                            lblYawHeading.Text = GyroInDegrees;
                            lblGPSHeading.Text = GPSHeading;
                            lblHeading2.Text = lblHeading.Text;
                        }
                    }

                    if (panelBatman.Visible)
                    {
                        lblpRoll.Text = RollInDegrees;
                        lblpYawHeading.Text = GyroInDegrees;
                        lblpGPSHeading.Text = GPSHeading;
                    }
                }

                //every second update all status ///////////////////////////   1 1 1 1 1 1 ////////////////////////////
                if (displayUpdateOneSecondCounter != oneSecond)
                {
                    //reset the counter
                    displayUpdateOneSecondCounter = oneSecond;

                    //counter used for saving field in background
                    saveCounter++;

                    //count up the ntrip clock only if everything is alive
                    if (startCounter > 50 && recvCounter < 20 && isNTRIP_RequiredOn)
                    {
                        IncrementNTRIPWatchDog();
                    }
                    //Have we connection
                    if (isNTRIP_RequiredOn && !isNTRIP_Connected && !isNTRIP_Connecting)
                    {
                        if (!isNTRIP_Starting && ntripCounter > 20)
                        {
                            StartNTRIP();
                        }
                    }

                    if (isNTRIP_Connecting)
                    {
                        if (ntripCounter > 28)
                        {
                            TimedMessageBox(2000, " Socket Connection Problem ", " Not Connecting to Caster ");
                            ReconnectRequest();
                        }
                        if (clientSocket != null && clientSocket.Connected)
                        {
                            //TimedMessageBox(2000, "NTRIP Not Connected", " At the StartNTRIP() ");
                            //ReconnectRequest();
                            //return;
                            SendAuthorization();
                        }

                    }

                    if (isNTRIP_RequiredOn)
                    {
                        //update byte counter and up counter
                        if (ntripCounter > 59) lblNTRIPSeconds.Text = (ntripCounter / 60) + " Mins";
                        else if (ntripCounter < 60 && ntripCounter > 22) lblNTRIPSeconds.Text = ntripCounter + " Secs";
                        else lblNTRIPSeconds.Text = "Connecting in " + (ntripCounter - 22);

                        pbarNtrip.Value = (byte)(tripBytes * 0.02);
                        lblNtripBytes.Text = ((tripBytes) * 0.001).ToString("###,###,###") + " Kb";

                        //watchdog for Ntrip
                        if (isNTRIP_Connecting) lblWatch.Text = "Authorizing";
                        else
                        {
                            if (NTRIP_Watchdog > 10) lblWatch.Text = "Waiting";
                            else lblWatch.Text = "Listening";
                        }

                        if (sendGGAInterval > 0 && isNTRIP_Sending)
                        {
                            lblWatch.Text = "Sending GGA";
                            isNTRIP_Sending = false;
                        }
                    }

                    //the main formgps window
                    if (isMetric)  //metric or imperial
                    {
                        //Hectares on the master section soft control and sections
                        btnSectionOffAutoOn.Text = fd.WorkedHectares;
                        lblSpeed.Text = SpeedKPH;

                        //status strip values
                        stripDistance.Text = fd.DistanceUserMeters + "\r\n" + fd.WorkedUserHectares2;

                        //from rateRelay
                        //txtBoxRecvArduino.Text = mc.serialRecvRelayRateStr;
                        //txtBoxSendArduino.Text = mc.relayRateData[2] + "," + mc.relayRateData[3] + "," + mc.relayRateData[4] //relay and speed x 4
                        //    + "," + mc.relayRateData[5] + "," + mc.relayRateData[6] + "," + mc.relayRateData[7] + "," + mc.relayRateData[8] //setpoint hi lo left and right
                        //+ "," + mc.relayRateData[9];

                        btnContour.Text = XTE; //cross track error

                    }
                    else  //Imperial Measurements
                    {
                        //acres on the master section soft control and sections
                        btnSectionOffAutoOn.Text = fd.WorkedAcres;
                        lblSpeed.Text = SpeedMPH;

                        //status strip values
                        stripDistance.Text = fd.DistanceUserFeet + "\r\n" + fd.WorkedUserAcres2;
                        btnContour.Text = InchXTE; //cross track error


                        //from rateRelay
                        //txtBoxRecvArduino.Text = mc.serialRecvRelayRateStr;
                        //txtBoxSendArduino.Text = mc.relayRateData[2] + "," + mc.relayRateData[3] + "," + mc.relayRateData[4] //relay and speed x 4
                        //    + "," + mc.relayRateData[5] + "," + mc.relayRateData[6] + "," + mc.relayRateData[7] + "," + mc.relayRateData[8] //setpoint hi lo left and right
                        //+ "," + mc.relayRateData[9];
                    }



                    //not Metric/Standard units sensitive
                    lblFixQuality.Text = FixQuality;
                    lblHz.Text = NMEAHz + "Hz " + (int)(frameTime);


                }//end every second

                //every half of a second update all status  ////////////////    0.5  0.5   0.5    0.5    /////////////////
                if (displayUpdateHalfSecondCounter != oneHalfSecond)
                {
                    //reset the counter
                    displayUpdateHalfSecondCounter = oneHalfSecond;

                    if (tabControl1.SelectedIndex == 3 && tabControl1.Visible)
                    {
                        //both
                        lblLatitude.Text = Latitude;
                        lblLongitude.Text = Longitude;
                        lblLookAhead.Text = lookaheadActual.ToString("N1") + " m";

                        txtBoxRecvAutoSteer.Text = mc.serialRecvAutoSteerStr;
                        txtBoxSendAutoSteer.Text = mc.autoSteerData[mc.sdRelayLo] + ", " + mc.autoSteerData[mc.sdSpeed]
                                                + ", " + guidanceLineDistanceOff + ", " + guidanceLineSteerAngle + ", " + mc.machineControlData[mc.cnYouTurn];


                        //up in the menu a few pieces of info
                        if (isJobStarted)
                        {
                            lblEasting.Text = "E:" + Math.Round(pn.fix.easting, 1).ToString();
                            lblNorthing.Text = "N:" + Math.Round(pn.fix.northing, 1).ToString();
                        }
                        else
                        {
                            lblEasting.Text = "E:" + ((int)pn.actualEasting).ToString();
                            lblNorthing.Text = "N:" + ((int)pn.actualNorthing).ToString();
                        }

                        tboxSentence.Text = recvSentenceSettings;

                    }
                } //end every 1/2 second

                  /////////////////////////////////////////////////////////   333333333333333  ////////////////////////////////////////
                if (displayUpdateThreeSecondCounter != threeSeconds)
                {
                    //reset the counter
                    displayUpdateThreeSecondCounter = threeSeconds;

                    if (panelBatman.Visible)
                    {
                        if (isMetric)
                        {
                            lblpAltitude.Text = Altitude;
                        }
                        else //imperial
                        {
                            lblpAltitude.Text = AltitudeFeet;
                        }
                    }

                    //The tabbed is selected and the info tab
                    if (tabControl1.SelectedIndex == 3 && tabControl1.Visible)
                    {
                        if (isMetric)
                        {
                            lblAltitude.Text = Altitude;
                        }
                        else //imperial
                        {
                            lblAltitude.Text = AltitudeFeet;
                            ////Boundary
                        }

                        //both
                        lblSats.Text = SatsTracked;
                        lblZone.Text = pn.zone.ToString();
                        tboxSentence.Text = recvSentenceSettings;
                    }

                    //the main formgps window
                    if (isMetric)  //metric or imperial
                    {
                        //status strip values
                        stripEqWidth.Text = vehiclefileName + (Math.Round(vehicle.toolWidth, 2)).ToString() + " m";

                        //Hectares per hour
                        lblAreaRate.Text = fd.WorkRateHectares;
                    }
                    else  //Imperial Measurements
                    {
                        //status strip values
                        stripEqWidth.Text = vehiclefileName + (Math.Round(vehicle.toolWidth * glm.m2ft, 2)).ToString() + " ft";

                        //Acres per hour
                        lblAreaRate.Text = fd.WorkRateAcres;
                    }

                    //not Metric/Standard units sensitive
                    btnABLine.Text = PassNumber;

                    //update the online indicator
                    if (recvCounter > 50)
                    {
                        stripOnlineGPS.Value = 1;
                        lblEasting.Text = "-";
                        lblNorthing.Text = gStr.gsNoGPS;
                        lblZone.Text = "-";
                        tboxSentence.Text = gStr.gsNoSentenceData;
                    }
                    else stripOnlineGPS.Value = 100;

                }//end every 3 seconds

                swFrame.Stop();

                //stop the timer and calc how long it took to do calcs and draw
                frameTime = (double)swFrame.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1000;

            } //there was a new GPS update

            //start timer again and wait for new fix
            tmrWatchdog.Enabled = true;

        }//wait till timer fires again.  
    }//end class
}//end namespace 