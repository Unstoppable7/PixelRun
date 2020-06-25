using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Este script está en el StorePanel del canvas
//ya que siempre se activa cuando se da click en abrir la tienda
//asi se comprueba si la ventana de cambio de personaje ya estaba abierta antes
public class CharacterPanelReset : MonoBehaviour
{
    public GameObject characterPage;

    //Se activa siempre que se abre la pagina para cambiar de personaje
    private void OnEnable(){
        if(characterPage.activeSelf){
            //Muestra el cambio de personajes siempre que se de en la opción de cambiar personaje
            //y se active la pagina en la interfaz
            CharacterManager.sharedInstance.DisplayChangeCharacter();
        }
    }
}
