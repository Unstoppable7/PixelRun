using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager sharedInstance { set; get; }

    //Referencia al playerMotor para saber cuando empieza a moverse
    PlayerMotor motor;

    //Referencia a la camara que seguirá al jugador para centrarla cuando hay un giro
    CameraFollow camera;

    //Referencia de juego iniciado
    public bool IsGameStarted { get; set; }


    #region Variables para la puntuacion

    //Propiedad a la puntuacion de cada partida
    public float Score { get; private set; }
    //Propiedad al nro de monedas recogidas
    public float Coins { get; private set; }
    //Propiedad a la puntuacion maxima obtenida
    public float HighScore { get; private set; }

    #endregion

    #region Variables UI

    //Texto de la puntuacion
    [SerializeField]
    Text scoreText;
    //Texto de la cant de monedas
    [SerializeField]
    Text coinsText;

    //Cuenta regresiva para reanudar el juego
    [SerializeField]
    Text countDownResumeText;

    //Boton de pausa    
    public GameObject pauseButton;

    //Menu luego de morir    
    public GameObject DeathMenu;

    #endregion

    private void Awake()
    {
        if (!sharedInstance)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //Inicializamos la variable en false porque el juego no ha comenzado
        IsGameStarted = false;

        //Asignamos la referencia al script playerMotor del jugador
        motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();

        //Asignamos al script de CameraFollow de la camara
        camera = Camera.main.GetComponent<CameraFollow>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Actualizo inicialmente la UI de las puntuaciones
        UpdateTextScore();
    }

    // Update is called once per frame
    void Update()
    {
        //Si el juego ya inició
        if (IsGameStarted)
        {
            //Cada medio segundo el jugador va aumentando su puntuacion
            Score += (Time.deltaTime * 2);
            //Asignamos al objeto Texto la puntacion formateada a un digito
            scoreText.text = Score.ToString("0"); //MEJORAR

            //Imprimimos por pantalla nuestra puntuacion con un solo digito
            //print("Puntacion: " + Score.ToString("0"));
        }
    }

    //Metodo para inciar el juego
    public void StartGame()
    {
        IsGameStarted = true;
        motor.StartRun();
    }

    //Metodo para recoger las monedas o coleccionables
    //collectableAmount: Me indica que valor tiene el objeto recogido
    public void GetCollectable(int collectableAmount)
    {
        //Aumentamos el nro de monedas recogidas
        Coins++;
        //Aumentamos la puntuacion del jugador
        Score += collectableAmount;

        //Actualizamos la UI de la puntacion
        UpdateTextScore();
    }

    //Metodo para terminar el juego
    public void GameOver()
    {
        //Totalizamos la puntacion final sumandole el nro de monedas recogidas
        HighScore += Score + Coins;

        //Detenemos el juego
        //IsGameStarted = false;

    }

    //Metodo para reiniciar el nivel
    public void PlayAgain()
    {
        //Invocamos el metodo para recargar la escena con un retaso de 1sec
        Invoke("LoadScene", 1f);
    }

    //Metodo para recargar la escena actual (Lo hacemos para poder llamarlo
    //mediante el Invoque)
    public void LoadScene()
    {
        //Cargamos la primera scene guardada (la escena del juego)
        SceneManager.LoadScene(0);
    }

    //Metodo que actualiza la puntuacion en la UI
    public void UpdateTextScore()
    {
        //Asignamos al objeto Texto la puntacion formateada a un digito
        scoreText.text = Score.ToString("0");

        //Asignamos al objeto Texto la cant de monedas formateada a un digito
        coinsText.text = Coins.ToString("0");
    }

    //Metodo llamado por el boton de la UI "Pause Button" para pausar el juego
    public void PauseGame()
    {
        //Pausamos el juego
        IsGameStarted = false;
        //Detenemos al personaje
        motor.StopRun();
    }

    //Metodo que será llamado cuando querramos reanudar el juego luego de 
    //una pausa (presionando el 'Resume Button')
    public void ResumeGame(bool ready = false)
    {
        //Si entra por primera vez
        if (!ready)
        {
            //Activamos el texto del temporizador
            countDownResumeText.gameObject.SetActive(true);
            //Iniciamos la corrutina para el temporizador
            StartCoroutine("CountDownResume");
        }        
        //Si el temporizador acabó
        if (ready)
        {
            //Detenemos la corrutina del temporizador
            StopCoroutine("CountDownResume");
            //Desactivamos el texto del temporizador
            countDownResumeText.gameObject.SetActive(false);
            //Iniciamos el juego 
            StartGame();
            //Reactivamos el boton de pausa
            pauseButton.gameObject.SetActive(true);
        }
    }

    //Corrutina que llevará a cabo el temporizador para reanudar el juego
    public IEnumerator CountDownResume()
    {
        //Ciclo que decrementa cada numero a mostrar
        for(int i = 3; i >=1 ; i-- )
        {
            //Asignamos el valor del ciclo al texto del temporizador
            countDownResumeText.text = i.ToString();
            //Retornamos la corrutina durante 1 segundo
            yield return new WaitForSeconds(1f);
        }
        //Volvemos a llamar al metodo de reanudar partida con el parametro
        //que permite llevar a cabo esto
        ResumeGame(true);
    }

    public void ContinueGame()
    {
        //Reanudamos la partida
        ResumeGame();

        //Llamo al metodo para revivir al personaje
        motor.Revive();
    }

    #region Encapsulamiento de metodos de la camara para acceder a ellos de manera publica desde GameManager

    //Cambia la direccion de la camara
    //se llama desde ChangeDirection.cs
    public void ChangeCameraDirection(int direction, float degrees){
        StartCoroutine(camera.ChangeDirection(direction, degrees));
    }

    //Centra la camara cuando gira el jugador
    //se llama desde ChangeDirection.cs
    public void PerfectCameraCenter(Vector3 center){
        camera.PerfectCenter(center);
    }

    #endregion

    #region Encapsulamiento de metodos del jugador para acceder a ellos de manera publica desde GameManager

    //Cambia la direccion del jugador
    //se llama desde ChangeDirection.cs
    public void ChangePlayerDirection(int direction, float degrees){
        motor.ChangeDirection(direction, degrees);
    }

    //Coloca al jugador exactamente en el centro despues de girar
    //se llama desde ChangeDirection.cs
    public void PerfectPlayerCenter(Vector3 center){
        motor.PerfectCenter(center);
    }

    //Retorna si el jugador está corriendo o no
    //se llama desde ChangeDirection.cs
    public bool IsPlayerRunning(){
        return motor.GetIsRunning();
    }

    //Asigna la inmunidad del jugador
    //se llama en LevelManager.cs
    public void SetImmunePlayer(bool isImmune){
        motor.SetImmune(isImmune);
    }

    //Regresa si el jugador es inmune o no
    //se llama desde ChangeDirection.cs
    public bool GetImmunePlayer(){
        return motor.GetImmune();
    }

    //Hace que el jugador pueda atacar
    //se llama desde CollectableAttack.cs
    public void SetPlayerAttack(bool isAttack){
        //Si está atacando, que no ataque de nuevo para que no se interrumpa
        //la animacion ni la velocidad del ataque actual
        if(motor.GetDestroy()){
            return;
        }
        motor.SetAttack(isAttack);
    }

    //Hace que el jugador active su escudo
    //se llama desde CollectableShield.cs
    public void SetPlayerShield(bool isShield, float time){
        //Si ya tiene un escudo activo o es inmune no se sobrepone otro escudo
        if(motor.GetShield() || motor.GetImmune()){
            return;
        }
        motor.SetShield(isShield);
        StartCoroutine(motor.Shield(time));
    }

    //Retorna si el jugador tiene escudo o no
    //se llama desde ChangeDirection.cs
    public bool GetPlayerShield(){
        return motor.GetShield();
    }

    #endregion
}
