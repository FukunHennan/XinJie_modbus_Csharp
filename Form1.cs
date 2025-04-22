using EasyModbus;
using System;
using System.Drawing;
using System.Windows.Forms;
namespace 信捷modbus上位机
{
   
    public partial class Form1 : Form
    {

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
        int HD414 = 41502;  //Or



        bool Check = false;
        bool TCP_Connect_OK = false;

        bool Location_Flag = false;

        bool X_Check_Flag, Y_Check_Flag, Z_Check_Flag;
        bool Ax_Check_Flag, Fx_Check_Flag, Ox_Check_Flag, Ay_Check_Flag, Fy_Check_Flag, Oy_Check_Flag, Az_Check_Flag, Fz_Check_Flag, Oz_Check_Flag, Ar_Check_Flag, Fr_Check_Flag, Or_Check_Flag;


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

        void TCP_Write_Dint(int Address, int Data)
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

        //位置指定运动函数

        void XYZ_Speed_Data_Write(string X_Location,string Y_location,string Z_location,string X_Speed,string Y_Speed,string Z_Speed) 
        {
            //速度写入
            TCP_Write_Dint(HD102, (int)Math.Round(double.Parse(X_Speed) * 100));
            TCP_Write_Dint(HD202, (int)Math.Round(double.Parse(Y_Speed) * 100));
            TCP_Write_Dint(HD302, (int)Math.Round(double.Parse(Z_Speed) * 100));

            //位置写入
            TCP_Write_Dint(HD104, (int)Math.Round(double.Parse(X_Location) * 100));
            TCP_Write_Dint(HD204, (int)Math.Round(double.Parse(Y_location) * 100));
            TCP_Write_Dint(HD304, (int)Math.Round(double.Parse(Z_location) * 100));



        }

        void XYZ_Speed_Data_Run(string X_GO, string Y_GO, string Z_GO)
        {
            if (X_textBox.Text != X_GO)            //判断X点是否已在指定位置
            {
                TCP.WriteSingleCoil(M1005, true);
            }
            if (Y_textBox.Text != Y_GO)            //判断Y点是否已在指定位置
            {
                TCP.WriteSingleCoil(M1105, true);
            }
            if (Z_textBox.Text != Z_GO)            //判断Z点是否已在指定位置
            {
                TCP.WriteSingleCoil(M1205, true);
            }
        }


        void Sin_Data_Write(string Ax,string Fx,string Ox,string Ay,string Fy,string Oy, string Az, string Fz, string Oz, string Ar, string Fr, string Or)
        {
            //X
            TCP_Write_Dint(HD110, (int)Math.Round(double.Parse(Ax) * 100));
            TCP_Write_Real(HD112, float.Parse(Fx));
            TCP_Write_Dint(HD114, (int)Math.Round(double.Parse(Ox) * 100));

            //Y
            TCP_Write_Dint(HD210, (int)Math.Round(double.Parse(Ay) * 100));
            TCP_Write_Real(HD212, float.Parse(Fy));
            TCP_Write_Dint(HD214, (int)Math.Round(double.Parse(Oy) * 100));

            //Z
            TCP_Write_Dint(HD310, (int)Math.Round(double.Parse(Az) * 100));
            TCP_Write_Real(HD312, float.Parse(Fz));
            TCP_Write_Dint(HD314, (int)Math.Round(double.Parse(Oz) * 100));

            //R
            TCP_Write_Dint(HD410, (int)Math.Round(double.Parse(Ar) * 100));
            TCP_Write_Real(HD412, float.Parse(Fr));
            TCP_Write_Dint(HD414, (int)Math.Round(double.Parse(Or) * 100));
        }


