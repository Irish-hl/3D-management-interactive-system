using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carScene : MonoBehaviour {
    // 设定绑定目标 ，在摄像机的属性板块中设置
    public Transform target;  
    // 设置距离目标的距离  
    float distance = 5.0f;  
    // 设置距离目标的高度  
    float height = 13.0f;  
    //转动的速度  
    float heightDamping = 2.0f;  
	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		// Early out if we don't have a target  
        if (!target)
             return;
  
    // 想要的高度  
    float wantedHeight = target.position.y + height;  
   
    //当前的高度  
    float currentHeight = transform.position.y;  
  
    //从当前的高度到想到的高度  
    currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);  
  
  
    // 设置于目标的Y轴的距离  
    //transform.position = target.position;//先让目标的位置和摄像机的位置一致  
    //transform.position += Vector3.right * distance;//改变摄像机的X轴  
  
    // 设置摄像机的位置  
    transform.position = new Vector3( target.position.x, currentHeight, target.position.z);  
    }  
}

