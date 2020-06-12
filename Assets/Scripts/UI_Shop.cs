using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Clase encargada de generar la User Interface de la tienda 
public class UI_Shop : MonoBehaviour {

    //Contenedores padre de los items
    private Transform page_1;
    /**
    private Transform page_2;
    private Transform page_3;
    private Transform page_4;
    **/

    //Plantilla para cada page de cada item de la tienda (revisar en el editor los componentes
    //que tiene consigo)
    private Transform shopItemTemplate_1;
    /**
    private Transform shopItemTemplate_2;
    private Transform shopItemTemplate_3;
    private Transform shopItemTemplate_4;
    **/

    //Variable manejada por la interfaz que me permite hacer generico el uso
    //de la tienda y no ligada a un script player
    private IShopCustomer shopCustomer;

    private void Awake() {
        //Referenciamos objetos
        page_1 = transform.Find("Page (1)");

        /**
         * Por ahora no lo estamos usando
        page_2 = transform.Find("Page (2)");
        page_3 = transform.Find("Page (3)");
        page_4 = transform.Find("Page (4)");
        **/

        //Agregar aqui las referencias a las demas plantillas 
        //para las demas pages
        shopItemTemplate_1 = page_1.Find("shopItemTemplate (1)");
        shopItemTemplate_1.gameObject.SetActive(false);

        /**
        shopItemTemplate_2 = page_1.Find("shopItemTemplate (2)");
        shopItemTemplate_2.gameObject.SetActive(false);
        shopItemTemplate_3 = page_1.Find("shopItemTemplate (3)");
        shopItemTemplate_3.gameObject.SetActive(false);
        shopItemTemplate_4 = page_1.Find("shopItemTemplate (4)");
        shopItemTemplate_4.gameObject.SetActive(false);
        **/

        shopCustomer = GameObject.FindGameObjectWithTag("GameManager")
                                        .GetComponent<IShopCustomer>();
    }

    private void Start() {
        //Creacion de los diferentes items PAGE_1
        CreateItemButton(Item.ItemType.Attack, 
            Item.GetSprite(Item.ItemType.Attack), 
            "Attack", Item.GetCost(Item.ItemType.Attack),
            page_1, shopItemTemplate_1, 0);

        CreateItemButton(Item.ItemType.Shield, 
            Item.GetSprite(Item.ItemType.Shield), 
            "Shield", Item.GetCost(Item.ItemType.Shield),
            page_1, shopItemTemplate_1, 1);        

    }

    //Metodo que me crea un item especifico enviandole por parametro
    //su tipo, el sprite, nombre, costo y posicion de arriba hacia abajo
    private void CreateItemButton(Item.ItemType itemType, Sprite itemSprite, string itemName, int itemCost, Transform parent, Transform template, int positionIndex) {
        //Instancio la plantilla en esta variable, asignandole el mismo padre
        Transform shopItemTransform = Instantiate(template, parent);
        //Activo el nuevo item
        shopItemTransform.gameObject.SetActive(true);
        //Obtengo su posicion o rectTranform
        RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();
        //Variable que me indica el alto de los items
        float shopItemHeight = 250f;

        //Posiciono el nuevo item debajo del ultimo generado con referencia
        //a la posicion de la plantilla
        shopItemRectTransform.anchoredPosition = 
            new Vector2(template.localPosition.x, 
            template.localPosition.y + (-shopItemHeight * positionIndex));

        //Asigno al item su nombre
        shopItemTransform.Find("nameText").GetComponent<TextMeshProUGUI>().SetText(itemName);
        //Asigno al item su costo
        shopItemTransform.Find("costText").GetComponent<TextMeshProUGUI>().SetText(itemCost.ToString());
        //Asigno al item su sprite
        shopItemTransform.Find("itemImage").GetComponent<Image>().sprite = itemSprite;

        //Agrega el evento al onClick del componente button del item instanaciado
        shopItemTransform.GetComponent<Button>().onClick.AddListener(() =>TryBuyItem(itemType));
    }
    
    //Metodo para intentar comprar un item
    public void TryBuyItem(Item.ItemType itemType) {

        //Intentamos comprar (si tenemos suficiente dinero entra)
        if (shopCustomer.TrySpendGoldAmount(Item.GetCost(itemType))) {
            //Compramos
            shopCustomer.BoughtItem(itemType);
        } else {
            //TODO
            //Si no tenemos dinero, mostramos una advertencia
            //Tooltip_Warning.ShowTooltip_Static("Cannot afford " + Item.GetCost(itemType) + "!");
            Debug.Log("No se puede comprar este item: " + itemType);
        }
    }
}
