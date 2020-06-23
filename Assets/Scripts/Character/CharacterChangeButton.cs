using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterChangeButton : MonoBehaviour, IPointerClickHandler
{
    [Range(-1, 1)]
    public int direction;

    public void OnPointerClick(PointerEventData pointerEventData){
        CharacterManager.sharedInstance.ChangeCharacter(direction);
    }
}
