using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateDot : MonoBehaviour
{
    // Start is called before the first frame update
    public Text matchingTitle;
    double time = 0.0f;
    int dotCnt = 1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.fixedTime - time > 0.4f){
            time = Time.fixedTime;
            string dot = "";
            switch(dotCnt){
                case 1:
                    dot = ".";
                    break;
                case 2:
                    dot = "..";
                    break;
                case 3:
                    dot = "...";
                    break;
            }
            if(dotCnt == 3) dotCnt = 0;
            dotCnt++;

            matchingTitle.text = "マッチング中" + dot;
        }
    }
}
