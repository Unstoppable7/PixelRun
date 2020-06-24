using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Guarda la información que tendrá cada personaje
//se puede crear un nuevo personaje dando click derecho en el menu de assets y crear New Character
[CreateAssetMenu(fileName = "Character - ", menuName = "New Character")]
public class Character : ScriptableObject
{
    public new string name;
    public int price;
    public GameObject prefab;
    public AnimationClip idleAnimation;
}
