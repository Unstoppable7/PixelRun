using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[System.Serializable]
public class MapPart{

    //Guarda el tile de transicion de zona o el tile inicial de cada parte
    public GameObject initTile;

    //Guarda los tiles que tienes obstaculos   
    public List<GameObject> obstacleTiles = new List<GameObject>();

    //Guarda los tiles seguros, tiles que no tienen obstaculos
    public List<GameObject> safeTiles = new List<GameObject>();

    //Guarda los tiles de la recta prediseñada
    public List<GameObject> straightPrefab = new List<GameObject>();

    //Guarda los tiles que giran a la izquierda el mapa
    public List<GameObject> rightTiles = new List<GameObject>();
    
    //Guarda los tiles que giran a la derecha el mapa
    public List<GameObject> leftTiles = new List<GameObject>();

    //Variable aleatoria que contendrá la cantidad de tiles que
    //tendrá cada parte del mapa cada vez que empieza a jugar
    //se inicializa en el start
    [HideInInspector]
    public int tilesCount;

    //Minimo de tiles que puede tener la parte del mapa
    [Range(10, 500)]
    public int minTilesCount;

    //Maximo de tiles que puede tener la parte del mapa
    [Range(10, 500)]
    public int maxTilesCount;

    //Asigna la cantidad aleatoria de tiles que tendrá la parte del mapa
    public void RandomTilesCount(){
        tilesCount = Random.Range(minTilesCount, maxTilesCount);
    }
}

public class LevelManager : MonoBehaviour
{
    //Singleton
    public static LevelManager sharedInstance { set; get; }

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
    float safeZone = 20f;

    //Index del ultimo tile creado
    int lastTileIndex;

    //Lista de tiles actualmente en pantalla
    //la inicializamos en un proceso mas adelante
    List<GameObject> activeTiles;

    //Guarda el lado hacia donde se instancian los nuevos tiles
    // -1 izquierda ---- 0 recto ---- +1 derecha
    int side = 0;

    //Lados al que puede girar la creacion de tiles -1 izquierda ---- 1 derecha
    //se elige aleatoriamente la dirección si es izquierda o derecha
    readonly int[] DIRECTIONS = {-1, 1};

    //Guarda la direccion que tendran que seguir los tiles
    Transform tileDirection;

    //Posicion inicial del objeto tileDirection (al reiniciar el nivel retomarlo)
    GameObject initPositionTileDirection;

    //Minimo valor que tendrá la siguiente curva
    int minNextCurve = 10;

    //Maximo valor que tendrá la siguiente curva
    int maxNextCurve = 13;

    //Cantidad de tiles desde la ultima curva
    int tilesCount = 0;

    //Guarda la posicion del ultimo tile que hace que el jugador
    //sea inmune se asigna en el metodo Inmunity del jugador
    //cuando se destruya ese ultimo tile de inmunidad la variable
    //inmune de jugador se vuelve false
    int lastImmunityTile;

    //Cantidad de tiles despues de la ultima curva
    //se elige aleatorio entre los valores minNextCurve y maxNextCurve
    int nextCurve;

    #region Variables para la creación de partes del mapa
    
    //Lista con las partes del mapa
    [SerializeField]
    List<MapPart> mapParts;

    //La parte del mapa actual (la que se estará creando)
    MapPart currentPart;

    //Cantidad de tiles que se han creado por cada parte del mapa
    int partTilesCount = 0;

    #endregion

    //Lista con las posiciones donde van a ir ubicados los obstaculos en la recta
    //en cada recta se toma el primer tile como el de la curva, siendo 0
    List<int> indexObstacle;

    //Bandera para manejar cuando se va a generar una recta prediseñada en el mapa
    bool prefabPart = true;

    //Variable que será manejada dependiendo de la dificultad del juego
    //inicialmente tomara los 11 primeros tiles de obstaculos del array
    //al pasar a dificultad media toma la totalidad del array ya que
    //luego del 11 se encuentran mas dificiles
    //todos los arrays de obstaculos de cada parte del mundo debe ser de 
    //11 tiles o si no se debe modificar esta variable
    //usamos 10 porque el random es inclusivo y no existe objeto en la
    //posicion 11
    int maxIndexObstacleTile = 10;

