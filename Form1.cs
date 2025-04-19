using EasyModbus;
using System;
using System.Drawing;
using System.Windows.Forms;
namespace 信捷modbus上位机
{
   
    public partial class Form1 : Form
    {
        int X_Max;      //X最大行程
        int Y_Max;      //Y最大行程
        int Z_Max;      //Z最大行程

        int X_Min;      //X最小行程
        int Y_Min;      //Y最小行程
        int Z_Min;      //Z最小行程


        int M1005 = 1005;   //X轴运动按键
        int M1007 = 1007;   //X轴正弦运动
        int M1008 = 1008;   //X轴正弦停止

        int M1105 = 1105;   //Y轴运动按键
        int M1107 = 1107;   //Y轴正弦运动
        int M1108 = 1108;   //Y轴正弦停止

        int M1205 = 1205;   //Z轴运动按键
        int M1207 = 1207;   //Z轴正弦运动
        int M1208 = 1208;   //Z轴正弦停止

        int M1307 = 1307;   //R轴正弦运动
        int M1308 = 1308;   //R轴正弦停止


        int HD104 = 41192;  //X轴位置点
        int HD204 = 41292;  //Y轴位置点
        int HD304 = 41392;  //Z轴位置点

        int D10 = 10;    //目前X轴位置
        int D12 = 12;    //目前Y轴位置
        int D14 = 14;    //目前Z轴位置

        int HD102 = 41190;  //X轴自动速度
        int HD202 = 41290;  //Y轴自动速度
        int HD302 = 41390;  //Z轴自动速度

        int HD110 = 41198;  //Ax
        int HD112 = 41200;  //Fx
        int HD114 = 41202;  //Ox
        int HD210 = 41298;  //Ay
        int HD212 = 41300;  //Fy
        int HD214 = 41302;  //Oy
        int HD310 = 41398;  //Az
        int HD312 = 41400;  //Fz
        int HD314 = 41402;  //Oz
        int HD410 = 41498;  //Ar
        int HD412 = 41500;  //Fr
        int HD414 = 41598;  //Or

        int X_Location_Start = 0, X_Location_Enhance = 0, X_location_End = 0;
        int Y_Location_Start = 0, Y_Location_Enhance = 0, Y_location_End = 0;
        int Z_Location_Start = 0, Z_Location_Enhance = 0, Z_location_End = 0;


        int Ax_Start = 0, Ax_End = 0, Ax_Enhance = 0;
        int Ay_Start = 0, Ay_End = 0, Ay_Enhance = 0;
        int Az_Start = 0, Az_End = 0, Az_Enhance = 0;
        int Ar_Start = 0, Ar_End = 0, Ar_Enhance = 0;

        float Fx_Start = 0, Fx_End = 0, Fx_Enhance = 0;
        float Fy_Start = 0, Fy_End = 0, Fy_Enhance = 0;
        float Fz_Start = 0, Fz_End = 0, Fz_Enhance = 0;
        float Fr_Start = 0, Fr_End = 0, Fr_Enhance = 0;

        int Ox_Start = 0, Ox_End = 0, Ox_Enhance = 0;
        int Oy_Start = 0, Oy_End = 0, Oy_Enhance = 0;
        int Oz_Start = 0, Oz_End = 0, Oz_Enhance = 0;
        int Or_Start = 0, Or_End = 0, Or_Enhance = 0;

        int Run_Time=0;
        int Rest_Time = 0;

        bool 数据写入标志位 = false;
        bool TCP_Connect_OK = false;
        bool Auto_Run_Flag = false;
        bool Location_Flag = false;
        bool Parameter_Flag = false;

        private ModbusClient TCP;
        public Form1()
        {
            InitializeComponent();

        }
        private float IntsToFloat(int[] nums)
        {
            byte[] bytes = new byte[4];
            // 小端序
            bytes[0] = (byte)(nums[0] & 0xFF);
            bytes[1] = (byte)(nums[0] >> 8);
            bytes[2] = (byte)(nums[1] & 0xFF);
            bytes[3] = (byte)(nums[1] >> 8);
            return BitConverter.ToSingle(bytes, 0);
        }

