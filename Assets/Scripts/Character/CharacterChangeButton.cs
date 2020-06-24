using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterChangeButton : MonoBehaviour, IPointerClickHandler
{
    //Dirección a la que se cambiara de personaje cuando se presione el boton
    //(-1 izquierda 1 derecha)
    [Range(-1, 1)]
    public int direction;

    public void OnPointerClick(PointerEventData pointerEventData){
        //Cambia al siguiente personaje solo si ya se hizo la animación del cambio
        //del personaje anterior
        if(CharacterManager.sharedInstance.isChange){
            CharacterManager.sharedInstance.ChangeCharacter(direction);
        }
    }
}