        private void 自动运动_Click(object sender, EventArgs e)
        {
           
            if (X方向自增_checkBox.Checked)
            {
                X_Check_Flag = true;
                Location_Flag = true;
                X_GO_textBox.Text = X_Start_textBox.Text;            
            }
            if (Y方向自增_checkBox.Checked)
            {
                Y_Check_Flag = true;
                Location_Flag = true;
                Y_GO_textBox.Text = Y_Start_textBox.Text;           
            }
            if (Z方向自增_checkBox.Checked)
            {
                Z_Check_Flag = true;
                Location_Flag = true;
                Z_GO_textBox.Text = Z_Start_textBox.Text;
            }

            if (Ax_checkBox.Checked) 
            {
                Ax_Check_Flag = true; 
                Ax写入值.Text = Ax_Start_textBox.Text;
              
            }
            if (Fx_checkBox.Checked)
            { 
                Fx_Check_Flag = true;
                Fx写入值.Text = Fx_Start_textBox.Text;
              
            }
            if (Ox_checkBox.Checked)
            { 
                Ox_Check_Flag = true;
                Ox写入值.Text = Ox_Start_textBox.Text;
              
            }

            if (Ay_checkBox.Checked)
            {
                Ay_Check_Flag = true;
                Ay写入值.Text = Ay_Start_textBox.Text;
            }

            if (Fy_checkBox.Checked)
            {
                Fy_Check_Flag = true;
                Fy写入值.Text = Fy_Start_textBox.Text;
               
            }
            if (Oy_checkBox.Checked)
            {
                Oy_Check_Flag = true;
                Oy写入值.Text = Oy_Start_textBox.Text;
               
            }

            if (Az_checkBox.Checked) 
            { 
                Az_Check_Flag = true;
                Az写入值.Text = Az_Start_textBox.Text;
              
            }
            if (Fz_checkBox.Checked)
            {
                Fz_Check_Flag = true;
                Fz写入值.Text = Fz_Start_textBox.Text;
              
            }
            if (Oz_checkBox.Checked)
            {
                Oz_Check_Flag = true;
                Oz写入值.Text = Oz_Start_textBox.Text;
              
            }

            if (Ar_checkBox.Checked) 
            {
                Ar_Check_Flag = true;
                Ar写入值.Text = Ar_Start_textBox.Text;
            }
            if (Fr_checkBox.Checked) 
            {
                Fr_Check_Flag = true;
                Fr写入值.Text = Fr_Start_textBox.Text;
            }
            if (Or_checkBox.Checked) 
            {
                Or_Check_Flag = true;
                Or写入值.Text = Or_Start_textBox.Text;
            }

            Timer_Run.Interval = 1;
            Timer_Run.Enabled = true;

        }

        private void 停止运动_Click(object sender, EventArgs e)
        {
            Timer_Run.Enabled = false;
            Timer_Rest.Enabled = false;

            Sin_Auto_Stop(true, false, false, false);
            Sin_Auto_Stop(false, true, false, false);
            Sin_Auto_Stop(false, false, true, false);
            Sin_Auto_Stop(false, false, false, true);

        }



        void Sin_Auto_Run(bool X_Flag, bool Y_Flag, bool Z_Flag, bool R_Flag)
        {
            if (X_Flag) { TCP.WriteSingleCoil(M1007, true); }
            if (Y_Flag) { TCP.WriteSingleCoil(M1107, true); }
            if (Z_Flag) { TCP.WriteSingleCoil(M1207, true); }
            if (R_Flag) { TCP.WriteSingleCoil(M1307, true); }
              
        }

