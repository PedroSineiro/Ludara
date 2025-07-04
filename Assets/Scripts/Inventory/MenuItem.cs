using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class MenuItem : MonoBehaviour
{

    public GameObject itemImage;
    public TextMeshProUGUI quantity;
    public MenuItemCardData item;
    public TextMeshProUGUI dialog;

    public void ShowItemDescription(){
        if(item.discovered){
            dialog.text = item.description;
        } else {
            dialog.text = "????????";
        }
    }

    public void HideItemDescription(){
        dialog.text = "";
    }

    public void UpdateItem(){

        quantity.text = "X" + item.quantity;
        Image imagem = itemImage.GetComponent<Image>();
        if(item.discovered){
            imagem.color = new Color(1f, 1f, 1f, 1f);
        } else {
            imagem.color = new Color(0f, 0f, 0f, 0.5f);
        }
    }

}
