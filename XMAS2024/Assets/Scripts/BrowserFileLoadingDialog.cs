using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using UnityEditor;
using System.IO;

public class BrowserFileLoadingDialog : MonoBehaviour, IPointerDownHandler
{
    public UIHandler handler;

#if UNITY_WEBGL && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void ReadTxtFile(string id);

    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log($"GO name: {gameObject.name}");
        ReadTxtFile(gameObject.name);
    }

    public void OnFileUploaded(string fileContent) {
        OnImport(fileContent);
    }

#elif UNITY_EDITOR

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        string path = EditorUtility.OpenFilePanel("Import Map File", "", "txt");
        if (path.Length != 0)
        {
            // Debug.Log($"file chosen {path}");
            var fileContent = File.ReadAllText(path);
            OnImport(fileContent);
        }
    }
    
    // empty meth for this target
    public void OnPointerDown(PointerEventData eventData) { }
#else

    // empty meth for all targets
    public void OnPointerDown(PointerEventData eventData) { }
#endif

    public void OnImport(string res)
    {
        handler.OnImport(res);
    }

    public void OnExport()
    {
        handler.OnExport();
    }

}
