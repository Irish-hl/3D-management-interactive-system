using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System;
using System.Collections;
using System.Net;
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
//蜂群理论、异构


namespace Demo_Song
{
    public class TcpServer
    {
        //私有成员
        private static byte[] result = new byte[1024];
        private int myProt = 8080;   //端口
        static Socket serverSocket;
        static Socket clientSocket;
        public static ScriptRuntime runtime;
        public static string carsLocation = "";

        Thread myThread;
        static Thread receiveThread;
        static int receiveNumber;
        public static dynamic obj;//可以绕过编译时类型检查,改为在运行时解析这些操作。 dynamic 类型简化了对 COM API（例如 Office Automation API）、动态 API（例如 IronPython 库）和 HTML 文档对象模型 (DOM) 的访问。
   
        //属性

        public int port { get; set; }
        //方法



        internal void StartServer()
        {
            //服务器IP地址  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
            serverSocket.Listen(12);    //设定最多12个排队连接请求  

            //Debug.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());

            //开启线程，对接入客户端进行监听
            myThread = new Thread(ListenClientConnect);
            myThread.Start();
        }
        internal void StartConnect()
        {
            runtime = Python.CreateRuntime();
           
        }

        internal void QuitServer()
        {

            serverSocket.Close();
            clientSocket.Close();
            myThread.Abort();
            receiveThread.Abort();


        }


        internal void SendMessage(string msg)
        {
            if (clientSocket == null||!clientSocket.Connected) Console.WriteLine("Socket为null");
            else clientSocket.Send(Encoding.ASCII.GetBytes(msg));//就是把字符串 str 按照简体中文(ASCIIEncoding.ASCII)的编码方式, 编码成 Bytes类型的字节流数组

        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private static void ListenClientConnect()
        {
            while (true)
            {
                try
                {
                    clientSocket = serverSocket.Accept();//以同步方式从侦听套接字的连接请求队列中提取第一个挂起的连接请求，然后创建并返回一个新 Socket


                    clientSocket.Send(Encoding.ASCII.GetBytes(carsLocation));
                    receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start(clientSocket);
                }
                catch (Exception)
                {

                }

            }
        }

        /// <summary>  
        /// 接收消息  
        /// </summary>  
        /// <param name="clientSocket"></param>  
        private static void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    receiveNumber = myClientSocket.Receive(result);
                    string receive=Encoding.ASCII.GetString(result, 0, receiveNumber);
                    Console.WriteLine(",{0}",receive);
                    obj.set_control_list((Convert.ToInt32(receive) % 10 + 1) + "");
                    if (receiveNumber < 30)
                    {
                        obj.set_speed(1000,1000);
                    }
                    else obj.set_speed(-1000,-1000);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Debug.WriteLine(ex.Message);
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                    Console.WriteLine(ex.Message);

                }
            }
        }
    }
}
