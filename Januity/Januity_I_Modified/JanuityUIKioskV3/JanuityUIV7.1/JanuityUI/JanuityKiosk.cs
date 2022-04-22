using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;


using System.Net.NetworkInformation;
using SuperWebSocket;
using SuperWebSocket.PubSubProtocol;
using System.Net;
using System.Windows.Forms.DataVisualization.Charting;
using JanuityUI.Modules;
using System.Threading;

namespace JanuityUI
{
    public partial class JanuityKiosk : Form
    {

        PerformanceCounter pcsent, pcreceived;
        private PerformanceCounter theCPUCounter;
        private PerformanceCounter theMemCounter;
        IPAddress[] localIPs;
        private static JanuityKiosk form = null;
        System.Windows.Forms.DataVisualization.Charting.Series series1, series2;
        public static WebSocketSession csession;
        static Thread PortDetection_process;
        private delegate void EnableDelegate(String value, String content);
        public JanuityKiosk()
        {
            InitializeComponent();



            this.Location = new Point(0, 0);

            this.Size = Screen.PrimaryScreen.WorkingArea.Size;







            // StartPosition was set to FormStartPosition.Manual in the properties window.
            // Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            //int w = Width >= screen.Width ? screen.Width : (screen.Width + Width) / 2;
            //int h = Height >= screen.Height ? screen.Height : (screen.Height + Height) / 2;
            //this.Location = new Point((screen.Width - w) / 2, (screen.Height - h) / 2);
            //this.Size = new Size(w, h);

            int h = Screen.PrimaryScreen.WorkingArea.Height;
            int w = Screen.PrimaryScreen.WorkingArea.Width;
            this.ClientSize = new Size(w, h);


            localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            PerformanceCounterCategory pcg = new PerformanceCounterCategory("Network Interface");
            string instance = pcg.GetInstanceNames()[0];
            pcsent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            pcreceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
            try
            {
                theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                theMemCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            }
            catch (Exception) { }

            chart1.Series.Clear();
            series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series1",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line,
                AxisLabel = ""

            };

            chart1.Series.Add(series1);
            chart1.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart1.ChartAreas[0].AxisY.IsMarginVisible = false;
            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart1.ChartAreas[0].AxisY.Maximum = 100;
            chart1.ChartAreas[0].AxisY.Minimum = 0;

            chart2.Series.Clear();
            series2 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series2",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line,
                AxisLabel = ""
            };

            chart2.Series.Add(series2);
            chart2.ChartAreas[0].AxisY.Maximum = 100;
            chart2.ChartAreas[0].AxisY.Minimum = 1;

            chart2.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart2.ChartAreas[0].AxisY.IsMarginVisible = false;
            chart2.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            chart2.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chart2.ChartAreas[0].AxisY.LabelStyle.Enabled = false;


            //PortDetection.portstart();

            //use BasicSubProtocol with current assembly as command assembly
            var appServer = new JanuityWebSocketServer(this);

            //Setup the appServer
            if (!appServer.Setup(2012)) //Setup with listening port
            {
                label40.Text = "Setup Failed";
                return;
            }
            //Try to start the appServer
            if (!appServer.Start())
            {
                label40.Text = "Failed to Start";
                return;
            }
            else
            {
                label40.Text = "Started Successfully";
                //Process.Start(@"C:\Program Files\Google\Chrome\Application\chrome.exe --start - fullscreen - incognito https://januity.instapract.com/web/kiosk");
            }
            form = this;
            //Starting Port Detection Process
            PortDetection_process = new Thread(PortDetection.PortStart);
            PortDetection_process.Start();

