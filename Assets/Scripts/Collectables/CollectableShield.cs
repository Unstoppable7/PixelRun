using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableShield : MonoBehaviour
{
    //Multiplicador que aumentará la duración del escudo
    public static int multiplier = 1;

    //Tiempo de duracion base del escudo
    private static float baseTime  = 5f;

    private void OnTriggerEnter(Collider other)
    {
        //Si el jugador agarra el powerup del shield
        if (other.CompareTag("Player"))
        {
            //Se activa un escudo por cierto tiempo
            GameManager.sharedInstance.SetPlayerShield(true, baseTime * multiplier);
            Destroy(gameObject);        
        }
    }
}