    //Temporizador para manejar el OnFastCurves
    Timer aTimer = new System.Timers.Timer();

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
        //Configuracion del temporizador, durará 15seg y al terminar
        //llama al metodo OnTimedEvent
        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        aTimer.Interval = 15000;

        //Referenciamos el transform del personaje
        playerTransform = GameObject.FindGameObjectWithTag("Player")
                            .transform;
        
        //Referencia al transform que está en la escena dentro del LevelManager object
        tileDirection = GameObject.Find("TileDirection").transform;

        //Guardamos la posicion y rotacion inicial del tileDirection
        initPositionTileDirection = new GameObject();
        initPositionTileDirection.transform.position = tileDirection.position;
        initPositionTileDirection.transform.rotation = tileDirection.rotation;

        //Inicializamos la lista de los tiles activos
        activeTiles = new List<GameObject>();

        //Generamos los tiles iniciales
        InitSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        //Si el jugador no está corriendo que no haga nada
        if(!GameManager.sharedInstance.IsPlayerRunning()){
            return;
        }

        //Si la posicion del jugador en z menos la zona segura es mayor a
        //que la posicion que hacemos spawn los tiles menos la multiplicacion
        //entre la cantidad de tiles en pantalla y el largo de c/u de ellos
        //creamos un nuevo tile

