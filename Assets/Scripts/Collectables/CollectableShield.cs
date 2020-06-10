using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableShield : MonoBehaviour
{
    //Tiempo de duracion del escudo
    public float time;

    private void OnTriggerEnter(Collider other)
    {
        //Si el jugador agarra el powerup del shield
        if (other.CompareTag("Player"))
        {
            //Se activa un escudo por cierto tiempo
            GameManager.sharedInstance.SetPlayerShield(true, time);
            Destroy(gameObject);            
        }
    }
}
