using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//Requerimos que este objeto tenga un componente de tipo Image
[RequireComponent(typeof(Image))]
//Heredamos de las interfaces que se encargan de manejar los eventos del mouse
public class TabButton : MonoBehaviour, IPointerEnterHandler, 
                IPointerClickHandler, IPointerExitHandler
{
    //Referencia al grupo de tabs
    public TabGroup tabGroup;
    //Referencia a la imagen que tiene el tab como fondo
    public Image background;
    //Eventos en el editor cuando un tab es seleccionado
    public UnityEvent onTabSelected;
    //Eventos en el editor cuando un tab es deseleccionado
    public UnityEvent onTabDeselected;

    #region Llamada a eventos del mouse

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);

    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Inicializamos el fondo del tab
        background = GetComponent<Image>();
        //Añadimos este tab al grupo de tabs
        tabGroup.Subscribe(this);
    }

    #region CallBacks

    //Acciones que van a suceder cuando este boton este seleccionado
    public void Select()
    {
        //Si el evento existe
        if(onTabSelected != null)
        {
            //Invocalo
            onTabSelected.Invoke();
        }
    }

    //Acciones que van a suceder cuando este boton este deseleccionado
    public void Deselect()
    {
        //Si el evento existe
        if (onTabDeselected != null)
        {
            //Invocalo
            onTabDeselected.Invoke();
        }
    }

    #endregion
}
