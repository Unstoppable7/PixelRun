using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    //Distancia entre c/u de los carriles
    const float DISTANCE = 1.6f;
    //Velocidad de rotacion del personaje cuando cambia de carril
    //o cuanto queremos que rote
    const float TURN_SPEED = 0.05f;

    #region Variables de movimiento

    //Fuerza de salto
    float jump = 5.5f;
    //Gravedad
    float gravity = 12f;
    //Velocidad vertical, define la vel con la que el personaje iniciará
    //su descenso despues de saltar
    float verticalVelocity;
    //Velocidad del personaje
    float speed = 5f;
    //Nro del carril en el que nos estamos ubicando
    //1 centro, +1 der, -1 izq
    int lane = 1;

    #endregion

    //Definimos la variable con la que vamos a manejar el character controller
    CharacterController controller;

    //Definimos el Animator para poder cambiar los parametros activar 
    //las animaciones
    Animator animatorController;

    //Variable para manejar cuando el personaje está corriendo
    bool isRunning;

    //Variable para manejar cuando el personaje está agachado
    bool isCrouch;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animatorController = GetComponent<Animator>();
        isRunning = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Si el personaje no está corriendo (al inicio del juego) no
        //hacemos nada
        if (!isRunning)
        {
            return;
        }
        //Revisamos cuando la tecla flecha izq es oprimida
        if (Input.GetKeyDown(KeyCode.LeftArrow)){
            ChangeLane(false);
        }
        //Revisamos cuando la tecla flecha der es oprimida        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(true);
        }


        //Este será el vector donde calcularemos hacia donde debo moverme
        //Se * por forward para que vaya hacia adelante en el eje z
        Vector3 targetPosition = transform.position.z * Vector3.forward;

        //Movemos nuestro personaje dependiendo del carril
        if(lane == 0)
        {            
            targetPosition += Vector3.left * DISTANCE;
        }else if(lane == 2)
        {
            targetPosition += Vector3.right * DISTANCE;
        }

        //Este será el vector de movimiento final de nuestro personaje
        Vector3 moveTarget = Vector3.zero;

        //Calculamos el vector final de movimiento (eje x) 
        //y lo normalizamos para evitar que en diagonal se sumen 
        //los vectores y a su vez le agregamos la velocidad correspondiente
        moveTarget.x = (targetPosition - transform.position).normalized.x
                        * speed;
        bool isGrounded = IsGrounded();

        //Cambiamos el parametro si ha tocado el suelo
        animatorController.SetBool("isGrounded", isGrounded);

        //Si el personaje está en el suelo 
        if (isGrounded)
        {
            //Definimos la vel vertical como -0.1f 
            verticalVelocity = -0.1f;

            bool bigJump = isBigJump();

            //Si presiona la barra espaciadora
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Asignamos como velocidad vertical la fuerza de salto
                verticalVelocity = jump;

                //Si delante del jugador hay un obstaculo para realizar el salto largo
                if(bigJump){
                    animatorController.SetTrigger("bigJump");
                }

                else{
                    //Disparamos la animacion de salto
                    animatorController.SetTrigger("Jump");
                }
            }
        }//Si no está en el suelo (está en el aire)
        else 
        {
            //Vamos disminuyendo la vel vertical con respecto
            //a la gravedad para que vaya cayendo poco a poco
            verticalVelocity -= (gravity * Time.deltaTime);
        }

        //El vector en el eje y será en este momento la vel vertical
        moveTarget.y = verticalVelocity;

        //En el eje z asignamos la velocidad que teniamos definida
        moveTarget.z = speed;

        //Aplicamos el movimiento a nuestro character controller
        controller.Move(moveTarget * Time.deltaTime);

        //Asignamos la velocidad que tiene el personaje al parametro
        //correspondiente para activar las diferentes animaciones
        animatorController.SetFloat("Speed", controller.velocity.z);

        #region Rotamos un poco al personaje cuando cambie de carril

        //Le asignamos la velocidad que lleva nuestro personaje
        //para tomarla como referencia de la direccion que lleva
        Vector3 dir = controller.velocity;

        //Definimos la direccion en el eje 'y' como cero
        dir.y = 0;

        //Rotamos al personaje tomando su eje z y modificandolo con lerp
        //el cual interpola dos puntos, en este caso entre la posicion
        //anterior y el pequeño giro que le estamos haciendo y la
        //velocidad a la cual se hará ese giro, que en realidad no es
        //la velocidad, es que tanto se va a girar el personaje entre
        //la diferencia del pto a y el pto b
        transform.forward = Vector3.Lerp(transform.forward, dir, 
                                        TURN_SPEED);

        #endregion

        //Jugador se agacha una sola vez y solo si está en el suelo
        if(Input.GetKeyDown(KeyCode.DownArrow) && !isCrouch && isGrounded){
            isCrouch = true;
            Crouch();
        }
    }

    //Metodo que me permite cambiar el jugador de posicion o carril 
    //recibo el booleano para manejar cuando se mueva a izq o der
    public void ChangeLane(bool isRight)
    {
        lane += (isRight) ? 1 : -1;

        //Clampeo la variable de forma que la limito a dos extremos
        lane = Mathf.Clamp(lane, 0, 2);
    }

    //Metodo para verificar si el personaje está tocando el suelo
    //Vamos a utilizar la tecnica del raycast para poder detectar el suelo
    private bool IsGrounded()
    {
        //Rayo que vamos a utilizar
        //El origen del rayo viene dado por el vector ubicado en
        //el punto central del limite del controlador en el eje x, 
        //el punto central del limite del controlador en el eje 'y' menos
        //la mitad del tamaño de los limites (la distancia que hay entre 
        //el centro y el borde, todo esto + 0.2f para que no este
        //en todo el borde si no un poco mas abajo y el punto central del
        //limite del controlador en el eje z.
        //y finalmente le damos una direccion hacia abajo (down)
        Ray groundRay = new Ray(new Vector3(controller.bounds.center.x,
        (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f,
                controller.bounds.center.z), Vector3.down);

        //Retornamos el resultado si el rayo toca el suelo o no
        //le damos el rayo que vamos a usar y la longitud del mismo
        return Physics.Raycast(groundRay, 0.2f + 0.1f);
    }

    //Me cambia el estado del personaje a corriendo, de esta forma inicia
    //el juego, este será manejado por un boton en la GUI
    public void StartRun()
    {
        isRunning = true;
    }

    //Me cambia el estado del personaje a detenido, de esta forma pausamos
    //el juego, este será manejado por un boton en la GUI
    public void StopRun()
    {
        isRunning = false;
        animatorController.SetFloat("Speed", 0);
    }

    //Por tener un character controler usamos un metodo diferente para
    //detectar las colisiones del jugador
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            Crash();
        }
    }

    //Metodo de muerte del personaje
    void Crash()
    {
        //Activamos el parametro de muerte del personaje para que se
        //anime
        animatorController.SetTrigger("Death");

        //Detenemos al personaje
        isRunning = false;
        //Detenemos el juego
        GameManager.sharedInstance.IsGameStarted = false;
        //Juego terminado
        //GameManager.sharedInstance.GameOver();

        //Activamos el death menu
        GameManager.sharedInstance.userInterfaceManager.DeathMenu
                                        .gameObject.SetActive(true);
        //Desactivamos el boton de pausa
        GameManager.sharedInstance.userInterfaceManager.pauseButton
                                        .gameObject.SetActive(false); ;
    }

    //Metodo que comprueba si adelante hay un obstaculo con el que
    //se pueda realizar la animacion de salto grande
    private bool isBigJump(){
        Ray forwardRay = new Ray(new Vector3(controller.bounds.center.x,
                                            controller.bounds.center.y,
                                            (controller.bounds.center.z + controller.bounds.extents.z)), Vector3.forward);

        RaycastHit hit;

        Debug.DrawRay(forwardRay.origin, forwardRay.direction * 3.5f, Color.red);

        if(Physics.Raycast(forwardRay, out hit, 3.5f, LayerMask.GetMask("BigJump"))){
            return true;
        }

        return false;
    }

    //Metodo que realiza la animacion de deslizarse en el suelo y
    // bajar el collider para simular que está agachado
    private void Crouch(){
        animatorController.SetTrigger("Slide");

        controller.height /= 2;
        controller.center /= 2;

        //Levanta al jugador en un tiempo aproximado a lo que dura la
        //animación de deslizarse
        Invoke("GetUp", 1.2f);
    }

    //Metodo para levantar al jugador colocando de nuevo la altura y
    //centro del controller
    private void GetUp(){
        controller.height *= 2;
        controller.center *= 2;
        isCrouch = false;
    }

    //Metodo para revivir al personaje
    public void Revive()
    {
        //Activo el trigger para que el personaje retome la animacion de pie
        animatorController.SetTrigger("Revive");

        //Detenemos al personaje
        StopRun();

        //Agrego inmunidad al player en los obstaculos actuales en la escena
        Inmunity();
    }

    //Metodo para dar inmunidad al personaje
    void Inmunity()
    {
        //Busco y guardo en un array todos los objetos con el tag obstacle
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        //Recorro el array de obstaculos
        foreach (GameObject obstacle in obstacles)
        {                  
            Collider[] colliders = obstacle.GetComponents<Collider>();

            //Vuelvo trigger los colliders de los obstaculos
            //Desactiva los que tienen varios colliders
            foreach (Collider collider in colliders){
                collider.isTrigger = true;
            }
        }
    }
}
