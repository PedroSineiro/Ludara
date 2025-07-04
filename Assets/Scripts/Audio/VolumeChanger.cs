using UnityEngine;

public class VolumeChanger : MonoBehaviour
{
    public float volume;

    public void ChangeVolume()
    {
        AudioManager.Instance.bgmSource.volume = volume;
    }
}
