using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SceneActions : MonoBehaviour
{
    public UnityEngine.UI.Image image;
    public Sprite imageToShow;
    public GameObject dialog;
    public GameObject inventoryButtons;
    public Button charactersButton;
    public UnityEngine.UI.Image background;
    public Sprite imagem;

    public Image characterButton;

    public Sprite militarButton;
    public Sprite futuristicButton;
    public Sprite amalgamateButton;
    public void ShowImage()
    {
        if (image != null)
            image.sprite = imageToShow;
            image.gameObject.SetActive(true);
    }

    public void HideImage()
    {
        if (image != null)
            image.sprite = imageToShow;
            image.gameObject.SetActive(false);
    }

    public void SwitchCharacterInventoryIconMilitar(){
        characterButton.sprite = militarButton;
    }

    public void SwitchCharacterInventoryIconFuturistic(){
        characterButton.sprite = futuristicButton;
    }

    public void SwitchCharacterInventoryIconAmalgamate(){
        characterButton.sprite = amalgamateButton;
    }

    public void SetImageColorWhite()
    {
        if (image != null)
            image.color = Color.white;

    }

    public void SwitchToBattlefield(){
        SwitchCharacterInventoryIconMilitar();
        SwitchScenarioImage();
        SetImageColorWhite();
        ShowImage();
    }
    
    public void SwitchToFuturistic(){
        SwitchCharacterInventoryIconFuturistic();
        SwitchScenarioImage();
        SetImageColorBlue();
        ShowImage();
    }

    public void SwitchToVoid(){
        SwitchCharacterInventoryIconAmalgamate();
        SwitchScenarioImage();
        SetImageColorWhite();
        ShowImage();
    }
    public void SetImageColorBlue()
    {
        if (image != null)
            image.color = new Color(0.3216447f,0.490566f,0.4842744f);

    }

    public void HideDialog(){
        dialog.SetActive(false);
    }

    public void SwitchScenarioImage()
    {
        background.sprite = imagem;
    }

    public void SwitchToQuarto()
    {
        background.sprite = imagem;
        HideImage();
    }

}