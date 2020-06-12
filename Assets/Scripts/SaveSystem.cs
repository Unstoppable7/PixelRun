using UnityEngine;

//Libreria para manejar archivos en el disco duro
using System.IO;
//Libreria para manejar el formateador de los archivos binarios
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    //Metodo para guardar la data en un archivo con los datos 
    //recibidos por parametro
	public static void SavePlayer(GameManager managerData)
    {
        //Creamos el objeto del formateador en binario
        BinaryFormatter formatter = new BinaryFormatter();

        //Ruta donde se guardaran los datos Unity tiene una ruta
        //especifica donde el guarda los datos a persistir dependiendo
        //del SO y a este le agregamos el nombre del archivo y su extension
        string path = Application.persistentDataPath + "/LocalSave.pixelRun";

        //En este caso la declaracion 'using' funciona como un try/catch
        //Objeto de flujo necesario para manejar archivos
        //Le damos la ruta del archivo y le indicamos que accion se quiere realizar
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            //Llamamos a la clase serializable en la cual vamos a tener los datos a guardar
            //mediante el constructor estos datos deben asignarse
            PlayerData data = new PlayerData(managerData);

            //Escribirmos o serializamos el archivo u objeto
            //Recibe el flujo del archivo y la data a guardar
            formatter.Serialize(stream, data);

            //Con using no es necesario cerrar el archivo, él lo hace
            //Cerramos el flujo del archivo
            //stream.Close();
        }
        
    }

    //Metodo para cargar la data del jugador
    public static PlayerData LoadPlayer()
    {
        //Indicamos la misma ruta usada en el metodo anterior
        string path = Application.persistentDataPath + "/LocalSave.pixelRun";

        //Si existe el archivo en la ruta especificada
        if (File.Exists(path))
        {
            //Creamos el objeto del formateador en binario
            BinaryFormatter formatter = new BinaryFormatter();

            //Objeto de flujo necesario para manejar archivos
            //Le damos la ruta del archivo y le indicamos que accion se quiere realizar
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                //Deserealizamos el archivo u objeto, lo casteamos y 
                //guardamos en una variable
                //Recibe el flujo del archivo
                PlayerData data = formatter.Deserialize(stream)
                                                as PlayerData;
                //Cerramos el flujo del archivo
                //stream.Close();

                //Retornamos los datos cargados
                return data;
            }

            
        }
        else
        {
            //Si no, indicamos el error y devolvemos nulo
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
