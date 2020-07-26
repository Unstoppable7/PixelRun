using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    //Singleton
    public static CollectableManager sharedInstance { set; get; }

    //Prefabs que se pueden poner en el mapa
    [Header("Collectable prefabs")]
    public GameObject coinPrefab;
    public GameObject shieldPrefab;
    public GameObject attackPrefab;

    [Header("Checker settings")]
    //Transform que guarda la posición y dirección del raycast
    public Transform obstacleChecker;

    //Que tan retirado del centro tiene que estar un obstáculo para
    //que se creen las monedas hacia un lado
    [Range(0.1f, 1f)]
    public float offsetX = 0.5f;

    //El suavizado de la curva en las monedas
    [Range(0.1f, 1f)]
    public float curveSmooth = 0.4f;

    //La altura maxima de los obstáculos para que las monedas se creen
    //sobre esos obstáculos
    [Range(1f, 2f)]
    public float maxObstacleHeight = 1.5f;

    [Header("Collectable probabilities")]
    //Probabilidad de cada tile en que se cree monedas
    [Range(0f, 1f)]
    public float coinProbability = 0.3f;

    //Probabilidad de cada tile en que se cree el escudo
    [Range(0f, 1f)]
    public float shieldProbability = 0.1f;

    //Probabilidad de cada tile en que se cree el ataque
    [Range(0f, 1f)]
    public float attackProbability = 0.1f;

    [Header("Pattern probabilities")]
    //Si se crea las monedas, es la probabilidad de que se cree en un obstáculo o no
    [Range(0f, 1f)]
    public float obstacleProbability = 0.5f;

    //En tiles vacios, hace que se cree las monedas en linea recta o no
    [Range(0f, 1f)]
    public float straightProbability = 0.7f;

    //En tiles vacios, hace que se cree las monedas en con curvas o no
    [Range(0f, 1f)]
    public float sinProbability = 0.2f;

    [Header("Collectable settings")]
    //Numero de monedas que se quiere que haya por tiles
    [Range(1, 12)]
    public int collectablesPerTile = 12;

    //Distancia del raycast para detectar los obstáculos, es igual al tamaño del tile para que
    //lo cubra todo
    int rayDistance = 6;

    //Distancia de separación que tendrán las monedas en los tiles, dependerá de la cantidad
    //de monedas que se quiera por tile
    float collectableDistance;

    //Nos dice si ya se creó el patron de monedas en ese tile, para que no se vuelva a crear
    bool createPattern;

    //Dirección que guarda la cueva actual al momento de que se cree una curva con las monedas
    //para que a la siguiente curva, si salen seguidas se invierte la direccion y las dos
    //curvas queden conectadas
    int currentSinDirection = 1;

    //Guarda el obstáculo del tile
    RaycastHit currentObstacle;

    void Awake(){
        if (!sharedInstance)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        //Si la cantidad de collectables por tiles es par se vuelve impar para que esten parejos
        //a los lados y uno esté en el centro exacto
        if(collectablesPerTile % 2 == 0){
            collectablesPerTile++;
        }

        //La distancia que tendrán los collectables dependiendo de la cantidad que habrá en cada tile
        //entre mas collectables por tile menos será la distancia entre ellos (max 12, como es par serian max 13 por tile)
        collectableDistance = (float)rayDistance / collectablesPerTile;
    }

    void FixedUpdate(){
        RaycastHit hit;

        //Si colisiona con un obstáculo o un objeto indestructibe
        if(Physics.Raycast(obstacleChecker.position, obstacleChecker.forward, out hit, rayDistance) && createPattern){
            //A false para que no se pueda crear otro patron en ese mismo tile
            //ya que un solo patron cubre todo el tile completo
            createPattern = false;

            //que no sea la pared de la curva (solo comprueba una vez el obstáculo)
            if(!hit.transform.parent.CompareTag("ChangeDirection")){
                currentObstacle = hit;
                
                //Si se crean monedas en el tile actual
                if(Random.value <= coinProbability){
                    //crea los collectables dependiendo del obstáculo con el que haya chocado
                    //si sale la probabilidad de que se cree en el obstáculo las monedas
                    if(Random.value <= obstacleProbability){
                        ObstaclePattern();
                    }
                }

                //Si no se crean las monedas, se pueden poner un solo collectable como escudo o el ataque
                else if(Random.value <= shieldProbability){
                    CreateShield();
                }

                else if(Random.value <= attackProbability){
                    CreateAttack();
                }
            }
        }

        //Si no detecta un obstáculo, se hace que el raycast detecte el suelo del tile actual
        //se le suma rayDistance/2 para que quede en el centro del tile (rayDistance es el mismo tamaño del tile)
        else if(Physics.Raycast(obstacleChecker.position + obstacleChecker.forward * rayDistance/2, -obstacleChecker.up, out hit, rayDistance) && createPattern){
            //no deja crear un patron dos veces en el mismo tile
            createPattern = false;
            currentObstacle = hit;
            
            //Si en el tile que no tiene obstáculos se elige crear monedas
            if(Random.value <= coinProbability){
                //Se crean los patrones dependiendo de su probabilidad individual para el tile actual
                //Si es el hueco en el agua y se elige colocar las monedas
                if(currentObstacle.collider.CompareTag("Hole")){
                    if(Random.value <= obstacleProbability){
                        ObstaclePattern();
                    }
                }

                //En el tile actual se crea una linea recta de collectables
                else if(Random.value <= straightProbability){
                    StraightPattern();
                }

                //Se crea el patron Sin si sale la probabilidad en el tile actual
                else if(Random.value <= sinProbability){
                    SinPattern();
                }
            }

            //Si no, se puede crear un escudo o ataque
            else if(Random.value <= shieldProbability){
                CreateShield();
            }

            else if(Random.value <= attackProbability){
                CreateAttack();
            }
        }

        //Si no choca con nada se hace el obstáculo actual vacio
        else{
            currentObstacle = new RaycastHit();
        }
    }

    //Update solo para mostrar los raycast
    void Update(){
        if(currentObstacle.collider){
            Vector3 upObstacle = new Vector3(
                currentObstacle.collider.bounds.center.x,
                currentObstacle.collider.bounds.max.y,
                currentObstacle.collider.bounds.center.z
            );

            Debug.DrawLine(obstacleChecker.position,  upObstacle, Color.red, 0f);
        }

        Debug.DrawLine(obstacleChecker.position, obstacleChecker.position + obstacleChecker.forward * rayDistance, Color.green, 0f);
        Debug.DrawLine(obstacleChecker.position + obstacleChecker.forward * rayDistance/2, obstacleChecker.position + obstacleChecker.forward * rayDistance/2 - obstacleChecker.up * rayDistance, Color.blue, 0f);
    }

    //Crea un patron de collectables dependiendo del obstáculo que detecta
    void ObstaclePattern()
    {
        //Si el obstáculo es muy alto no hace nada
        if(currentObstacle.collider.bounds.max.y > maxObstacleHeight){
            return;
        }

        //Se halla solo el valor en x global del detector
        Vector3 checkerLocalX = new Vector3(
            obstacleChecker.position.x * obstacleChecker.right.x,
            obstacleChecker.position.y * obstacleChecker.right.y,
            obstacleChecker.position.z * obstacleChecker.right.z
        );

        //Se convierte la posición en x a local del detector de obstáculos
        checkerLocalX = obstacleChecker.InverseTransformDirection(checkerLocalX);

        //Se halla solo el valor en x global del obstáculo
        Vector3 obstacleColliderLocalX = new Vector3(
            currentObstacle.collider.bounds.center.x * obstacleChecker.right.x,
            currentObstacle.collider.bounds.center.y * obstacleChecker.right.y,
            currentObstacle.collider.bounds.center.z * obstacleChecker.right.z
        );
        
        //Se convierte la posición en x a local del obstáculo
        obstacleColliderLocalX = obstacleChecker.InverseTransformDirection(obstacleColliderLocalX);

        //Posición donde se colocará el collectable
        Vector3 positionTarget = obstacleChecker.position;

        //For que coloca la cantidad de collectables
        for(int i=1; i<=collectablesPerTile; i++){
            Transform collectableTemp = Instantiate(coinPrefab, positionTarget, Quaternion.identity).transform;

            //Se guardan los collectables como hijos del obstáculo actual para que se borren junto con el tile
            //si no se agarran
            collectableTemp.SetParent(currentObstacle.transform);
            
            //Falta mejorarlo
            //Cuando está girado en alungos grados los pone al contrario :c

            //Si el collider del obstáculo está a la derecha del centro mas la distancia horizontal
            if(obstacleColliderLocalX.x > (checkerLocalX.x + offsetX)){
                //Hace que la posición del collectable sea a la izquierda
                if(obstacleChecker.rotation == Quaternion.Euler(0, 180, 0) || obstacleChecker.rotation == Quaternion.Euler(0, -180, 0)){
                    positionTarget += obstacleChecker.right * Mathf.Sin(i * collectableDistance) * curveSmooth;
                }

                else{
                    positionTarget += obstacleChecker.right * -Mathf.Sin(i * collectableDistance) * curveSmooth;
                }
            }

            //Si el collider del obstáculo está a la izquierda del centro mas la distancia horizontal
            else if(obstacleColliderLocalX.x < (checkerLocalX.x - offsetX)){
                //Hace que la posición del collectable sea a la derecha
                if(obstacleChecker.rotation == Quaternion.Euler(0, 180, 0) || obstacleChecker.rotation == Quaternion.Euler(0, -180, 0)){
                    positionTarget += obstacleChecker.right * -Mathf.Sin(i * collectableDistance) * curveSmooth;
                }

                else{
                    positionTarget += obstacleChecker.right * Mathf.Sin(i * collectableDistance) * curveSmooth;
                }
            }
            //Hasta aqui hay que ver para arreglarlo :c
            //-----------------------------------------------------------------------------------------------
            
            //Si el obstáculo está en el centro o no muy alejado mas haya del offsetX
            //entonces pondrá los colelctables por encima del obstáculo
            else{
                positionTarget += obstacleChecker.up * Mathf.Sin(i * collectableDistance) * curveSmooth;
            }

            //Aumenta la posición hacia adelante para el siguiente collectable
            positionTarget += obstacleChecker.forward * collectableDistance;
        }
    }

    //Crea una linea recta de collectables
    void StraightPattern(){
        //Posición donde se colocará el collectable
        Vector3 positionTarget = obstacleChecker.position;

        //For para crear la cantidad de collectables por tile
        for(int i=1; i<=collectablesPerTile; i++){
            //Se guarda como hijo para que se borre si no se agarran los collectables
            Transform collectableTemp = Instantiate(coinPrefab, positionTarget, Quaternion.identity).transform;
            collectableTemp.SetParent(currentObstacle.transform);

            //Solo avanza la posición en linea recta para el siguiente collectable
            positionTarget += obstacleChecker.forward * collectableDistance;
        }
    }

    //Crea curvas con la funcion Seno
    void SinPattern(){
        //Posición del collectable
        Vector3 positionTarget = obstacleChecker.position;

        //For para crear la cantidad de collectables por tile
        for(int i=1; i<=collectablesPerTile; i++){
            Transform collectableTemp = Instantiate(coinPrefab, positionTarget, Quaternion.identity).transform;
            collectableTemp.SetParent(currentObstacle.transform);

            //En x se le suma el valor de seno para que vaya haciendo el patron de la curva
            positionTarget += obstacleChecker.right * currentSinDirection * Mathf.Sin(i * collectableDistance) * curveSmooth;

            //Hacia adelante se suma para el siguiente collectable
            positionTarget += obstacleChecker.forward * collectableDistance;
        }

        //Se invierte la direccion que tendrá la curva del patron seno para que sea al contrario que el anterior
        //asi si salen dos iguales seguidos se crea las dos curvas contrarias de forma fluida
        currentSinDirection = -currentSinDirection;
    }

    //Actualiza la posición del detector de obstáculos. Se llama cada vez que se crea un tile nuevo
    public void UpdateObstacleChecker(Transform currentTile){
        //Actualiza la posición y rotacion del detector a la misma del tile
        obstacleChecker.rotation = currentTile.rotation;

        obstacleChecker.position = new Vector3(
            currentTile.position.x,
            1,
            currentTile.position.z
        );

        //Se le resta la mitad del tamaño del tile para que quede al inicio del tile y el ray detecte
        //desde el inicio hasta el final del tile
        obstacleChecker.position += -obstacleChecker.forward * rayDistance/2;

        //Cada vez que se crea un tile y se actualiza la posición se hace que se pueda crear un patron
        //en ese tile
        createPattern = true;
    }

    //Coloca un escudo un poco mas arriba que la posición del obstáculo
    void CreateShield(){
        Transform collectableTemp = Instantiate(shieldPrefab, currentObstacle.transform.position + Vector3.up * 1.5f, Quaternion.identity).transform;
        collectableTemp.SetParent(currentObstacle.transform);
    }

    //Coloca un ataque un poco mas arriba que la posición del obstáculo
    void CreateAttack(){
        Transform collectableTemp = Instantiate(attackPrefab, currentObstacle.transform.position + Vector3.up * 1.5f, Quaternion.identity).transform;
        collectableTemp.SetParent(currentObstacle.transform);
    }
}
