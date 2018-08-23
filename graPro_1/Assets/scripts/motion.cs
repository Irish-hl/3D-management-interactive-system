using System.Collections;
using System.Collections.Generic;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;


public class motion : MonoBehaviour
{
    private GameObject ui;
    //小车原本的目的坐标
    private Vector3 dest;
    private float x;
    private float z;
    //小车当前的状态：行驶中、暂停中、停止
    public string state;
    //小车当前设备状态，工作true，不工作false
    private bool isOk = true;
    //小车的行驶速度
    public float step;
    //当前小车是否在运动
    private bool flag;
    private float angle;
    private float SliderNo;
    //修改过后的目的坐标
    private float SliderX;
    private float SliderY;
    public string carName;

    const int portNo = 8080;
    private TcpClient _client;
    byte[] data;
    string Error_Message;
    int n;
    bool isStart = false;


    // Use this for initialization
    void Start()
    {
        carName = this.name;
        n = Convert.ToInt32(carName);
        //每个小车是一个客户端
        ui = GameObject.Find("Canvas/info");
        //x = UnityEngine.Random.Range(0, 210);
        //z = UnityEngine.Random.Range(0, 200);
        //angle = Mathf.Atan(x / z) * 180 / 3.14f;        //step = 4.0f* Time.deltaTime;
        //this.transform.rotation = Quaternion.Euler(0, unityClient.angle[n], 0);//自转相应的角度    
        step = 0.1f;
        flag = false;
        state = "行驶中";
    }

    // Update is called once per frame
    void Update()
    {
        //当前物体的移动
        x = unityClient.allCarX[n];
        z = unityClient.allCarY[n];
        //if (!isStart) { this.transform.rotation = Quaternion.Euler(0, unityClient.angle[n], 0); isStart = true; }//自转相应的角度
        dest = new Vector3(x, 0, z);
        gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, dest, step);
        this.transform.rotation = Quaternion.Euler(0, unityClient.angle[n], 0);
        if (this.transform.localPosition == dest)
            state = "停止";
        if (unityClient.msg == 1 && state != "停止")
        {
            step = 0; state = "暂停中"; flag = true; //Debug.Log(carName+"号小车被迫暂停");
        }
        if (unityClient.msg == 2 && state != "停止")
        {
            step = 0.1f; state = "行驶中"; flag = false; //Debug.Log(carName + "号小车继续行驶");
        }
        if (unityClient.changeCarNum == carName)
        {
            unityClient.allCarX[n] = unityClient.changeCarX;
            unityClient.allCarY[n] = unityClient.changeCarY;
            //float anglexy;
            //if (this.transform.localPosition.z == unityClient.allCarY[n]) anglexy = 270.0f;
            //else anglexy = Math.Abs(Mathf.Atan((unityClient.changeCarX - this.transform.localPosition.x) / (unityClient.changeCarY - this.transform.localPosition.z))) * 180 / 3.14f + 180.0f;
            //Debug.Log("angle:" + anglexy);
            x = unityClient.allCarX[n];
            z = unityClient.allCarY[n];
            //针对新坐标在原坐标的四个不同区域内，做不同的角度处理
            /*if (x > this.transform.localPosition.x && z > this.transform.localPosition.z)
                this.transform.rotation = Quaternion.Euler(0, (180 + anglexy), 0);//右上区域
            if (x > this.transform.localPosition.x && z < this.transform.localPosition.z)
                this.transform.rotation = Quaternion.Euler(0, -anglexy, 0);//右下区域
            if (x < this.transform.localPosition.x && z > this.transform.localPosition.z)
                this.transform.rotation = Quaternion.Euler(0, (180 - anglexy), 0);//左上
            else this.transform.rotation = Quaternion.Euler(0, anglexy, 0);//左下*/

            //dest = new Vector3(x, 0, z);//修改目的坐标
            state = "行驶中";
            unityClient.changeCarNum = "";
        }

        // Debug.Log("msg：" + unityClient.msg);
    }


    /// <summary>
    /// 鼠标滑过事件(当鼠标在小车附近时)
    /// </summary>
    void OnMouseOver()
    {
        ui.GetComponent<Text>().text = "当前小车编号:" + (Convert.ToInt32(carName) + 1) + "\n目的坐标：(" + (x * 2) + "," + (z * 2) + ")\n当前状态：" + state + "\n当前坐标：" + (this.transform.localPosition.x * 2) + "," + (this.transform.localPosition.y * 2);
    }



    /// <summary>
    /// 鼠标移出事件(当鼠标离开小车附近时)
    /// </summary>
    void OnMouseExit()
    {
        ui.GetComponent<Text>().text = "";
    }

    /// <summary>
    /// 鼠标左键按下事件
    /// </summary>
    void OnMouseDown()
    {
        if (Input.GetMouseButton(0) && state != "停止")//按下左键
        {
            if (flag == false)
            {
                step = 0; state = "暂停中"; flag = true;
                unityClient.SendMessage("3" + carName);
                Debug.Log(carName + "号小车请求暂停");
            }
            else
            {
                step = 0.1f; state = "行驶中"; flag = false;
                unityClient.SendMessage("2" + carName);
                Debug.Log(carName + "号小车请求开始");
            }
        }
    }
}
    