        private int[] FloatToInts(float num)
        {
            int[] nums = new int[2];
            byte[] bytes = BitConverter.GetBytes(num);
            nums[0] = BitConverter.ToInt16(bytes, 0); // 低位部分
            nums[1] = BitConverter.ToInt16(bytes, 2);
            return nums;
        }

        void TCP_Write_Real(int Address,float Data)
        {
            TCP.WriteMultipleRegisters(Address, FloatToInts(Data));
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        int TCP_Read_Register(int Address)
        {
            int Read_value = 0;
            if (TCP.Connected)
            {
                int Real_holdingRegisters;
                int[] holdingRegisters = TCP.ReadHoldingRegisters(Address, 1);
                if (holdingRegisters[0] < 0)
                {
                    Real_holdingRegisters = 2 * 32768 + holdingRegisters[0];
                    Read_value = Real_holdingRegisters;
                }
                else
                {
                    Read_value = holdingRegisters[0];
                }
            }
            return Read_value;
        }

        bool TCP_Read_Coils(int Address)
        {
            bool[] dat = { false, true };

            if (TCP.Connected)
            {
                dat = TCP.ReadCoils(Address, 1);
            }
            return dat[0];
        }

        private void 自动运动_Click(object sender, EventArgs e)
        {
            if(X方向自增_checkBox.Checked|Y方向自增_checkBox.Checked| Z方向自增_checkBox.Checked)
            {
                Run_Time = int.Parse(运动时间.Text);
                Rest_Time = int.Parse(间歇时间.Text);
                Auto_Run_Flag = true;
                Location_Flag=true;
                //MessageBoxHelper.ShowAutoClose("固定值已经写入", "操作完成", 1000);
                if (X方向自增_checkBox.Checked)
                {

                }
                else if(Y方向自增_checkBox.Checked)
                {

                }
                else if(Z方向自增_checkBox.Checked)
                {

                }
            }
        }

        private void 停止运动_Click(object sender, EventArgs e)
        {
            Auto_Run_Flag=false;
        }

        void TCP_Write(int Address, int Data)
        {
            int Real_Write_holdingRegisters;
            if (TCP.Connected)
            {

                if (Data > 60000 | Data < 0)
                {
                    MessageBox.Show("数据有误！");
                    return;
                }
                if (Data >= 32768)
                {
                    Real_Write_holdingRegisters = -2 * 32768 + Data;
                    TCP.WriteSingleRegister(Address, Real_Write_holdingRegisters);
                }
                else
                {
                    TCP.WriteSingleRegister(Address, Data);

                }
            }


        }

        void Sin_Auto_Run(bool X_Flag, bool Y_Flag, bool Z_Flag, bool R_Flag)
        {
            TCP.WriteSingleCoil(M1007, X_Flag);
            TCP.WriteSingleCoil(M1107, Y_Flag);
            TCP.WriteSingleCoil(M1207, Z_Flag);
            TCP.WriteSingleCoil(M1307, R_Flag);
        }

        void Sin_Auto_Stop(bool X_Flag, bool Y_Flag, bool Z_Flag, bool R_Flag)
        {
            TCP.WriteSingleCoil(M1008, !X_Flag);
            TCP.WriteSingleCoil(M1108, !Y_Flag);
            TCP.WriteSingleCoil(M1208, !Z_Flag);
            TCP.WriteSingleCoil(M1308, !R_Flag);
        }

        private void 位置增量值写入_Click(object sender, EventArgs e)
        {
            X_Location_Start = int.Parse(X_Start_textBox.Text);
            Y_Location_Start = int.Parse(Y_Start_textBox.Text);
            Z_Location_Start = int.Parse(Z_Start_textBox.Text);

            X_Location_Enhance = int.Parse(X_Enhance_textBox.Text);
            Y_Location_Enhance = int.Parse(Y_Enhance_textBox.Text);
            Z_Location_Enhance = int.Parse(Z_Enhance_textBox.Text);

            X_location_End = int.Parse(X_End_textBox.Text);
            Y_location_End = int.Parse(Y_End_textBox.Text);
            Z_location_End = int.Parse(Z_End_textBox.Text);
        }


        void Data_Time_Show()
        {
            //位置显示
            X_textBox.Text = (TCP_Read_Register(D10) * 1.0 / 100).ToString();
            Y_textBox.Text = (TCP_Read_Register(D12) * 1.0 / 100).ToString();
            Z_textBox.Text = (TCP_Read_Register(D14) * 1.0 / 100).ToString();


            //正弦数据显示
            Ax_Time_textBox.Text = (TCP_Read_Register(HD110) * 1.0 / 100).ToString();
            Fx_Time_textBox.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD112, 2)).ToString();
            Ox_Time_textBox.Text = (TCP_Read_Register(HD114) * 1.0 / 100).ToString();

