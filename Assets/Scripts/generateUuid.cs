using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics;

using WebSocketSharp;
using WebSocketSharp.Net;


public class generateUuid : MonoBehaviour
{
    // Start is called before the first frame update
    public static string iPhoneUUID = "";
    public Text showUUID;
    public Text connectOK;
    private WebSocket ws;

    bool iPhoneOKFlag = false;

    void Start()
    {
        iPhoneUUID = Guid.NewGuid().ToString().Substring(0, 5);
        showUUID.text = iPhoneUUID;

        ws = new WebSocket("ws://localhost:8000/control/" + iPhoneUUID);

        ws.OnMessage += (sender, e) =>
        {
            if(e.Data == "iPhoneOK"){
                iPhoneOKFlag = true;
            }
        };

        ws.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if(iPhoneOKFlag){
            iPhoneOKFlag = false;
            ws.Send("UnityOK");//3ウェイハンドシェイク用
            connectOK.text = "接続を確認しました。";
        }
    }

    public void goTop(){
        ws.Close();
        SceneManager.LoadScene("titleScene");
    }
}
