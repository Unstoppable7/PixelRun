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

    public TextMeshProUGUI characteName;
    public TextMeshProUGUI characterPrice;

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
            Transform temp = Instantiate(character.prefab, Vector3.zero, Quaternion.Euler(0, 180f, 0)).transform;
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
        change = true;
    }

    public void HiddenChangeCharacter(){
        slide.gameObject.SetActive(false);
        GameManager.sharedInstance.motor.gameObject.SetActive(true);
        change = false;
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
        if(GameManager.sharedInstance.TrySpendGoldAmount(characters[characterTemp].price)){
            characters[characterTemp].price = 0;
            GameManager.sharedInstance.PlayerCharacter = characterTemp;
            GameManager.sharedInstance.motor.ChangePlayerCharacter();
            ChangeButtonName();
        }

        else{
            print("No se pude comprar el personaje: " + characters[characterTemp].name);
        }
    }

    public void ChangeButtonName(){
        characteName.text = characters[characterTemp].name;
        characterPrice.text = "" + characters[characterTemp].price;
    }
}