            Ay_Time_textBox.Text = (TCP_Read_Register(HD210) * 1.0 / 100).ToString();
            Fy_Time_textBox.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD212, 2)).ToString();
            Oy_Time_textBox.Text = (TCP_Read_Register(HD214) * 1.0 / 100).ToString();

            Az_Time_textBox.Text = (TCP_Read_Register(HD310) * 1.0 / 100).ToString();
            Fz_Time_textBox.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD312, 2)).ToString();
            Oz_Time_textBox.Text = (TCP_Read_Register(HD314) * 1.0 / 100).ToString();

            Ar_Time_textBox.Text = (TCP_Read_Register(HD410) * 1.0 / 100).ToString();
            Fr_Time_textBox.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD412, 2)).ToString();
            Or_Time_textBox.Text = (TCP_Read_Register(HD414) * 1.0 / 100).ToString();


        }

        void Data_One_Show()
        {
            //位置显示
            X_GO_textBox.Text = (TCP_Read_Register(D10) * 1.0 / 100).ToString();
            Y_GO_textBox.Text = (TCP_Read_Register(D12) * 1.0 / 100).ToString();
            Z_GO_textBox.Text = (TCP_Read_Register(D14) * 1.0 / 100).ToString();

            //正弦固定数据显示
            Ax写入值.Text = (TCP_Read_Register(HD110) * 1.0 / 100).ToString();
            Fx写入值.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD112, 2)).ToString();
            Ox写入值.Text = (TCP_Read_Register(HD114) * 1.0 / 100).ToString();

            Ay写入值.Text = (TCP_Read_Register(HD210) * 1.0 / 100).ToString();
            Fy写入值.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD212, 2)).ToString();
            Oy写入值.Text = (TCP_Read_Register(HD214) * 1.0 / 100).ToString();

            Az写入值.Text = (TCP_Read_Register(HD310) * 1.0 / 100).ToString();
            Fz写入值.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD312, 2)).ToString();
            Oz写入值.Text = (TCP_Read_Register(HD314) * 1.0 / 100).ToString();

            Ar写入值.Text = (TCP_Read_Register(HD410) * 1.0 / 100).ToString();
            Fr写入值.Text = IntsToFloat(TCP.ReadHoldingRegisters(HD412, 2)).ToString();
            Or写入值.Text = (TCP_Read_Register(HD414) * 1.0 / 100).ToString();
            //正弦增量参数显示
            Ax_Start_textBox.Text = Ax写入值.Text;
            Ay_Start_textBox.Text = Ay写入值.Text;
            Az_Start_textBox.Text = Az写入值.Text;
            Ar_Start_textBox.Text = Ar写入值.Text;

            Fx_Start_textBox.Text = Fx写入值.Text;
            Fy_Start_textBox.Text = Fy写入值.Text;
            Fz_Start_textBox.Text = Fz写入值.Text;
            Fr_Start_textBox.Text = Fr写入值.Text;


            Ox_Start_textBox.Text = Ox写入值.Text;
            Oy_Start_textBox.Text = Oy写入值.Text;
            Oz_Start_textBox.Text = Oz写入值.Text;
            Or_Start_textBox.Text = Or写入值.Text;

            Ax_End_textBox.Text = Ax写入值.Text;
            Ay_End_textBox.Text = Ay写入值.Text;
            Az_End_textBox.Text = Az写入值.Text;
            Ar_End_textBox.Text = Ar写入值.Text;

            Fx_End_textBox.Text = Fx写入值.Text;
            Fy_End_textBox.Text = Fy写入值.Text;
            Fz_End_textBox.Text = Fz写入值.Text;
            Fr_End_textBox.Text = Fr写入值.Text;


            Ox_End_textBox.Text = Ox写入值.Text;
            Oy_End_textBox.Text = Oy写入值.Text;
            Oz_End_textBox.Text = Oz写入值.Text;
            Or_End_textBox.Text = Or写入值.Text;

            Ax_Enhance_textBox.Text = Ax_Enhance.ToString();
            Ay_Enhance_textBox.Text = Ay_Enhance.ToString();
            Az_Enhance_textBox.Text = Az_Enhance.ToString();
            Ar_Enhance_textBox.Text = Ar_Enhance.ToString();

            Fx_Enhance_textBox.Text = Fx_Enhance.ToString();
            Fy_Enhance_textBox.Text = Fy_Enhance.ToString();
            Fz_Enhance_textBox.Text = Fz_Enhance.ToString();
            Fr_Enhance_textBox.Text = Fr_Enhance.ToString();

            Ox_Enhance_textBox.Text = Ox_Enhance.ToString();
            Oy_Enhance_textBox.Text = Oy_Enhance.ToString();
            Oz_Enhance_textBox.Text = Oz_Enhance.ToString();
            Or_Enhance_textBox.Text = Or_Enhance.ToString();

            //速度显示
            X_Speed_textBox.Text = (TCP_Read_Register(HD102) * 1.0 / 100).ToString();
            Y_Speed_textBox.Text = (TCP_Read_Register(HD202) * 1.0 / 100).ToString();
            Z_Speed_textBox.Text = (TCP_Read_Register(HD302) * 1.0 / 100).ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void XYZ_button_Click(object sender, EventArgs e)
        {
            bool X_Flag = false, Y_Flag = false, Z_Flag = false;
            if (X_textBox.Text != X_GO_textBox.Text)            //判断X点是否已在指定位置
            {
                TCP.WriteSingleCoil(M1005, true);
                X_Flag = true;
            }
            if (Y_textBox.Text != Y_GO_textBox.Text)            //判断Y点是否已在指定位置
            {
                TCP.WriteSingleCoil(M1105, true);
                Y_Flag = true;
            }
            if (Z_textBox.Text != Z_GO_textBox.Text)            //判断Z点是否已在指定位置
            {
                TCP.WriteSingleCoil(M1205, true);
                Z_Flag = true;
            }
            if (X_Flag | Y_Flag | Z_Flag)
            {
                MessageBoxHelper.ShowAutoClose("设备正在前往指定位置", "操作完成", 1000);
                X_Flag = Y_Flag = Z_Flag = false;
            }
            else
            {
                MessageBoxHelper.ShowAutoClose("设备已经处于目标位置", "操作失败", 1000);
            }
        }       //定点运动按键按下  ->  指定位置运动执行函数

        private void Data_Write_button_Click(object sender, EventArgs e)        
        {
            数据写入标志位 = true;
            if (TCP.Connected)
            {
                //位置写入
                TCP_Write(HD104, (int)Math.Round(double.Parse(X_GO_textBox.Text) * 100));
                TCP_Write(HD204, (int)Math.Round(double.Parse(Y_GO_textBox.Text) * 100));
                TCP_Write(HD304, (int)Math.Round(double.Parse(Z_GO_textBox.Text) * 100));

                //速度写入
                TCP_Write(HD102, (int)Math.Round(double.Parse(X_Speed_textBox.Text) * 100));
                TCP_Write(HD202, (int)Math.Round(double.Parse(Y_Speed_textBox.Text) * 100));
                TCP_Write(HD302, (int)Math.Round(double.Parse(Z_Speed_textBox.Text) * 100));

            }
        }   //数据写入按键按下  ->  指定位置和运动速度写入参数


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            try
            {
                TCP = new ModbusClient(IP_textBox.Text, int.Parse(Port_textBox.Text));
                TCP.Connect();
                if (TCP.Connected)
                {
                    MessageBoxHelper.ShowAutoClose("设备连接成功", "操作完成", 1000);
                    Connect_button.BackColor = Color.Green;
                    TCP_Connect_OK = true;
                    Data_One_Show();
                }
            }
            catch (EasyModbus.Exceptions.ConnectionException ex)
            {
                MessageBoxHelper.ShowAutoClose($"连接失败: {ex.Message}", "错误");
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowAutoClose($"连接失败: {ex.Message}", "错误");
            }
        }           //连接按键点下，开始连接函数     ->没有对连接失败做处理

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void Timer_1s_Tick(object sender, EventArgs e)
        {
            bool X_Data_Write_OK = false;
            bool Y_Data_Write_OK = false;
            bool Z_Data_Write_OK = false;
            if (TCP_Connect_OK)
            {
                Data_Time_Show();    //数据实时刷新

                if (数据写入标志位)        //检测现在位置是否在指定位置
                {
                    if (TCP_Read_Register(HD104) == (int)Math.Round(double.Parse(X_GO_textBox.Text) * 100))
                    {
                        X_Data_Write_OK = true;
                    }
                    if (TCP_Read_Register(HD204) == (int)Math.Round(double.Parse(Y_GO_textBox.Text) * 100))
                    {
                        Y_Data_Write_OK = true;
                    }
                    if (TCP_Read_Register(HD304) == (int)Math.Round(double.Parse(Z_GO_textBox.Text) * 100))
                    {
                        Z_Data_Write_OK = true;
                    }
                    if ((X_Data_Write_OK) && (Y_Data_Write_OK) && (Z_Data_Write_OK))
                    {
                        数据写入标志位 = false;
                        X_Data_Write_OK = false;
                        Y_Data_Write_OK = false;
                        Z_Data_Write_OK = false;
                        MessageBoxHelper.ShowAutoClose("数据写入成功", "操作完成", 1000);
                    }

                }

            }
        }                   //秒级定时器处理函数

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void 固定值写入_Click(object sender, EventArgs e)
        {
            //X
            TCP_Write(HD110, (int)Math.Round(double.Parse(Ax写入值.Text) * 100));
            TCP_Write_Real(HD112,float.Parse(Fx写入值.Text));
            TCP_Write(HD114, (int)Math.Round(double.Parse(Ox写入值.Text) * 100));

            //Y
            TCP_Write(HD210, (int)Math.Round(double.Parse(Ay写入值.Text) * 100));
            TCP_Write_Real(HD212, float.Parse(Fy写入值.Text));
            TCP_Write(HD214, (int)Math.Round(double.Parse(Oy写入值.Text) * 100));

            //Z
            TCP_Write(HD310, (int)Math.Round(double.Parse(Az写入值.Text) * 100));
            TCP_Write_Real(HD312, float.Parse(Fz写入值.Text));
            TCP_Write(HD314, (int)Math.Round(double.Parse(Oz写入值.Text) * 100));

            //R
            TCP_Write(HD410, (int)Math.Round(double.Parse(Ar写入值.Text) * 100));
            TCP_Write_Real(HD412, float.Parse(Fr写入值.Text));
            TCP_Write(HD414, (int)Math.Round(double.Parse(Or写入值.Text) * 100));
            MessageBoxHelper.ShowAutoClose("固定值已经写入", "操作完成", 1000);
        }

        private void 增量值写入_Click(object sender, EventArgs e)
        {
            Ax_Start = (int)Math.Round(double.Parse(Ax_Start_textBox.Text) * 100);
            Ay_Start = (int)Math.Round(double.Parse(Ay_Start_textBox.Text) * 100);
            Az_Start = (int)Math.Round(double.Parse(Az_Start_textBox.Text) * 100);
            Ar_Start = (int)Math.Round(double.Parse(Ar_Start_textBox.Text) * 100);

            Fx_Start = float.Parse(Fx_Start_textBox.Text);
            Fy_Start = float.Parse(Fy_Start_textBox.Text);
            Fz_Start = float.Parse(Fz_Start_textBox.Text);
            Fr_Start = float.Parse(Fr_Start_textBox.Text);

            Ox_Start = (int)Math.Round(double.Parse(Ox_Start_textBox.Text) * 100);
            Oy_Start = (int)Math.Round(double.Parse(Oy_Start_textBox.Text) * 100);
            Oz_Start = (int)Math.Round(double.Parse(Oz_Start_textBox.Text) * 100);
            Or_Start = (int)Math.Round(double.Parse(Or_Start_textBox.Text) * 100);

            Ax_End = (int)Math.Round(double.Parse(Ax_End_textBox.Text) * 100);
            Ay_End = (int)Math.Round(double.Parse(Ay_End_textBox.Text) * 100);
            Az_End = (int)Math.Round(double.Parse(Az_End_textBox.Text) * 100);
            Ar_End = (int)Math.Round(double.Parse(Ar_End_textBox.Text) * 100);

            Fx_End = float.Parse(Fx_End_textBox.Text);
            Fy_End = float.Parse(Fy_End_textBox.Text);
            Fz_End = float.Parse(Fz_End_textBox.Text);
            Fr_End = float.Parse(Fr_End_textBox.Text);

            Ox_End = (int)Math.Round(double.Parse(Ox_End_textBox.Text) * 100);
            Oy_End = (int)Math.Round(double.Parse(Oy_End_textBox.Text) * 100);
            Oz_End = (int)Math.Round(double.Parse(Oz_End_textBox.Text) * 100);
            Or_End = (int)Math.Round(double.Parse(Or_End_textBox.Text) * 100);

            Ax_Enhance = (int)Math.Round(double.Parse(Ax_Enhance_textBox.Text) * 100);
            Ay_Enhance = (int)Math.Round(double.Parse(Ay_Enhance_textBox.Text) * 100);
            Az_Enhance = (int)Math.Round(double.Parse(Az_Enhance_textBox.Text) * 100);
            Ar_Enhance = (int)Math.Round(double.Parse(Ar_Enhance_textBox.Text) * 100);

            Fx_Enhance = (int)Math.Round(double.Parse(Fx_Enhance_textBox.Text) * 100);
            Fy_Enhance = (int)Math.Round(double.Parse(Fy_Enhance_textBox.Text) * 100);
            Fz_Enhance = (int)Math.Round(double.Parse(Fz_Enhance_textBox.Text) * 100);
            Fr_Enhance = (int)Math.Round(double.Parse(Fr_Enhance_textBox.Text) * 100);

            Ox_Enhance = (int)Math.Round(double.Parse(Ox_Enhance_textBox.Text) * 100);
            Oy_Enhance = (int)Math.Round(double.Parse(Oy_Enhance_textBox.Text) * 100);
            Oz_Enhance = (int)Math.Round(double.Parse(Oz_Enhance_textBox.Text) * 100);
            Or_Enhance = (int)Math.Round(double.Parse(Or_Enhance_textBox.Text) * 100);

        }



        private void X正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(true, false, false, false);
            MessageBoxHelper.ShowAutoClose("正在X方向正弦运动", "操作完成", 500);
        }

        private void X正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(true, false, false, false);
            MessageBoxHelper.ShowAutoClose("X方向正弦运动已停止", "操作完成", 500);
        }

        private void Y正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(false, true, false, false);
            MessageBoxHelper.ShowAutoClose("正在Y方向正弦运动", "操作完成", 500);
        }

        private void Y正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(false, true, false, false);
            MessageBoxHelper.ShowAutoClose("Y方向正弦运动已停止", "操作完成", 500);
        }

        private void Z正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(false, false, true, false);
            MessageBoxHelper.ShowAutoClose("正在Z方向正弦运动", "操作完成", 500);
        }

        private void Z正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(false, false, true, false);
            MessageBoxHelper.ShowAutoClose("Z方向正弦运动已停止", "操作完成", 500);
        }

        private void R正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(false, false, false, true);
            MessageBoxHelper.ShowAutoClose("正在R方向正弦运动", "操作完成", 500);
        }

        private void R正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(false, false, false, true);
            MessageBoxHelper.ShowAutoClose("R方向正弦运动已停止", "操作完成", 500);
        }

        private void Timer_ms_Tick(object sender, EventArgs e)
        {
            int count = 0;
            int X_Location_Old_Value = X_Location_Start;
            int Y_Location_Old_Value = Y_Location_Start;
            int Z_Location_Old_Value = Z_Location_Start;

            if (Auto_Run_Flag == true)
            {
                if (Location_Flag && Parameter_Flag)
                {
                    Auto_Run_Flag = false;
                    MessageBoxHelper.ShowAutoClose("位置和正弦运动只能有一个的参数可以增量", "操作失败", 1000);
                } 
                else if (Location_Flag)
                {
                    count++;
                    if (count == Run_Time)
                    {
                       
                    }
                    if (count == Run_Time + Rest_Time)
                    {
                        count = 0;
                        if (X方向自增_checkBox.Checked)
                        {
                            if(X_Location_Old_Value>= Ax_End)
                            {
                                X方向自增_checkBox.Enabled = false;
                            }
                            X_Location_Old_Value = int.Parse(X_Start_textBox.Text) + int.Parse(X_Enhance_textBox.Text);
                        }
                        if (Y方向自增_checkBox.Checked)
                        {
                            if (Y_Location_Old_Value >= Ax_End)
                            {
                                X方向自增_checkBox.Enabled = false;
                            }
                            Y_Location_Old_Value = int.Parse(X_Start_textBox.Text) + int.Parse(X_Enhance_textBox.Text);

                        }
                        if (Z方向自增_checkBox.Checked)
                        {
                            Z_Location_Old_Value = int.Parse(X_Start_textBox.Text) + int.Parse(X_Enhance_textBox.Text);
                        }
                    }
                    //if ()
                    //{
                    //    Auto_Run_Flag = false;
                    //}
                }
                else if (Parameter_Flag)
                {

                }
            }

        }
    }
    public static class MessageBoxHelper
    {
        public static void ShowAutoClose(string message, string title = "提示", int timeoutMs = 1500)
        {
            var form = new Form
            {
                Width = 350,
                Height = 120,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false,
                TopMost = true
            };

            var label = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("微软雅黑", 10)
            };
            form.Controls.Add(label);

            var timer = new Timer { Interval = timeoutMs };
            timer.Tick += (s, e) => {
                timer.Stop();
                form.Close();
                form.Dispose();
            };

            form.Shown += (s, e) => timer.Start();
            form.Show();
        }
    }               //自动关闭的消息提示
}
//第一种运动情况:在指定点以增量振幅运动
//第二种运动情况：振幅不变，以指定增量改变指定点