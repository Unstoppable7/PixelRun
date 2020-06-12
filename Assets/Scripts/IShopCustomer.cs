
//Interfaz que será utilizada para hacer generico el uso de la tienda
//debemos heredar nuestro player de esta interfaz e implementar estos metodos
public interface IShopCustomer {

    //Metodo que indica que compró el item
    void BoughtItem(Item.ItemType itemType);
    //Metodo retornará si se puede o no gastar cierta cantidad de oro
    bool TrySpendGoldAmount(int coinsAmount);

}
