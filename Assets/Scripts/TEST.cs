using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject parent;
    public GameObject camera;
    void Start()
    {
        if(matchingManager.playerUUID != matchingManager.roomUUID){
            camera.transform.parent = parent.transform;
            camera.transform.localPosition = new Vector3(0f, 2f, -6.3f);
        }

    }

    // Update is called once per frame
    void Update()
    {
    }
}
