using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    //Velocidad de giro del collectable
    public float velocity;

    // Update is called once per frame
    void Update()
    {
        //Hacemos girar el collectable
        transform.Rotate(Vector3.up, Time.deltaTime * velocity, Space.World);
    }
}
