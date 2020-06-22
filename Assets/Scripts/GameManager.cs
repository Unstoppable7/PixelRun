using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, IShopCustomer
{
    //Singleton
    public static GameManager sharedInstance { set; get; }

    //TODO modificar los inputs en Change direction de forma que pueda 
    //acceder a ellos desde PlayerMotor no tener que hacer publico esta 
    //variable para saber si hubo swipe hacia los lados
    //Referencia al playerMotor para saber cuando empieza a moverse
    public PlayerMotor motor;

    //Referencia al manejador del nivel
    LevelManager levelManager;

    //Referencia a la camara que seguirá al jugador para centrarla cuando hay un giro
    public CameraFollow camera;

    //Posicion inicial de la camara
    GameObject initPositionCamera;

    //Referencia al UIManager
    public UIManager userInterfaceManager;

    //Referencia de juego iniciado
    public bool IsGameStarted { get; set; }

    bool applicationQuit = false;

    #region Variables de juego

    //Propiedad a la puntuacion de cada partida
    public float Score { get; private set; }
    //Propiedad al nro de monedas recogidas
    public int Coins { get; private set; }
    //Propiedad al nro de monedas recogidas inGame
    public int CurrentCoins { get; private set; }
    //Propiedad a la puntuacion maxima obtenida
    public float HighScore { get; private set; }

    //Guarda las coroutinas para poder detenerlas
    Coroutine coroutineShield;

    //Personaje actual del jugador
    [HideInInspector]
    public int PlayerCharacter;

    //Lista de personajes disponibles
    public CharacterData[] Characters;

#endregion

    public event EventHandler OnScoreAmountChanged;

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

        //Asignamos la referencia al script LevelManager
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();

        //Asignamos al script de CameraFollow de la camara
        camera = Camera.main.GetComponent<CameraFollow>();
        //Asignamos la pos inicial de la camara
        initPositionCamera = new GameObject();
        initPositionCamera.transform.position = camera.transform.position;
        initPositionCamera.transform.rotation = camera.transform.rotation;

        //Cargamos los datos locales del jugador
        LoadPlayerData();

        //Actualizamos las monedas disponibles del jugador
        userInterfaceManager.UpdateCoinsAvailable(Coins);
    }

    // Update is called once per frame
    void Update()
    {                   
        //Si el juego ya inició
        if (IsGameStarted)
        {
            //Cada medio segundo el jugador va aumentando su puntuacion
            Score += (Time.deltaTime * 2);
            //Llamamos al metodo que actualiza en tiempo real el score del jugador
            userInterfaceManager.UpdateTextScore(Score);

            //Imprimimos por pantalla nuestra puntuacion con un solo digito
            //print("Puntacion: " + Score.ToString("0"));
        }
    }

    //Metodo llamado cuando el focus de la aplicacion cambia, su valor entra
    //por la variable focus
    private void OnApplicationFocus(bool focus)
    {
        //Si pierde el foco de la app y el juego está iniciado
        if (!focus && IsGameStarted)
        {
            //Pausame el juego activando el boton de pausa
            userInterfaceManager.pauseButton.GetComponent<Button>()
                                                    .onClick.Invoke();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause || applicationQuit)
        {
            //Guardamos localmente los datos del jugador obtenidos en partida
            SaveSystem.SavePlayer(this);
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
        CurrentCoins++;
        //Aumentamos la puntuacion del jugador
        Score += collectableAmount;

        //Actualizamos la UI de la puntacion
        userInterfaceManager.ResfreshTextScore(CurrentCoins, Score);
        //TODO
        //OnScoreAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    //Metodo para terminar el juego
    public void GameOver(bool save = false)
    {
        if (save)
        {
            //Totalizamos la puntacion final sumandole el nro de monedas recogidas
            HighScore += Score + Coins;

            //Aumentamos las monedas del jugador con las modenas conseguidas 
            //en la partida
            Coins += CurrentCoins;

            //Actualizamos la UI
            userInterfaceManager.UpdateCoinsAvailable(Coins);

            //Guardamos localmente los datos del jugador obtenidos en partida
            SaveSystem.SavePlayer(this);

        }
        //Detenemos todas las corrutinas
        StopAllCoroutines();

        //Actualizamos toda la UI
        userInterfaceManager.RefreshUIAfterGameOver();

        //Reiniciamos el nivel
        PlayAgain();
    }

    void ResetGameVariables()
    {
        Score = 0;
        CurrentCoins = 0;
    }

    //Metodo para reiniciar el nivel
    void PlayAgain()
    {
        //Borramos todos los tiles activos
        levelManager.DeleteAllTiles();

        //Generamos nuevamente tiles.
        levelManager.InitSpawn();

        //Posicionamos al jugador en el inicio
        motor.ResetPosition();

        //Levantamos al player
        motor.Revive(false);

        //Desactivamos las habilidades del jugador
        motor.OffAllAbilities();

        //Restablecemos variables de juego
        ResetGameVariables();

        //Actualizo la UI
        userInterfaceManager.ResfreshTextScore(0, 0);

        //Restablecemos a la posicion inicial la camara
        ResetPositionCamera();
    }
    
    //Metodo para recargar la escena actual (Lo hacemos para poder llamarlo
    //mediante el Invoque)
    public void LoadScene()
    {
        //Cargamos la primera scene guardada (la escena del juego)
        SceneManager.LoadScene(0);
    }

    //Metodo llamado por el boton de la UI "Pause Button" para pausar el juego
    public void PauseGame()
    {
        //Pausamos el juego
        IsGameStarted = false;
        //Detenemos al personaje
        motor.StopRun();
    }

    //Metodo para continuar el juego
    public void ContinueGame()
    {   
        //Si el jugador está ahogado se saca del agua
        if(motor.GetDrowned()){
            //Saca al jugador del agua
            motor.OutWater();
        }

        //Reanudamos la partida
        ResumeGame();

        //Llamo al metodo para revivir al personaje
        motor.Revive(true);

    }

    
    //Metodo que será llamado cuando querramos reanudar el juego luego de 
    //una pausa (presionando el 'Resume Button')
    public void ResumeGame(bool ready = false)
    {
        //Si entra por primera vez
        if (!ready)
        {
            //Activamos el texto del temporizador
            userInterfaceManager.countDownResumeText.gameObject.SetActive(true);

            //Iniciamos la corrutina para el temporizador
            StartCoroutine("CountDownResume");
        }
        //Si el temporizador acabó
        if (ready)
        {
            //Detenemos la corrutina del temporizador
            StopCoroutine("CountDownResume");

            //Desactivamos el texto del temporizador
            userInterfaceManager.countDownResumeText.gameObject.SetActive(false);
            //Iniciamos el juego 
            StartGame();
            //Reactivamos el boton de pausa
            userInterfaceManager.pauseButton.gameObject.SetActive(true);
        }
    }

    //Corrutina que llevará a cabo el temporizador para reanudar el juego
    public IEnumerator CountDownResume()
    {
        //Ciclo que decrementa cada numero a mostrar
        for (int i = 3; i >= 1; i--)
        {
            //Asignamos el valor del ciclo al texto del temporizador
            userInterfaceManager.countDownResumeText.text = i.ToString();
            //Retornamos la corrutina durante 1 segundo
            yield return new WaitForSeconds(1f);
        }
        //Volvemos a llamar al metodo de reanudar partida con el parametro
        //que permite llevar a cabo esto
        ResumeGame(true);
    }

    //Metodo para cargar los datos almacenados localmente
    public void LoadPlayerData()
    {
        //Recuperamos los datos en el archivo local
        PlayerData data = SaveSystem.LoadPlayer();

        //Si los datos existen
        if (data != null)
        {
            //Asignamos los datos extraidos a las variables actuales
            Coins = data.Coins;
            PlayerCharacter = data.Character;
        }
        else
        {
            //Inicializamos en cero las variables que se estan guardando 
            //localmente en este momento del desarrollo
            CurrentCoins = 0;
            Coins = 0;
        }
        
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
        //Si tiene el escudo activado
        if(motor.GetShield()){
            StopCoroutine(coroutineShield);
        }

        //Se alarga el tiempo de duración del escudo
        motor.SetShield(isShield);
        coroutineShield = StartCoroutine(motor.Shield(time));
    }

    //Retorna si el jugador tiene escudo o no
    //se llama desde ChangeDirection.cs
    public bool GetPlayerShield(){
        return motor.GetShield();
    }

    #endregion

    #region Metodos implementados por la interfaz IShopCustomer

    //Compra el item y lo aplica
    public void BoughtItem(Item.ItemType itemType)
    {
        //Al comprar el item se llama la funcion que aplica el mismo
        switch (itemType)
        {
            case Item.ItemType.Attack: Debug.Log("Se compró el item Attack"); break;
            case Item.ItemType.Shield: Debug.Log("Se compró el item Shield"); break;
        }
    }

    //Revisa si tenemos suficiente dinero para comprar
    public bool TrySpendGoldAmount(int coinsAmount)
    {
        //Si el oro que tenemos es mayor o igual al que se quiere gastar
        if (Coins >= coinsAmount)
        {
            //Disminuimos nuestro oro actual
            Coins -= coinsAmount;
            //Llama al evento que modifica la UI del oro
            userInterfaceManager.UpdateCoinsAvailable(Coins);

            //FIX En este caso guardamos para efectos que en el editor se vea
            //el resultado pero en la app de android no deberia ser necesario
            //ya que se guarda al el salirse de la app (revisar)
            SaveSystem.SavePlayer(this);

            //Retornamos verdadero porque sí se logró comprar
            return true;
        }
        else
        {
            //No se pudo comprar porque no hay suficiente oro
            return false;
        }
    }

    #endregion

    //Metodo para restablecer la posicion inicial de la camara
    public void ResetPositionCamera()
    {
        camera.transform.SetPositionAndRotation(initPositionCamera.transform.position, 
                                                initPositionCamera.transform.rotation);
    }

    public void ResetGame()
    {
        PlayAgain();

        ResumeGame();
    }
}
