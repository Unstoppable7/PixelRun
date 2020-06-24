using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    //Singleton
    public static CharacterManager sharedInstance { set; get; }

    //Lista de personajes disponibles que podrá elegir el jugador
    public Character[] characters;

    //Si está activo el cambio de personajes o no
    [HideInInspector]
    public bool change;

    //Transform que se usa para desplazar los personajes de un lado al otro
    //contendrá los prefabs de los personajes para que el jugador pueda verlos
    Transform slide;

    //La distancia de separación que tendrán los personajes hacia el lado
    float characterDistance = 2f;

    //Guarda la posicion del personaje que está viendo el jugador en pantalla
    [HideInInspector]
    public int characterTemp;

    //Nombre del personaje en la pantalla
    public TextMeshProUGUI characteName;

    //Precio del personaje en la pantalla
    public TextMeshProUGUI characterPrice;

    //Nos dice si está realizando la animacion de cambio de personaje o no
    [HideInInspector]
    public bool isChange = true;

    //Velocidad de la animación
    float animationSpeed = 10f;

    void Awake()
    {
        if (!sharedInstance)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        slide = transform.Find("Slide");

        //Crea los personajes en pantalla pero no los muestra
        //solo los deja creados y ocultos en el transform slide
        CreateCharacters();
    }

    void Update(){
        //Realiza la animación horizontal cuando se cambia de personaje
        ChangeCharacterPosition();
    }

    void CreateCharacters(){
        Vector3 targetPosition = Vector3.zero;

        //Se recorre la lista de los personajes
        foreach (Character character in characters){
            //Y se van instanciando cada uno
            Transform characterTemp = Instantiate(character.prefab).transform;

            //Animator temporal del personaje que se está creando
            //se usa para cambiar la animacion idle que verá el usuario
            //se una animacion para cada personaje
            Animator animatorTemp = characterTemp.GetComponent<Animator>();
            
            //Se coloca el personahe dentro del transform slide
            //para realizar la animacion de cambio de personaje
            characterTemp.SetParent(slide);

            //Se le asigna la posicion que tendrá el personaje al lado del anterior
            characterTemp.localPosition = targetPosition; 

            //Se actualiza la posición que tendrá el siguiente personaje
            targetPosition += Vector3.right * characterDistance;
            
            //Se crea en tiempo de ejecución una copia del animator del personaje actual
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animatorTemp.runtimeAnimatorController);

            //Se cambia la animacion idle general la que tendrá cada personaje
            animatorOverrideController["Idle 1"] = character.idleAnimation;

            //Luego se sustituye el animator de este personaje por el que contiene la nueva animación
            animatorTemp.runtimeAnimatorController = animatorOverrideController;
        }
    }

    //Activa el slide y muestra los personajes que puede elegir el usuario
    public void DisplayChangeCharacter(){
        //Asignamos el personaje temporal al que tiene seleccionado el jugador
        //para que cuando se muestre la animación, vaya directo al personaje
        //que tiene seleccionado el jugador
        characterTemp = GameManager.sharedInstance.PlayerCharacter;

        //Se oculta el personaje actual que tiene el jugador con el character controller
        //(el que tiene la logica para moverse por el mapa)
        GameManager.sharedInstance.motor.gameObject.SetActive(false);

        //Cambia la posición del slide para que muestre al personaje que tiene
        //seleccionado el jugador actualmente (con el que está jugando)
        slide.position = -(Vector3.right * characterDistance * characterTemp);

        //Muestra la lista de personajes disponibles en el juego
        slide.gameObject.SetActive(true);

        //Hace que el cambio de personajes esté activo
        change = true;

        //Realiza la animación del cambiar de personaje (que gire 180 grados)
        ChangeCharacter(0);
    }

    //Oculta la seleccion de personajes
    public void HiddenChangeCharacter(){
        //Coloca a 0 grados el personaje que está viendo el jugador para que cuando
        //vuelva a iniciar el cambio de personajes realize la animacion de nuevo a 180 grados
        //en el caso de que el perosnaje que está viendo es el mismo que tiene puesto
        slide.GetChild(characterTemp).rotation = Quaternion.Euler(0, 0, 0);

        //Se ocultan los personajes
        slide.gameObject.SetActive(false);

        //Activa el motor del player para que pueda empezar a moverse
        GameManager.sharedInstance.motor.gameObject.SetActive(true);

        //Hace que el cambio de personajes esté desactivado
        change = false;
    }

    //Mueve poco a poco el transform del slide para realizar la animación horizontal de los personajes
    public void ChangeCharacterPosition(){
        slide.position = Vector3.Lerp(slide.position, -(Vector3.right * characterDistance * characterTemp), animationSpeed * Time.deltaTime);
    }

    //Cambia el personaje que verá el jugador
    public void ChangeCharacter(int direction){
        //La animación de cambio se pone como falsa
        isChange = false;

        //Se hace referencia al personaje actual y a su animator para reproducir la animacion
        Transform currentCharacter = slide.GetChild(characterTemp);
        Animator currentAnimator = currentCharacter.GetComponent<Animator>();

        //Cuando apenas abre la seleccion de personajes
        if(direction == 0){
            //Rota al personaje actual 180 grados
            //Como al inicio siempre se girará un solo personaje (que es el que tiene seleccionado actualmente)
            //se llama al metodo de rotar a un solo personaje
            StartCoroutine(Turn180(currentCharacter));
            
            //Hace la animación de mover los pies del personaje
            currentAnimator.SetBool("CharacterPreview", true);

            //Actualiza los datos del personaje en la interfaz grafica
            ChangeButtonName();

            //Hacemo return solo al inicio ya que solo rotará un personaje
            //cuando recien se abre la seleccion de personajes
            return;
        }

        //Movemos al personaje que está viendo en pantalla a la izquierda o derecha
        //dependiendo de donde se haya dado click
        characterTemp += direction;

        //Aseguramos que el caracter a elegir esté entre 0 (el primero) y el ultimo element del array de personajes
        characterTemp = Mathf.Clamp(characterTemp, 0, characters.Length-1);

        //Si se presiona para un lado quiere decir que se cambia de personaje
        //entonces guardamos la referencia del personaje al siguiente personaje queva a ver el jugador
        //en pantalla
        Transform nextCharacter = slide.GetChild(characterTemp);
        Animator nextAnimator = nextCharacter.GetComponent<Animator>();

        //Ahora se realiza la animación de los dos personajes el actual
        //y el siguiente que visualizará
        StartCoroutine(Turn180(currentCharacter, nextCharacter));

        //Activamos las animaciones de los pies del personaje, del actual y del proximo que verá en pantalla
        currentAnimator.SetBool("CharacterPreview", false);
        nextAnimator.SetBool("CharacterPreview", true);

        //Se actualiza igual el boton para comprar en la interfaz con el siguiente personaje
        ChangeButtonName();
    }

    //Gira el transform 180 grados del personaje actual y del siguiente
    //el actual (como ya estria girado mirando a la camara) lo rota a 0 grados (lo pone de espalda)
    //y el siguiente lo rota a 180 (lo pone mirando a la camara, ya que es el que va a ver el jugador en pantalla)
    IEnumerator Turn180(Transform currentCharacter, Transform nextCharacter){
        while(Quaternion.Angle(currentCharacter.rotation, Quaternion.Euler(0, 0, 0)) > 0 && 
            Quaternion.Angle(nextCharacter.rotation, Quaternion.Euler(0, 180f, 0)) > 0){
                currentCharacter.rotation = Quaternion.Lerp(currentCharacter.rotation, Quaternion.Euler(0, 0, 0), animationSpeed * Time.deltaTime);
                nextCharacter.rotation = Quaternion.Lerp(nextCharacter.rotation, Quaternion.Euler(0, 180f, 0), animationSpeed * Time.deltaTime);

                yield return null;
        }

        //Despues de la animación nos aseguramos que esten en la rotación que queremos
        currentCharacter.rotation = Quaternion.Euler(0, 0, 0);
        nextCharacter.rotation = Quaternion.Euler(0, 180f, 0);

        //Y se termina la animacion del cambio del personaje colcoando a true
        isChange = true;
    }

    //Gira a 180 grados un solo personaje, el primero cuando recien se abre el selector de personajes
    IEnumerator Turn180(Transform currentCharacter){
        while(Quaternion.Angle(currentCharacter.rotation, Quaternion.Euler(0, 180f, 0)) > 0){
            currentCharacter.rotation = Quaternion.Lerp(currentCharacter.rotation, Quaternion.Euler(0, 180f, 0), animationSpeed * Time.deltaTime);
            yield return null;
        } 

        //Despues de la animación nos aseguramos que esten en la rotación que queremos
        currentCharacter.rotation = Quaternion.Euler(0, 180f, 0);
        
        //Y se termina la animacion del cambio del personaje colcoando a true
        isChange = true;
    }

    //Metodo que selecciona el personaje cuando se oprime el boton de seleccionar personaje
    public void SelectCharacter(){
        //Comprueba si hay dinero suficiente
        if(GameManager.sharedInstance.TrySpendGoldAmount(characters[characterTemp].price)){
            //Si se puede comprar:
            //Actualiza el precio del personaje a 0
            characters[characterTemp].price = 0;

            //Asigna de una vez el nuevo personaje comprado al personaje que usará el jugador
            GameManager.sharedInstance.PlayerCharacter = characterTemp;

            //Realiza el cambio de personaje con el animator para que pueda animarse
            GameManager.sharedInstance.motor.ChangePlayerCharacter();

            //Se actualiza el precio en la interfaz de usuario
            ChangeButtonName();
        }

        else{
            print("No se pude comprar el personaje: " + characters[characterTemp].name);
        }
    }

    //Actualiza los datos del personaje en la interfaz de usuario
    public void ChangeButtonName(){
        characteName.text = characters[characterTemp].name;
        characterPrice.text = "" + characters[characterTemp].price;
    }
}
