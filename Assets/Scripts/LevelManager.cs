using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //Singleton
    public static LevelManager sharedInstance { set; get; }

    [SerializeField]
    //Guarda los tiles que estarán en linea recta
    List<GameObject> straightTiles = new List<GameObject>();

    //Guarda los tiles que giran a la derecha el mapa
    [SerializeField]
    List<GameObject> rightTiles = new List<GameObject>();

    //Guarda los tiles que giran a la izquierda el mapa
    [SerializeField]
    List<GameObject> leftTiles = new List<GameObject>();

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

    //Guarda el lado hacia donde se instancian los nuevos tiles
    // -1 izquierda ---- 0 recto ---- +1 derecha
    int side = 0;

    //Lados al que puede girar la creacion de tiles -1 izquierda ---- 1 derecha
    //se elige aleatoreamente la dirección si es izquierda o derecha
    readonly int[] DIRECTIONS = {-1, 1};

    //Guarda la direccion que tendran que seguir los tiles
    Transform tileDirection;

    //Minimo valor que tendrá la siguiente curva
    int minNextCurve = 6;

    //Maximo valor que tendrá la siguiente curva
    int maxNextCurve = 20;

    //Cantidad de tiles desde la ultima curva
    int tilesCount = 0;

    //Guarda la posicion del ultimo tile que hace que el jugador
    //sea inmune se asigna en el metodo Inmunity del jugador
    int lastImmunityTile;

    //Cantidad de tiles despues de la ultima curva
    //se legie aleatoreo entre los valores minNextCurve y maxNextCurve
    int nextCurve;

    void Awake(){
        if (!sharedInstance)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Referenciamos el transform del personaje
        playerTransform = GameObject.FindGameObjectWithTag("Player")
                            .transform;
        
        //Referencia al transform que está en la escena dentro del LevelManager object
        tileDirection = GameObject.Find("TileDirection").transform;

        //Inicializamos la lista de los tiles activos
        activeTiles = new List<GameObject>();

        //Se crea la primera curva del mapa
        nextCurve = Random.Range(minNextCurve, maxNextCurve);

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

        if (Vector3.Distance(playerTransform.position, activeTiles[0].transform.position) > safeZone)
        {
            //Creamos un nuevo tile en nuestro camino
            SpawnTile();
            //Eliminamos el ultimo tile
            DeleteLastTile();

            //Si el tile hasta donde el jugador es inmune es mayor a cero
            if(lastImmunityTile > 0){
                //Se va restando cuando se elimina un tile anterior ya que tambien se va
                //disminuyendo
                lastImmunityTile--;
            }

            else{
                //Cuando ya se haya eliminado el ultimo tile hasta donde el jugador es inmune
                //volvemos false la variable inmune del jugador
                GameManager.sharedInstance.SetImmunePlayer(false);
            }
        }
    }

    //Metodo para spawnear o aparecer un tile en pantalla
    //prefabIndex: Nos indica que tile de la lista vamos a spawnear
    public void SpawnTile(int prefabIndex = -1)
    {
        //Objeto que tendrá el tile instanciado
        GameObject tile;

        //Instanciamos en el game object el tile de la lista correspodiente
        //al index indicado
        if(prefabIndex == -1)
        {
            //Si se llama sin especificar, por defecto busco uno aleatorio
            //Si es momento de crear la curva
            if(nextCurve - tilesCount == 0){
                //Se elige una direccion aleatorea
                side = DIRECTIONS[Random.Range(0, DIRECTIONS.Length)];

                //Se elige la cantidad de tiles de la siguiente curva
                nextCurve = Random.Range(minNextCurve, maxNextCurve);

                //Se asigna a cero la cantidad de tiles despues de la curva ya que se crea una nueva curva
                tilesCount = 0;
            }

            else{
                side = 0;
            }

            //Si la direccion va en linea recta
            //instancia los tiles de linea recta
            if(side == 0){
                tile = Instantiate(straightTiles[RandomTileIndex(straightTiles.Count)]) as GameObject;
            }

            //Si la direccion cambia a la derecha
            //instancia los tiles de derecha
            else if(side == 1){
                tile = Instantiate(rightTiles[RandomTileIndex(rightTiles.Count)]) as GameObject;
            }

            //Si la direccion cambia a la izquierda
            //instancia los tiles de izquierda
            else{
                tile = Instantiate(leftTiles[RandomTileIndex(leftTiles.Count)]) as GameObject;
            }
        }
        else
        {
            tile = Instantiate(straightTiles[prefabIndex]) as GameObject;
        }

        //Hacemos padre del tile a este objeto LevelManager
        tile.transform.SetParent(transform);

        //Posicionamos este nuevo tile en el lugar indicado por la var spawnZ
        //Lo multiplicamos por forward ya que este vector es 1 en z
        //Se crean los primeros dos tiles en linea recta
        if(activeTiles.Count < 2){
            tile.transform.position = Vector3.forward * spawnZ;
        }

        else{
            //Se posiciona el nuevo tile tomando como referencia la posicion del tile anterior
            //y se usa la direccion local de tileDirecion (tileDirecion.forward) para que se siga colocando en linea recta
            //dependiendo de hacia donde esté rotado ese transform, como se usa como referencia el tile anterior obteniendo
            //su posicion solo es necesario agregarle el tamaño del tile para que se coloque en la nueva posicion y hacia la nueva direccion
            tile.transform.position = activeTiles[activeTiles.Count-1].transform.position + tileDirection.forward * tileLenght;

            //Se cambia la direccion hacia adelande local de tileDirecion para que el forward se actualize y sea
            //la nueva direccion de los proximos tiles en linea recta
            //se actualiza despues de la posicion para que el siguiente tile tenga en cuenta el giro y no el actual
            tileDirection.rotation *= Quaternion.Euler(Vector3.up * side * 90);
            
            //Si se mantiene en linea recta sigue la direccion de rotacion local
            if(side == 0){
                tile.transform.rotation = tileDirection.rotation;
            }

            //Si hay un giro en el mapa, el tile de giro mantiene la rotación normal (igual que el anterior)
            //ya que este no girará si no los de linea recta que vienen despues del giro
            else{
                tile.transform.rotation = activeTiles[activeTiles.Count-1].transform.rotation;
            }
        }

        //Sumamos al valor de posicion spawnZ el tamaño del tile
        spawnZ += tileLenght;

        //Añadimos este nuevo tile a la lista de tiles activos
        activeTiles.Add(tile);

        //Se aumenta la cantidad de tiles creados despues de una curva
        tilesCount++;
    }

    //Metodo para eliminar los tiles que van quedando atras
    public void DeleteLastTile()
    {
        //Destruimos el primer tile de la lista
        Destroy(activeTiles[0]);
        //Removemos de la lista el primero espacio para que se reordene
        activeTiles.RemoveAt(0);
    }

    //Metodo para generar un index aleatorio de nuestros tiles
    //recibe como parametro la cantidad de items de cada lista
    int RandomTileIndex(int listCount)
    {
        //Si nuestra lista de tiles totales está vacia o tiene solo 1
        //no tenemos de donde seleccionar aleatoriamente
        if (listCount <= 1)
        {
            return 0;
        }

        //Igualamos el ultimo tile creado para evitar que salgan dos seguidos
        int randomIndex = lastTileIndex;

        //Mientras sean iguales el ultimo con el random, buscame otro
        while(randomIndex == lastTileIndex)
        {
            randomIndex = Random.Range(0, listCount);
        }

        //Reescribimos el index del ultimo tile creado
        lastTileIndex = randomIndex;

        //Retornamos el index aleatorio
        return randomIndex;
    }

    //Guarda la ultima posicion del tile hasta donde el jugador será inmune
    public void SetLastImmunityTile(){
        lastImmunityTile = activeTiles.Count-1;
    }
}
