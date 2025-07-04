using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public List<CardData> allCards;
    public List<MenuItemCardData> allItems;
    public List<Hability> allHabilities;
    public EventMemory eventMemory;
    public SceneManaging sceneManaging;
    public static string SavePath => Application.persistentDataPath + "/save.json";
    public string sceneName;
    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    public void SaveGame()
    {
        GameSaveData saveData = new();

        // Salva a cena atual
        saveData.sceneName = sceneName;

        // Salva os dados dos personagens
        foreach (var card in allCards)
        {
            CardSaveData data = new CardSaveData
            {
                cardId = card.cardId,
                health = card.health,
                damage = card.damage,
                obtained = card.obtained,
                died = card.died
            };
            saveData.cards.Add(data);
        }

        foreach (var card in allCards)
        {
            CardSaveData data = new CardSaveData
            {
                cardId = card.cardId,
                health = card.health,
                damage = card.damage,
                obtained = card.obtained,
                died = card.died
            };
            saveData.cards.Add(data);
        }

        foreach (var item in allItems)
        {
            ItemSaveData data = new ItemSaveData
            {
                itemId = item.itemId,
                discovered = item.discovered,
                quantity = item.quantity
            };
            saveData.items.Add(data);
        }

        foreach (var hability in allHabilities)
        {
            HabilitySaveData data = new HabilitySaveData
            {
                habilityId = hability.habilityId,
                discovered = hability.discovered,
            };
            saveData.habilities.Add(data);
        }

        EventSaveData eventSaveData = new EventSaveData { newEnemy = eventMemory.newEnemy || eventMemory.newEnemySpawned };

        saveData.eventMemory = eventSaveData;
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        sceneManaging.SwitchToMenu();
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        // Restaura os dados dos personagens
        foreach (var savedCard in saveData.cards)
        {
            var card = allCards.Find(c => c.cardId == savedCard.cardId);
            if (card != null)
            {
                card.health = savedCard.health;
                card.damage = savedCard.damage;
                card.obtained = savedCard.obtained;
                card.died = savedCard.died;
            }
        }

        foreach (var savedItem in saveData.items)
        {
            var item = allItems.Find(i => i.itemId == savedItem.itemId);
            if (item != null)
            {
                item.discovered = savedItem.discovered;
                item.quantity = savedItem.quantity;
            }
        }

        foreach (var savedHability in saveData.habilities)
        {
            var hability = allHabilities.Find(h => h.habilityId == savedHability.habilityId);
            if (hability != null)
            {
                hability.discovered = savedHability.discovered;
            }
        }

        eventMemory.newEnemy = saveData.eventMemory.newEnemy;

        // Carrega a cena salva
        sceneManaging.SwitchToSpecificScene(saveData.sceneName);
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }

    public bool CheckIfSave()
    {
        return File.Exists(SavePath);
    }
}

internal class GameSaveData
{
    public string sceneName;
    public List<CardSaveData> cards = new();
    public List<ItemSaveData> items = new();
    public List<HabilitySaveData> habilities = new();
    public EventSaveData eventMemory = new();
}

internal class CardSaveData
{
    public int cardId { get; set; }
    public int health { get; set; }
    public int damage { get; set; }
    public bool obtained { get; set; }
    public bool died { get; set; }
}

internal class ItemSaveData
{
    public int itemId { get; set; }
    public int quantity { get; set; }
    public bool discovered { get; set; }
}

internal class HabilitySaveData
{
    public int habilityId { get; set; }
    public bool discovered { get; set; }
}

internal class EventSaveData
{
    public bool newEnemy { get; set; }

}