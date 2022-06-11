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
            filename = d + "/" + System.DateTime.Now.ToString("yyyy/MM/dd") + ".log";
        }

        try
        {
            System.IO.File.AppendAllText(filename, logString + "\n");
            System.IO.File.AppendAllText(filename, stackTrace);
            System.IO.File.AppendAllText(filename, "--------------------------------------------------------\n");

        }
        catch { }
    }
}
