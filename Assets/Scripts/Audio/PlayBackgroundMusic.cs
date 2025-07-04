using UnityEngine;

public class PlayBackgroundMusic : MonoBehaviour
{
    public AudioClip bgMusic;
    public bool init;

    void Start()
    {
        if(init)
            PlayBGM();
    }

    public void PlayBGM(){
        AudioManager.Instance.PlayBGM(bgMusic);
    }
}