            this.WindowState = FormWindowState.Maximized;
        }

        public static void ShowUI()
        {
            if (form != null)
                form.ShowComponents();

        }
        private void ShowComponents()
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    ShowComponents();
                }));
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.TopLevel = true;
                this.TopMost = true;
            }
        }
        // Static method, call the non-static version if the form exist.
        public static void StaticUI(String value, String content)
        {
            if (form != null)
                form.ContentTextBox(value, content);
        }
        private void ContentTextBox(String value, String content)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    ContentTextBox(value, content);
                }));
            }
            else
            {
                //MessageBox.Show(value + "");
                if (value.Contains("label24"))
                {
                    label24.Text = content;
                }
                else if (value.Contains("label25"))
                {
                    label25.Text = content;
                }
                else if (value.Contains("label26"))
                {
                    label26.Text = content;
                }
                else if (value.Contains("label27"))
                {
                    label27.Text = content;
                }
                else if (value.Contains("label28"))
                {
                    label28.Text = content;
                }
                else if (value.Contains("label29"))
                {
                    label29.Text = content;
                }
                else if (value.Contains("label30"))
                {
                    label30.Text = content;
                }
                else if (value.Contains("label33"))
                {
                    label33.Text = content;
                }
                else if (value.Contains("label34"))
                {
                    label34.Text = content;
                }
                else if (value.Contains("label35"))
                {
                    label35.Text = content;
                }
                else if (value.Contains("label36"))
                {
                    label36.Text = content;
                }
                else if (value.Contains("label37"))
                {
                    label37.Text = content;
                }
                else if (value.Contains("label38"))
                {
                    label38.Text = content;
                }
                else if (value.Contains("label39"))
                {
                    label39.Text = content;
                }
                else if (value.Contains("label40"))
                {
                    label40.Text = content;
                }
                else if (value.Contains("label45"))
                {
                    label45.Text = content;
                }
                else if (value.Contains("label46"))
                {
                    label46.Text = content;
                }
                else if (value.Contains("label48"))
                {
                    label48.Text = content;
                }
                else if (value.Contains("label50"))
                {
                    label50.Text = content;
                }
            }
        }



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Console.WriteLine("adb server exit");
                foreach (System.Diagnostics.Process myProc in System.Diagnostics.
                    Process.GetProcesses())
                {
                    if (myProc.ProcessName.Contains("adb"))
                    { myProc.Kill(); }
                    // kioskLog.SrushtyLog_Temp("Processes" + myProc);
                }
            }
            catch (Exception ex)
            {
                kioskLog.SrushtyLog_Temp("Processes Exception" + ex);
            }
            BloodPressure.Close_BP_Port();
            SpO2.Close_SpO2_Port();
            WeightScale.Close_Weight_Port();

            Process.GetCurrentProcess().Kill();
            System.Windows.Forms.Application.Exit();
        }

        private void mainMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        int XAxis = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                string cpuUsage = this.theCPUCounter.NextValue().ToString("0.00");
                string RAMUsage = this.theMemCounter.NextValue().ToString("0.00");
                this.label14.Text = "CPU: " + cpuUsage + "%";
                series1.Points.AddXY(XAxis, cpuUsage);

                if (chart1.Series[0].Points.Count >= 20)
                    chart1.Series[0].Points.RemoveAt(0);

                series2.Points.AddXY(XAxis, RAMUsage);
                if (chart2.Series[0].Points.Count >= 20)
                    chart2.Series[0].Points.RemoveAt(0);

                this.label23.Text = "RAM: " + RAMUsage + "%";
                label32.Text = "Ethernet:\n\n" + localIPs[1] + "\nSent: " + pcsent.NextValue() / 1024 + "\nReceived: " + pcreceived.NextValue() / 1024;
                XAxis++;
            }
            catch (Exception) { }
        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        public void uilabel(String content)
        {
            label33.Text = content;
        }



        public void getNetworkStatics()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                MessageBox.Show(adapter.Name + "");
            }
            // Select desired NIC
            var reads = Enumerable.Empty<double>();
            var sw = new Stopwatch();
            var lastBr = nics.Single(n => n.Name == "Ethernet 2").GetIPv4Statistics().BytesReceived;
            for (var i = 0; i < 1000; i++)
            {

                sw.Restart();
                Thread.Sleep(100);
                var elapsed = sw.Elapsed.TotalSeconds;
                var br = nics.Single(n => n.Name == "Ethernet 2").GetIPv4Statistics().BytesReceived;

                var local = (br - lastBr) / elapsed;
                lastBr = br;

                // Keep last 20, ~2 seconds
                reads = new[] { local }.Concat(reads).Take(20);

                if (i % 10 == 0)
                { // ~1 second
                    var bSec = reads.Sum() / reads.Count();
                    var kbs = (bSec * 8) / 1024;
                    MessageBox.Show("Kb/s ~ " + kbs);
                }
            }
        }



        private void calibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }


        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

        }

        private void JanuityKiosk_Load(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void label45_Click(object sender, EventArgs e)
        {

        }

        private void label46_Click(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void label36_Click(object sender, EventArgs e)
        {

        }

        private void weightCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WeightCalibration WC = new WeightCalibration();
            WC.ShowDialog();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void bloodPressureCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

      

      



        private void button6_Click(object sender, EventArgs e)
        {
            pictureBox1.Show();
            this.WindowState = FormWindowState.Maximized;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Hide();
            groupBox2.Show();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            groupBox1.Hide();
            groupBox2.Hide();
        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void chart2_Click_1(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void fileexplorer_Click(object sender, EventArgs e)
        {
            Process.Start("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "taskmgr";
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            groupBox1.Show();
            groupBox2.Show();
            pictureBox1.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Srushty Global Solutions");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            }
            catch (Exception ex)
            {

            }
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {


        }
    }
}