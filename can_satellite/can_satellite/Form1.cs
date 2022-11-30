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
using System.IO;
namespace can_satellite
{
    public partial class Form1 : Form
    {
        int a=1;
        int b = 1;
        int c = 1;
        int d = 1;
        int tempsensor = 0;
        string str;
        int xyzactivation = 0;//자이로 센서 활성화 on/off 변수
        int gpsactivation = 0;
        int dltks = 0;
        int savevoid = 0;
        public Form1()
        {
            InitializeComponent();
            //웹브라우져 자바스크립트 오류 없애기
            webBrowser1.ScriptErrorsSuppressed = true;
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
                listBox1.Items.Add("포트가 열렸습니다");
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
            char[] ReciveArray = ReceiveData.ToArray();
            //listBox1.Items.Add(ReceiveData);
            try
            {
                if (xyzactivation == 1 && ReceiveData.Length == 13 && ReciveArray[0] == 'x')
                {
                    int data_x = 0, data_y = 0, data_z = 0;
                    data_x = Convert.ToInt32(ReceiveData.Substring(1, 3));
                    data_y = Convert.ToInt32(ReceiveData.Substring(5, 3));
                    data_z = Convert.ToInt32(ReceiveData.Substring(9, 3));
                    xyzchart_drow(data_x, data_y, data_z);
                    if (savevoid == 1)
                    {
                        xyzchart_save(data_x, data_y, data_z);
                    }
                }
                if (ReciveArray[0] == 'a' && dltks == 1)
                {
                    float ppm = Convert.ToSingle(ReceiveData.Substring(1, ReceiveData.Length - 1));
                    ppm_drow(ppm);
                    if (savevoid == 1)
                    {
                        ppm_save(ppm);
                    }
                }
                if (ReciveArray[0] == 'C' && tempsensor == 1)
                {
                    temp_drow(Convert.ToInt32(ReceiveData.Substring(1, 3)));
                    
                    int hum = (ReciveArray[5] - '0') * 100 + (ReciveArray[6] - '0') * 10 + ReciveArray[7] - '0';
                    hum_drow(hum);
                    if (savevoid == 1)
                    {
                        temp_save(Convert.ToInt32(ReceiveData.Substring(1, 3)));
                        hum_save(hum);
                    }
                }
                if(gpsactivation==1 && ReciveArray[0] == 'D')
                {
                    label5.Text = ReceiveData.Substring(1, ReceiveData.Length - 1);
                }
                if (gpsactivation == 1 && ReciveArray[0] == 'L')
                {
                    label6.Text = ReceiveData.Substring(1, ReceiveData.Length - 1);
                }
                if (gpsactivation == 1 && ReciveArray[0] == 'S')
                {
                    label14.Text = ReceiveData.Substring(1, ReceiveData.Length - 1);
                }
            }
            catch (Exception n)
            {
                MessageBox.Show("error{0}",n.Message);
            }
        }
        private void temp_drow(int temp)//온도 센서 chart 그리는 함수
        {
            chart3.Series[0].Points.AddXY((double)c, temp);

            if (chart3.Series[0].Points.Count > 8)
            {
                chart3.Series[0].Points.RemoveAt(0);
            }

            chart3.ChartAreas[0].AxisX.Maximum = c;
            chart3.ChartAreas[0].AxisX.Minimum = chart3.Series[0].Points[0].XValue;
            chart3.ChartAreas[0].AxisY.Maximum = 50;
            chart3.ChartAreas[0].AxisY.Minimum = -50;
            c++;
        }

        private void xyzchart_save(int x,int y, int z)
        {   
            StreamWriter writer;
            writer = File.AppendText("./xyz.txt");
            writer.WriteLine(x.ToString()+","+ y.ToString()+","+ z.ToString()+ DateTime.Now.ToString("yymmdd"));
            writer.Close();
        }

        private void ppm_save(float a)
        {
            StreamWriter writer;
            writer = File.AppendText("./ppm.txt");
            writer.WriteLine(a.ToString() + DateTime.Now.ToString("yymmdd"));
            writer.Close();
        }
        private void hum_save(int hum)
        {
            StreamWriter writer;
            writer = File.AppendText("./hum.txt");
            writer.WriteLine(hum.ToString() + DateTime.Now.ToString("yymmdd"));
            writer.Close();
        }
        private void temp_save(int temp)
        {
            StreamWriter writer;
            writer = File.AppendText("./temp.txt");
            writer.WriteLine(temp.ToString() + DateTime.Now.ToString("yymmdd"));
            writer.Close();
        }
        private void hum_drow(int hum)//습도 센서 chart 그리는 함수
        {
            chart4.Series[0].Points.AddXY((double)d, hum);

            if (chart4.Series[0].Points.Count > 8)
            {
                chart4.Series[0].Points.RemoveAt(0);
            }

            chart4.ChartAreas[0].AxisX.Maximum = d;
            chart4.ChartAreas[0].AxisX.Minimum = chart4.Series[0].Points[0].XValue;
            chart4.ChartAreas[0].AxisY.Maximum = 100;
            chart4.ChartAreas[0].AxisY.Minimum = 0;
            d++;
        }
        private void ppm_drow(float ppm)//자이로 센서 chart 그리는 함수
        {
            chart2.Series[0].Points.AddXY((double)b, ppm);

            if (chart2.Series[0].Points.Count > 8)
            {
                chart2.Series[0].Points.RemoveAt(0);
            }

            chart2.ChartAreas[0].AxisX.Maximum = b;
            chart2.ChartAreas[0].AxisX.Minimum = chart2.Series[0].Points[0].XValue;
            chart2.ChartAreas[0].AxisY.Maximum = 3500;
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            b++;
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (xyzactivation == 0)
            {
                button1.Text = "OFF";
                xyzactivation = 1;
                listBox1.Items.Add("자이로센서 ON");
            }
            else
            {
                button1.Text = "ON";
                xyzactivation = 0;
                listBox1.Items.Add("자이로센서 OFF");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (gpsactivation == 0)
            {
                button2.Text = "OFF";
                gpsactivation = 1;
                listBox1.Items.Add("GPS ON");
            }
            else
            {
                button2.Text = "ON";
                gpsactivation = 0;
                listBox1.Items.Add("GPS OFF");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dltks == 0)
            {
                button4.Text = "OFF";
                dltks = 1;
                listBox1.Items.Add("이산화 탄소 측정 ON");
            }
            else
            {
                button4.Text = "ON";
                dltks = 0;
                listBox1.Items.Add("이산화 탄소 측정 OFF");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (tempsensor == 0)
            {
                button5.Text = "OFF";
                tempsensor = 1;
                listBox1.Items.Add("온습도 측정 ON");
            }
            else
            {
                button5.Text = "ON";
                tempsensor = 0;
                listBox1.Items.Add("온습도 측정 OFF");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //try
            //{
             //   StreamWriter writer;
              //  writer = File.CreateText("./writeTest.txt");
               // writer.WriteLine("텍스트 파일 세로 쓰기 성공");
                //writer.Close();
            //}catch(Exception m)
            //{
              //  StreamWriter writer;
               // writer = File.AppendText("./writeTest.txt");
              //  writer.WriteLine("텍스트 파일 이어 쓰기 성공");
              //  writer.Close();
            //}
            if (savevoid == 0)
            {
                button6.Text = "OFF";
                savevoid = 1;
                listBox1.Items.Add("저장 ON");
            }
            else
            {
                button6.Text = "ON";
                savevoid = 0;
                listBox1.Items.Add("저장 OFF");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();
            chart4.Series.Clear();
        }
    }
}
