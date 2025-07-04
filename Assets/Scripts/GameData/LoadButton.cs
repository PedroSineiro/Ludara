using System.IO;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    public SaveManager saveManager;
    public Button button;
    void Awake()
    {
        button.interactable = saveManager.CheckIfSave();
    }
}
