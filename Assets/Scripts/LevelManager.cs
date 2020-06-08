using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //Singleton
    public static LevelManager sharedInstance { set; get; }

    [SerializeField]
    //Lista total de tiles que generaremos aleatoriamente
    List<GameObject> allTiles = new List<GameObject>();

    //Referencia al transform del personaje
    Transform playerTransform;

    //Posicion en el eje Z del spawn de los tiles
    float spawnZ = 0.0f;

    //Tamaño en largo de los tiles
    float tileLenght = 6f;

    //Cantidad de tiles en pantalla
    int amountTiles = 20;

    //Zona segura donde podremos eliminar un tile porque ya está fuera de
    //pantalla
    float safeZone = 15f;

    //Index del ultimo tile creado
    int lastTileIndex;

    //Lista de tiles actualmente en pantalla
    //la inicializamos en un proceso mas adelante
    List<GameObject> activeTiles;

    // Start is called before the first frame update
    void Start()
    {
        //Referenciamos el transform del personaje
        playerTransform = GameObject.FindGameObjectWithTag("Player")
                            .transform;

        //Inicializamos la lista de los tiles activos
        activeTiles = new List<GameObject>();

        //Cargamos los tiles iniciales
        for (int i = 0; i < amountTiles; i++)
        {
            //Nos aseguramos que ni el primero ni el segundo de los tiles
            //tenga un obstaculo en el centro, por lo que le forzamos a poner 
            //el tile que se encuentra en la posicion 0 (debemos asegurarnos 
            //que este no tiene obstaculos en el centro)
            if (i < 2)
            {
                SpawnTile(0);

            }
            else //Despues del 2do tile generamos aleatorios
            {
                SpawnTile();
            }
           
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Si la posicion del jugador en z menos la zona segura es mayor a
        //que la posicion que hacemos spawn los tiles menos la multiplicacion
        //entre la cantidad de tiles en pantalla y el largo de c/u de ellos
        //creamos un nuevo tile
        if (playerTransform.position.z - safeZone >
            (spawnZ - amountTiles * tileLenght))
        {
            //Creamos un nuevo tile en nuestro camino
            SpawnTile();
            //Eliminamos el ultimo tile
            DeleteLastTile();
        }
    }

    //Metodo para spawnear o aparecer un tile en pantalla
    //prefabIndex: Nos indica que tile de la lista vamos a spawnear
    void SpawnTile(int prefabIndex = -1)
    {
        //Objeto que tendrá el tile instanciado
        GameObject tile;

        //Instanciamos en el game object el tile de la lista correspodiente
        //al index indicado
        if(prefabIndex == -1)
        {
            //Si se llama sin especificar, por defecto busco uno aleatorio
            tile = Instantiate(allTiles[RandomTileIndex()]) as GameObject;
        }
        else
        {
            tile = Instantiate(allTiles[prefabIndex]) as GameObject;
        }

        //Hacemos padre del tile a este objeto LevelManager
        tile.transform.SetParent(transform);

        //Posicionamos este nuevo tile en el lugar indicado por la var spawnZ
        //Lo multiplicamos por forward ya que este vector es 1 en z
        tile.transform.position = Vector3.forward * spawnZ;

        //Sumamos al valor de posicion spawnZ el tamaño del tile
        spawnZ += tileLenght;

        //Añadimos este nuevo tile a la lista de tiles activos
        activeTiles.Add(tile);
    }

    //Metodo para eliminar los tiles que van quedando atras
    void DeleteLastTile()
    {
        //Destruimos el primer tile de la lista
        Destroy(activeTiles[0]);
        //Removemos de la lista el primero espacio para que se reordene
        activeTiles.RemoveAt(0);
    }

    //Metodo para generar un index aleatorio de nuestros tiles
    int RandomTileIndex()
    {
        //Si nuestra lista de tiles totales está vacia o tiene solo 1
        //no tenemos de donde seleccionar aleatoriamente
        if (allTiles.Count <= 1)
        {
            return 0;
        }
        //Igualamos el ultimo tile creado para evitar que salgan dos seguidos
        int randomIndex = lastTileIndex;
        //Mientras sean iguales el ultimo con el random, buscame otro
        while(randomIndex == lastTileIndex)
        {
            randomIndex = Random.Range(0, allTiles.Count);
        }
        //Reescribimos el index del ultimo tile creado
        lastTileIndex = randomIndex;

        //Retornamos el index aleatorio
        return randomIndex;
    }

}
