using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JanuityUI.Modules;
using System.Windows.Forms;
using System.IO;

namespace JanuityUI
{
    public partial class WeightCalibration : Form
    {

        private static WeightCalibration form = null;
        String weightVolt = "";
        public WeightCalibration()
        {
            InitializeComponent();            
        }

        private void WeightCalibration_Load(object sender, EventArgs e)
        {
            form = this;
        }

        public void CurrentWeightValue(String WeightVolt) {
            
            offsettx.Text = "WeightVolt";
            kioskLog.SrushtyLog_Weight("CurrentWeightValue: " + WeightVolt);
        }

        private void WClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static void StaticUI( String content)
        {
            if (form != null)
                form.ContentTextBox(content);
        }
        private void ContentTextBox(String content)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () {
                    ContentTextBox(content);
                }));
            }
            else
            {
                textBox1.Text = content;
                weightVolt = content;
            }
        }

        private void Regoffset_Click(object sender, EventArgs e)
        {
            offsettx.Text = weightVolt;
        }

        private void RegSpan_Click(object sender, EventArgs e)
        {
            spantx.Text= weightVolt;
        }

        private void Calibrate_Click(object sender, EventArgs e)
        {
            try
            {
                String offset = offsettx.Text;
                String span = spantx.Text;
                String load = loadTx.Text;
                float loadinKg = (float)(float.Parse(load) / 2.20462);
                load = loadinKg.ToString();
                String tolerance = "0.1";
                var path = @"C:\Srushty Global Solutions\config.txt";
                String[] lines = { offset, span, load, tolerance };
                File.WriteAllLines(path, lines);

                if (WeightScale.ReadCalibrationWeight())
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show("ReadCalibrationWeight");
                }
            } catch (Exception Ex) {
                MessageBox.Show((IWin32Window)Ex, "Exception");
            }

            
        }
    }
}
