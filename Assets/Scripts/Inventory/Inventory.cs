using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public GameObject characters;  
    public GameObject items;
    public List<MenuItem> itemsCards;
    public GameObject habilities;

    public List<MenuHability> habilitiesLabels;
    public GameObject inventoryButtons;
    public Button charactersToggleButton;        // Botão de abrir/fechar
    public TextMeshProUGUI charactersTooltipText;         // Texto do tooltip
    public Button itemsToggleButton;        // Botão de abrir/fechar
    public TextMeshProUGUI itemsTooltipText;         // Texto do tooltip
    public Button habilitiesToggleButton;        // Botão de abrir/fechar
    public TextMeshProUGUI habilitiesTooltipText;         // Texto do tooltip
    public string charactersText = "Personagens";
    public string itemsText = "Itens";
    public string habilitiesText = "Habilidades";
    private bool isCharactersOpen = false;
    private bool isItemsOpen = false;
    private bool isHabilitiesOpen = false;
    public AudioClip inventoryAudio;

    void Start()
    {
        if (charactersToggleButton != null)
            charactersToggleButton.onClick.AddListener(ToggleCharacters);

        if (charactersTooltipText != null)
            charactersTooltipText.gameObject.SetActive(false);

        if (itemsToggleButton != null)
            itemsToggleButton.onClick.AddListener(ToggleItems);

        if (itemsTooltipText != null)
            itemsTooltipText.gameObject.SetActive(false);

        if (habilitiesToggleButton != null)
            habilitiesToggleButton.onClick.AddListener(ToggleHabilities);

        if (charactersTooltipText != null)
            habilitiesTooltipText.gameObject.SetActive(false);
    }

    public void InitInventory()
    {
        inventoryButtons.SetActive(true);
        ToggleCharacters();
    }

    public void ToggleCharacters()
    {
        AudioManager.Instance.PlaySFX(inventoryAudio);
        isCharactersOpen = !isCharactersOpen;
        characters.SetActive(isCharactersOpen);
        if(isCharactersOpen){
            items.SetActive(!isCharactersOpen);
            isItemsOpen = !isCharactersOpen;
            habilities.SetActive(!isCharactersOpen);
            isHabilitiesOpen = !isCharactersOpen;
        }
        UpdateCharacters();

        if (charactersTooltipText != null)
            charactersTooltipText.text = charactersText;
    }

    public void ToggleItems()
    {
        AudioManager.Instance.PlaySFX(inventoryAudio);
        isItemsOpen = !isItemsOpen;
        items.SetActive(isItemsOpen);
        if(isItemsOpen){
            UpdateItems();
            characters.SetActive(!isItemsOpen);
            isCharactersOpen = !isItemsOpen;
            habilities.SetActive(!isItemsOpen);
            isHabilitiesOpen = !isItemsOpen;
        }
        
        //UpdateItems();

        if (itemsTooltipText != null)
            itemsTooltipText.text = itemsText;
    }

    public void ToggleHabilities()
    {
        AudioManager.Instance.PlaySFX(inventoryAudio);
        isHabilitiesOpen = !isHabilitiesOpen;
        habilities.SetActive(isHabilitiesOpen);
        if(isHabilitiesOpen){
            UpdateHabilities();
            characters.SetActive(!isHabilitiesOpen);
            isCharactersOpen = !isHabilitiesOpen;
            items.SetActive(!isHabilitiesOpen);
            isItemsOpen = !isHabilitiesOpen;
        }
        
        //UpdateHabilities();

        if (habilitiesTooltipText != null)
            habilitiesTooltipText.text = habilitiesText;
    }

    private void UpdateCharacters()
    {
        foreach (Transform child in characters.transform)
        {
            if (!child.gameObject.CompareTag("Carta")) continue;

            Card card = child.GetComponent<Card>();
            if (card == null || card.image == null || card.card == null) continue;

            Image imagem = card.image.GetComponent<Image>();
            if (imagem == null) continue;

            var data = card.card;

            if (data.obtained)
            {
                if (data.died)
                {
                    imagem.color = new Color(1f, 0f, 0f, 0.5f);
                    card.canShowInfo = true;
                } else {
                    imagem.color = new Color(1f, 1f, 1f, 1f);
                    card.canShowInfo = true;
                }
            }
            else
            {
                imagem.color = new Color(0f, 0f, 0f, 0.5f);
                card.canShowInfo = false;
            }
        }
    }

    public void UpdateItems(){
        foreach(var menuItem in itemsCards){
            menuItem.UpdateItem();
        }
    }

    public void UpdateHabilities(){
        foreach(var menuHability in habilitiesLabels){
            menuHability.UpdateHability();
        }
    }


    public void ShowCharactersTooltip()
    {
        if (charactersTooltipText != null)
        {
            charactersTooltipText.text = charactersText;
            charactersTooltipText.gameObject.SetActive(true);
        }
    }

    public void HideCharactersTooltip()
    {
        if (charactersTooltipText != null)
            charactersTooltipText.gameObject.SetActive(false);
    }

    public void ShowItemsTooltip()
    {
        if (itemsTooltipText != null)
        {
            itemsTooltipText.text = itemsText;
            itemsTooltipText.gameObject.SetActive(true);
        }
    }

    public void HideItemsTooltip()
    {
        if (itemsTooltipText != null)
            itemsTooltipText.gameObject.SetActive(false);
    }

    public void ShowHabilitiesTooltip()
    {
        if (habilitiesTooltipText != null)
        {
            habilitiesTooltipText.text = habilitiesText;
            habilitiesTooltipText.gameObject.SetActive(true);
        }
    }

    public void HideHabilitiesTooltip()
    {
        if (habilitiesTooltipText != null)
            habilitiesTooltipText.gameObject.SetActive(false);
    }
}
