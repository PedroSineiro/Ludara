using UnityEngine;

public class PlayButtonClick : MonoBehaviour
{
    public AudioClip buttonClick;
    public void PlayButtonClickSound(){
        AudioManager.Instance.PlaySFX(buttonClick);
    }
}
