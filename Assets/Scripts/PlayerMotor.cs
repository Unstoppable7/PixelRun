using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    //Distancia entre c/u de los limites
    const float DISTANCE = 2.5f;

    //Velocidad de rotacion del personaje cuando cambia de carril
    //o cuanto queremos que rote
    const float TURN_SPEED = 0.05f;

    #region Variables de movimiento

    //Este será el vector de movimiento final de nuestro personaje
    Vector3 moveTarget;
    //Fuerza de salto
    float jump = 7.5f;

    //Gravedad
    float gravity = 20f;

    //Velocidad vertical, define la vel con la que el personaje iniciará
    //su descenso despues de saltar
    float verticalVelocity;
    [SerializeField]
    //Velocidad del personaje
    float speed = 15f;

    //Tiempo en segundos que dura en acelerar desde la velocidad inicial a la maxima
    float accelerationTime = 0.01f;

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

    //Variable que guarda la velocidad actual que tiene el juego
    //se usa para mantener el tiempo de velocidad actual cuando muere
    //y reanudar a la misma velocidad
    float initTime;
    //Booleano que me indica si el giroscopio está disponible y activo
    //en el movil
    bool gyroEnabled;

    //Giroscopio del movil
    Gyroscope gyro;

    //Quaternion para manejar el giroscopio
    Quaternion rot;

    #region Variables del double touch

    //Nos dice si el jugador está ahogado o no
    bool isDrowned;
    private const float DOUBLE_TOUCH_TIME = 0.2f;
    private float lastTouchTime = 0;

    #endregion

    #region Variables del Swipe

    Vector2 startTouchPosition, endTouchPosition;
    //public bool swipeUp { get; private set; } = false;
    //public bool swipeDown { get; private set; } = false;
    //TODO al cambiar el input de girar 90° del change direction aqui
    //hacer private el set
    //public bool swipeRight { get; set; } = false;
    //public bool swipeLeft { get; set; } = false;

    #endregion

    //Vector3 con los valores limitados del acelerometro
    float inputAccelerometer;

    //Velocidad horizontal a aplicar cuando el jugador gire el telefono
    public float sensitivityAccelerometer { get; set; } = 1.5f;

    public float move { get; set; } = 0;

    double lastMove;

    //Los datos del personaje que usará el jugador
    Character character;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        //Actualiza al personaje con sus animaciones y el modelo
        ChangePlayerCharacter();

        isRunning = false;

        //Referenciamos la variable del giroscopio 
        //dependiendo del resultado del metodo
        gyroEnabled = EnableGyro();

        //Posiciona los limites al lado del jugador
        StartLimits();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(gyro.attitude);

        //Debug.Log("Acelerometro: " + Input.acceleration);

        //Si el personaje no está corriendo (al inicio del juego) no
        //hacemos nada
        bool isGrounded = IsGrounded();

        //Si el personaje no está corriendo 
        if (!isRunning)
        {
            //Si no está tocando el suelo se le aplica solo la gravedad
            //Para que caiga cuando se estrelle y no quede en el aire
            //y tambien solo si no está "ahogado" para que no siga bajando siempre
            //si el jugador se ahoga y tarda en dar la opcion de continuar o salir
            if (!isGrounded && !isDrowned)
            {
                verticalVelocity -= (gravity * Time.deltaTime);
                controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            }
            return;
        }

        //Este será el vector de movimiento final de nuestro personaje
        moveTarget = Vector3.zero;

        //Si el personaje está en el suelo o no
        isGrounded = IsGrounded();

        //Cambiamos el parametro si ha tocado el suelo
        animatorController.SetBool("isGrounded", isGrounded);

        //TRY copiar todo este codigo a una funcion y llamarlo en el metodo del swipe
        //Si el personaje está en el suelo 
        if (isGrounded)
        {
            //Definimos la vel vertical como -0.1f 
            verticalVelocity = -0.1f;

            bool bigJump = isBigJump();

            //Revisamos si se realizó un swipe y no está atacando                   
            //Si presiona la barra espaciadora o hace swipe hacia arriba
            if ((Input.GetKeyDown(KeyCode.Space) || SwipeCheck(1)) && !isDestroy)
            {
                //SwipeUp en falso
                //swipeUp = false;

                //Asignamos como velocidad vertical la fuerza de salto
                verticalVelocity = jump;

                //Si delante del jugador hay un obstaculo para realizar el salto largo
                if (bigJump)
                {
                    animatorController.SetTrigger("bigJump");
                }

                else
                {
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

        //El movimiento viene dado por, hacia adelante por el transform forward
        //(0,0,1) que luego se le multiplicará la velocidad (speed) y hacia los 
        //lados por el transform right (1,0,0) por en caso de presionar los
        //botones configurados en el editor por el axis 'horizontal' o por
        //el eje x del acelerometro del movil este multiplicado por una variable
        //que aumenta la sensibilidad, al ser transform.right estamos tomando en
        //cuenta la x local del personaje, cuando el gira cambian son sus variables
        //globales        

        /**
            //Con Giroscopio
            moveTarget = (transform.forward + (transform.right
                                * ((Input.GetAxis("Horizontal") != 0) ?
                                Input.GetAxis("Horizontal") :
                                (-gyro.rotationRateUnbiased.z) * sensitivityGyro) ) );
        
        **/

        AcelerometerMove();

        //Con Acelerometro
        moveTarget = (transform.forward + (transform.right
                            * ((Input.GetAxis("Horizontal") != 0) ?
                            Input.GetAxis("Horizontal") :
                            (move) * sensitivityAccelerometer)));

        /**
         //Con Acelerometro
         moveTarget = (transform.forward + (transform.right
                             * ((Input.GetAxis("Horizontal") != 0) ?
                             Input.GetAxis("Horizontal") :
                             (Input.acceleration.x) * sensitivityAccelerometer)));
         **/


        //moveTarget = transform.forward;


        //transform.position = Vector3.MoveTowards(transform.position, (transform.forward + transform.right * Input.acceleration.x)*20, Time.deltaTime*5);
        //Vector3 vector = transform.right * Input.acceleration.x * 2;
        //Debug.Log(Vector3.MoveTowards(transform.position, vector, Time.deltaTime));
        //El Vector en el eje x será la direccion de lo que se esté presionando (-1, 0, 1) por
        //la velocidad de movimiento
        moveTarget *= speed;

        //El vector en el eje y será en este momento la vel vertical
        moveTarget.y = verticalVelocity;

        //transform.position = Vector3.MoveTowards(transform.position, moveTarget, Time.deltaTime);
        //Aplicamos el movimiento a nuestro character controller
        controller.Move(moveTarget * Time.deltaTime);
        //controller.SimpleMove(moveTarget);

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
        if ((Input.GetKeyDown(KeyCode.DownArrow) || SwipeCheck(2)) && !isCrouch && isGrounded)
        {
            //swipeDown = false;        
            isCrouch = true;
            StartCoroutine(Crouch());
        }

        //Si el jugador puede atacar una sola vez y presiona la w o hace 
        //double touch para atacar
        if (isAttack && (Input.GetKeyDown(KeyCode.W) || CheckDoubleTap()))
        {

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

        //Se aumenta la velocidad del juego poco a poco
        Time.timeScale = Mathf.Lerp(Time.timeScale, 1.5f, Time.deltaTime * accelerationTime);

        //Si el jugador se ahoga
        if (transform.position.y < -5 && !isDrowned)
        {
            isDrowned = true;
            Crash();
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
        animatorController.SetTrigger("StartRun");

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
            //Si choca con obstaculos y tiene el escudo
            //vuelve el collider del obstaculo a trigger
            //para que lo atraviese
            if (isShield)
            {
                hit.collider.isTrigger = true;
                return;
            }

            //Si puede destruir, destruye el obstaculo con el que choca
            else if (isDestroy)
            {
                Destroy(hit.gameObject);
            }

            //Si no muere
            else
            {
                Crash(hit.collider);
            }
        }

        //Si se estrella con algo que no se puede destruir
        else if (hit.gameObject.CompareTag("Indestructible"))
        {
            //Y no tiene el escudo ni es inmune entonces muere
            if (!isShield && !isImmune)
            {
                Crash(hit.collider);
            }

            //Si no traspasa el objeto
            else
            {
                hit.collider.isTrigger = true;
            }
        }
    }

    //Metodo de muerte del personaje
    void Crash(Collider collider = null)
    {
        //Se destruye el collider del objecto con el que colisiona
        //ya que como se aplica la gravedad cuando revive
        //si queda encima de un collider no logra hacer la animacion de morir
        Destroy(collider);

        //Cambiamos la velocidad del juego para que aumente de velocidad las animaciónes
        //y todo a la vez, asi no es necesario aumentar la velocidad del jugador
        //ya que se moveria muy rapido y no estaría uniforme con los tiles del mapa

        //Guardamos la velocidad actual antes de hacer cualquier cosa
        initTime = Time.timeScale;

        //Se le da la velocidad normal del juego
        Time.timeScale = 1;

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
    private bool isBigJump()
    {
        Ray forwardRay = new Ray(new Vector3(controller.bounds.center.x,
                                            controller.bounds.center.y,
                                            (controller.bounds.center.z + controller.bounds.extents.z)), transform.forward);

        RaycastHit hit;

        Debug.DrawRay(forwardRay.origin, forwardRay.direction * 3.5f, Color.red);

        if (Physics.Raycast(forwardRay, out hit, 3.5f, LayerMask.GetMask("BigJump")))
        {
            return true;
        }

        return false;
    }

    //Metodo que realiza la animacion de deslizarse en el suelo y
    // bajar el collider para simular que está agachado
    private IEnumerator Crouch()
    {
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
    public void Revive(bool continueGame)
    {
        //Activo el trigger para que el personaje retome la animacion de pie
        animatorController.SetTrigger("Revive");

        //Detenemos al personaje
        StopRun();

        if (continueGame)
        {
            //Agrego inmunidad al player en los obstaculos actuales en la escena
            Inmunity();

            //Despues que hace todo lo de revivir a velocidad normal
            //se regresa a la velocidad con la que venia corriendo
            Time.timeScale = initTime;
            //Posicionamos en el centro al player
            ResetPositionX();
        }
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
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
            }
        }

        //Guarda los objectos con el tag Indestructible
        GameObject[] indestructibles = GameObject.FindGameObjectsWithTag("Indestructible");

        //Recorro el array de indestructibles
        foreach (GameObject indestructible in indestructibles)
        {
            //Si el object indestructible pertenece a el cambio de direccion
            //no destruye sus colliders
            if (!indestructible.transform.parent.CompareTag("ChangeDirection"))
            {
                Collider[] colliders = indestructible.GetComponents<Collider>();

                //Vuelvo trigger los colliders de los indestructibles
                //Desactiva los que tienen varios colliders
                foreach (Collider collider in colliders)
                {
                    collider.isTrigger = true;
                }
            }
        }

        //Se asigna el ultimo tile hasta donde el jugador es inmune
        LevelManager.sharedInstance.SetLastImmunityTile();
        isImmune = true;
        StartCoroutine(InmunityShield());
    }

    //Cambia la rotacion del jugador a la nueva direccion
    //se llama desde ChangeDirection.cs
    public void ChangeDirection(int direction, float degrees)
    {
        transform.rotation *= Quaternion.Euler(0, degrees * direction, 0);
    }

    //Centra al jugador en la posicion correcta antes de seguir avanzando
    //cuando hay un giro. Se llama desde ChangeDirection.cs
    public void PerfectCenter(Vector3 center)
    {
        //Primero desactivamos el controller para poder centrarlo con transform
        controller.enabled = false;
        //Se centra el jugador justo en la posicion del tile de la curva
        transform.position = new Vector3(center.x, transform.position.y, center.z);
        //Se vuelve activar el controller para que se siga moviendo
        controller.enabled = true;
    }

    //Inicializa la posicion de los colliders de los limites en el mapa
    void StartLimits()
    {
        GameObject playerLimits = GameObject.Find("PlayerLimits");

        BoxCollider[] limitsCollider = playerLimits.GetComponents<BoxCollider>();

        //Limite izquierdo
        limitsCollider[0].center = new Vector3(-DISTANCE, limitsCollider[0].center.y, limitsCollider[0].center.z);

        //Limite derecho
        limitsCollider[1].center = new Vector3(DISTANCE, limitsCollider[1].center.y, limitsCollider[1].center.z);
    }

    #region Metodos Geters y Seters del jugador

    //Retorna si el jugador está corriendo o no
    public bool GetIsRunning()
    {
        return isRunning;
    }

    //Retorna si el jugador es inmune o no
    public bool GetImmune()
    {
        return isImmune;
    }

    //Retorna si el jugador tiene escudo o no
    public bool GetShield()
    {
        return isShield;
    }

    //Retorna si el jugador está destruyendo o no
    public bool GetDestroy()
    {
        return isDestroy;
    }

    //Asigna el valor a isImmune
    public void SetImmune(bool isImmune)
    {
        this.isImmune = isImmune;
    }

    //Asigna el valor a isAttack
    public void SetAttack(bool isAttack)
    {
        this.isAttack = isAttack;
    }

    //Asigna el valor a isShield
    public void SetShield(bool isShield)
    {
        this.isShield = isShield;
    }

    //Regresa si está ahogado o no
    public bool GetDrowned()
    {
        return isDrowned;
    }

    #endregion


    #region Animaciones de immunity, attack y shield

    //Hace la animacion del ataque
    IEnumerator Attack()
    {
        //Realizamos la animacion de attack
        animatorController.SetTrigger("Attack");

        float initSpeed = speed;

        //Velocidad en 0 para dar un "efecto" como de preparacion para el golpe
        speed = 0f;
        yield return new WaitForSeconds(1f);

        //Despues de un segundo se aumenta la velocidad para que de el efecto de impulso
        speed = 20f;
        yield return new WaitForSeconds(0.5f);

        //Despues de un tiempo se coloca la velocidad normal del movimiento
        speed = initSpeed;

        //Despues del efecto de atacar, se vuelve isDestroy a false
        //para que ya no pueda destruir obstaculos
        isDestroy = false;
    }

    //Hace la animacion del escudo
    public IEnumerator Shield(float time)
    {
        //Se guarda el gameObject shield dentro del jugador
        GameObject shield = transform.Find("Shield").gameObject;

        //Se muestra el escudo
        while (isShield && time > 0)
        {
            shield.SetActive(true);
            time -= Time.deltaTime;
            yield return null;
        }

        //Despues del tiempo de duracion del escudo se deja de mostrar
        shield.SetActive(false);

        //Y se desactiva el escudo, para que pueda chocar
        isShield = false;
    }

    //Hace la animacion de la inmunidad
    IEnumerator InmunityShield()
    {
        //Se guarda el gameObject shield dentro del jugador
        GameObject shield = transform.Find("Shield").gameObject;
        GameObject[] holes = GameObject.FindGameObjectsWithTag("Hole");

        foreach (GameObject hole in holes)
        {
            //activa el collider de los huecos en el agua para que no se caiga
            hole.GetComponent<BoxCollider>().enabled = true;
        }

        //Se guarda el color inicial del escudo
        Color initShieldColor = shield.GetComponent<Renderer>().material.color;

        //Color que tendrá el escudo cuando es inmune
        Color inmmuneShieldColor = new Color32(246, 255, 146, 89);

        //Mientras que sea inmune muestro el escudo de color amarillo
        while (isImmune)
        {
            shield.GetComponent<Renderer>().material.color = inmmuneShieldColor;
            shield.SetActive(true);
            yield return null;
        }

        //Cuando deja de ser inmune el jugador desactivo el escudo para que no se vea
        shield.SetActive(false);

        //Y lo regreso a su color inicial (que es el color morado del powerup)
        shield.GetComponent<Renderer>().material.color = initShieldColor;
    }


    //Saca al jugador del agua. Se llama desde GameManager antes de empezar la coroutina
    //del contador para volver a jugar
    public void OutWater()
    {
        isDrowned = false;
        controller.enabled = false;
        transform.position = new Vector3(transform.position.x, 5f, transform.position.z);
        controller.enabled = true;
    }

    #endregion

    //Metodo que me verifica si el dispositivo tiene giroscopio y de ser asi
    //lo activa
    private bool EnableGyro()
    {
        //Si el dispositivo soporta giroscoio
        if (SystemInfo.supportsGyroscope)
        {
            //Referenciamos la variable a el input del dispositivo
            gyro = Input.gyro;
            //Habilitamos el giroscopio
            gyro.enabled = true;

            return true;
        }
        return false;
    }

    //Metodo para revisar cuando se realiza un double touch
    private bool CheckDoubleTap()
    {
        //Si se realiza al menos un touch
        if (Input.GetMouseButtonDown(0))
        {
            //Almaceno el tiempo desde el ultimo touch
            float timeSinceLastTouch = Time.time - lastTouchTime;

            //Si el tiempo desde el ultimo touch es menor o igual al
            //tiempo estimado ha considerarse como double touch
            if (timeSinceLastTouch <= DOUBLE_TOUCH_TIME && Time.time > DOUBLE_TOUCH_TIME + 1)
            {
                lastTouchTime = 0;
                return true;
            }
            //Asigno el tiempo en que se realizó el ultimo touch desde que 
            //comenzó la aplicacion
            lastTouchTime = Time.time;
        }
        return false;

    }

    //Metodo para verificar si se realizo un swap
    //direction: Arriba = 1, Abajo = 2, Derecha = 3, Izquierda = 4
    public bool SwipeCheck(int direction)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float largeOfSwipe = 150f;

            //bool band = true;
            //Si hay al menos un touch y este está en fase de inicio recien tocado
            //sin soltarse, guardo la posicion inicial de ese primer touch
            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }

            //Si hay al menos un touch y este está en fase de salida o recien soltado
            if (touch.phase == TouchPhase.Moved)
            {
                //Guardo la posicion final de este touch
                endTouchPosition = touch.position;

                if (endTouchPosition.y - largeOfSwipe > startTouchPosition.y && direction == 1)
                {
                    //swipeUp = true;
                    return true;
                }

                if (endTouchPosition.y + largeOfSwipe < startTouchPosition.y && direction == 2)
                {
                    //swipeDown = true;
                    return true;

                }

                if (endTouchPosition.x - largeOfSwipe > startTouchPosition.x && direction == 3)
                {
                    //swipeRight = true;
                    return true;

                }

                if (endTouchPosition.x + largeOfSwipe < startTouchPosition.x && direction == 4)
                {
                    //swipeLeft = true;
                    return true;

                }
            }
        }
        return false;
    }

    //Metodo que posiciona al jugador en el punto central del mapa.
    public void ResetPositionX()
    {
        transform.position = (new Vector3(0, transform.position.y, 0));
    }

    //Metodo que posiciona al jugador en el punto inicial del juego.
    public void ResetPosition()
    {
        transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 0, 0));
    }

    public void OffAllAbilities()
    {
        isAttack = false;
        isShield = false;
        isImmune = false;


        //Se guarda el gameObject shield dentro del jugador
        GameObject shield = transform.Find("Shield").gameObject;

        //Se muestra el escudo
        shield.SetActive(false);
    }

    //Metodo para reiniciar los triggers de las animaciones StartRun y Revive
    //sucedia que si tomaba un escudo y luego salia del juego hacia el menu, estos
    //trigers por alguna razon seguian activos entonces al volver a iniciar una partida
    //cuando el player moria, inmediatamente se levantaba y empezaba a correr sin presionar 
    //nada, esto lo soluciona llamandolo en al presionar el boton menu y restart
    //TODO mejorar de alguna forma
    public void ResetTriggers()
    {
        animatorController.ResetTrigger("StartRun");
        animatorController.ResetTrigger("Revive");
    }

    /**
    //Metodo que limita los valores del acelerometro
    public float AcelerometerMove()
    {
        //TRY que los valoren del acelerometro sean directamente la posicion
        //del player y manejar los limites etc;

        //Limitamos los valores del acelerometro
        inputAccelerometer = Input.acceleration.x;

        float inputLimit = 0.4f;
        float centerRange = 0.1f;
        float speedToCenter = 2f;
        float transformCenterRange = 0.1f;

        if( (inputAccelerometer>= -centerRange && inputAccelerometer <= centerRange) && (transform.position.x < -transformCenterRange || transform.position.x > transformCenterRange) )
        {
            Debug.Log("1");

            if (lastMove < -centerRange)
            {
                Debug.Log("moveRight");

                //moveTarget = (transform.forward + (transform.right
                //            * 0.01f));
                lastMove = inputAccelerometer;

                return speedToCenter;

            }else if(lastMove > centerRange)
            {
                Debug.Log("moveLeft");

                //moveTarget = (transform.forward + (transform.right
                //            * -0.01f));
                lastMove = inputAccelerometer;

                return -speedToCenter;
            }

        }else if (inputAccelerometer < -inputLimit)
        {
            Debug.Log("2");

            //inputAccelerometer = -0.3f;
            //move left
            //controller.Move(new Vector3(-SpeedX, moveTarget.y, moveTarget.z) * Time.deltaTime);
            //moveTarget.x = transform.right * new Vector3(-SpeedX, 0, 0);
            lastMove = inputAccelerometer;

            return -inputLimit;

        }else if (inputAccelerometer > inputLimit)
        {

            Debug.Log("3");

            //inputAccelerometer = 0.3f;
            //move right
            //controller.Move(new Vector3(SpeedX, moveTarget.y, moveTarget.z) * Time.deltaTime);
            //moveTarget.x = SpeedX;
            lastMove = inputAccelerometer;

            return inputLimit;
        }
        
        //if(lastMove - inputAccelerometer < 0.00001 || lastMove - inputAccelerometer > -0.00001)
        //{
        //    lastMove = inputAccelerometer;
        //    return 0;

        //}
        
        Debug.Log("NADA");

        lastMove = inputAccelerometer;
        return inputAccelerometer;
    }
    **/
    /**
    public void AcelerometerMove()
    {

        double input = Input.acceleration.x;
        double highLimit = 0.4, lowLimit = 0.03f;

        if(input < -highLimit)
        {
            input = -highLimit;
        }
        if (input > highLimit)
        {
            input = highLimit;
        }
        if(input < lowLimit && input > -lowLimit)
        {
            input = 0;
        }
        if (lastMove != input)
        {
            if ((lastMove - input) > lowLimit || (lastMove - input) < -lowLimit)
            {
                move = (float)Math.Round(input - lastMove, 2);
            }
            else
            {
                move = 0;
            }
        }         
        lastMove = input;

        //Debug.Log(gyro.attitude);        
        Debug.Log(move);

        //return (int) -move*10 ;
        //return (float) -Math.Round(move, 3)*3;
        
    }
    **/

    public void AcelerometerMove()
    {
        double input = Input.acceleration.x;

        double highLimit = 0.4f, lowLimit = 0.05f;

        if (input < -highLimit)
        {
            input = -highLimit;
        }
        if (input > highLimit)
        {
            input = highLimit;
        }
        if (input < lowLimit && input > -lowLimit)
        {
            input = 0;
        }
        if (lastMove != input)
        {
            move = (float)Math.Round(input, 2);
        }
        else
        {
            move = 0;
        }
    }

    //Actualiza el personaje con sus animaciones y su modelo
    public void ChangePlayerCharacter(){
        //Si hay un personaje activo lo elimina
        Transform currentCharacter = transform.Find("Character");

        if(currentCharacter){
            Destroy(currentCharacter.gameObject);
        }

        //Se actualiza el modelo y animator del personaje que tiene seleccionado (guardado)
        character = CharacterManager.sharedInstance.characters[GameManager.sharedInstance.PlayerCharacter];

        //Se crea el juego Character dentro del GameObject Player
        GameObject characterGameObject = Instantiate(character.prefab);
        characterGameObject.name = "Character";
        characterGameObject.transform.SetParent(transform);

        //Se actualiza los objetos de rotacion para girar al personaje cuando esté jugando
        rotation = characterGameObject.transform;

        //Y se actualiza el animator para poder realizar las animaciones
        animatorController = characterGameObject.GetComponent<Animator>();
    }
}