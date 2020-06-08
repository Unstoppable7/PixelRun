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

    //Referencia de juego iniciado
    public bool IsGameStarted { get; set; }


    #region Variables para la puntuacion

    //Propiedad a la puntuacion de cada partida
    public float Score { get; set; }
    //Propiedad al nro de monedas recogidas
    public float Coins { get; set; }
    //Propiedad a la puntuacion maxima obtenida
    public float HighScore { get; set; }

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
        motor = GameObject.FindGameObjectWithTag("Player")
                .GetComponent<PlayerMotor>();
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
}
