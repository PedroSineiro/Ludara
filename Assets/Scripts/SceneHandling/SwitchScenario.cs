using UnityEngine;
public class SwitchScenario : MonoBehaviour
{
    public UnityEngine.UI.Image background;
    public Sprite imagem;
    public void SwitchScenarioImage()
    {
        background.sprite = imagem;
    }
}
