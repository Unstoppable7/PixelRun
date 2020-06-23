using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPanelReset : MonoBehaviour
{
    public GameObject characterPage;

    private void OnEnable(){
        if(characterPage.activeSelf){
            CharacterManager.sharedInstance.DisplayChangeCharacter();
        }
    }
}
