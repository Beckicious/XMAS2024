using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void DownloadTxtFile(string fileName, string fileContent);

#endif

    public XmasGrid grid;

    public void OnImport(string userInput)
    {
        Debug.Log("Import");

        // DEBUG
        grid.LoadSolution(userInput);
    }

    public void OnExport()
    {
        Debug.Log("Export");
        string exportString = grid.GetGridAsString();
        Debug.Log(exportString);

#if UNITY_WEBGL && !UNITY_EDITOR

        string fileName = "solution.txt";
        DownloadTxtFile(fileName, exportString);

#elif UNITY_EDITOR
        // Download File
        var path = EditorUtility.SaveFilePanel("Save solution", "", "solution", "txt");

        if (path.Length != 0)
        {
            File.WriteAllText(path, exportString);
        }
#else
        Debug.Log("#else");
#endif
    }

    public void OnReset()
    {
    }

    //private static JsonBoard LoadJson(string path)
    //{
    //    var txt = Resources.Load<TextAsset>(path).text;
    //    return JsonUtility.FromJson<JsonBoard>(txt);
    //}

    //public static JsonBoard LoadDefaultBoard()
    //{
    //    return LoadJson("defaultBoard");
    //}
}


