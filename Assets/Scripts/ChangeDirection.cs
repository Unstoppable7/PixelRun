using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDirection : MonoBehaviour
{
    //Direccion de la curva izquierda o derecha
    [Range(-1, 1)]
    public int direction = 0;

    //Grados a los que girará el jugador
    public float degrees = 90f;

    //Guardará si esl jugador presiona para realizar el giro
    bool changePressed;

    void Update(){        

        /**
        //Si se presiona la tecla e cuando gira a la derecha
        if( (Input.GetKeyDown(KeyCode.E) || 
            GameManager.sharedInstance.motor.swipeRight) && direction == 1){

            GameManager.sharedInstance.motor.swipeRight = false;
            changePressed = true;
        }

        //O si se presiona q cuando gira a la izquierda
        else if( (Input.GetKeyDown(KeyCode.Q) ||
            GameManager.sharedInstance.motor.swipeLeft) && direction == -1){

            GameManager.sharedInstance.motor.swipeLeft = false;
            changePressed = true;
        }
        **/
    }

    void OnTriggerStay(Collider other){
        //Si el jugador no está corriendo en el tile de la curva
        //no se gira
        if(!GameManager.sharedInstance.IsPlayerRunning()){
            return;
        }

        if(other.CompareTag("Player")){
            //Si el jugador no presiona para girar y no es inmune
            //y no tiene escudo
            //no hace nada (se estrella)
            //Debug.LogError("DIRECTION: "+direction);
            /**
            if (
                //aqui
                //!changePressed &&
                !GameManager.sharedInstance.GetImmunePlayer() &&
                !GameManager.sharedInstance.GetPlayerShield())
            {
                return;
            }
            **/
            //aqui
            //Bandera a false
            //changePressed = false;
            //Debug.LogError("ENTRAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            //Si se presiona la tecla o el swipe correspondiente y está el tile
            //en esa direccion, el jugador gira
            if ( 
                    (   (Input.GetKeyDown(KeyCode.E) || GameManager.sharedInstance.motor.SwipeCheck(3) )
                        && 
                        (direction == 1)
                    )
                    ||
                    (   (Input.GetKeyDown(KeyCode.Q) || GameManager.sharedInstance.motor.SwipeCheck(4) )
                        && 
                        (direction == -1)
                    ) 
                    ||
                    GameManager.sharedInstance.GetImmunePlayer()
                    ||
                    GameManager.sharedInstance.GetPlayerShield()
                )
            {
                //GameManager.sharedInstance.motor.swipeRight = false;
                //GameManager.sharedInstance.motor.swipeLeft = false;

                //Ahora si el jugador presiona para girar o está inmune o tiene el escudo entonces gira
                //Destruye los triggers del tile de la curva para asegurarnos de que no gire varias veces
                //en el mismo tile
                DestroyColliders();
                
                //Centramos al jugador antes de girarlo
                GameManager.sharedInstance.PerfectPlayerCenter(transform.position);

                //Se gira al jugador
                GameManager.sharedInstance.ChangePlayerDirection(direction, degrees);

                //Centramos la camara antes de girarla
                //El vector por parametro solo tendrá la posicion en Z local de la curva
                //servirá para centrar a la camara antes de girar y hacer que esta posicion sea
                //la nueva x local y que no se siga moviendo en el centro
                GameManager.sharedInstance.PerfectCameraCenter(transform.InverseTransformDirection(new Vector3(
                    transform.position.x * transform.forward.x,
                    transform.position.y * transform.forward.y,
                    transform.position.z * transform.forward.z
                )));

                //Se gira la camara
                GameManager.sharedInstance.ChangeCameraDirection(direction, degrees);
            }
        }
    }

    //Elimina los triggers del prefab de la curva para que no gire varias veces
    void DestroyColliders(){
        BoxCollider[] colliders = gameObject.GetComponents<BoxCollider>();

        //Solo eliminamos los triggers, para asegurarnos que las colisiones normales
        //aun queden alli y el jugador choque si no llega a girar
        foreach (BoxCollider collider in colliders){
            if(collider.isTrigger){
                Destroy(collider);
            }
        }
    }
}
