using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character - ", menuName = "New Character")]
public class CharacterData : ScriptableObject
{
    public new string name;
    public int price;
    public GameObject model;
    public Avatar avatar;
}
