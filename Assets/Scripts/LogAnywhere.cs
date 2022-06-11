using UnityEngine;

public class LogAnywhere : MonoBehaviour
{
    string filename = "";
    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }

    public void Log(string logString, string stackTrace, LogType type)
    {
        if (filename == "")
        {
            string d = System.Environment.GetFolderPath(
              System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            System.IO.Directory.CreateDirectory(d);
            filename = d + "/my_happy_log.txt";
        }

        try
        {
            System.IO.File.AppendAllText(filename, System.DateTime.Now + "\n");
            System.IO.File.AppendAllText(filename, stackTrace + "\n");
            System.IO.File.AppendAllText(filename, logString + "\n");
        }
        catch { }
    }
}
