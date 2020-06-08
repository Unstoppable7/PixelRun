using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsPanelManager : MonoBehaviour
{
    //Buttons
    public GameObject storeButton;
    public GameObject playButton;
    public GameObject menuButton;
    public GameObject backButton;
    public GameObject noNameButton;
    public GameObject homeButton;

    //Panels
    public GameObject settingsPanel;
    public GameObject statsPanel;
    public GameObject homePanel;
    public GameObject storePanel;

    //Menus
    public GameObject mainMenu;

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
    }
    public void BackButton()
    {                
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
}
