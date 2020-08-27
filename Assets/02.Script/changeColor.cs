using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //미션 클리어
        if (Input.GetKey(KeyCode.Escape)) {
            gameObject.GetComponent<MeshRenderer>().material = Resources.Load("light_green") as Material;
        }
    }
}
