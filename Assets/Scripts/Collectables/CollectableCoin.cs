using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableCoin : MonoBehaviour
{
    //Valor del collectable
    [SerializeField]
    private int collectableAmount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.sharedInstance.GetCollectable(collectableAmount);
            Destroy(gameObject);            
        }
    }
}
