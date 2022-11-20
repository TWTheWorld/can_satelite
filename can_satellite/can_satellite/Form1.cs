using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO.Ports;
namespace can_satellite
{
    public partial class Form1 : Form
    {
        int a=1;
        string str;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            portsearch.DataSource = SerialPort.GetPortNames();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            chart1.Series[0].Points.AddXY((double)a, 1);
            
            if (chart1.Series[0].Points.Count > 10)
            {
                chart1.Series[0].Points.RemoveAt(0);
            }
            chart1.ChartAreas[0].AxisX.Maximum = a;
            chart1.ChartAreas[0].AxisX.Minimum = chart1.Series[0].Points[0].XValue;
            a++;
        }

        private void Serial_connect_Click(object sender, EventArgs e)
        {

            if (!serialPort1.IsOpen)  //시리얼포트가 열려 있지 않으면
            {
                serialPort1.PortName = portsearch.Text;  //콤보박스의 선택된 COM포트명을 시리얼포트명으로 지정
                serialPort1.BaudRate = 9600;  //보레이트 변경이 필요하면 숫자 변경하기
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Parity = Parity.None;
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived); //이것이 꼭 필요하다

                serialPort1.Open();  //시리얼포트 열기

                label1.Text = "포트가 열렸습니다.";
                portsearch.Enabled = false;  //COM포트설정 콤보박스 비활성화
            }
            else  //시리얼포트가 열려 있으면
            {
                label1.Text = "포트가 이미 열려 있습니다.";
            }

        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
           
            this.Invoke(new EventHandler(MySerialReceived));
        }
        private void MySerialReceived(object s, EventArgs e)  //여기에서 수신 데이타를 사용자의 용도에 따라 처리한다.
        {
            String ReceiveData = serialPort1.ReadLine();  //시리얼 버터에 수신된 데이타를 ReceiveData 읽어오기
            
            int data_x=0, data_y=0, data_z=0;
            //try {
            if (ReceiveData.Length == 13)
            {
                data_x = Convert.ToInt32(ReceiveData.Substring(1, 3));
                data_y = Convert.ToInt32(ReceiveData.Substring(5, 3));
                data_z = Convert.ToInt32(ReceiveData.Substring(9, 3));
                xyzchart_drow(data_x, data_y, data_z);
            }            
        }
        private void xyzchart_drow(int data_x, int data_y, int data_z)//자이로 센서 chart 그리는 함수
        {
            chart1.Series[0].Points.AddXY((double)a, data_x);
            chart1.Series[1].Points.AddXY((double)a, data_y);
            chart1.Series[2].Points.AddXY((double)a, data_z);

            if (chart1.Series[0].Points.Count > 10)
            {
                chart1.Series[0].Points.RemoveAt(0);
                chart1.Series[1].Points.RemoveAt(0);
                chart1.Series[2].Points.RemoveAt(0);
            }

            chart1.ChartAreas[0].AxisX.Maximum = a;
            chart1.ChartAreas[0].AxisX.Minimum = chart1.Series[0].Points[0].XValue;
            chart1.ChartAreas[0].AxisY.Maximum = 100;
            chart1.ChartAreas[0].AxisY.Minimum = -100;
            a++;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
