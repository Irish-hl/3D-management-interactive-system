using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class unityClient : MonoBehaviour {
    const int portNo = 8080;
    static TcpClient _client;
    byte[] data;
    string Error_Message;
    public GameObject[] cameras;
    public GameObject mainCamera;
    public GameObject topCamera;
    int i = 0;
    //静态变量，可被motion.cs访问
    public static int msg = 0;
    public static string changeCarNum="";
    public static float[] angle = new float[10];
    public static int changeCarX;
    public static int changeCarY;
    public static int[] allCarX = new int[10];
    public static int[] allCarY = new int[10];
    public static bool getAllCarLocation = false;



	// Use this for initialization
	void Start () {
        try
       {
           _client = new TcpClient();
           _client.Connect("127.0.0.1", portNo);
           Debug.Log("连接服务器成功");
           data = new byte[_client.ReceiveBufferSize];
           _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);//开始异步读操作
           
        }
       catch 
       {

           Debug.Log("连接服务器失败");
       }
        cameras = new GameObject[10];
        for (int i = 0; i < 10; i++)
            cameras[i] = GameObject.Find(i + "/CarCamera" + i);
        mainCamera = GameObject.Find("Main Camera");
        topCamera = GameObject.Find("TopCamera");
	}
	
	// Update is called once per frame
	void Update () {
        i++;
        if (i == 30)
        {
            i = 0;
            if(msg>2&&msg<15)changeView(msg); 
            //Debug.Log("："+msg);
        }

        
	}
   
    /// <summary>
    /// 关闭所有摄像头
    /// </summary>
    void setCamerasClose()
    {
        for (int i = 0; i < 10; i++)
            cameras[i].SetActive(false);
        mainCamera.SetActive(false);
        topCamera.SetActive(false);
    }

    /// <summary>
    /// 开启指定摄像头
    /// </summary>
    /// <param name="choose">摄像头编号 3:主视 4:俯视 5-14：小车摄像头</param>
    public void changeView(int choose)
    {
        //设置所有摄像头关闭
        setCamerasClose();
        //Debug.Log(choose);
        //将下拉列表中选中的摄像头开启
        if (choose == 3) mainCamera.SetActive(true);
        else if (choose == 4) topCamera.SetActive(true);
        else cameras[choose - 5].SetActive(true);
    }

    /// <summary>
    /// Socket接收到消息的回调函数
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveMessage(IAsyncResult ar)
   {
           int bytesRead;
           bytesRead = _client.GetStream().EndRead(ar);
           
           
           if (bytesRead < 1)
           {
               return;
           }
           else
           {
               string message = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);
               //data = null;
               Debug.Log("收到的信息为" + System.Text.Encoding.ASCII.GetString(data, 0, bytesRead)); 
               try
               {
                   msg=Convert.ToInt32(message);
               }
               catch
               {
                   string []carArr=message.Split(new char[]{';'});
                   if (carArr.Length == 3)
                   {
                       changeCarNum = carArr[0];
                       changeCarX = Convert.ToInt32(carArr[1]);
                       changeCarY = Convert.ToInt32(carArr[2]);
                       //Debug.Log("收到更改坐标的小车信息为" + changeCarNum + "," + changeCarX + "," + changeCarY); 
                   }
                   else
                   {
                       for (int j = 0; j < 30; j+=3)
                       {
                           if (carArr[j] != "-1")
                           {
                               allCarX[j / 3] = Convert.ToInt32(carArr[j])/2;
                               allCarY[j / 3] = Convert.ToInt32(carArr[j + 1])/2;
                               angle[j / 3] = -90.0f-Convert.ToInt32(carArr[j + 2]);
                           }
                       }
                   }
               }           
            }
           _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
   }

    /// <summary>
    /// 关闭TCPSocket
    /// </summary>
   void OnDestroy()
   {
       _client.Close();
   }

    /// <summary>
    /// 向wpf发送信息
    /// </summary>
    /// <param name="message">发送的信息</param>
   public new static void SendMessage(string message)
   {
       try
       {
           NetworkStream ns = _client.GetStream();
           byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
           ns.Write(data, 0, data.Length);
           ns.Flush();
       }
       catch (Exception ex)
       {
           Debug.Log(ex.ToString());
       }
   }
}
