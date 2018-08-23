using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Net;
using System.IO;

namespace ServerSocket
{
    class Program
    {
        //存放接收到的数据
        private static byte[] result = new byte[1024];
        private const int port = 8088;
        private static string IpStr = "127.0.0.1";
        private static Socket serverSocket;

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(IpStr);
            IPEndPoint ip_end_point = new IPEndPoint(ip, port);
            //创建服务器Socket对象，并设置相关属性
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定ip和端口
            serverSocket.Bind(ip_end_point);
            //设置最长的连接请求队列长度
            serverSocket.Listen(10);
            //{0}为占位符，代表后面字符串数组的下标
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //在新线程中监听客户端的连接
            Thread thread = new Thread(ClientConnectListen);
            thread.Start();
            Console.ReadLine();
        }

        /// <summary>
        /// 客户端连接请求监听
        /// </summary>
        private static void ClientConnectListen()
        {
            //服务器为新的客户端连接创建一个Socket对象
            Socket clientSocket = serverSocket.Accept();
            Console.WriteLine("客户端{0}成功连接", clientSocket.RemoteEndPoint.ToString());
            //向连接的客户端发送连接成功的数据
            serverSendMessage(clientSocket,"客户端连接成功！");
            //Console.Write("请输入通信数据 ");
            //string order = Console.ReadLine();
            //buffer.WriteString(order);
            //clientSocket.Send(WriteMessage(buffer.ToBytes())); 
            
             //服务器为每个客户端连接创建一个线程来接受该客户端发送的消息
             Thread thread = new Thread(RecieveMessage);
             thread.Start(clientSocket);

          
        }

        /// <summary>
        /// 数据转换，网络发送需要两部分数据，一是数据长度，二是主体数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static byte[] WriteMessage(byte[] message)
        {
            MemoryStream ms = null;
            using (ms = new MemoryStream())
            {
                ms.Position = 0;
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(message);
                writer.Flush();
                return ms.ToArray();
            }
        }
        private static void serverSendMessage(Socket clientSocket,string message)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteString(message);
            clientSocket.Send(WriteMessage(buffer.ToBytes()));
        }

        /// <summary>
        /// 服务器接收指定客户端Socket的消息
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void RecieveMessage(object clientSocket)
        {
            Socket mClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //获得接收到的数据长度
                    int receiveNumber = mClientSocket.Receive(result);
                    Console.WriteLine("接收客户端{0}消息", mClientSocket.RemoteEndPoint.ToString());
                    //Console.WriteLine("接收客户端字节数组为{1}",  BitConverter.ToString(result.ToArray()));
                    ByteBuffer buff = new ByteBuffer(result);
                    //读取二进制数据中前4个字节的数据，即读取一个整数
                    int num = buff.ReadInt();
                    Console.WriteLine("从客户端接收到的数据编号为{0}", num);
                    switch (num)
                    {
                        case 1: Console.WriteLine("同意！"); ; break;
                        case 2:order_2(buff, mClientSocket); break;
                        case 3:order_3(buff,mClientSocket); break;
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    mClientSocket.Shutdown(SocketShutdown.Both);
                    mClientSocket.Close();
                    break;
                }
            }
        }
        //处理命令3-登录账号  3 i-s i-s
        public static void order_3(ByteBuffer buff,Socket mClientSocket)
        {
            //读取剩余二进制数据中的账户字符串长度
            int accountLen = buff.ReadInt();
            //读取账户字符串
            string account = buff.ReadString(accountLen);
            //读取密码字符串长度
            int passLen = buff.ReadInt(); 
            //读取密码字符串
            string password = buff.ReadString(passLen);
            Console.WriteLine("从客户端接收到的账户长度为{0},账户为{1},密码长度是{2}，密码是{3}", accountLen,account,passLen,password);
            serverSendMessage(mClientSocket, "用户账户登录成功");
        }
        public static void order_2(ByteBuffer buff, Socket mClientSocket)
        {
            //读取剩余二进制数据中的字符串长度
            int accountLen = buff.ReadInt();
            //读取字符串
            string account = buff.ReadString(accountLen);
            Console.WriteLine("从客户端接收到的字符串长度为{0},字符串为{1}", accountLen, account);
            serverSendMessage(mClientSocket, "命令2收到");
        }
    }
}
