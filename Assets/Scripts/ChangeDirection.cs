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
        changePressed = false;

        //Si se presiona la tecla e cuando gira a la derecha
        if(Input.GetKeyDown(KeyCode.E) && direction == 1){
            changePressed = true;
        }

        //O si se presiona q cuando gira a la izquierda
        else if(Input.GetKeyDown(KeyCode.Q) && direction == -1){
            changePressed = true;
        }
    }

    void OnTriggerStay(Collider other){
        //Si el jugador no está corriendo en el tile de la curva
        //no se gira
        if(!GameManager.sharedInstance.IsPlayerRunning()){
            return;
        }

        if(other.CompareTag("Player")){
            //Si el jugador no presiona para girar, y si el jugador no es inmune
            //no hace nada (se estrella)
            if(!changePressed && !GameManager.sharedInstance.GetImmunePlayer()){
                return;
            }
            
            //Ahora si el jugador presiona para girar o está inmune (gira)
            //Destruye los triggers del giro
            DestroyColliders();

            //Centramos al jugador antes de girarlo
            GameManager.sharedInstance.PerfectPlayerCenter(transform.position);

            //Se gira al jugador
            GameManager.sharedInstance.ChangePlayerDirection(direction, degrees);

            //Centramos la camara antes de girarla
            GameManager.sharedInstance.PerfectCameraCenter(transform.InverseTransformDirection(new Vector3(
                transform.position.x * transform.forward.x,
                transform.position.y * transform.forward.y,
                transform.position.z * transform.forward.z
            )));

            //Se gira la camara
            GameManager.sharedInstance.ChangeCameraDirection(direction, degrees);
        }
    }

    //Elimina los triggers del prefab de la curva para que no gire varias veces
    void DestroyColliders(){
        BoxCollider[] colliders = gameObject.GetComponents<BoxCollider>();

        foreach (BoxCollider collider in colliders){
            if(collider.isTrigger){
                Destroy(collider);
            }
        }
    }
}