        if (Vector3.Distance(playerTransform.position, activeTiles[0].transform.position) > safeZone)
        {
            //Si la dificultad actual es facil
            if(GameManager.sharedInstance.currentDifficulty == Difficulty.easy)
            {
                //Creamos un nuevo tile simple en nuestro camino
                SpawnTile();
            }
            else
            {//Si no, si es media o alta

                //Llamamos a modo dificultad media
                MediumDifficultyMode();

                //Si la bandera de recta prediseñada está activa
                if (prefabPart)
                {
                    //Iniciamos la logica de generar la recta prediseñada
                    SpawnTile(false);
                }
                else
                {
                    //Creamos un tile simple
                    SpawnTile();

                    //Numero aleatorio que manejará la probabilidad de que
                    //se creen rectas prediseñadas
                    int rnd = Random.Range(0, 25);
                    if(rnd == 7)
                    {
                        prefabPart = true;
                    }
                }

                //Si la dificultad actual es dificil
                if (GameManager.sharedInstance.currentDifficulty == Difficulty.hard)
                {
                    //Numero aleatorio que manejará la probabilidad de que
                    //se creen curvas rapidas, iniciamos el timer
                    //para que se generen las curvas por al menos 15sec
                    //creando aproximadamente entre 4/5 curvas seguidas
                    //o menos
                    int rnd = Random.Range(0, 15);
                    if (rnd ==7)
                    {
                        OnFastCurves(true);
                        aTimer.Enabled = true;
                    }
                }
            }

            //Eliminamos el ultimo tile
            DeleteLastTile();

            //Si el tile hasta donde el jugador es inmune es mayor a 4
            //mayor a 4 para tomar en cuenta los tiles que no
            //han sido destruidos aun
            if(lastImmunityTile > 4){
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

    //Metodo que se llamará al acabar el timer
    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        aTimer.Enabled = false;
        OnFastCurves(false);
        Debug.Log("Entra");

    }

    //Spawn inicial de los tiles 
    public void InitSpawn()
    {
        //Asignamos valor inicial a las variables
        spawnZ = 0.0f;
        tilesCount = 0;        
        tileDirection.position = initPositionTileDirection.transform.position;
        tileDirection.rotation = initPositionTileDirection.transform.rotation;
        partTilesCount = 0;

        //Inicializamos la cantidad de tiles que tendrá cada parte
        foreach (MapPart part in mapParts)
        {
            part.RandomTilesCount();
        }

        //La parte que se creará será la primera al inicio
        currentPart = mapParts[0];
        GameManager.sharedInstance.camera.initPosition = 0;

        //Se crea la primera curva del mapa
        nextCurve = Random.Range(minNextCurve, maxNextCurve);

        //Damos memoria a la lista
        indexObstacle = new List<int>();

        //Generamos las posiciones aleatorias donde iran ubicados los tiles
        //de los obstaculos
        GenerateIndexOfObstacles(nextCurve);

        //Cargamos los tiles iniciales
        for (int i = 0; i < amountTiles; i++)
        {
            //Generamos un tile   
            SpawnTile();
        }
    }

    //Metodo para spawnear o aparecer un tile en pantalla
    //bool random: Nos genera la logica si se crean tiles
    //aleatorios o una secuencia prediseñada
    public void SpawnTile(bool random = true)
    {    
        //Objeto que tendrá el tile instanciado
        GameObject tile;

        //Si es momento de crear la curva
        if (nextCurve - tilesCount == 0){

            //Si en la parte o zona actual se va a generar el primer tile
            //es decir la transicion entre partes, no realizo la curva aqui
            //sino en el siguiente tile
            if(currentPart.tilesCount < 1){

                //Atrasamos la curva 1 tile mas para que se pueda poner el prefab
                //de inicio de la parte siguiente del mundo
                nextCurve++;
                side = 0;
            }else{

                //Se elige una direccion aleatoria
                side = DIRECTIONS[Random.Range(0, DIRECTIONS.Length)];

                //Se asigna a cero la cantidad de tiles despues de la curva ya que se crea una nueva curva
                tilesCount = 0;

                //Si no se van a tomar tiles aleatorios si no prediseñados
                if (!random)
                {
                    //La proxima curva se va a generar en 15 tiles
                    nextCurve = 15;
                }
                else //Si no, generamos la curva de forma aleatoria al igual que los tiles
                {
                    //Se elige la cantidad de tiles de la siguiente curva
                    nextCurve = Random.Range(minNextCurve, maxNextCurve);

                    //Generamos las posiciones aleatorias donde iran ubicados los tiles
                    //de los obstaculos
                    GenerateIndexOfObstacles(nextCurve);

                }
            }
        }else{
            side = 0;
        }

        //Si la direccion va en linea recta
        //instancia los tiles de linea recta
        if (side == 0){

            //El primer tile de la nueva parte del mapa será el prefab en la posición 0 del array
            //Solo se instancia una vez este prefab durante toda la creción de la nueva parte
            //Ya que este le va a indicar al usuario que está entrando a la nueva parte del mapa
            if(partTilesCount < 1){
                tile = Instantiate(currentPart.initTile);

                //Si no, si la siguiente curva es menor a 15 tiles pasamos a la logica de generacion
                //de tiles en forma aleatoria
            }else if (nextCurve  < 15){
                
                if(partTilesCount < 2){
                    tile = Instantiate(currentPart.safeTiles[RandomTileIndex(0, currentPart.safeTiles.Count)]) as GameObject;
                }else
                //Los dos tiles despues de la curva y la entrada a la nueva parte serán seguros en el centro
                if (tilesCount < 3){
                
                    //Se instancia el de la posicion 1 ya que el de la posición 0 es la entrada a la nueva parte
                    //(Solo queremos que se instancie la entrada a esa parte una sola vez)
                    tile = Instantiate(currentPart.safeTiles[RandomTileIndex(0, currentPart.safeTiles.Count)]) as GameObject;
                }else{

                    //Si la lista no está vacia
                    if (indexObstacle.Count != 0)
                    {

                        //Si el numero del tile actual es igual al que debo ponerle un obstaculo
                        if (tilesCount == indexObstacle[0])
                        {
                            //Instancio el obstaculo
                            tile = Instantiate(currentPart.obstacleTiles[RandomTileIndex(0, maxIndexObstacleTile)]) as GameObject;
                            //Remuevo esa posicion de la lista y se reordena
                            indexObstacle.RemoveAt(0);
                        }
                        else
                        {
                            //Si no, sigo poniendo tiles seguros
                            tile = Instantiate(currentPart.safeTiles[RandomTileIndex(0, currentPart.safeTiles.Count)]) as GameObject;
                        }
                    }else{
                        //Si no, sigo poniendo tiles seguros
                        tile = Instantiate(currentPart.safeTiles[RandomTileIndex(0, currentPart.safeTiles.Count)]) as GameObject;
                    }
                }
            
            } 
            //Si no, si la siguiente curva es dentro de 15 tiles pasamos a la logica de 
            //generacion de recta prediseñada
            else
            {
                //TRY hacer aleatorio la eleccion entre los arrays prediseñados que hayan
                //Deben ser un tile menos que la nextCurve es decir 14, por que la curva se
                //cuenta como el primer tile.
                //Hacemos tilesCount-1 porque tilesCount cuenta el cero en la curva antes
                //de la recta, si no llegaria a la posicion 14 y son de 0-13
                tile = Instantiate(currentPart.straightPrefab[tilesCount-1]) as GameObject;

                //Si el numero de tiles creados en esta recta es igual a la cantidad total del
                //array prediseñado -1 hemos llegado a un tile antes del final, 
                //entonces en la siguiente vuelta pasamos a la logica de generacion aleatoria
                //que al seguir nextCurve en 15 no pasará nada, pero ya random será falso y 
                //en la siguiente vuelta la curva si se genera aleatoria y continua normal
                if(tilesCount == currentPart.straightPrefab.Count - 1)
                {
                    //Dejamos de generar rectas prediseñadas
                    prefabPart = false;
                }
            }
        }
        else if (side == 1){
            //Si la direccion cambia a la derecha
            //instancia los tiles de derecha
            tile = Instantiate(currentPart.rightTiles[RandomTileIndex(0, currentPart.rightTiles.Count)]) as GameObject;
        }else{
            //Si la direccion cambia a la izquierda
            //instancia los tiles de izquierda
            tile = Instantiate(currentPart.leftTiles[RandomTileIndex(0, currentPart.leftTiles.Count)]) as GameObject;
        }
        
        //Hacemos padre del tile a este objeto LevelManager
        tile.transform.SetParent(transform);

        //Si solo hay 1 tile activo
        if(activeTiles.Count < 2){
            //Posicionamos este nuevo tile en el lugar indicado por la var spawnZ
            //Lo multiplicamos por forward ya que este vector es 1 en z
            tile.transform.position = Vector3.forward * spawnZ;

            //Sumamos al valor de posicion spawnZ el tamaño del tile
            spawnZ += tileLenght;
        }else{

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
            }else{
                //Si hay un giro en el mapa, el tile de giro mantiene la rotación normal (igual que el anterior)
                //ya que este no girará si no los de linea recta que vienen despues del giro
                tile.transform.rotation = activeTiles[activeTiles.Count-1].transform.rotation;
            }
        }

        //Añadimos este nuevo tile a la lista de tiles activos
        activeTiles.Add(tile);

        //Se aumenta la cantidad de tiles creados despues de una curva
        tilesCount++;

        //Se aumenta la cantidad de tiles de una parte del mapa
        partTilesCount++;

        //Si la cantidad de tiles de la parte del mapa llega al limite
        if(currentPart.tilesCount - partTilesCount == 0){
            //Se selecciona la parte siguiente del array de partes del mapa
            int nextIndex = mapParts.IndexOf(currentPart) + 1;

            //Si es la ultima parte se regresa a la primera
            if(nextIndex >= mapParts.Count){
                nextIndex = 0;
            }
            
            //Se reasigna la parte actual que se va a empezar a instanciar
            currentPart = mapParts[nextIndex];

            //Se regresa a 0 la cantidad de tiles de la nueva parte del mapa
            //que se va a empezar a crear
            partTilesCount = 0;
        }
    }

