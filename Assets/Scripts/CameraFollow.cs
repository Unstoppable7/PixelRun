using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Objeto el cual seguirá la camara
    public Transform target;

    //Distancia entre la camara y el target
    public Vector3 offset;

    //Velocidad del suavizado
    public float smoothSpeed;

    //Posicion que tendra la camara en su x local para que no se mueva
    //en el centro
    float initPosition;

    //Rotacion a la que se quiere hacer la animacion
    Quaternion targetRotation;

    //Posicion que tendrá la camara
    Vector3 newPosition;

    //Limites del jugador que se moverán igual que la camara
    //manteniendose quietos en el centro
    Transform playerLimits;

    void Start(){
        //Referencia al gameobject que contiene el collider con los limites
        playerLimits = GameObject.Find("PlayerLimits").transform;
    }

    //Llamamos un metodo nativo de Unity el cual se llama cada vez que
    //todos los demas metodos Update terminan, se usa especialmente para
    //el seguimiento de la camara
    private void LateUpdate()
    {
        //Revisamos si el juego ya comenzó para ubicar la camara sobre el
        //personaje y comenzarlo a seguir
        if (GameManager.sharedInstance.IsGameStarted)
        {
            //Posicion donde vamos a situar la camara
            newPosition = target.position + target.TransformDirection(offset);

            newPosition.y = offset.y;

            //Si el jugador gira a 90 o -90 grados hacemos que la camara en la posicion z sea
            //la inicial que tenga al momento de la curva para que no se pueda mover
            //y siempre quede en el centro
            if(target.rotation.eulerAngles.y == 90 || target.rotation.eulerAngles.y == 270){
                newPosition.z = initPosition;
            }

            //Si está a 0 o 180 hacemos que no se mueva en el eje x
            else{
                newPosition.x = initPosition;  
            }

            //Asignamos al transform de la camara la nueva posicion, usamos
            //el metodo Lerp() para que el movimiento sea mas suave
            transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);
            
            //Asignamos la posicion de los limites que será igual al la del jugador
            //quitandole el offset que se le da a la camara
            playerLimits.position = newPosition - target.TransformDirection(offset);

            //La rotacion de los limites será siempre igual al del jugador para que estén siempre
            //a los lados del mismo. Asi evitamos los colliders en las paredes de cada prefab
            playerLimits.rotation = target.rotation;
        }        
    }

    //Asigna la posicion central de la camara para que no se pueda mover en el centro
    public void PerfectCenter(Vector3 center){
        initPosition = center.z;
    }

    //Cambia la rotacion de la camara a la nueva direccion
    //se llama desde ChangeDirection.cs
    public IEnumerator ChangeDirection(int direction, float degrees){
        targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + degrees * direction, 0);
        
        while(Quaternion.Angle(transform.rotation, targetRotation) > 0){
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
            yield return null;
        }

        //Nos aseguramos de que la rotacion final sea a la que queremos llegar
        //despues de que se haya realizado la animación de la rotación
        transform.rotation = targetRotation;
    }
}