        void Sin_Auto_Stop(bool X_Flag, bool Y_Flag, bool Z_Flag, bool R_Flag)
        {
            if (X_Flag) { TCP.WriteSingleCoil(M1008, false); }
            if (Y_Flag) { TCP.WriteSingleCoil(M1108, false); }
            if (Z_Flag) { TCP.WriteSingleCoil(M1208, false); }
            if (R_Flag) { TCP.WriteSingleCoil(M1308, false); }
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

            //速度显示
            X_Speed_textBox.Text = (TCP_Read_Register(HD102) * 1.0 / 100).ToString();
            Y_Speed_textBox.Text = (TCP_Read_Register(HD202) * 1.0 / 100).ToString();
            Z_Speed_textBox.Text = (TCP_Read_Register(HD302) * 1.0 / 100).ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
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
                    Connect_button.BackColor = Color.Green;
                    TCP_Connect_OK = true;
                    Data_One_Show();
                }
            }
            catch (EasyModbus.Exceptions.ConnectionException ex)
            {
            }
            catch (Exception ex)
            {

            }
        }

        private void Data_Write_button_Click(object sender, EventArgs e)        
        {
            XYZ_Speed_Data_Write(X_GO_textBox.Text, Y_GO_textBox.Text, Z_GO_textBox.Text, X_Speed_textBox.Text, Y_Speed_textBox.Text, Z_Speed_textBox.Text);
            XYZ_Speed_Data_Run(X_GO_textBox.Text, Y_GO_textBox.Text, Z_GO_textBox.Text);
        }   


        private void 固定值写入_Click(object sender, EventArgs e)
        {
            Sin_Data_Write(Ax写入值.Text, Fx写入值.Text,Ox写入值.Text,Ay写入值.Text,Fy写入值.Text,Oy写入值.Text, Az写入值.Text, Fz写入值.Text, Oz写入值.Text, Ar写入值.Text, Fr写入值.Text, Or写入值.Text);

        }


        private void X正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(true, false, false, false);

        }

        private void X正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(true, false, false, false);
        }

        private void Y正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(false, true, false, false);
        }



        private void Y正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(false, true, false, false);
        }



        private void Z正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(false, false, true, false);
        }

        private void Z正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(false, false, true, false);
        }

        private void R正弦启动_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Run(false, false, false, true);

        }

        private void R正弦停止_button_Click(object sender, EventArgs e)
        {
            Sin_Auto_Stop(false, false, false, true);
        }
        private void Timer_1s_Tick(object sender, EventArgs e)
        {

            if (TCP_Connect_OK)
            {
                Data_Time_Show();    //数据实时刷新
            }
        }
        private void Timer_Run_Tick(object sender, EventArgs e)
        {
                 

                    Sin_Data_Write(Ax写入值.Text, Fx写入值.Text, Ox写入值.Text, Ay写入值.Text, Fy写入值.Text, Oy写入值.Text, Az写入值.Text, Fz写入值.Text, Oz写入值.Text, Ar写入值.Text, Fr写入值.Text, Or写入值.Text);

                    if (X正弦_checkBox.Checked) { Sin_Auto_Run(true, false, false, false); }
                    if (Y正弦_checkBox.Checked) { Sin_Auto_Run(false, true, false, false); }
                    if (Z正弦_checkBox.Checked) { Sin_Auto_Run(false, false, true, false); }
                    if (R正弦_checkBox.Checked) { Sin_Auto_Run(false, false, false, true); }

            if (!(Ax_Check_Flag && Fx_Check_Flag && Ox_Check_Flag && Ay_Check_Flag && Fy_Check_Flag && Oy_Check_Flag && Az_Check_Flag && Fz_Check_Flag && Oz_Check_Flag && Ar_Check_Flag && Fr_Check_Flag && X_Check_Flag && Y_Check_Flag && Z_Check_Flag))
            {
                Check = true;
            }

            Timer_Rest.Interval = int.Parse(运动时间.Text);
                    Timer_Rest.Enabled = true;
                    Timer_Run.Enabled = false;
        }
              

        private void Timer_Rest_Tick(object sender, EventArgs e)    
        {
            {
                if (Ax_Check_Flag)
                {
                    Ax写入值.Text = (int.Parse(Ax写入值.Text) + int.Parse(Ax_Enhance_textBox.Text)).ToString();

                    if (int.Parse(Ax写入值.Text) == int.Parse(Ax_End_textBox.Text))
                    {
                        Ax_Check_Flag = false;
                    }
                }
                if (Fx_Check_Flag)
                {
                    Fx写入值.Text = (int.Parse(Fx写入值.Text) + int.Parse(Fx_Enhance_textBox.Text)).ToString();

                    if (int.Parse(Fx写入值.Text) == int.Parse(Fx_End_textBox.Text))
                    {
                        Fx_Check_Flag = false;
                    }
                }
                if (Ox_Check_Flag)
                {
                    Ox写入值.Text = (int.Parse(Ox写入值.Text) + int.Parse(Ox_Enhance_textBox.Text)).ToString();

                    if (Ox写入值.Text == Ox_End_textBox.Text)
                    {
                        Ox_Check_Flag = false;
                    }
                }

                if (Ay_Check_Flag)
                {
                    Ay写入值.Text = (int.Parse(Ay写入值.Text) + int.Parse(Ay_Enhance_textBox.Text)).ToString();

                    if (Ay写入值.Text == Ay_End_textBox.Text)
                    {
                        Ay_Check_Flag = false;
                    }
                }
                if (Fy_Check_Flag)
                {
                    Fy写入值.Text = (int.Parse(Fy写入值.Text) + int.Parse(Fy_Enhance_textBox.Text)).ToString();

                    if (Fy写入值.Text == Fy_End_textBox.Text)
                    {
                        Fy_Check_Flag = false;
                    }
                }
                if (Oy_Check_Flag)
                {
                    Oy写入值.Text = (int.Parse(Oy写入值.Text) + int.Parse(Oy_Enhance_textBox.Text)).ToString();

                    if (Oy写入值.Text == Oy_End_textBox.Text)
                    {
                        Oy_Check_Flag = false;
                    }
                }

                if (Az_Check_Flag)
                {
                    Az写入值.Text = (int.Parse(Az写入值.Text) + int.Parse(Az_Enhance_textBox.Text)).ToString();

                    if (Az写入值.Text == Az_End_textBox.Text)
                    {
                        Az_Check_Flag = false;
                    }
                }
                if (Fz_Check_Flag)
                {
                    Fz写入值.Text = (int.Parse(Fz写入值.Text) + int.Parse(Fz_Enhance_textBox.Text)).ToString();

                    if (Fz写入值.Text == Fz_End_textBox.Text)
                    {
                        Fz_Check_Flag = false;
                    }
                }
                if (Oz_Check_Flag)
                {
                    Oz写入值.Text = (int.Parse(Oz写入值.Text) + int.Parse(Oz_Enhance_textBox.Text)).ToString();

                    if (Oz写入值.Text == Oz_End_textBox.Text)
                    {
                        Oz_Check_Flag = false;
                    }
                }

                if (Ar_Check_Flag)
                {
                    Ar写入值.Text = (int.Parse(Ar_Start_textBox.Text) + int.Parse(Ar_Enhance_textBox.Text)).ToString();
                    if (Ar写入值.Text == Ar_End_textBox.Text)
                    {
                        Ar_Check_Flag = false;
                    }
                }
                if (Fr_Check_Flag)
                {
                    Fr写入值.Text = (int.Parse(Fr_Start_textBox.Text) + int.Parse(Fr_Enhance_textBox.Text)).ToString();

                    if (Fr写入值.Text == Fr_End_textBox.Text)
                    {
                        Fr_Check_Flag = false;
                    }
                }
                if (Or_Check_Flag)
                {
                    Or写入值.Text = (int.Parse(Or_Start_textBox.Text) + int.Parse(Or_Enhance_textBox.Text)).ToString();

                    if (Or写入值.Text == Or_End_textBox.Text)
                    {
                        Or_Check_Flag = false;
                    }
                }
                { 
                if (X_Check_Flag)
                {
                    X_GO_textBox.Text = (int.Parse(X_GO_textBox.Text) + int.Parse(X_Enhance_textBox.Text)).ToString();
                    if (X_GO_textBox.Text == X_End_textBox.Text)
                    {
                        X_Check_Flag = false;
                    }
                }
                if (Y_Check_Flag)
                {
                    Y_GO_textBox.Text = (int.Parse(Y_GO_textBox.Text) + int.Parse(Y_Enhance_textBox.Text)).ToString();
                    if (Y_GO_textBox.Text == Y_End_textBox.Text)
                    {
                        Y_Check_Flag = false;
                    }
                }
                if (Z_Check_Flag)
                {
                    Z_GO_textBox.Text = (int.Parse(Z_GO_textBox.Text) + int.Parse(Z_Enhance_textBox.Text)).ToString();
                    if (Z_GO_textBox.Text == Z_End_textBox.Text)
                    {
                        Z_Check_Flag = false;
                    }
                }
                }
            }

            if(Check)
            {
                Timer_Run.Enabled = false;
                Timer_Rest.Enabled = false;
            }

            Sin_Auto_Stop(true, false, false, false);
            Sin_Auto_Stop(false, true, false, false);
            Sin_Auto_Stop(false, false, true, false);
            Sin_Auto_Stop(false, false, false, true);

            if (Location_Flag)
            {
                XYZ_Speed_Data_Write(X_GO_textBox.Text, Y_GO_textBox.Text, Z_GO_textBox.Text, X_Speed_textBox.Text, Y_Speed_textBox.Text, Z_Speed_textBox.Text);
                XYZ_Speed_Data_Run(X_GO_textBox.Text, Y_GO_textBox.Text, Z_GO_textBox.Text);
            }

            Timer_Run.Interval = int.Parse(间歇时间.Text);
            Timer_Run.Enabled = true;
            Timer_Rest.Enabled = false;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        private void label23_Click(object sender, EventArgs e)
        {

        }
        private void label17_Click(object sender, EventArgs e)
        {

        }
        private void label8_Click(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
