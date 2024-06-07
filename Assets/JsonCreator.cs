using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class JsonCreator : MonoBehaviour
{
    [SerializeField] string filePath;
    [SerializeField] bool generateFileTrigger;
    [SerializeField] ArrayHolder messagesHolder;

    private void Update()
    {
        if(generateFileTrigger)
        {
           string  newfilePath = Path.Combine(Application.persistentDataPath, "arrayData.json");
            SerializableArray messagesArraySr = new SerializableArray(messagesHolder.constantMessagesList.ToArray());
            string testString = JsonUtility.ToJson(messagesArraySr);
            File.WriteAllText(filePath, testString);
            Debug.Log(testString);
            generateFileTrigger = false;
        }
    }
    public void WriteJson( string path, txtReader.message[] messages)
    {
        SerializableArray messagesArraySr = new SerializableArray(messages);
        string jsonedArray = JsonUtility.ToJson(messagesArraySr);
        File.WriteAllText(path, jsonedArray);
    }
    public txtReader.message[] getCompleteArray()
    {
        string jsonArray = File.ReadAllText(filePath);
        SerializableArray serializableArray = JsonUtility.FromJson<SerializableArray>(jsonArray);
        return serializableArray.Array;
    }
    private class SerializableArray
    {
        public txtReader.message[] Array;

        public SerializableArray(txtReader.message[] array)
        {
            Array = array;
        }
    }
}

