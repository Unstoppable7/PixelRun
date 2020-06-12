using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    //Distancia entre c/u de los limites
    const float DISTANCE = 2.8f;
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

    //Transform que usaremos para rotar al jugador
    //sin afectar su movimiento hacia adelante
    //es el primer hijo del GameObject player
    Transform rotation;

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

    //Variable que nos dice si el jugador es  inmune o no
    //para hacer que las curvas giren automaticamente al jugador
    //cuando es inmune
    bool isImmune;

    //Nos dice si el jugador puede atacar o no
    //solo se usa para realizar la animacion cuando
    //vaya a realizar el ataque y no se repita la animacion
    bool isAttack;

    //Se usa para comprobar que puede destruir obstaculos
    //mientras está atacando, para no estrellarse con un
    //obstaculo sino destruirlo
    bool isDestroy;

    //Nos dice si tiene el escudo activado o no
    bool isShield;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animatorController = GetComponent<Animator>();
        rotation = transform.GetChild(0);
        isRunning = false;

        //Posiciona los limites al lado del jugador
        StartLimits();
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

        //Este será el vector de movimiento final de nuestro personaje
        Vector3 moveTarget = Vector3.zero;

        //Si el personaje está en el suelo o no
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

        moveTarget = transform.forward + transform.right * Input.GetAxis("Horizontal");
        //El Vector en el eje x será la direccion de lo que se esté presionando (-1, 0, 1) por
        //la velocidad de movimiento

        moveTarget *= speed;

        //El vector en el eje y será en este momento la vel vertical
        moveTarget.y = verticalVelocity;

        //Aplicamos el movimiento a nuestro character controller
        controller.Move(moveTarget * Time.deltaTime);

        #region Rotamos un poco al personaje cuando cambie de carril

        //Le asignamos la velocidad que lleva nuestro personaje
        //para tomarla como referencia de la direccion que lleva
        Vector3 dir = controller.velocity;

        //Definimos la direccion en el eje 'y' como cero
        dir.y = 0;

        //Se rota al objeto dentro del jugador
        //para que no afecte el movimiento hacia adelante
        rotation.forward = Vector3.Lerp(rotation.forward, dir, TURN_SPEED);

        #endregion

        //Jugador se agacha una sola vez y solo si está en el suelo
        if(Input.GetKeyDown(KeyCode.DownArrow) && !isCrouch && isGrounded){
            isCrouch = true;
            StartCoroutine(Crouch());
        }

        //Si el jugador puede atacar una sola vez y presiona la w para atacar
        if(isAttack && Input.GetKeyDown(KeyCode.W)){
            //Hacemos isAttack false para que esté lista de una vez
            //para la animacion de ataque si se agarra otro powerup de ataque mientras
            //está atacando 
            isAttack = false;
            
            //isDestroy se pone a true y nos dice que puede destruir los obstaculos
            //con los que choca el personaje
            isDestroy = true;

            //Inicia el ataque del jugador
            StartCoroutine(Attack());
        }
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
        animatorController.SetFloat("Speed", speed);
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
        if(hit.gameObject.CompareTag("Obstacle")){
            //Si choca con obstaculos y tiene el escudo
            //vuelve el collider del obstaculo a trigger
            //para que lo atraviese
            if(isShield){
                hit.collider.isTrigger = true;
                return;
            }
            
            //Si puede destruir, destrulle el obstaculo con el que choca
            else if(isDestroy){
                Destroy(hit.gameObject);
            }

            //Si no muere
            else{
                Crash();
            }
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
                                            (controller.bounds.center.z + controller.bounds.extents.z)), transform.forward);

        RaycastHit hit;

        Debug.DrawRay(forwardRay.origin, forwardRay.direction * 3.5f, Color.red);

        if(Physics.Raycast(forwardRay, out hit, 3.5f, LayerMask.GetMask("BigJump"))){
            return true;
        }

        return false;
    }

    //Metodo que realiza la animacion de deslizarse en el suelo y
    // bajar el collider para simular que está agachado
    private IEnumerator Crouch(){
        animatorController.SetTrigger("Slide");

        controller.height /= 2;
        controller.center /= 2;

        //Levanta al jugador en un tiempo aproximado a lo que dura la
        //animación de deslizarse
        yield return new WaitForSeconds(1.2f);
        
        //levanta al jugador colocando de nuevo la altura y
        //centro del controller
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

        //Se asigna el ultimo tile hasta donde el jugador es inmune
        LevelManager.sharedInstance.SetLastImmunityTile();
        isImmune = true;
        StartCoroutine(InmunityShield());
    }

    //Cambia la rotacion del jugador a la nueva direccion
    //se llama desde ChangeDirection.cs
    public void ChangeDirection(int direction, float degrees){
        transform.rotation *= Quaternion.Euler(0, degrees * direction, 0);
    }

    //Centra al jugador en la posicion correcta antes de seguir avanzando
    //cuando hay un giro. Se llama desde ChangeDirection.cs
    public void PerfectCenter(Vector3 center){
        //Primero desactivamos el controller para poder centrarlo con transform
        controller.enabled = false;
        //Se centra el jugador justo en la posicion del tile de la curva
        transform.position = new Vector3(center.x, transform.position.y, center.z);
        //Se vuelve activar el controller para que se siga moviendo
        controller.enabled = true;
    }

    //Inicializa la posicion de los colliders de los limites en el mapa
    void StartLimits(){
        GameObject playerLimits = GameObject.Find("PlayerLimits");

        BoxCollider[] limitsCollider = playerLimits.GetComponents<BoxCollider>();

        //Limite izquierdo
        limitsCollider[0].center = new Vector3(-DISTANCE, limitsCollider[0].center.y, limitsCollider[0].center.z);

        //Limite derecho
        limitsCollider[1].center = new Vector3(DISTANCE, limitsCollider[1].center.y, limitsCollider[1].center.z);
    }

    #region Metodos Geters y Seters del jugador

    //Retorna si el jugador está corriendo o no
    public bool GetIsRunning(){
        return isRunning;
    }

    //Retorna si el jugador es inmune o no
    public bool GetImmune(){
        return isImmune;
    }

    //Retorna si el jugador tiene escudo o no
    public bool GetShield(){
        return isShield;
    }

    //Retorna si el jugador está destruyendo o no
    public bool GetDestroy(){
        return isDestroy;
    }

    //Asigna el valor a isImmune
    public void SetImmune(bool isImmune){
        this.isImmune = isImmune;
    }

    //Asigna el valor a isAttack
    public void SetAttack(bool isAttack){
        this.isAttack = isAttack;
    }
    
    //Asigna el valor a isShield
    public void SetShield(bool isShield){
        this.isShield = isShield;
    }

    #endregion


    #region Animaciones de immunity, attack y shield

    //Hace la animacion del ataque
    IEnumerator Attack(){
        //Realizamos la animacion de attack
        animatorController.SetTrigger("Attack");

        //Velocidad en 0 para dar un "efecto" como de preparacion para el golpe
        speed = 0f;
        yield return new WaitForSeconds(1f);

        //Despues de un segundo se aumenta la velocidad para que de el efecto de impulso
        speed = 20f;
        yield return new WaitForSeconds(0.5f);

        //Despues de un tiempo se coloca la velocidad normal del movimiento
        speed = 5f;

        //Despues del efecto de atacar, se vuelve isDestroy a false
        //para que ya no pueda destruir obstaculos
        isDestroy = false;
    }

    //Hace la animacion del escudo
    public IEnumerator Shield(float time){
        //Se guarda el gameObject shield dentro del jugador
        GameObject shield = transform.GetChild(1).gameObject;

        //Se muestra el escudo
        shield.SetActive(true);
        yield return new WaitForSeconds(time);

        //Despues del tiempo de duracion del escudo se deja de mostrar
        shield.SetActive(false);

        //Y se desactiva el escudo, para que pueda chocar
        isShield = false;
    }

    //Hace la animacion de la inmunidad
    IEnumerator InmunityShield(){
        //Se guarda el gameObject shield dentro del jugador
        GameObject shield = transform.GetChild(1).gameObject;

        //Se guarda el color inicial del escudo
        Color initShieldColor = shield.GetComponent<Renderer>().material.color;

        //Color que tendrá el escudo cuando es inmune
        Color inmmuneShieldColor = new Color32(246, 255, 146, 89);

        //Mientras que sea inmune muestro el escudo de color amarillo
        while(isImmune){
            shield.GetComponent<Renderer>().material.color = inmmuneShieldColor;
            shield.SetActive(true);
            yield return null;
        }

        //Cuando deja de ser inmune el jugador desactivo el escudo para que no se vea
        shield.SetActive(false);

        //Y lo regreso a su color inicial (que es el color morado del powerup)
        shield.GetComponent<Renderer>().material.color = initShieldColor;
    }

    #endregion
}
