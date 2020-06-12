using UnityEngine;
using System.Reflection;

public class GameAssets : MonoBehaviour {

    private static GameAssets _i;

    public static GameAssets i {
        get {
            if (_i == null) _i = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            return _i;
        }
    }

    //Sprites
    public Sprite s_Attack;
    public Sprite s_Shield;

}
