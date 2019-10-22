using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO.Ports;
using System.Configuration;
using System.Security.Principal;

namespace HardwareMonitor
{
    public partial class fmrMain : Form
    {
        //Khai báo biến
        ManagementScope scope = new ManagementScope(@"\\.\root\OpenHardwareMonitor");
        ObjectQuery query;
        ManagementObjectSearcher moSearch;
        ManagementObjectCollection moCollection;
        string a;


        private static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public fmrMain()
        {
            InitializeComponent();
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            if (IsRunAsAdmin()) Console.WriteLine("admin");
            scope.Connect();
            serialPort1.BaudRate = 9600;
        }

        public string _getValue(string Id)
        {
            string str = "";
            string strQuery = "SELECT Value FROM Sensor WHERE InstanceId = " + Id;
            query = new ObjectQuery(strQuery);
            moSearch = new ManagementObjectSearcher(scope, query);
            moCollection = moSearch.Get();
            foreach (ManagementObject mo in moCollection)
            {
                str = mo["Value"].ToString();
                break;
            }
            return str;
        }
        public string getIntValue(string Id, int length)
        {
            int res = (int)float.Parse(_getValue(Id));
            string result = res.ToString();
            while (result.Length < length)
            {
                result = " " + result;
            }
            return result;
        }
        public string getFloatValue(string Id, int length)
        {
            float res = float.Parse(_getValue(Id));
            string result = "";
            result = res.ToString("0.0");

            while (result.Length < length)
            {
                result = " " + result;
            }
            return result;
        }
        public void fillText()
        {

            //CPU TEMP
            a = _getValue("3848") + ",";

            //CPU Load
            a += getIntValue("3843",3) + ",";

            //CPU Clock
            a += getIntValue("3849", 3) + ",";

            //ram Load         
            a += getIntValue("3859",3) + ",";

            //Used ram
            a += getFloatValue("3860",3) + ",";

            //GPU Clock        
            a += getIntValue("3865",4) + ",";

            //GPU Mem Clock
            a += getIntValue("3866",4) + ",";

            //GPU load
            a += getIntValue("3868",3) + ",";

            //GPU Ram Used
            float ram = float.Parse(_getValue("3873")) / 1024;
            a += ram.ToString("0.0") + ",";

            //GPU Ram total
            ram =float.Parse(_getValue("3872"))/1024;
            a += ram.ToString("0") + ",";

            //GPU Ram load
            a += getIntValue("3875", 3) + ",";

            //GPU Temperature
            a += getIntValue("3863",2) + "*";

            //textBox1.Text = a;
            textBox1.BackColor = Color.White;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbbCom.DataSource = SerialPort.GetPortNames();
            txtTimer.Text = ConfigurationManager.AppSettings["timer"].ToString();
            timer1.Interval = (int)(double.Parse(txtTimer.Text) * 1000);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                fillText();
                sentTb.Text = a;
                serialPort1.Write(a);
            }
            catch
            {
                Error("OpenHardwareMonitor chưa được khởi động!");

            }
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = "";
                
                if (!serialPort1.IsOpen)
                {
                    serialPort1.PortName = cbbCom.Text;
                    serialPort1.Open();
                    timer1.Start();
                    btnConn.Text = "Ngắt kết nối";
                    btnConn.BackColor = Color.Blue;
                }
                else
                {
                    timer1.Stop();
                    btnConn.Text = "Kết nối";
                    btnConn.BackColor = Color.Red;
                    serialPort1.Close();
                    Error("Đang dừng");
                }
            }
            catch (Exception ex)
            {
                //Error(ex.ToString());
                Error("Đang ngắt kết nối hoặc có lỗi xảy ra!");
            }
        }
        void Error(string str)
        {
            textBox1.Text = str;
            timer1.Stop();
            btnConn.Text = "Kết nối";
            btnConn.BackColor = Color.Red;
            serialPort1.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                _config.AppSettings.Settings["timer"].Value = txtTimer.Text;
                _config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {
                MessageBox.Show("Có lỗi xảy ra, không lưu được!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbbCom_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
