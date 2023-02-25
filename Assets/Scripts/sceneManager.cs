using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static string ORIGIN = "192.168.1.54:8000";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void goiPhone(){
        SceneManager.LoadScene("iPhoneConnect");
    }

    public void goMatching(){
        SceneManager.LoadScene("MatchingScene");
    }

    public void goTop(){
        SceneManager.LoadScene("titleScene");
    }
}
