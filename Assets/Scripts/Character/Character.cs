using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character - ", menuName = "New Character")]
public class Character : ScriptableObject
{
    public new string name;
    public int price;
    public GameObject model;
    public Avatar avatar;
}
