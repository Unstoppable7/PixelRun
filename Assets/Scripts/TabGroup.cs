using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TabGroup : MonoBehaviour
{
    //Lista de todos los tabs
    public List<TabButton> tabButtons;
    //Tab seleccionado
    public TabButton selectedTab;
    //Lista de paginas a utilizar con los tabs (manejado en el editor)
    public List<GameObject> pagesToSwap;

    #region Estados de los tabs

    //Sin interaccion
    public Sprite tabIdle;
    //Mouse encima
    public Sprite tabHover;
    //clickado
    public Sprite tabActive;

    #endregion


    //Metodo para incluir cada tab a la lista
    public void Subscribe(TabButton button)
    {
        //Si la lista ha sido inicializada
        if (tabButtons == null)
        {
            //Inicializamos la lista
            tabButtons = new List<TabButton>();
        }

        //Agregamos el tab a la lista
        tabButtons.Add(button);
    }

    //Metodo que acciona cuando el mouse entra al tab
    public void OnTabEnter(TabButton button)
    {
        //Reinicio el estado de los tabs
        ResetTabs();

        //Si no existe tab seleccionado ó este tab no es el seleccionado
        if(!selectedTab || button != selectedTab)
        {
            //Asigno el estado de mouse encima
            button.background.sprite = tabHover;
        }
        
    }

    //Metodo que acciona cuando el mouse sale del tab
    public void OnTabExit(TabButton button)
    {
        //Reinicio el estado de los tabs
        ResetTabs();
    }

    //Metodo que acciona cuando clicamos al tab
    public void OnTabSelected(TabButton button)
    {
        //Si ya existe un tab seleccionado (anterior)
        if (selectedTab)
        {
            //CallBack deseleccionalo
            selectedTab.Deselect();
        }

        //Asigno referencia del tab seleccionado
        selectedTab = button;
        //CallBack seleccionalo
        selectedTab.Select();
    
        //Reinicio el estado de los tabs
        ResetTabs();
        //Asigno el estado de tab clickado
        button.background.sprite = tabActive;
        //Llamo al metodo que me activa y desactiva las pages
        //le mando por parametro el indicie de este tab
        SwapPages(button.transform.GetSiblingIndex());
    }

    //Metodo para reiniciar el estado de los tabs
    public void ResetTabs()
    {
        //Recorremos c/u de los tabs
        foreach (TabButton button in tabButtons)
        {
            //Si existe un tab seleccionado y es el que se está recorriendo
            //salte el contenido de esta iteracion y continue en la siguien
            if(selectedTab && button == selectedTab) { continue; }
            button.background.sprite = tabIdle;
        }
    }

    //Metodo para activar o desactivar la page correspondiente al tab
    //estan ordenadas segun su indice (de arriba hacia abajo)
    public void SwapPages(int index)
    {
        //Recorremos todas las pages
        for (int i = 0; i < pagesToSwap.Count; i++)
        {   
            //Si la page de esta iteracion es la que corresponde al indice del
            //tab
            if(i == index)
            {
                //Activo la page
                pagesToSwap[i].SetActive(true);
            }
            else
            {//Si no desactivo esta page
                pagesToSwap[i].SetActive(false);
            }
        }
    }
}