    //Metodo encargado de agregar las nuevas posiciones
    //donde iran ubicados los obstaculos en una recta
    public void GenerateIndexOfObstacles(int nextCurve)
    {
        //Por defecto al menos 1 obstaculo
        int cantObstacles = 1;

        //Si antes de la proxima curva hay entre 10 y 11 tiles
        if (nextCurve >= 10 && nextCurve <= 11)
        {
            //Elijo aleatoriamente entre 1 o 2 obstaculos
            cantObstacles = Random.Range(1, 3);
        }
        else if(nextCurve == 12)
        {
            //Si es mayor a 11 tiles
            //elijo aletoriamente entre 1 a 3 obstaculos
            cantObstacles = Random.Range(1, 4);
        }
        else
        {
            return;
        }

        //Objeto random
        System.Random rnd = new System.Random();

        //Array que llevara las posibles posiciones a poner los tiles
        int[] obstacleTilePositions;

        //Dependiendo de la cantidad de obstaculos que vamos a generar
        switch (cantObstacles)
        {
            //Si es 1
            case 1:
                //Agrego las posibles posiciones al array
                obstacleTilePositions = new int[] { 4, 5, 6, 7 };

                //Escojo aleatoriamente una posicion y la agrego a la lista
                indexObstacle.Add(obstacleTilePositions[rnd.Next(obstacleTilePositions.Length)]);
                break;
            //Si son 2
            case 2:
                //Si la proxima curva es en 10 tiles
                if (nextCurve == 10)
                {
                    //Los obstaculos se van a generar en la pos 4 y 7
                    indexObstacle.Add(4);
                    indexObstacle.Add(7);

                }//Si la proxima curva tiene 11 tiles
                else if (nextCurve == 11)
                {
                    //Agrego las posibles posiciones al array
                    obstacleTilePositions = new int[] { 4, 5 };

                    //Escojo aleatoriamente una posicion y la agrego a la lista
                    indexObstacle.Add(obstacleTilePositions[rnd.Next(obstacleTilePositions.Length)]);

                    //Y agrego el otro en la pos 8
                    indexObstacle.Add(8);

                }//Si la proxima curva tiene mas de 11 tiles
                else
                {
                    //Agrego las posibles posiciones al array
                    obstacleTilePositions = new int[] { 8, 9 };

                    //Agrego manualmente la posicion 5
                    indexObstacle.Add(5);

                    //Escojo aleatoriamente una posicion y la agrego a la lista
                    indexObstacle.Add(obstacleTilePositions[rnd.Next(obstacleTilePositions.Length)]);
                }
                break;
            //Si son 3 obstaculos
            case 3:
                //Agrego manualmente las 3 posiciones
                indexObstacle.Add(3);
                indexObstacle.Add(6);
                indexObstacle.Add(9);
                break;
        }
    }

