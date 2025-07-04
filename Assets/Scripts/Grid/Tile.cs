using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public Vector2Int gridPosition;

    public bool occupied = false;
    public bool spawnable = false;

    public CardData cardData;
    public EnemyCardData enemyCardData;
    public ItemCardData itemCardData;

    public Image thumbnailHolder;

    public GameObject inspector;

    public void SetCard(CardData data)
    {
        ClearData();
        cardData = data;
        thumbnailHolder.gameObject.SetActive(true);
        thumbnailHolder.sprite = data.thumbnailImage;
        occupied = true;

        // Checa invisibilidade
        if (data.isInvisible)
        {
            SetThumbnailAlpha(0.4f); // Translucidez para jogador
        }
        else
        {
            SetThumbnailAlpha(1f);
        }
    }

    public void SetEnemy(EnemyCardData data)
        {
            ClearData();
            enemyCardData = data;
            thumbnailHolder.gameObject.SetActive(true);
            thumbnailHolder.sprite = data.thumbnailImage;
            occupied = true;

            // Checa invisibilidade
            if (data.isInvisible)
            {
                SetThumbnailAlpha(0f); // Invisibilidade total para inimigos
            }
            else
            {
                SetThumbnailAlpha(1f);
            }
        }


    public void SetItem(ItemCardData data)
    {
        ClearData();
        itemCardData = data;
        thumbnailHolder.gameObject.SetActive(true);
        thumbnailHolder.sprite = data.thumbnailImage;
        occupied = true;
    }

    public void ClearData()
    {
        cardData = null;
        enemyCardData = null;
        itemCardData = null;

        // Reset visual antes de esconder
        SetThumbnailAlpha(1f);

        thumbnailHolder.sprite = null;
        thumbnailHolder.gameObject.SetActive(false);
        occupied = false;
    }
    public bool HasCard()
    {
        if(cardData != null || enemyCardData != null || itemCardData != null){
            return true;
        } else {
            return false;
        }
        
    }

    private void SetThumbnailAlpha(float alpha)
    {
        if (thumbnailHolder != null)
        {
            Color c = thumbnailHolder.color;
            c.a = alpha;
            thumbnailHolder.color = c;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateInspector();
    }

    [System.Obsolete]
    public void OnTileClicked()
    {
        var combatManager = FindObjectOfType<CombatManager>();
        if (combatManager == null) return;

        combatManager.TryPlaceCharacter(this);
    }

    void UpdateInspector()
    {
        if (inspector == null) return;

        Image image = inspector.transform.Find("Card").GetComponent<Image>();
        TextMeshProUGUI HealthLabel = inspector.transform.Find("HealthLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI AtkLabel = inspector.transform.Find("AtkLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI RangeLabel = inspector.transform.Find("RangeLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI MovLabel = inspector.transform.Find("MovLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI SpeLabel = inspector.transform.Find("SpeLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI StatusLabel = inspector.transform.Find("StatusLabel").GetComponent<TextMeshProUGUI>();

        if (cardData != null)
        {
            image.sprite = cardData.cardImage;
            image.gameObject.SetActive(true);
            HealthLabel.text = "Vida: " + cardData.currenthealth + "/" + cardData.health;
            AtkLabel.text = "Ataque: " + cardData.damage;
            RangeLabel.text = "Alcance: " + cardData.range;
            MovLabel.text = "Movimento: " + cardData.movement;
            SpeLabel.text = "Habilidade: " + cardData.specialProperties;
            string stattus = cardData.isInvisible? "Invisível":"";
            if(cardData.isSupported){
                if(stattus==""){
                    stattus = "Apoiado";
                } else {
                    stattus += ", Apoiado";
                }
            }
            StatusLabel.text = "Status: " + stattus;
        }
        else if (enemyCardData != null && !enemyCardData.isInvisible)
        {
            image.sprite = enemyCardData.cardImage;
            image.gameObject.SetActive(true);
            HealthLabel.text = "Vida: " + enemyCardData.currenthealth + "/" + enemyCardData.health;
            AtkLabel.text = "Ataque: " + enemyCardData.damage;
            RangeLabel.text = "Alcance: " + enemyCardData.range;
            MovLabel.text = "Movimento: " + enemyCardData.movement;
            SpeLabel.text = "Habilidade: " + enemyCardData.specialProperties;
            string stattus = enemyCardData.isSupported? "Apoiado":"";
            StatusLabel.text = "Status: " + stattus;
        }
        else if (itemCardData != null)
        {
            image.sprite = itemCardData.cardImage;
            image.gameObject.SetActive(true);
            HealthLabel.text = "Vida: " + itemCardData.currenthealth + "/" + itemCardData.health;
            AtkLabel.text = "Ataque: " + itemCardData.damage;
            RangeLabel.text = "Alcance: " + itemCardData.range;
            MovLabel.text = "Movimento: " + itemCardData.movement;
            SpeLabel.text = "Habilidade: " + itemCardData.specialProperties;
            StatusLabel.text = "Status: ";
        }
        else
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
            HealthLabel.text = "Vida: ";
            AtkLabel.text = "Ataque: ";
            RangeLabel.text = "Alcance: ";
            MovLabel.text = "Movimento: ";
            SpeLabel.text = "Habilidade: ";
            StatusLabel.text = "Status: ";
        }
    }

    [System.Obsolete]
    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClicked();
    }
}
