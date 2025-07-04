using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManaging : MonoBehaviour
{
    public Image fadeImage;

    public AudioClip beginningMusic;

    public AudioClip backgroundMusic;

    public AudioClip medievalBattle;

    public AudioClip militarBattle;

    public AudioClip futuristicBattle;

    public void Start()
    {
        StartCoroutine(FadeFromBlack());
    }

    public void StartGame()
    {
        AudioManager.Instance.PlayBGM(beginningMusic);
        StartCoroutine(FadeAndTransition("Beginning"));
        
    }

    public void SwitchToCombat1()
    {
        StartCoroutine(FadeAndTransition("Combat1"));
    }

    public void SwitchToCombat2(){
        StartCoroutine(FadeAndTransition("Combat2"));
    }

    public void SwitchToCombat3(){
        StartCoroutine(FadeAndTransition("Combat3"));
    }

    public void SwitchToCombat4(){
        StartCoroutine(FadeAndTransition("Combat4"));
    }

    public void SwitchToCombat5(){
        StartCoroutine(FadeAndTransition("Combat5"));
    }

    public void SwitchToCombat6(){
        StartCoroutine(FadeAndTransition("Combat6"));
    }

    public void SwitchToCombatFinal(){
        StartCoroutine(FadeAndTransition("CombatFinal"));
    }

    public void SwitchToCredits(){
        StartCoroutine(FadeAndTransition("Credits"));
    }
    public void SwitchToMenu(){
        StartCoroutine(FadeAndTransition("Menu"));
    }

    public void SwitchToSpecificScene(string sceneName){
        StartCoroutine(FadeAndTransition(sceneName));
    }

    public IEnumerator FadeFromBlack()
    {
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1f; // Começa escuro
        fadeImage.color = color;

        float elapsed = 0f;

        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / 2f));
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 0f);
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeAndTransition(string sceneName = null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            yield return new WaitForSeconds(0.5f);

            float elapsed = 0f;

            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / 2f);
                fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            if(sceneName!=null)
                SceneManager.LoadScene(sceneName);
        }    

}