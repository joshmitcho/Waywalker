using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public static class ImportUtils
{
    public static T ImportJson<T>(string path)
    {
        string jsonText = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(jsonText);
    }
    
    public static T ImportXml<T>(string path)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlWriterSettings ws = new XmlWriterSettings();
            ws.NewLineHandling = NewLineHandling.Entitize;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return (T)serializer.Deserialize(stream);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception importing xml file: " + e);
            return default;
        }
    }
}