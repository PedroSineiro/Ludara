using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneEventManager : MonoBehaviour
{
    public Button rollButton;

    public AudioClip diceAudio;

    public EventMemory eventMemory;

    public GameObject dialog;

    public TextMeshProUGUI dialogText;

    public Button dialogButton;

    public TextMeshProUGUI eventText;

    public ButtonManager buttonManager;

    public GameObject characterSelection;

    public List<CharacterOption> characters;

    public bool deadCharacters;

    public SaveManager saveManager;

    public string nextScene;

    public void InitEvent()
    {
        rollButton.gameObject.SetActive(true);
        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(() => RollDice());
        dialog.SetActive(false);
    }

    public void RollDice(){
        AudioManager.Instance.PlaySFX(diceAudio);
        rollButton.gameObject.SetActive(false);
        int number = Random.Range(1, 5);
        dialog.SetActive(true);
        dialogText.text = "Você tirou " + number + ".";

        dialogButton.onClick.RemoveAllListeners();
        dialogButton.onClick.AddListener(() => {
            dialogText.text = "";
            dialogButton.onClick.RemoveAllListeners();
            HandleEvent(number);
        });
    }

    public void HandleEvent(int number){
        if(number==1){
            eventMemory.newEnemy = true;
            saveManager.sceneName = nextScene;
            dialogText.text = "O próximo combate será mais difícil...";
            dialogButton.onClick.RemoveAllListeners();
            dialogButton.onClick.AddListener(() => {
                dialogText.text = "";
                dialogButton.onClick.RemoveAllListeners();
                buttonManager.InitEvent();

            });
        } else if (number == 2 || number == 3)
        {
            dialog.SetActive(false);
            characterSelection.SetActive(true);

            eventText.gameObject.SetActive(true);
            eventText.text = number==2?"Escolha quem deve ganhar +4 de vida.": "Escolha quem deve ganhar +2 de ataque.";

            foreach (var character in characters)
            {
                Transform optionTransform = character.option.transform;

                var icon = optionTransform.Find("Icon")?.GetComponent<Image>();
                var nameText = optionTransform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                var button = optionTransform.Find("Button")?.GetComponent<Button>();

                if (!character.card.obtained)
                {
                    icon.color = Color.black;
                    nameText.text = "????";
                }
                else if(!character.card.died)
                {
                    icon.color = Color.white;
                    nameText.text = character.card.cardName;
                } else {
                    icon.color = Color.red;
                    nameText.text = character.card.cardName;
                }
                button.onClick.RemoveAllListeners();
                if (character.card.obtained && !character.card.died)
                {
                    if(number==2){
                        button.onClick.AddListener(() => {
                            saveManager.sceneName = nextScene;
                            eventText.gameObject.SetActive(false);
                            eventText.text = "";
                            character.card.health+=4;
                            if(character.card.specialProperties=="Mecha")
                                character.card.nonMechaCard.health+=4;
                            if(character.card.specialProperties=="Transformar")
                                character.card.transformedCard.health+=4;
                            characterSelection.SetActive(false);
                            dialog.SetActive(true);
                            eventText.gameObject.SetActive(false);
                            dialogText.text = character.card.cardName+" Ganhou +4 de vida máxima.";
                            dialogButton.onClick.RemoveAllListeners();
                            dialogButton.onClick.AddListener(() => {
                                buttonManager.InitEvent();
                            });
                        });
                    } else {
                        button.onClick.AddListener(() => {
                            saveManager.sceneName = nextScene;
                            eventText.gameObject.SetActive(false);
                            eventText.text = "";
                            character.card.damage+=2;
                            if(character.card.specialProperties=="Mecha")
                                character.card.nonMechaCard.damage+=2;
                            if(character.card.specialProperties=="Transformar")
                                character.card.transformedCard.damage+=2;
                            characterSelection.SetActive(false);
                            dialog.SetActive(true);
                            eventText.gameObject.SetActive(false);
                            dialogText.text = character.card.cardName+" Ganhou +2 de ataque.";
                            dialogButton.onClick.RemoveAllListeners();
                            dialogButton.onClick.AddListener(() => {
                                buttonManager.InitEvent();
                            });
                        });
                    }
                }
            }
        } else {
            deadCharacters = false;
            foreach(var character in characters){
                if(character.card.died)
                    deadCharacters = true;
            }
            if(deadCharacters){
                eventText.gameObject.SetActive(true);
                eventText.text = "Escolha quem vai ser trazido de volta.";
                dialog.SetActive(false);
                characterSelection.SetActive(true);
                foreach (var character in characters)
                {
                    Transform optionTransform = character.option.transform;

                    var icon = optionTransform.Find("Icon")?.GetComponent<Image>();
                    var nameText = optionTransform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                    var button = optionTransform.Find("Button")?.GetComponent<Button>();

                    if (!character.card.obtained)
                    {
                        icon.color = Color.black;
                        nameText.text = "????";
                    }
                    else if(!character.card.died)
                    {
                        icon.color = Color.white;
                        nameText.text = character.card.cardName;
                    } else {
                        icon.color = Color.red;
                        nameText.text = character.card.cardName;
                    }
                    button.onClick.RemoveAllListeners();
                    if (character.card.obtained && character.card.died)
                    {
                        saveManager.sceneName = nextScene;
                        eventText.text = "";
                        button.onClick.AddListener(() => {
                            character.card.died=false;
                            characterSelection.SetActive(false);
                            dialog.SetActive(true);
                            dialogText.text = character.card.cardName+" está disponível novamente.";
                            dialogButton.onClick.RemoveAllListeners();
                            dialogButton.onClick.AddListener(() => {
                                buttonManager.InitEvent();
                            });
                        });
                    }
                }
            } else {
                dialogText.text = "Você não possui um personagem derrotado. role novamente.";
                dialogButton.onClick.RemoveAllListeners();
                dialogButton.onClick.AddListener(() => {
                    dialog.SetActive(false);
                    InitEvent();
                });
                
            }
        }
    }
}
