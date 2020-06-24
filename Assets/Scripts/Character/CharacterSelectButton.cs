using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour, IPointerClickHandler
{
    //Cuando se da click en el boton de seleccionar personaje
    //Hace el cambio de personaje si tiene dinero suficiente
    public void OnPointerClick(PointerEventData pointerEventData){
        CharacterManager.sharedInstance.SelectCharacter();
    }
}
