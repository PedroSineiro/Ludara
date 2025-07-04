using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class MenuHability : MonoBehaviour
{
    public TextMeshProUGUI habilityName;
    public Hability hability;
    public TextMeshProUGUI dialog;

    public void ShowHabilityDescription(){
        if(hability.discovered){
            dialog.text = hability.description;
        } else {
            dialog.text = "????????";
        }
    }

    public void HideHabilityDescription(){
        dialog.text = "";
    }

    public void UpdateHability(){

        if(!hability.discovered){
            var incognito = Regex.Replace(hability.habilityName, @"\w", "?");
            habilityName.text = incognito;
        } else {
            habilityName.text = hability.habilityName;
        }
    }
}
