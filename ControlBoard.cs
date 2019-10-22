using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace Ogrodoot_Control_Panel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] availablePorts = SerialPort.GetPortNames();
                foreach(string port in availablePorts)
                {
                    ComPortBox.Items.Add(port);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("No Communication Port was detected. Please connect and restart the application.");
            }
            
        }

        private void conBtn_Click(object sender, EventArgs e)
        {
            serial.PortName = ComPortBox.SelectedItem.ToString();
            serial.BaudRate = Convert.ToInt32(BaudBox.SelectedItem);
            serial.DataBits = 8;
            tryagain:
            try
            {
                serial.Open();
            }
            catch(Exception ex)
            {
                var response = MessageBox.Show("The COM port is busy! Try again!","Port Busy",MessageBoxButtons.RetryCancel);
                if(response == DialogResult.Retry)
                {
                    goto tryagain;
                }
                else if(response == DialogResult.Cancel)
                {
                    this.Close();
                }
            }
            conBtn.Enabled = false;
            disBtn.Enabled = true;

        }

        private void ComPortBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            conBtn.Enabled = true;
        }

        private void disBtn_Click(object sender, EventArgs e)
        {
            serial.Close();
            disBtn.Enabled = false;
            conBtn.Enabled = true;
        }


        private void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(serial.IsOpen)
            {
                string data = serial.ReadLine();
                ConsoleLog(data);
                string signal = data.Substring(0,3);
                if(signal == "a00")
                {
                    string overheatingData = data.Substring(3, 2);
                    string failureData = data.Substring(6, 2);
                    Monitoring(overheatingData, failureData);
                }
                if(signal == "m0f")
                {
                    MessageBox.Show("Front Signal was sent.");
                }
                if(signal == "m0c")
                {
                    string ctrlSignal = data.Substring(3,2);
                    if(ctrlSignal == "01")
                    {
                        string temp = data.Substring(5,2);
                        string hum = data.Substring(8, 2);
                        MonitorSurfaceBlock(temp,hum);
                    }
                    if(ctrlSignal == "02")
                    {
                        string temp = data.Substring(5,2);
                        MonitorUndergroundBlock(temp);
                    }
                    //m0c0110*23>
                }
             
            }
            
        }

        private void Monitoring(string OHD, string FHD)
        {
            BeginInvoke(new EventHandler(delegate
            {
                OHLog.AppendText(OHD + "\n");
                OHLog.ScrollToCaret();
                FailLog.AppendText(FHD + "\n");
                FailLog.ScrollToCaret();

                if (Convert.ToInt32(OHD) > 60)
                {
                    OHLog.BackColor = Color.Red;
                }
                else
                {
                    OHLog.BackColor = Color.White;
                }
            }
            ));
        }

        private void MonitorSurfaceBlock(string temp, string hum)
        {
            BeginInvoke(new EventHandler(delegate
            {
                surfaceTemp.Text = temp+"'C";
                Humidity.Text = hum+"%";
            }));
        }

        private void MonitorUndergroundBlock(string temp)
        {
            BeginInvoke(new EventHandler(delegate
            {
                ugTemp.Text = temp+"'C";
            }));
        }

        private void ConsoleLog(string data)
        {
            BeginInvoke(new EventHandler(delegate
            {
                consoleBox.AppendText(data+"\n");
            }
            ));
        }

        private void m0f_Click(object sender, EventArgs e)
        {
            if(serial.IsOpen)
            {
                serial.Write("m0f");
            }
        }

        private void m0c01xR_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Write("m0c01xR");
            }
        }

        private void m0c02xR_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Write("m0c02xR");
            }
        }
    }
}