    //Metodo para eliminar los tiles que van quedando atras
    public void DeleteLastTile()
    {
        //Destruimos el primer tile de la lista
        Destroy(activeTiles[0]);
        //Removemos de la lista el primero espacio para que se reordene
        activeTiles.RemoveAt(0);
    }

    //Metodo para eliminar todos los tiles activos en el juego.
    public void DeleteAllTiles()
    {
        //Almacenamos el total de tiles activos en este momento, para poder
        //recorrer la totalidad de la lista, porque al irse eliminando
        //va disminuyendo esta variable
        int totalActiveTiles = activeTiles.Count;
        for (int i = 0; i < totalActiveTiles; i++)
        {
            //Destruimos el primer objeto
            Destroy(activeTiles[0]);
            //Removemos el primer objeto y esto mueve toda la lista
            //quedando de primero el que estaba de segundo, por esto
            //siempre eliminamos el tile que este de primero
            activeTiles.RemoveAt(0);
        }        
    }

    //Metodo para generar un index aleatorio de nuestros tiles
    //recibe como parametro la cantidad de items de cada lista
    int RandomTileIndex(int initIndex, int listCount)
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
            randomIndex = Random.Range(initIndex, listCount);
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

    //Metodo que cambiará los limites establecidos para escoger el tamaño
    //de la recta o la distancia entre cada curva, generando rectas 
    //cortas
    public void OnFastCurves(bool active)
    {
        if (active)
        {
            //Modificamos los tiles minimos entre cada curva
            //para que se den curvas mas rapido, haciendo la
            //zona recta mas corta
            minNextCurve = 5;
            maxNextCurve = 7;

        }
        else
        {
            minNextCurve = 10;
            maxNextCurve = 13;
        }
    }

    //Metodo que modificará ciertos parametros para hacer el vj más dificil
    public void MediumDifficultyMode()
    {
        //El maximo pasa a ser la totalidad de los tiles del array
        //desbloqueando así tiles de obstaculos mas complejos de atravesar
        maxIndexObstacleTile = currentPart.obstacleTiles.Count-1;
    }

}
