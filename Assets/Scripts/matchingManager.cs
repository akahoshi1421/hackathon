using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using UnityEngine.SceneManagement;

using WebSocketSharp;
using WebSocketSharp.Net;

public class matchingManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static string playerUUID = "";
    public static string roomUUID = "";

    private WebSocket ws2;

    bool loadOK = false;

    [System.Serializable]
    public class OKUsers
    {
        public string p1;
        public string p2;
    }

    void Start()
    {
        playerUUID = Guid.NewGuid().ToString();

        ws2 = new WebSocket("ws://" + sceneManager.ORIGIN + "/matching/matching");

        ws2.OnMessage += (sender, e) =>
        {
            OKUsers users = JsonUtility.FromJson<OKUsers>(e.Data);
            if(users.p1 == playerUUID || users.p2 == playerUUID){
                roomUUID = users.p1;
                loadOK = true;
            }
        };

        ws2.Connect();
        
        ws2.Send(playerUUID);
    }

    // Update is called once per frame
    void Update()
    {
        if(loadOK){
            loadOK = false;
            ws2.Close();
            SceneManager.LoadScene("GameScene");
        }
    }
}
