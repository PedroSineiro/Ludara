using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public bool isEvent = false;
    public Button dialogButton;
    public TextMeshProUGUI dialogText;

    public Image fadeImage;
    public List<EventBase> events;
    private int currentIndex = 0;

    void Start()
    {
        if(!isEvent){
            dialogButton.onClick.AddListener(NextEvent);
            NextEvent();
        }
    }

    public void InitEvent()
    {
        currentIndex = 0;
        dialogButton.onClick.RemoveAllListeners();
        dialogButton.onClick.AddListener(NextEvent);
        NextEvent();
    }

    public void NextEvent()
    {
        if (currentIndex >= events.Count) return;

        var currentEvent = events[currentIndex];

        if (currentEvent is Dialog dialog)
        {
            if (!dialog.IsFinished())
            {
                StartCoroutine(dialog.Execute(this));
                return;
            } else {
                dialogText.text = "";
            }
        }

        currentIndex++;
        if (currentIndex < events.Count)
        {
            StartCoroutine(events[currentIndex].Execute(this));
        }
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
