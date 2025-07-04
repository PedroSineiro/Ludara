using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExitButton : MonoBehaviour
{
    public TextMeshProUGUI tooltip;

    public SaveManager saveManager;
    public void ShowTooltip()
    {
        tooltip.text = "Salvar e Sair";
    }

    public void HideTooltip()
    {
        tooltip.text = "";
    }
}
