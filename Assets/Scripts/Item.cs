using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item {

    public enum ItemType {
        Attack,
        Shield
    }

    public static int GetCost(ItemType itemType) {
        switch (itemType) {
        default:
        case ItemType.Attack:        return 50;
        case ItemType.Shield:          return 20;
        }
    }

    public static Sprite GetSprite(ItemType itemType) {
        switch (itemType) {
        default:
        case ItemType.Attack:    return GameAssets.i.s_Attack;
        case ItemType.Shield:      return GameAssets.i.s_Shield;
        }
    }

}
