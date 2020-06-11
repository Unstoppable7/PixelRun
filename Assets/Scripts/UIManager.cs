﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;//Me ayudara a formatear el texto de la vida en tiempo real dentro del Update

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
    //private bool isPause = false;   

    #endregion

    #region inHome
    
    //Texto de la cant de monedas
    public TextMeshProUGUI totalCoins;
    //Texto de la cant de gemas
    public TextMeshProUGUI totalGems;

    #endregion

    //Vamos a construir el texto en tiempo real para eso usamos esta funcion de la libreria mencionada arriba
    StringBuilder sb = new StringBuilder("SCORE: ");

    //Metodo que actualiza la puntuacion en la UI    
    public void ResfreshTextScore(int currentCoins, float score)
    {
        //Asignamos al objeto Texto la puntacion formateada a un digito
        scoreText.text = score.ToString("0");

        //Asignamos al objeto Texto la cant de monedas formateada a un digito
        coinsText.text = currentCoins.ToString("0");
    }

    public void UpdateTextScore(float score)
    {
        //Vamos a construir el texto en tiempo real para eso usamos esta funcion de la libreria mencionada arriba
        //StringBuilder sb = new StringBuilder("SCORE: ");//Palabras por defecto     
        //Con Append le agregamos texto, en este caso se ira actualizando el score actual
        sb.AppendFormat("{0:0.0}",score);

        //Asignamos el texto final al componente
        scoreText.text = sb.ToString();

        sb.Clear();
    }

}
