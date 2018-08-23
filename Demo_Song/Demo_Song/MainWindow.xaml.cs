using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Windows.Resources;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Text;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;

namespace Demo_Song
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int size_state = 0;//窗口状态：最大化/正常
        TcpServer WpfToUnity;
        bool isPlay = true;
        bool isClick = false;
        ObservableCollection<Member> memberData;//动态数据集合
        double targetX = 0, targetY = 0;
        bool[] isRed = new bool[10];
        int[] carLocation = new int[20];
        FileStream fs;//文件流对象
        StreamWriter sw;//指向文件流的流读取器
        DispatcherTimer askLocationTimer;
        int[] carRole = new int[10];
        System.DateTime currentTime = new System.DateTime(); 
        string myConnectionStr = "server=localhost;User Id=root;password=;Database=background";


        /// <summary>
        /// 创建主窗口
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //初始化表格数据
            memberData = new ObservableCollection<Member>();
            addDataGrid(memberData);
            dataGrid.DataContext = memberData;

            for (int i = 0; i < 10;i++)
            {
                isRed[i] = false;
            }

            //使用FileStream类创建文件，使用StreamWriter类，将数据写入到文件
            fs = new FileStream("Location\\小车坐标.txt", FileMode.Open, FileAccess.ReadWrite);
            sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));

            this.Closed += MainWindow_Closed;
            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;

            WpfToUnity = new TcpServer();
            try
            {
               
                try
                {
                    WpfToUnity.StartConnect();//连接中心服务器
                    TcpServer.obj = TcpServer.runtime.UseFile("wpfUser.py");
                    MessageBox.Show("连接中心服务器成功");
                }
               catch
                {
                    MessageBox.Show("连接中心服务器失败");
                    Environment.Exit(0);
                }
                WpfToUnity.StartServer();//开启unity服务器
                MessageBox.Show("unity服务器开启成功");

                TcpServer.obj.login("irishuang","123456789");
                TcpServer.obj.set_control_list("1,2,3,4,5,6,7,8,9,10");
                TcpServer.carsLocation = TcpServer.obj.get_location();
                //TcpServer.carsLocation = "20;50;30;40;50;45;60;50;60;80;50;70;50;80;20;50;60;40;45;40;50;25;20;50;55;40;35;0;0;0";
                MessageBox.Show("获取各小车坐标为：" + TcpServer.carsLocation);

                initDataGrid();
                //初始化白板中小车图片位置
                initiCarPic();
            }
            catch
           {
                MessageBox.Show("unity服务器开启失败");
            } 
                //设置定时向服务器请求小车接下来的坐标
                askLocationTimer = new DispatcherTimer();
                askLocationTimer.Tick += new EventHandler(askLocation);
                askLocationTimer.Interval = new TimeSpan(0, 0, 1);//每1秒执行一次
                askLocationTimer.Start();
        }

        /// <summary>
        /// 定义一个新类
        /// </summary>
        public class Member
        {
           public string CarId { get; set; }
            public string CarState { get; set; }
            public string CarX { get; set; }
            public string CarY { get; set; }
            public string CarAngle { get; set; }

        }

        /// <summary>
        /// 向服务器寻求最新坐标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void askLocation(object sender, EventArgs e)
        {
            //MessageBox.Show("定时器");
            TcpServer.obj.set_control_list("1,2,3,4,5,6,7,8,9,10");
            if (TcpServer.carsLocation != TcpServer.obj.get_location())
            {
                TcpServer.carsLocation = TcpServer.obj.get_location();
                WpfToUnity.SendMessage(TcpServer.carsLocation);
                initiCarPic();
                initDataGrid();
            }
            
        }

        /// <summary>
        /// 初始化表格数据
        /// </summary>
        private void initDataGrid()
        {
            currentTime = System.DateTime.Now;
            
            string[] xs = TcpServer.carsLocation.Split(new Char[] { ';' }, StringSplitOptions.None);
            int i = 0, j = 1,k=2;
            sw.Write("\r\n当前时间：" + currentTime + "\r\n");
            foreach (Member item in memberData)
            {
                if (xs[i] == "-1")
                {
                    item.CarState = "故障";
                    sw.Write(item.CarId + "号小车 状态：" + item.CarState + "\r\n");
                }
                else if (xs[i] == "0")
                    {
                        item.CarState = "未使用";
                        sw.Write(item.CarId + "号小车 状态：" + item.CarState + "\r\n");
                       
                    }
                    else
                    {
                        item.CarX = xs[i];
                        item.CarY = xs[j];
                        item.CarAngle = xs[k];
                        item.CarState = "正常";
                        sw.Write(item.CarId + "号小车 状态：" + item.CarState + " X:" + item.CarX + " Y:" + item.CarY + " 角度：" + item.CarAngle + "\r\n");
                    }
                i += 3;
                j += 3;
                k += 3;
                dataGrid.ItemsSource = null;
                dataGrid.ItemsSource = memberData;
            }
        }

        /// <summary>
        /// 初始化设置画布
        /// </summary>
        private void initiCarPic()
        {
            string[] carPicLocationStr = TcpServer.carsLocation.Split(new Char[] { ';' }, StringSplitOptions.None);
            double[] carPicLocation = new double[20];
            int j = 0;
            for (int i = 0; i < 30; i++)
            {
                if ((i + 1) % 3 != 0)
                {
                    if (carPicLocationStr[i] != "-1") carPicLocation[j] = (Convert.ToDouble(carPicLocationStr[i])*2);
                    j++;
                }
                
            }
            Canvas.SetLeft(carPic_1, carPicLocation[0] ); Canvas.SetBottom(carPic_1, carPicLocation[1] );
            Canvas.SetLeft(carPic_2, carPicLocation[2]); Canvas.SetBottom(carPic_2, carPicLocation[3] );
            Canvas.SetLeft(carPic_3, carPicLocation[4] ); Canvas.SetBottom(carPic_3, carPicLocation[5] );
            Canvas.SetLeft(carPic_4, carPicLocation[6] ); Canvas.SetBottom(carPic_4, carPicLocation[7] );
            Canvas.SetLeft(carPic_5, carPicLocation[8] ); Canvas.SetBottom(carPic_5, carPicLocation[9] );
            Canvas.SetLeft(carPic_6, carPicLocation[10] ); Canvas.SetBottom(carPic_6, carPicLocation[11]);
            Canvas.SetLeft(carPic_7, carPicLocation[12]); Canvas.SetBottom(carPic_7, carPicLocation[13]);
            Canvas.SetLeft(carPic_8, carPicLocation[14]); Canvas.SetBottom(carPic_8, carPicLocation[15]);
            Canvas.SetLeft(carPic_9, carPicLocation[16] ); Canvas.SetBottom(carPic_9, carPicLocation[17] );
            Canvas.SetLeft(carPic_10, carPicLocation[18] ); Canvas.SetBottom(carPic_10, carPicLocation[19] );
        }

        /// <summary>
        /// 添加初始数据到表格
        /// </summary>
        /// <param name="memberData">小车状态信息数据集合</param>
        private void addDataGrid(ObservableCollection < Member > memberData)
        {
            memberData.Add(new Member()
            {
                CarId = "1",
                CarState = "故障",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "2",
                CarState = "故障",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "3",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "4",
                CarState = "故障",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "5",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "6",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "7",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "8",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "9",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
            memberData.Add(new Member()
            {
                CarId = "10",
                CarState = "正常",
                CarX = "0",
                CarY = "0",
                CarAngle = "0º",
            });
        }

        /// <summary>
        /// 关闭总桌面程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 关闭主窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Closed(object sender, EventArgs e)
        {
            unityhost.Form1_FormClosed();

            askLocationTimer.Stop();
            WpfToUnity.QuitServer();
            sw.Close();
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            unityhost.Form1_Deactivate();
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            unityhost.Form1_Activated();
        }

        /// <summary>
        /// 最大化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_normal_Click(object sender, RoutedEventArgs e)
        {
            if (size_state == 0)
            {
                this.WindowState = WindowState.Normal;
                this.Width = 1280;
                this.Height = 800;
                size_state = 1;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                size_state = 0;
            }
        }

        /// <summary>
        /// 最小化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {


            }

        }

        /// <summary>
        /// 开始/暂停按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseDown_stop(object sender, MouseEventArgs e)
        {
            if (isPlay)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/play.png", UriKind.Relative));
                stopplayPic.Source = img;
                TcpServer.obj.set_control_list("1,2,3,4,5,6,7,8,9,10");
                TcpServer.obj.set_speed(-1000,-1000);
                WpfToUnity.SendMessage("1");
            }
            else
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/stop.png", UriKind.Relative));
                stopplayPic.Source = img;
                TcpServer.obj.set_control_list("1,2,3,4,5,6,7,8,9,10");
                TcpServer.obj.set_speed(1000,1000);
                WpfToUnity.SendMessage("2");
            }
            isPlay =!isPlay;
        }


        /// <summary>
        /// 主视图按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseDown_mainView(object sender, MouseEventArgs e)
        {
            WpfToUnity.SendMessage("3");
        }

        /// <summary>
        /// 俯视图按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseDown_topView(object sender, MouseEventArgs e)
        {
            WpfToUnity.SendMessage("4");
        }

        /// <summary>
        /// 小车视野的单选框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            string carIdStr = sender.ToString().Substring(39,1);
            int carId = Convert.ToInt32(carIdStr)+4;
            WpfToUnity.SendMessage(carId.ToString());
            //MessageBox.Show(carIdStr);
        }

        /// <summary>
        /// 小车图片点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseDown_carDot1(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/11.png", UriKind.Relative));
                carPic_1.Source = img;
                isClick = true;
                isRed[0] = true;
            }

            e.Handled = true;
        }
        private void Image_MouseDown_carDot2(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/22.png", UriKind.Relative));
                carPic_2.Source = img;
                isClick = true;
                isRed[1] = true;
            }

            e.Handled = true;
        }
        private void Image_MouseDown_carDot3(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/33.png", UriKind.Relative));
                carPic_3.Source = img;
                isClick = true;
                isRed[2] = true;
            }
            e.Handled = true;
        }
        private void Image_MouseDown_carDot4(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/44.png", UriKind.Relative));
                carPic_4.Source = img;
                isClick = true;
                isRed[3] = true;
            }


            e.Handled = true;
        }
        private void Image_MouseDown_carDot5(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/55.png", UriKind.Relative));
                carPic_5.Source = img;
                isClick = true;
                isRed[4] = true;
            }

            e.Handled = true;
        }
        private void Image_MouseDown_carDot6(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/66.png", UriKind.Relative));
                carPic_6.Source = img;
                isClick = true;
                isRed[5] = true;
            }

            e.Handled = true;
        }
        private void Image_MouseDown_carDot7(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/77.png", UriKind.Relative));
                carPic_7.Source = img;
                isClick = true;
                isRed[6] = true;
            }
            e.Handled = true;
        }
        private void Image_MouseDown_carDot8(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/88.png", UriKind.Relative));
                carPic_8.Source = img;
                isClick = true;
                isRed[7] = true;
            }

            e.Handled = true;
        }
        private void Image_MouseDown_carDot9(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/99.png", UriKind.Relative));
                carPic_9.Source = img;
                isClick = true;
                isRed[8] = true;
            }
            e.Handled = true;
        }
        private void Image_MouseDown_carDot10(object sender, MouseButtonEventArgs e)
        {
            if (!isClick)
            {
                BitmapImage img = new BitmapImage(new Uri("Assets/img/00.png", UriKind.Relative));
                carPic_10.Source = img;
                isClick = true;
                isRed[9] = true;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 状态模块画布点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isClick)
            {
                isClick = false;
                targetX = e.GetPosition(canvas).X;
                targetY = e.GetPosition(canvas).Y;
                //MessageBox.Show(targetX + "," + (395-targetY));
                int picId = -1;
                for (int i = 0; i < 10; i++)
                {
                    if (isRed[i])
                    {
                        picId = i;
                        isRed[i] = false; break;
                    }
                }
                BitmapImage img;
                if (picId != 9)
                {
                    img = new BitmapImage(new Uri("Assets/img/" + (picId + 1) + ".png", UriKind.Relative));
                }
                else img = new BitmapImage(new Uri("Assets/img/0.png", UriKind.Relative));

                switch (picId)
                {

                    case 0: carPic_1.Source = img; Canvas.SetLeft(carPic_1, targetX - 13); Canvas.SetTop(carPic_1, targetY - 13);
                        WpfToUnity.SendMessage("0;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 1: carPic_2.Source = img; Canvas.SetLeft(carPic_2, targetX - 13); Canvas.SetTop(carPic_2, targetY - 13);
                        WpfToUnity.SendMessage("1;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 2: carPic_3.Source = img; Canvas.SetLeft(carPic_3, targetX - 13); Canvas.SetTop(carPic_3, targetY - 13);
                        WpfToUnity.SendMessage("2;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 3: carPic_4.Source = img; Canvas.SetLeft(carPic_4, targetX - 13); Canvas.SetTop(carPic_4, targetY - 13);
                        WpfToUnity.SendMessage("3;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 4: carPic_5.Source = img; Canvas.SetLeft(carPic_5, targetX - 13); Canvas.SetTop(carPic_5, targetY - 13);
                        WpfToUnity.SendMessage("4;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 5: carPic_6.Source = img; Canvas.SetLeft(carPic_6, targetX - 13); Canvas.SetTop(carPic_6, targetY - 13);
                        WpfToUnity.SendMessage("5;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 6: carPic_7.Source = img; Canvas.SetLeft(carPic_7, targetX - 13); Canvas.SetTop(carPic_7, targetY - 13);
                        WpfToUnity.SendMessage("6;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 7: carPic_8.Source = img; Canvas.SetLeft(carPic_8, targetX - 13); Canvas.SetTop(carPic_8, targetY - 13);
                        WpfToUnity.SendMessage("7;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 8: carPic_9.Source = img; Canvas.SetLeft(carPic_9, targetX - 13); Canvas.SetTop(carPic_9, targetY - 13);
                        WpfToUnity.SendMessage("8;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                    case 9: carPic_10.Source = img; Canvas.SetLeft(carPic_10, targetX - 13); Canvas.SetTop(carPic_10, targetY - 13);
                        WpfToUnity.SendMessage("9;" + (int)(targetX * 0.25 - 3.25) + ";" + (int)(98 - targetY * 0.25)); break;
                }
                TcpServer.obj.set_control_list((picId + 1) + "");
                TcpServer.obj.set_target_pts(targetX/2 + "," + (395-targetY)/2);
            }
        }
        /// <summary>
        /// 各小车角色设置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void roleRadioButton_Click(object sender, RoutedEventArgs e)
        {
            string carRoleStr = sender.ToString().Substring(39, 1);
            carRole[Convert.ToInt32(carRoleStr)] = Convert.ToInt32(sender.ToString().Substring(40, 1));
        }

        /// <summary>
        /// 发送角色之黑板按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setRole_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection connection = new MySqlConnection(myConnectionStr);
            MySqlCommand cmd;
            connection.Open();
            string a = "";
            bool isRoleOk = true;
            for (int i = 0; i < 10; i++)
            {
                a += carRole[i] + " ";
                if (carRole[i] == 0)
                    isRoleOk = false;
            }
            if (!isRoleOk) MessageBox.Show("请先设置完整！");
            else
            {
                cmd = connection.CreateCommand();

                for (int i = 0; i < 10; i++)
                {
                    string sql = "update carRole set role=" + carRole[i] + " where carId=" + (i + 1);
                    cmd.CommandText = sql.ToString();
                    cmd.ExecuteNonQuery();
                }


                cmd.CommandText = "select * from carRole";
                MySqlDataAdapter adap = new MySqlDataAdapter(cmd);//创建数据适配器对象,执并行数据库操作命令
                DataTable ds = new DataTable();//创建数据集对象
                adap.Fill(ds);//填充数据集
                carGridView.ItemsSource = ds.DefaultView;
                connection.Close();
            }
        }

        /// <summary>
        /// 查看角色介绍按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void seeRole_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("基于角色的访问控制\r\n role1：可以看到公开的小车信息，但自己的信息保密\r\n role2：可以看到公开的小车信息，自己的信息也公开\r\n role3：不可以看到公开的小车信息，自己的信息公开");
        }

        /// <summary>
        /// 更新版本按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("当前已是最新版本");
        }
       
     
    }
}
