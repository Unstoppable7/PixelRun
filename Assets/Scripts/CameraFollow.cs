using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Objeto el cual seguirá la camara
    public Transform target;

    //Distancia entre la camara y el target
    public Vector3 offset = new Vector3(0f, 0.5f, -10f);

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
            Vector3 newPosition = target.position + offset;

            //Hacemos cero el eje X para que la camara este en el centro
            //de la pantalla
            newPosition.x = 0;

            //Asignamos al transform de la camara la nueva posicion, usamos
            //el metodo Lerp() para que el movimiento sea mas suave
            transform.position = Vector3.Lerp(transform.position, newPosition,
                                              Time.deltaTime);
        }        
    }
}
