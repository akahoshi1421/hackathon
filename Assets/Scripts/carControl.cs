using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using WebSocketSharp;
using WebSocketSharp.Net;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; //このホイールがエンジンにアタッチされているかどうか
    public bool steering; // このホイールがハンドルの角度を反映しているかどうか
}

public class carControl : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // 個々の車軸の情報
    public float maxMotorTorque; //ホイールに適用可能な最大トルク
    public float maxSteeringAngle; // 適用可能な最大ハンドル角度
    public float maxBrakeTorque;
 
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelBL;
    public WheelCollider wheelBR;
 
    public Transform wheelFLTrans;
    public Transform wheelFRTrans;
    public Transform wheelBLTrans;
    public Transform wheelBRTrans;
    
    float steering = 0.0f;
    float motor = 0.0f;

    public static WebSocket ws4;

    bool carMove = false;

    bool isiPhone = false;

    bool brake;

    [System.Serializable]
    public class iPhoneHandle
    {
        public int accel = 0;
        public int brake = 0;
        public float gyro = 0.0f;
        public int gear = 1;//1が前進、0が後退
    }    

    iPhoneHandle handle;


    float speedCounter = 0.0f;//iPhone加速用
    float wheelCounter = 0.0f;//iPhoneハンドル用

    bool brakeTrue = false;
    bool accelTrue = false;

    GameObject brakeLightR;
    GameObject brakeLightL;
 
    void Start()
    {
        if((matchingManager.playerUUID != matchingManager.roomUUID && name == "Blue Super Car 01") || (matchingManager.playerUUID == matchingManager.roomUUID && name == "Red Super Car 01")){
            if(generateUuid.iPhoneUUID != ""){
                isiPhone = true;
                handle = new iPhoneHandle();

                ws4 = new WebSocket("ws://" + sceneManager.ORIGIN + "/control/" + generateUuid.iPhoneUUID);

                ws4.OnMessage += (sender, e) =>
                {
                    handle = JsonUtility.FromJson<iPhoneHandle>(e.Data);
                    carMove = true;  
                };

                ws4.Connect();
            }

            brakeLightR = GameObject.Find("RBrakeLight");
            brakeLightL = GameObject.Find("LBrakeLight");
        }
    }
 
    // Update is called once per frame
    void Update()
    {
        if((matchingManager.playerUUID != matchingManager.roomUUID && name == "Blue Super Car 01") || (matchingManager.playerUUID == matchingManager.roomUUID && name == "Red Super Car 01")){
            //wheelcolliderの回転速度に合わせてタイヤモデルを回転させる
            wheelFLTrans.Rotate( wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelFRTrans.Rotate( wheelFR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelBLTrans.Rotate( wheelBL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelBRTrans.Rotate( wheelBR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            
            //wheelcolliderの角度に合わせてタイヤモデルを回転する（フロントのみ）
            wheelFLTrans.localEulerAngles = new Vector3(wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle - wheelFLTrans.localEulerAngles.z, wheelFLTrans.localEulerAngles.z);
            wheelFRTrans.localEulerAngles = new Vector3(wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z, wheelFRTrans.localEulerAngles.z);
        }
    }
 
    public void FixedUpdate()
    {
        if((matchingManager.playerUUID != matchingManager.roomUUID && name == "Blue Super Car 01") || (matchingManager.playerUUID == matchingManager.roomUUID && name == "Red Super Car 01")){
            //スタートできるなら(カウントダウン終了後)
            if(carPos.countDownEnd){
                if(isiPhone){
                    //iPhoneを用いた操作
                        if(handle.accel == 1){
                            //前のフレームもtrueなら(iPhoneでボタンを押し続けている)
                            if(accelTrue){
                                if(speedCounter < 1.0f && -1.0f < speedCounter){
                                    if(handle.gear == 1){
                                        speedCounter += 0.1f;
                                    }
                                    else{
                                        speedCounter -= 0.1f;
                                    }

                                    speedCounter = float.Parse(speedCounter.ToString("f2"));

                                    motor = -maxMotorTorque * speedCounter;
                                }

                            }
                            accelTrue = true;
                        }
                        else{
                            accelTrue = false;
                            //エンジンブレーキ
                            if(speedCounter != 0f){
                                if(speedCounter > 0f){
                                    speedCounter -= 0.05f;
                                }
                                else if(speedCounter < 0f){
                                    speedCounter += 0.05f;
                                }
                                speedCounter = float.Parse(speedCounter.ToString("f2"));

                                motor = -maxMotorTorque * speedCounter;
                            }
                        }

                        //ブレーキ操作
                        if(handle.brake == 1){
                            if(brakeTrue){
                                speedCounter = 0f;
                                brake = true;
                            }
                            brakeTrue = true;
                        }
                        else{
                            brakeTrue = false;
                            brake = false;
                        }

                        float gyro = handle.gyro >= 60 ? 120: handle.gyro;
                        gyro = handle.gyro <= -60 ? -60 : gyro;
                        float steer = float.Parse(((-1 * gyro / 30) / 4).ToString("f1"));

                        steering = maxSteeringAngle * steer;
                    
                }
                else{
                    //通常のキー操作
                    motor = -maxMotorTorque * Input.GetAxis("Vertical");
                    steering = maxSteeringAngle * Input.GetAxis("Horizontal");
                    brake = Input.GetKey(KeyCode.Space);
                }


                foreach (AxleInfo axleInfo in axleInfos)
                {
                    if (axleInfo.steering)
                    {
                        axleInfo.leftWheel.steerAngle = steering;
                        axleInfo.rightWheel.steerAngle = steering;
                    }
                    if (axleInfo.motor)
                    {
                        axleInfo.leftWheel.motorTorque = motor;
                        axleInfo.rightWheel.motorTorque = motor;
                    }
                    if(brake){
                        brakeLightR.GetComponent<Light>().intensity = 0.5f;
                        brakeLightL.GetComponent<Light>().intensity = 0.5f;
                        axleInfo.leftWheel.brakeTorque = maxBrakeTorque;
                        axleInfo.rightWheel.brakeTorque = maxBrakeTorque;
                    }
                    else
                    {
                        brakeLightR.GetComponent<Light>().intensity = 0.0f;
                        brakeLightL.GetComponent<Light>().intensity = 0.0f;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.brakeTorque = 0;
                    }
                }
            }
        }
        
    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.name == "Goal"){
            if(name == "Blue Super Car 01"){
                if(matchingManager.playerUUID != matchingManager.roomUUID){
                    carPos.SendWin(matchingManager.playerUUID);
                    closeCarControlWS();
                    SceneManager.LoadScene("WinScene");
                }
            }
            else if(name == "Red Super Car 01"){
                if(matchingManager.playerUUID == matchingManager.roomUUID){
                    carPos.SendWin(matchingManager.playerUUID);
                    closeCarControlWS();
                    SceneManager.LoadScene("WinScene");
                }
            }
        }
    }

    public static void closeCarControlWS(){
        carControl.ws4.Close();
    }
}
