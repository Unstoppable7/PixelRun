using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;//Me ayudara a formatear el texto de la vida en tiempo real dentro del Update

//TODO cambiar la llamada a los metodos de actualizar la interfaz por medio de
//eventos y no referenciando esta clase en el GameManager
public class UIManager : MonoBehaviour
{
    #region inGame
    
    //Texto de la puntuacion    
    public TextMeshProUGUI scoreText;
    //Texto de la cant de monedas     
    public TextMeshProUGUI coinsText;
    //Texto de la cant de gemas     
    public TextMeshProUGUI gemsText;
    //Cuenta regresiva para reanudar el juego    
    public TextMeshProUGUI countDownResumeText;
    //Boton de pausa    
    public GameObject pauseButton;
    //Menu luego de morir    
    public GameObject DeathMenu;
    //Contenedor de la UI en el juego      
    public GameObject playingUI;
    #endregion

    #region inHome

    //Buttons
    public GameObject storeButton;
    public GameObject playButton;
    public GameObject menuButton;
    public GameObject backButton;
    public GameObject noNameButton;
    public GameObject homeButton;
    //Texto de la cant de monedas
    public TextMeshProUGUI totalCoins;
    //Texto de la cant de gemas
    public TextMeshProUGUI totalGems;
    //Panels
    public GameObject homePanel;
    public GameObject settingsPanel;
    public GameObject statsPanel;
    public GameObject storePanel;
    public GameObject buttonsPanel;
    //Menus
    public GameObject mainMenu;

    #endregion

    //Vamos a construir el texto en tiempo real para eso usamos esta funcion de la libreria mencionada arriba
    StringBuilder sb = new StringBuilder();    

    public void EnterPanel()
    {
        homeButton.SetActive(false);
        backButton.SetActive(true);
        storeButton.SetActive(false);
        playButton.SetActive(true);
        menuButton.SetActive(false);

    }

    public void EnterMenu()
    {
        menuButton.SetActive(false);
        homeButton.SetActive(true);
        storeButton.SetActive(false);
        playButton.SetActive(true);
        backButton.SetActive(false);

    }

    public void EnterHome()
    {
        homeButton.SetActive(false);
        menuButton.SetActive(true);
        storeButton.SetActive(true);
        playButton.SetActive(false);
        backButton.SetActive(false);
    }

    public void PlayGame()
    {
        settingsPanel.SetActive(false);
        storePanel.SetActive(false);
        statsPanel.SetActive(false);
        homePanel.SetActive(false);
        mainMenu.SetActive(false);
        buttonsPanel.SetActive(false);
        playingUI.SetActive(true);
        pauseButton.SetActive(true);
    }
    public void BackButton()
    {
        CharacterManager.sharedInstance.HiddenChangeCharacter();
        
        if (storePanel)
        {
            storePanel.SetActive(false);
            //Si el objeto main menu está inactivo
            if (!mainMenu.activeSelf)
            {
                //Regresamos al home
                EnterHome();
                //Salimos de la funcion
                return;
            }
        }

        if (statsPanel) statsPanel.SetActive(false);

        if (settingsPanel) settingsPanel.SetActive(false);

        EnterMenu();
    }

    //Metodo que actualiza la puntuacion en la UI    
    public void ResfreshTextScore(int currentCoins, float score)
    {
        //Asignamos al objeto Texto la puntacion formateada a un digito
        scoreText.text = score.ToString("0");

        //Asignamos al objeto Texto la cant de monedas formateada a un digito
        coinsText.text = currentCoins.ToString("0");
    }
    //TODO
    /**
    private void Instance_OnScoreAmountChanged(object sender, System.EventArgs e)
    {
        UpdateText();
    }
    **/

    public void UpdateText()
    {
        //Asignamos al objeto Texto la puntacion formateada a un digito
        scoreText.text = GameManager.sharedInstance.Score.ToString("0");

        //Asignamos al objeto Texto la cant de monedas formateada a un digito
        coinsText.text = GameManager.sharedInstance.CurrentCoins.ToString("0");

    }
    public void UpdateTextScore(float score)
    {
        //Vamos a construir el texto en tiempo real para eso usamos esta funcion de la libreria mencionada arriba
        //StringBuilder sb = new StringBuilder("SCORE: ");//Palabras por defecto     
        sb.Append("SCORE: ");
        //Con Append le agregamos texto, en este caso se ira actualizando el score actual
        sb.AppendFormat("{0:0.0}",score);

        //Asignamos el texto final al componente
        scoreText.text = sb.ToString();

        sb.Clear();
    }

    //Actualiza las monedas actuales disponibles para el jugador
    public void UpdateCoinsAvailable(int coins)
    {
        totalCoins.text = coins.ToString();
    }

    public void RefreshUIAfterGameOver()
    {
        homePanel.SetActive(true);
        playingUI.SetActive(false);
        buttonsPanel.SetActive(true);
    }
}
