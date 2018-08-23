using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {
    public float sensitivityMouse = 2f;
    public float sensitivetyMouseWheel = 10f;
    public float moveStep = 1000f;
	// Use this for initialization
	void Start () {
		
	}
	// Update is called once per frame
	void Update () {
        //滚轮实现镜头缩进和拉远
        if (Input.GetAxis("Mouse ScrollWheel") != 0)//鼠标滚轮相应函数，向前滚返回正数，向后滚返回负数
        {
            this.GetComponent<Camera>().fieldOfView = this.GetComponent<Camera>().fieldOfView - Input.GetAxis("Mouse ScrollWheel") * sensitivetyMouseWheel;
        }
        //按着鼠标右键实现视角转动
        if (Input.GetMouseButton(1))//button值设定为 0对应左键 ， 1对应右键 ， 2对应中键。
        {
            transform.Rotate(-Input.GetAxis("Mouse Y") * sensitivityMouse, Input.GetAxis("Mouse X") * sensitivityMouse, 0);
        }
        //按着鼠标左键实现视角移动
        if (Input.GetMouseButton(0))
        {
            //获取鼠标的x和y的值，乘以速度和Time.deltaTime是因为这个可以是运动起来更平滑
            if(!(Input.GetAxis("Mouse X")>160&&Input.GetAxis("Mouse Y")>280))
            {
                float h = Input.GetAxis("Mouse X") * moveStep * Time.deltaTime;
                float v = Input.GetAxis("Mouse Y") * moveStep * Time.deltaTime;
                // 设置当前摄像机移动，y轴并不改变    
                // 需要摄像机按照世界坐标移动，而不是按照它自身的坐标移动，所以加上Spance.World 
                this.transform.Translate(-h, 0, -v, Space.World);
            }
        }
    }
}
