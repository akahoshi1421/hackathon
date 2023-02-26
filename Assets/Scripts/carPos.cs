using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using WebSocketSharp;
using WebSocketSharp.Net;

public class carPos : MonoBehaviour
{
    // Start is called before the first frame update

    public static WebSocket ws3;
    public Text CountDownText;
    public Text forceText;

    //相手の位置
    GameObject enemyObject;

    //自分の位置
    GameObject myObject;

    public static bool startOK = false;

    public static bool countDownEnd = false;

    float countTimeStart = 0.0f;

    float count = 0.0f;

    int countDown = 5;

    float sendTimer = 0.0f;

    float forceTop = 0.0f;

    [System.Serializable]
    public class CarData
    {
        public float posx = 0.0f;
        public float posy = 0.0f;
        public float posz = 0.0f;
        public float rot = 0.0f;
        public float rotx = 0.0f;
        public float roty = 0.0f;
        public float rotz = 0.0f;
        public int isBrake = 0;
        public int isConnect = 0;
        public string isClear = "";
        public string senderUUID = "";
    }

    CarData enemy;
    
    void Start()
    {
        startOK = false;
        countDownEnd = false;
        ws3 = new WebSocket("ws://" + sceneManager.ORIGIN + "/play/" + matchingManager.roomUUID);

        //敵の車はどれかを確定
        if(matchingManager.roomUUID != matchingManager.playerUUID){
            enemyObject = GameObject.Find("Red Super Car 01");
            myObject = GameObject.Find("Blue Super Car 01");
        }
        else{
            enemyObject = GameObject.Find("Blue Super Car 01");
            myObject = GameObject.Find("Red Super Car 01");
        }

        //車が荒ぶらない様に敵の車のwheelcolliderを削除
        GameObject wheels = enemyObject.transform.Find("Wheels").gameObject;

        GameObject fr = wheels.transform.Find("FR").gameObject;
        GameObject fl = wheels.transform.Find("FL").gameObject;
        GameObject br = wheels.transform.Find("BR").gameObject;
        GameObject bl = wheels.transform.Find("BL").gameObject;

        Destroy(fr.GetComponent<WheelCollider>());
        Destroy(fl.GetComponent<WheelCollider>());
        Destroy(br.GetComponent<WheelCollider>());
        Destroy(bl.GetComponent<WheelCollider>());

        ws3.OnMessage += (sender, e) =>
        {
            enemy = JsonUtility.FromJson<CarData>(e.Data);

            //開始合図
            if(enemy.isConnect != 0){
                startOK = true;
            }
        };

        ws3.Connect();

        forceTop = Time.fixedTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(startOK){
            startOK = false;
            countTimeStart = Time.fixedTime;  
        } 

        //カウントダウン処理
        if(countTimeStart != 0.0f){
            if(Time.fixedTime - countTimeStart > 5f){
                CountDownText.text = "";
                forceText.text = "";
                countDownEnd = true;
            }

            if(!countDownEnd && Time.fixedTime - count > 1.0f){
                count = Time.fixedTime;
                CountDownText.text = countDown.ToString();
                countDown--;
            }
        }

        //マッチングエラーが起きたら強制終了
        if(Time.fixedTime - forceTop > 10f){
            if(countTimeStart == 0.0f){
                SceneManager.LoadScene("TitleScene");
            }
        }

        
        if(countDownEnd){

            if(enemy.senderUUID != matchingManager.playerUUID){
                //相手の処理
                //位置
                enemyObject.transform.position = new Vector3(enemy.posx, enemy.posy, enemy.posz);

                GameObject rWheel = enemyObject.transform.Find("PP_Wheel_FR").gameObject;
                GameObject lWheel = enemyObject.transform.Find("PP_Wheel_FL").gameObject;

                //ホイール回転位置
                rWheel.transform.rotation = Quaternion.Euler(0f, enemy.rot, 0f);
                lWheel.transform.rotation = Quaternion.Euler(0f, enemy.rot, 0f);

                //車体回転位置
                enemyObject.transform.rotation = Quaternion.Euler(enemy.rotx, enemy.roty, enemy.rotz);

                //ブレーキランプを点灯させるか
                GameObject brakeLightR = enemyObject.transform.Find("RBrakeLight").gameObject;
                GameObject brakeLightL = enemyObject.transform.Find("LBrakeLight").gameObject;
                if(enemy.isBrake == 1){
                    brakeLightR.GetComponent<Light>().range = 0.5f;
                    brakeLightL.GetComponent<Light>().range = 0.5f;
                }
                else{
                    brakeLightR.GetComponent<Light>().range = 0.0f;
                    brakeLightL.GetComponent<Light>().range = 0.0f;
                }


                if(enemy.isClear != ""){
                    ws3.Close();
                    SceneManager.LoadScene("LoseScene");
                    return;//一応後ろが実行されない様に
                }
            }


            //自分の位置を相手に送信
            if(Time.fixedTime - sendTimer > 0.1f){
                sendTimer = Time.fixedTime;

                CarData sendMyCar = new CarData();
                sendMyCar.posx = myObject.transform.position.x;
                sendMyCar.posy = myObject.transform.position.y;
                sendMyCar.posz = myObject.transform.position.z;
                sendMyCar.senderUUID = matchingManager.playerUUID;
                
                GameObject MYrWheel = myObject.transform.Find("PP_Wheel_FR").gameObject;

                sendMyCar.rot = MYrWheel.transform.eulerAngles.y;

                sendMyCar.rotx = myObject.transform.eulerAngles.x;
                sendMyCar.roty = myObject.transform.eulerAngles.y;
                sendMyCar.rotz = myObject.transform.eulerAngles.z;

                GameObject brakeLightRObject = enemyObject.transform.Find("RBrakeLight").gameObject;
            
                sendMyCar.isBrake = brakeLightRObject.GetComponent<Light>().range != 0.0f ? 1 : 0;

                ws3.Send(JsonUtility.ToJson(sendMyCar));
            }
            
            if(Input.GetKeyDown(KeyCode.R)){
                myObject.transform.position = new Vector3(myObject.transform.position.x, myObject.transform.position.y + 3f, myObject.transform.position.z);
                myObject.transform.rotation = Quaternion.Euler(0f, myObject.transform.eulerAngles.y, 0f);
            }
            
        }

        
    }

    public static void SendWin(string uuid){
        CarData winCarData = new CarData();
        winCarData.isClear = uuid;

        carPos.ws3.Send(JsonUtility.ToJson(winCarData));
        carPos.ws3.Close();
    }
}
