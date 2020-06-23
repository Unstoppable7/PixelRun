using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    //Singleton
    public static CharacterManager sharedInstance { set; get; }

    //Lista de personajes disponibles
    public Character[] characters;

    [HideInInspector]
    public bool change;

    Transform slide;

    float characterDistance = 2f;

    [HideInInspector]
    public int characterTemp;

    public TextMeshProUGUI characterButtonText;

    void Awake()
    {
        if (!sharedInstance)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        slide = transform.Find("Slide");

        CreateCharacters();
    }

    void Update(){
        ChangeCharacterPosition();
    }

    void CreateCharacters(){
        Vector3 targetPosition = Vector3.zero;

        foreach (Character character in characters){
            Transform temp = Instantiate(character.model, Vector3.zero, Quaternion.Euler(0, 180f, 0)).transform;
            temp.SetParent(slide);
            temp.localPosition = targetPosition; 
            targetPosition += Vector3.right * characterDistance;
        }
    }

    public void DisplayChangeCharacter(){
        characterTemp = GameManager.sharedInstance.PlayerCharacter;
        GameManager.sharedInstance.motor.gameObject.SetActive(false);

        slide.position = -(Vector3.right * characterDistance * characterTemp);
        ChangeButtonName();
        slide.gameObject.SetActive(true);
    }

    public void HiddenChangeCharacter(){
        slide.gameObject.SetActive(false);
        GameManager.sharedInstance.motor.gameObject.SetActive(true);
    }

    public void ChangeCharacterPosition(){
        slide.position = Vector3.Lerp(slide.position, -(Vector3.right * characterDistance * characterTemp), 5f * Time.deltaTime);
    }

    public void ChangeCharacter(int direction){
        characterTemp += direction;
        characterTemp = Mathf.Clamp(characterTemp, 0, characters.Length-1);

        ChangeButtonName();
    }

    public void SelectCharacter(){
        GameManager.sharedInstance.PlayerCharacter = characterTemp;
        GameManager.sharedInstance.motor.ChangePlayerCharacter();
    }

    public void ChangeButtonName(){
        characterButtonText.text = characters[characterTemp].name;
    }
}
