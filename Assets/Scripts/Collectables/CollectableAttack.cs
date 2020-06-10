using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //Si el jugador agarra el powerup de atacar
        if (other.CompareTag("Player"))
        {
            //Habilita la opcion de atacar con W
            GameManager.sharedInstance.SetPlayerAttack(true);
            Destroy(gameObject);            
        }
    }
}
