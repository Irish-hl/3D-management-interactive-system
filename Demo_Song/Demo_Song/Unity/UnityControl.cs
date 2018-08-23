using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Demo_Song
{
    public partial class UnityControl : UserControl
    {

        //自定义用户控件，系统默认工具的组合
        //直接调用Win32 的API
        [DllImport("User32.dll")]
        /// <summary>
        /// 该函数改变指定窗口的位置和尺寸。对于顶层窗口，位置和尺寸是相对于屏幕的左上角的：对于子窗口，位置和尺寸是相对于父窗口客户区的左上角坐标的。
        /// </summary>
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        /// <summary>
        /// 该函数枚举一个父窗口的所有子窗口
        /// </summary>
        /// <param name="hWnd">其窗口程序将接收消息的窗口的句柄</param>
        /// <param name="func"> 回调函数的地址.回调函数的返回值将会影响到这个API函数的行为。如果回调函数返回true，则枚举继续直到枚举完成；如果返回false，则将会中止枚举。</param>
        /// <param name="lParam">指定附加的消息指定信息</param>
        /// <returns></returns>
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        [DllImport("user32.dll")]
        /// <summary>
        /// 该函数将指定的消息发送到一个或多个窗口。此函数为指定的窗口调用窗口程序，直到窗口程序处理完消息再返回。　
        /// </summary>
        /// <param name="hWnd">父窗口句柄</param>
        /// <param name="msg">指定被发送的消息</param>
        /// <param name="wParam">指定附加的消息指定信息</param>
        /// <param name="lParam">指定附加的消息指定信息</param>
        /// <returns></returns>
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private Process process;
        private IntPtr unityHWND = IntPtr.Zero;//IntPtr类型称为“平台特定的整数类型”，它们用于本机资源，如窗口句柄。将句柄设置为0

        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);





        public UnityControl()
        {
            InitializeComponent();
            this.Load += UnityControl_Load;
            panel1.Resize += panel1_Resize;
        }

        /// <summary>
        /// 加载自定义用户控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnityControl_Load(object sender, EventArgs e)
        {
            try
            {
                process = new Process();
                process.StartInfo.FileName = Application.StartupPath + @"\UnityApp\carManage.exe";
                process.StartInfo.Arguments = "-parentHWND " + panel1.Handle.ToInt32() + " " + Environment.CommandLine;//传递被调用的进程Main(string [] args)中的args[]字符串数组
                process.StartInfo.UseShellExecute = true;//使用外壳来运行程序
                process.StartInfo.CreateNoWindow = true;//启动新的窗口来执行这个脚本

                process.Start();

                process.WaitForInputIdle();
                unityHWND = process.MainWindowHandle;
                EnumChildWindows(panel1.Handle, WindowEnum, IntPtr.Zero);

                unityHWNDLabel.Text = "Unity HWND: 0x" + unityHWND.ToString("X8");
            }
            catch (Exception ex)
            {
                unityHWNDLabel.Text = ex.Message;
                //MessageBox.Show(ex.Message);
            }
        }

        internal void ActivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }

        internal void DeactivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
        }

        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHWND = hwnd;
            ActivateUnityWindow();
            return 0;
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            MoveWindow(unityHWND, 0, 0, panel1.Width, panel1.Height, true);
            ActivateUnityWindow();
        }

        // Close Unity application
        internal void Form1_FormClosed()
        {
            try
            {
                process.CloseMainWindow();

                Thread.Sleep(1000);
                while (process.HasExited == false)
                    process.Kill();
            }
            catch (Exception)
            {

            }
        }

        internal void Form1_Activated()
        {
            ActivateUnityWindow();
        }

        internal void Form1_Deactivate()
        {
            DeactivateUnityWindow();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
