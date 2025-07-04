using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CombatManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyCardData enemyData;
        public Vector2Int spawnPosition;
    }

    public GridManager gridManager;

    public SceneManaging sceneManaging;

    [Header("Save")]

    public EventMemory eventMemory;

    public SaveManager saveManager;

    [Header("Inimigos")]
    public List<EnemySpawnData> enemiesToSpawn;

    public EnemySpawnData additionalEnemyToSpawn;

    private Dictionary<EnemyCardData, Tile> enemyTiles = new();

    [Header("Seleção de Personagens")]
    public GameObject actionsMenu;
    public GameObject charactersSelect;
    public TextMeshProUGUI combatText;
    public int maxCharacters;
    public Button cancelButton;
    public Button concludeButton;

    private Dictionary<CardData, Tile> characterTiles = new();

    [Header("Fase de Combate")]
    public GameObject characterOptions; // UI com botões: Atacar, Mover, Especial, Item, Encerrar, Cancelar

    public Button attackButton;

    public Button moveButton;

    public Button itemButton;

    public TextMeshProUGUI addItemText;

    public Button specialButton;

    public Button concludeButtonCombat;
    public Button cancelButtonCombat;

    [Header("Menu de Item")]

    public GameObject itemOptions;
    public Button cancelItemButton;
    public List<ItemOption> itemOptionButtons;

    public MenuItemCardData healingPotion;
    public MenuItemCardData lightning;
    public MenuItemCardData medKit;
    public MenuItemCardData grenade;
    public MenuItemCardData holographicShield;
    public MenuItemCardData turret;

    [HideInInspector] public List<CharacterInCombat> selectedCharactersInOrder = new(); // Guarda os personagens e dados necessários para combate

    [HideInInspector] private Coroutine highlightCoroutine;
    [HideInInspector] private Tile highlightedTile;

    [System.Serializable]
    public class CharacterInCombat
    {
        public CardData card;
        public Tile currentTile;
        public int usedMovement;

        public int usedAttacks;
    }

    private int currentCharacterIndex = 0;
    private CharacterInCombat activeCharacter = null;

    [HideInInspector] public List<Tile> placedTiles = new();
    [HideInInspector] public List<CardData> selectedCharacters = new();
    private CharacterOption selectedOption = null;
    private bool isPlacingCharacter = false;

    private bool isPlayerTurn = true;
    private bool isCombatOver = false;

    [SerializeField] private string winSceneName = "Menu";
    [SerializeField] private string looseSceneName = "Menu";

    private Dictionary<ItemCardData, Tile> itemTiles = new();

    private Dictionary<Tile, int> protectedTiles = new();

    void Start()
    {
        StartCombatPhase();
    }

    public void StartCombatPhase()
    {
        eventMemory.newEnemySpawned = false;

        if (eventMemory.newEnemy)
        {
            enemiesToSpawn.Add(additionalEnemyToSpawn);
            eventMemory.newEnemy = false;
            eventMemory.newEnemySpawned = true;
        }
        // 1. Spawn dos inimigos
        foreach (var enemy in enemiesToSpawn)
        {
            if (gridManager.tiles.TryGetValue(new Vector2(enemy.spawnPosition.x, enemy.spawnPosition.y), out Tile tile))
            {
                tile.SetEnemy(enemy.enemyData);
                enemyTiles[enemy.enemyData] = tile;
            }
        }

        // 2. Ativação do menu de seleção
        if (charactersSelect.transform.parent != null)
            charactersSelect.transform.parent.gameObject.SetActive(true);
        if (actionsMenu != null)
            actionsMenu.SetActive(true);
        cancelButton.gameObject.SetActive(false);
        concludeButton.gameObject.SetActive(false);

        // 3. Atualiza texto
        UpdateCombatText(0); // ainda não selecionou nenhum

        // 4. Configurações nos personagens

        foreach (Transform child in charactersSelect.transform)
        {
            CharacterOption option = child.GetComponent<CharacterOption>();
            if (option != null && option.card != null)
            {
                Image icon = child.Find("Icon").GetComponent<Image>();
                TextMeshProUGUI nameText = child.Find("Name").GetComponent<TextMeshProUGUI>();

                if (!option.card.obtained)
                {
                    icon.color = Color.black;
                    nameText.text = "????";
                    option.selectable = false;
                }
                else if(!option.card.died)
                {
                    icon.color = Color.white;
                    nameText.text = option.card.name;
                    option.selectable = true;
                } else {
                    icon.color = Color.red;
                    nameText.text = option.card.name;
                    option.selectable = false;
                }
            }
        }

        HighlightSpawnableTiles(true);
    }

    public void SelectCharacter(CharacterOption option)
    {
        if (!option.selectable || selectedCharacters.Count >= maxCharacters || isPlacingCharacter) return;

        selectedOption = option;
        isPlacingCharacter = true;

        var icon = option.transform.Find("Icon").GetComponent<Image>();
        icon.color = new Color(1f, 1f, 1f, 0.5f);

        combatText.text = $"Posicione o Personagem {option.card.name}";

        cancelButton.gameObject.SetActive(true);
    }

    public void TryPlaceCharacter(Tile tile)
    {
        if (!isPlacingCharacter || selectedOption == null || tile.occupied || !tile.spawnable) return;

        tile.SetCard(selectedOption.card);
        selectedCharacters.Add(selectedOption.card);
        placedTiles.Add(tile);
        characterTiles[selectedOption.card] = tile;
        

        var icon = selectedOption.transform.Find("Icon").GetComponent<Image>();
        icon.color = Color.gray;
        selectedOption.selectable = false;

        selectedOption = null;
        isPlacingCharacter = false;

        UpdateCombatText(selectedCharacters.Count);

        if (selectedCharacters.Count > 0)
            concludeButton.gameObject.SetActive(true);

        cancelButton.gameObject.SetActive(true);

        HighlightSpawnableTiles(true);
    }

    public void OnCancel()
    {
        if (isPlacingCharacter && selectedOption != null)
        {
            var icon = selectedOption.transform.Find("Icon").GetComponent<Image>();
            icon.color = Color.white;

            selectedOption = null;
            isPlacingCharacter = false;
            HighlightSpawnableTiles(true);
            UpdateCombatText(selectedCharacters.Count);
        }
        else if (selectedCharacters.Count > 0)
        {
            var lastTile = placedTiles[^1];
            var removedCard = lastTile.cardData;

            lastTile.ClearData();
            placedTiles.RemoveAt(placedTiles.Count - 1);
            selectedCharacters.RemoveAt(selectedCharacters.Count - 1);

            foreach (Transform child in charactersSelect.transform)
            {
                CharacterOption option = child.GetComponent<CharacterOption>();
                if (option.card == removedCard)
                {
                    var icon = option.transform.Find("Icon").GetComponent<Image>();
                    icon.color = Color.white;
                    option.selectable = true;
                    break;
                }
            }
            UpdateCombatText(selectedCharacters.Count);

            concludeButton.gameObject.SetActive(selectedCharacters.Count > 0);
        }

        cancelButton.gameObject.SetActive(isPlacingCharacter || selectedCharacters.Count > 0);
    }

    public void OnConclude()
    {
        HideHighlightSpawnableTiles();
        StartCombat();
    }

    public void StartCombat()
    {
        selectedOption = null;

        charactersSelect.transform.parent.gameObject.SetActive(false);

        selectedCharactersInOrder.Clear();

        foreach (var card in selectedCharacters)
        {
            if (characterTiles.TryGetValue(card, out Tile tile))
            {
                selectedCharactersInOrder.Add(new CharacterInCombat
                {
                    card = card,
                    currentTile = tile,
                    usedMovement = 0
                });
            }
            else
            {
                Debug.LogWarning($"Tile não encontrado para o personagem {card.name}");
            }
        }

        currentCharacterIndex = 0;
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        if (currentCharacterIndex >= selectedCharactersInOrder.Count)
        {
            Debug.Log("Todos os personagens jogaram, iniciar turno dos inimigos...");
            return;
        }

        activeCharacter = selectedCharactersInOrder[currentCharacterIndex];
        activeCharacter.card.numberOfTurns++;
        UpdateSpecialHability();
        UpdateSpecialButton();
        HighlightActiveCharacter(activeCharacter.currentTile,true);

        if (combatText != null)
            combatText.text = $"Turno de {activeCharacter.card.cardName}";

        if (characterOptions != null)
            characterOptions.SetActive(true);

        if (cancelButtonCombat != null)
            cancelButtonCombat.gameObject.SetActive(false);
    }

    private void UpdateSpecialHability(){
        if(activeCharacter.card.specialProperties=="Camuflagem Assassina"){
            if(activeCharacter.card.isInvisible){
                activeCharacter.card.isInvisible = false;
                activeCharacter.card.canGoInvisible = false;
                StartCoroutine(PlayGettingVisible());
            } else {
                activeCharacter.card.canGoInvisible = true;
            }
        } else if(activeCharacter.card.specialProperties=="Transformar"){
            if(activeCharacter.card.isTransformed){
                TransformCharacter(false);
            } else {
                activeCharacter.card.canTransform = true;
            }
        } else if(activeCharacter.card.specialProperties=="Recarregar"){
            if(activeCharacter.card.needRecharge){
                activeCharacter.card.needRecharge = false;
                activeCharacter.usedAttacks = 1;
            }
        } else if((activeCharacter.card.specialProperties=="Magia" || activeCharacter.card.specialProperties=="Mecânico" || activeCharacter.card.specialProperties=="Armeiro") 
        && activeCharacter.card.numberOfTurns%3==0){
            var items = activeCharacter.card.generateableItems;
            
            if (items != null && items.Count > 0)
            {
                int index = Random.Range(0, items.Count);
                var selectedItem = items[index];

                selectedItem.quantity += 1;
                selectedItem.discovered = true;
                StartCoroutine(BlinkAddItemText());
            }
        }
        
    }

    private void UpdateSpecialButton(){
        specialButton.gameObject.SetActive(false);
        specialButton.onClick.RemoveAllListeners();
        if(activeCharacter.card.specialProperties=="Camuflagem Assassina"){
            specialButton.gameObject.SetActive(true);
            specialButton.GetComponentInChildren<TextMeshProUGUI>().text = "Camuflagem";
            specialButton.interactable = false;
            if(activeCharacter.card.canGoInvisible){
                specialButton.interactable = true;
                specialButton.onClick.AddListener(() => GetInvisible());
            }
        } else if(activeCharacter.card.specialProperties=="Telepatia Apoiadora"){
            specialButton.gameObject.SetActive(true);
            specialButton.GetComponentInChildren<TextMeshProUGUI>().text = "Apoiar";
            specialButton.interactable = true;
            specialButton.onClick.AddListener(() => SupportCharacter());
        } else if(activeCharacter.card.specialProperties=="Transformar"){
            specialButton.gameObject.SetActive(true);
            specialButton.GetComponentInChildren<TextMeshProUGUI>().text = "Transformar";
            specialButton.interactable = false;
            if(activeCharacter.card.canTransform){
                specialButton.interactable = true;
                specialButton.onClick.AddListener(() => TransformCharacter(true));
            }
        }
    }

    private void SupportCharacter()
    {
        if(activeCharacter.card.supportedCharacter != null){
            activeCharacter.card.supportedCharacter.isSupported = false;
            activeCharacter.card.supportedCharacter.supporterCharacter = null;
        }

        activeCharacter.card.supportedCharacter = null;

        ResetActionButtons(false);
        cancelButtonCombat.onClick.RemoveAllListeners();
        cancelButtonCombat.onClick.AddListener(() =>
        {
            cancelButtonCombat.gameObject.SetActive(false);
            ResetActionButtons(true);
            ClearTileHighlights();
            DisableCharacterTileButtons();
            combatText.text = $"Turno de {activeCharacter.card.cardName}";
        });

        combatText.text = "Selecione quem apoiar";

        foreach(var character in characterTiles){
            if(character.Key != activeCharacter.card){
                var currentChar = character.Key;
                var button = character.Value.transform.Find("Button").GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                character.Value.GetComponent<Image>().color = Color.green;
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() =>
                {
                    activeCharacter.card.supportedCharacter = currentChar;
                    currentChar.isSupported = true;
                    currentChar.supporterCharacter = activeCharacter.card;
                    cancelButtonCombat.gameObject.SetActive(false);
                    ResetActionButtons(true);
                    ClearTileHighlights();
                    DisableCharacterTileButtons();
                    combatText.text = $"Turno de {activeCharacter.card.cardName}";
                });
            }
        }

    }

    private void GetInvisible(){
        specialButton.interactable = false;
        activeCharacter.card.isInvisible = true;
        activeCharacter.card.canGoInvisible = false;
        StartCoroutine(PlayCharacterGettingInvisible());
    }

    private void GetEnemyInvisible(Tile enemyTile){
        StartCoroutine(PlayEnemyGettingInvisible(enemyTile));
    }

    private IEnumerator PlayCharacterGettingInvisible()
    {
        Image icon = activeCharacter.currentTile.transform.Find("Icon")?.GetComponent<Image>();

        // Para o piscar temporariamente
        StopBlinkIcon();

        float duration = 1.0f;
        float t = 0f;

        Color startColor = icon.color;
        Color endColor = startColor;
        
        endColor.a = 0.3f; // parcialmente invisível

        while (t < duration)
        {
            icon.color = Color.Lerp(startColor, endColor, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        icon.color = endColor;
        StartBlinkIcon(icon);
    }

    private IEnumerator PlayEnemyGettingInvisible(Tile enemyTile)
    {
        Image icon = enemyTile.transform.Find("Icon")?.GetComponent<Image>();

        // Para o piscar temporariamente
        StopBlinkIcon();

        float duration = 1.0f;
        float t = 0f;

        Color startColor = icon.color;
        Color endColor = startColor;

        endColor.a = 0f; // completamente invisível

        while (t < duration)
        {
            icon.color = Color.Lerp(startColor, endColor, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        icon.color = endColor;
    }

    private IEnumerator PlayGettingVisible(Tile enemyTile = null)
    {

        Image icon = enemyTile!=null?enemyTile.transform.Find("Icon")?.GetComponent<Image>():activeCharacter.currentTile.transform.Find("Icon")?.GetComponent<Image>();

        float duration = 1.0f;
        float t = 0f;

        Color startColor = icon.color;
        Color endColor = startColor;
        endColor.a = 1f;

        while (t < duration)
        {
            icon.color = Color.Lerp(startColor, endColor, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        icon.color = endColor;

        // Retoma o piscar se necessário
        StartBlinkIcon(icon);
    }

    private void StartBlinkIcon(Image icon)
    {
        if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
        highlightCoroutine = StartCoroutine(BlinkIconColor(icon));
    }

    private void StopBlinkIcon()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }
    }

    private void TransformCharacter(bool transform)
    {
        if(transform){
            specialButton.interactable = false;
            activeCharacter.card.canTransform = false;
        }

        var oldCard = activeCharacter.card;
        var newCard = transform? activeCharacter.card.transformedCard : activeCharacter.card.nonTransformedCard;

        AudioManager.Instance.PlaySFX(oldCard.transformAudio);

        foreach(var characterInCombat in selectedCharactersInOrder){
            if(characterInCombat.card==oldCard){
                var card = oldCard;
                if(card.isSupported){
                    var supporter = card.supporterCharacter;
                    card.isSupported = false;
                    newCard.isSupported = true;
                    newCard.supporterCharacter = supporter;
                    supporter.supportedCharacter = newCard;
                    card.supporterCharacter = null;
                }
                characterInCombat.card = newCard;
                characterInCombat.card.currenthealth = transform?card.currenthealth + 8: Mathf.Min(card.currenthealth,newCard.health);
                if (characterTiles.ContainsKey(card))
                {
                    var tile = characterTiles[card];
                    characterTiles.Remove(card);
                    characterTiles.Add(characterInCombat.card,tile);
                    tile.ClearData();
                    tile.SetCard(characterInCombat.card);
                }
            }
        }
    }

    public void OnAttack()
    {
        CharacterInCombat currentChar = activeCharacter;

        // Verifica se ainda pode atacar
        int maxAttacks = currentChar.card.specialProperties == "Golpe Duplo" ? 2 : 1;
        if (currentChar.usedAttacks >= maxAttacks)
            return;

        // Desabilita outros botões e ativa o de cancelar
        ResetActionButtons(false);

        // Muda o texto
        combatText.text = $"Selecione quem {currentChar.card.cardName} deve atacar.";

        cancelButtonCombat.gameObject.SetActive(true);
        cancelButtonCombat.onClick.RemoveAllListeners();
        cancelButtonCombat.onClick.AddListener(() =>
        {
            cancelButtonCombat.gameObject.SetActive(false);
            ResetActionButtons(true);
            ClearTileHighlights();
            DisableEnemyTileButtons();
            HighlightActiveCharacter(activeCharacter.currentTile,true);
            combatText.text = $"Turno de {activeCharacter.card.cardName}";
        });


        // Limpa seleção anterior
        ClearTileHighlights();

        // Mostra inimigos no alcance
        foreach (var enemy in enemyTiles)
        {
            var canHitFlyer = true;
            if(enemy.Value.enemyCardData.specialProperties=="Voador"){
               canHitFlyer = currentChar.card.range>1?true:false;
            }
            if (IsTargetInRange(currentChar.currentTile.gridPosition, enemy.Value.gridPosition, currentChar.card.range) 
            && !enemy.Value.enemyCardData.isInvisible
            && canHitFlyer)
            {
                // Destaca o inimigo
                enemy.Value.GetComponent<Image>().color = Color.green;

                // Deixa o inimigo clicável
                Transform buttonTransform = enemy.Value.transform.Find("Button");
                if (buttonTransform != null)
                {
                    Button enemyButton = buttonTransform.GetComponent<Button>();
                    if (enemyButton != null)
                    {
                        enemyButton.gameObject.SetActive(true);
                        enemyButton.onClick.RemoveAllListeners();
                        enemyButton.onClick.AddListener(() =>
                        {
                            StartCoroutine(HandlePlayerAttack(enemy.Key, enemy.Value));
                            if (isCombatOver) return;
                            combatText.text = $"Turno de {activeCharacter.card.cardName}";
                        });
                    }
                }
            }
        }
    }

    private IEnumerator HandlePlayerAttack(EnemyCardData enemy, Tile tile)
    {
        var damage = activeCharacter.card.specialProperties == "Camuflagem Assassina"? activeCharacter.card.damage + 3: activeCharacter.card.damage;
        enemy.currenthealth -= activeCharacter.card.isSupported?damage*2:damage;

        if(activeCharacter.card.specialProperties=="Recarregar"){
            activeCharacter.card.needRecharge = true;
        }

        AudioManager.Instance.PlaySFX(activeCharacter.card.attackAudio);
        yield return StartCoroutine(PlayDamageAnimation(tile));

        CheckIfEnemyDied(enemy, tile);

        activeCharacter.usedAttacks++;
        ResetActionButtons(true);
        ClearTileHighlights();
        HighlightActiveCharacter(activeCharacter.currentTile,true);
    }

    private bool IsTargetInRange(Vector2Int origin, Vector2Int target, int range)
    {
        int dx = Mathf.Abs(origin.x - target.x);
        int dy = Mathf.Abs(origin.y - target.y);
        return dx + dy <= range || Mathf.Max(dx, dy) <= range;
    }

    private void ResetActionButtons(bool reset)
    {

        bool attackReset = reset;
        bool moveReset = reset;
        bool specialReset = reset;
        if(reset){
            if((activeCharacter.card.specialProperties=="Golpe Duplo" && (activeCharacter.usedAttacks == 2)) || 
            (activeCharacter.card.specialProperties!="Golpe Duplo" && activeCharacter.usedAttacks == 1))
            attackReset = false;
            if(activeCharacter.card.movement==activeCharacter.usedMovement)
            moveReset = false;
            if(activeCharacter.card.specialProperties=="Camuflagem Assassina"){
                specialReset = activeCharacter.card.canGoInvisible;
            } else if(activeCharacter.card.specialProperties=="Transformar"){
                specialReset = !activeCharacter.card.isTransformed && activeCharacter.card.canTransform;
            }
        }
        attackButton.interactable = attackReset;
        moveButton.interactable = moveReset;
        specialButton.interactable = specialReset;
        itemButton.interactable = reset;
        concludeButtonCombat.interactable = reset;
        cancelButtonCombat.gameObject.SetActive(!reset);
    }

    private void ClearTileHighlights(bool checkIfGreen = false)
    {
        foreach (var tile in gridManager.tiles.Values)
        {
            if(!checkIfGreen){
                tile.GetComponent<Image>().color = protectedTiles.ContainsKey(tile)? Color.blue : Color.white;
            } else {
                if(tile.GetComponent<Image>().color == Color.green){
                    tile.GetComponent<Image>().color = protectedTiles.ContainsKey(tile)? Color.blue : Color.white;
                }
            }


            Transform buttonTransform = tile.transform.Find("Button");
            if (buttonTransform != null)
            {
                buttonTransform.gameObject.SetActive(false);
                buttonTransform.GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }
    }

    private void DisableEnemyTileButtons()
    {
        foreach (var enemyTile in enemyTiles.Values)
        {
            var button = enemyTile.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }
    }

    private void DisableCharacterTileButtons()
    {
        foreach (var characterTile in characterTiles.Values)
        {
            var button = characterTile.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator PlayDamageAnimation(Tile tile)
    {
        Image tileImage = tile.GetComponent<Image>();
        Color originalColor = protectedTiles.ContainsKey(tile)?Color.blue:(highlightedTile==tile?Color.green:Color.white);

        tileImage.color = Color.red;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tileImage.color = Color.Lerp(Color.red, originalColor, elapsed / duration);
            yield return null;
        }

        tileImage.color = originalColor;
    }

    private void CheckEndOfCombat()
    {
        if (isCombatOver) return;

        bool allEnemiesDefeated = enemyTiles.Count == 0;
        bool allCharactersDead = selectedCharactersInOrder.All(c => c.card.currenthealth <= 0);

        if (allEnemiesDefeated)
        {
            var nextSceneName = winSceneName;
            combatText.text = "Você venceu!";
            EndCombat(winSceneName);
        }
        else if (allCharactersDead)
        {
            saveManager.DeleteSave();
            var nextSceneName = looseSceneName;
            combatText.text = "Você perdeu...";
            EndCombat(looseSceneName);
        }
    }

    private void EndCombat(string sceneName)
    {
        isCombatOver = true;
        characterOptions.SetActive(false);
        cancelButtonCombat.gameObject.SetActive(false);
        StartCoroutine(sceneManaging.FadeAndTransition(sceneName));
    } 
    
    public void OnMove()
    {
        ResetActionButtons(false); // desativa os botões de ação
        cancelButtonCombat.gameObject.SetActive(true);

        cancelButtonCombat.onClick.RemoveAllListeners();
        cancelButtonCombat.onClick.AddListener(() =>
        {
            cancelButtonCombat.gameObject.SetActive(false);
            ClearTileHighlights();
            DisableMovementTileButtons();
            ResetActionButtons(true);
            HighlightActiveCharacter(activeCharacter.currentTile,true);
        });

        int remainingMovement = activeCharacter.card.movement - activeCharacter.usedMovement;
        var reachableTiles = GetReachableTiles(activeCharacter.currentTile, remainingMovement);

        foreach (var tile in reachableTiles)
        {
            // ignorar tiles ocupados
            if (characterTiles.ContainsValue(tile) || enemyTiles.ContainsValue(tile) || itemTiles.ContainsValue(tile)) continue;

            tile.GetComponent<Image>().color = Color.green;

            var button = tile.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
            {
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    int dist = GetDistance(activeCharacter.currentTile.gridPosition, tile.gridPosition);
                    activeCharacter.usedMovement += dist;

                    // Limpa o tile antigo
                    activeCharacter.currentTile.ClearData();
                    HighlightActiveCharacter(activeCharacter.currentTile,false);
                    // Atualiza a posição lógica
                    characterTiles[activeCharacter.card] = tile;
                    activeCharacter.currentTile = tile;
                    tile.SetCard(activeCharacter.card);
                    ClearTileHighlights();
                    HighlightActiveCharacter(activeCharacter.currentTile,true);
                    DisableMovementTileButtons();
                    cancelButtonCombat.gameObject.SetActive(false);
                    ResetActionButtons(true);

                });
            }
        }
    }

    private List<Tile> GetReachableTiles(Tile startTile, int maxDistance)
    {
        List<Tile> reachable = new List<Tile>();
        Queue<(Tile tile, int dist)> queue = new Queue<(Tile tile, int dist)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((startTile, 0));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            if (dist > 0) reachable.Add(current);
            if (dist == maxDistance) continue;

            foreach (var neighbor in GetNeighborTiles(current.gridPosition))
            {
                if (visited.Contains(neighbor)) continue;

                visited.Add(neighbor);
                queue.Enqueue((neighbor, dist + 1));
            }
        }

        return reachable;
    }

    private List<Tile> GetNeighborTiles(Vector2Int position)
    {
        List<Tile> neighbors = new List<Tile>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),    // direita
            new Vector2Int(-1, 0),   // esquerda
            new Vector2Int(0, 1),    // cima
            new Vector2Int(0, -1),   // baixo
            new Vector2Int(1, 1),    // cima direita
            new Vector2Int(-1, 1),   // cima esquerda
            new Vector2Int(1, -1),   // baixo direita
            new Vector2Int(-1, -1)   // baixo esquerda
        };

        foreach (var dir in directions)
        {
            Vector2 key = new Vector2(position.x + dir.x, position.y + dir.y);
            if (gridManager.tiles.TryGetValue(key, out Tile tile))
            {
                neighbors.Add(tile);
            }
        }

        return neighbors;
    }

    private void DisableMovementTileButtons()
    {
        foreach (var kvp in gridManager.tiles)
        {
            var button = kvp.Value.transform.Find("Button")?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }
    }

    public void HighlightActiveCharacter(Tile tile, bool highlight)
    {
        var icon = tile.transform.Find("Icon")?.GetComponent<Image>();

        var image = tile.GetComponent<Image>();

        if (highlight)
        {
            image.color = Color.green;
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }

            highlightedTile = tile;
            highlightCoroutine =  StartCoroutine(BlinkIconColor(icon));
        }
        else
        {
            image.color = protectedTiles.ContainsKey(tile)?Color.blue:Color.white;
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }

            Color current = icon.color;
            icon.color = new Color(1f, 1f, 1f, current.a);
            highlightedTile = null;
        }
    }

    private IEnumerator BlinkIconColor(Image icon)
    {
        Color colorA = Color.white;
        Color colorB = new Color(0.7f, 1f, 0.7f, 1f);
        float duration = 1.0f;

        while (true)
        {
            float originalAlpha = icon.color.a;

            float t = 0f;
            while (t < duration)
            {
                Color lerped = Color.Lerp(colorA, colorB, t / duration);
                lerped.a = originalAlpha;
                icon.color = lerped;
                t += Time.deltaTime;
                yield return null;
            }

            t = 0f;
            while (t < duration)
            {
                Color lerped = Color.Lerp(colorB, colorA, t / duration);
                lerped.a = originalAlpha;
                icon.color = lerped;
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    public void EndTurn()
    {
        if (isCombatOver) return;
        HighlightActiveCharacter(activeCharacter.currentTile,false);
        currentCharacterIndex++;

        if (currentCharacterIndex < selectedCharactersInOrder.Count)
        {
            activeCharacter = selectedCharactersInOrder[currentCharacterIndex];
            activeCharacter.card.numberOfTurns++;
            combatText.text = "Turno de " + activeCharacter.card.cardName;
            UpdateSpecialHability();
            UpdateSpecialButton();
            HighlightActiveCharacter(activeCharacter.currentTile,true);
            ResetActionButtons(true);
            cancelButtonCombat.gameObject.SetActive(false);
        }
        else
        {
            characterOptions.SetActive(false);
            isPlayerTurn = false;
            StartNextPhase();
        }
    }

    public void StartNextPhase()
    {
        StartCoroutine(NextPhaseCoroutine());
    }

    private IEnumerator NextPhaseCoroutine()
    {
        characterOptions.SetActive(false);

        yield return StartCoroutine(TurretTurnCoroutine());

        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        StartCoroutine(EnemyTurnCoroutine());
    }

    public void StartTurretTurn()
    {
        StartCoroutine(TurretTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        foreach (var enemyData in enemiesToSpawn)
        {
            if (isCombatOver) yield break;
            if (!enemyTiles.ContainsKey(enemyData.enemyData) || enemyData.enemyData.currenthealth <= 0)
                continue;

            var enemy = enemyData.enemyData;
            Tile enemyTile = enemyTiles[enemy];
            combatText.text = "Turno de " + enemy.cardName;
            HighlightActiveCharacter(enemyTile, true);
            yield return new WaitForSeconds(1.0f);
            if(enemy.specialProperties == "Camuflagem"){
                if(enemy.canGoInvisible){
                    enemy.isInvisible = true;
                    enemy.canGoInvisible = false;
                    GetEnemyInvisible(enemyTile);
                    yield return new WaitForSeconds(1.0f);
                } else if(enemy.isInvisible){
                    enemy.isInvisible = false;
                    enemy.canGoInvisible = false;
                    StartCoroutine(PlayGettingVisible(enemyTile));
                } else {
                    enemy.canGoInvisible = true;
                }
            }

            if(enemy.specialProperties!="Telepatia Apoiadora"){
                // Primeira tentativa: atacar onde está
                var targets = GetAttackableTargets(enemy, enemyTile);
                if (targets.Count > 0)
                {
                    yield return AttackTargets(enemy, enemyTile, targets);
                }
                else
                {
                    // Segunda tentativa: ver se algum personagem pode ser atingido com movimento + alcance
                    var reachableTiles = GetReachableTiles(enemyTile, enemy.movement);
                    bool attacked = false;

                    foreach (var tile in reachableTiles)
                    {
                        if (characterTiles.Values.Any(t => t != null && IsTargetInRange(tile.gridPosition, t.gridPosition, enemy.range) && !t.cardData.isInvisible) && !tile.occupied)
                        {
                            // Move até essa tile
                            HighlightActiveCharacter(enemyTile, false);
                            enemyTile.ClearData();
                            tile.SetEnemy(enemy);
                            enemyTiles[enemy] = tile;
                            HighlightActiveCharacter(tile, true);

                            yield return new WaitForSeconds(1.0f);

                            // Ataca agora da nova posição
                            var newTargets = GetAttackableTargets(enemy, tile);
                            if (newTargets.Count > 0)
                            {
                                yield return AttackTargets(enemy, tile, newTargets);
                            }

                            attacked = true;
                            break;
                        }
                    }

                    if (!attacked)
                    {
                        var closestTarget = GetClosestCharacter(enemyTile);
                        if (closestTarget != null)
                        {
                            reachableTiles = GetReachableTiles(enemyTile, enemy.movement);
                            Tile bestTile = null;
                            int shortestDistance = int.MaxValue;

                            foreach (var tile in reachableTiles)
                            {
                                if (tile.HasCard()) continue;

                                int distance = GetDistance(tile.gridPosition, closestTarget.currentTile.gridPosition);
                                if (distance < shortestDistance)
                                {
                                    shortestDistance = distance;
                                    bestTile = tile;
                                }
                            }

                            if (bestTile != null)
                            {
                                HighlightActiveCharacter(enemyTile, false);
                                enemyTile.ClearData();
                                bestTile.SetEnemy(enemy);
                                enemyTiles[enemy] = bestTile;
                                HighlightActiveCharacter(bestTile, true);
                                yield return new WaitForSeconds(1.0f);
                            }
                        }
                    }
                }
                HighlightActiveCharacter(enemyTiles[enemy], false);
            } else {
                if(enemy.supportedCharacter==null){
                    foreach(var e in enemyTiles){
                        if(e.Key != enemy && !e.Key.isSupported){
                            e.Key.isSupported = true;
                            e.Key.supporterCharacter = enemy;
                            enemy.supportedCharacter = e.Key;
                            break;
                        }
                    }
                }
                List<Tile> reachableTiles = GetReachableTiles(enemyTile, enemy.movement);
                List<Tile> characterTileList = characterTiles.Values
                    .Where(t => t != null && t.cardData != null && !t.cardData.isInvisible)
                    .ToList();

                Tile furthestTile = enemyTile;
                int maxTotalDistance = -1;

                foreach (Tile tile in reachableTiles)
                {
                    if (tile.occupied) continue;

                    int totalDistance = 0;
                    foreach (Tile charTile in characterTileList)
                    {
                        totalDistance += GetDistance(tile.gridPosition, charTile.gridPosition);
                    }

                    if (totalDistance > maxTotalDistance)
                    {
                        maxTotalDistance = totalDistance;
                        furthestTile = tile;
                    }
                }

                // Move se encontrou uma posição mais segura
                if (furthestTile != enemyTile)
                {
                    HighlightActiveCharacter(enemyTile, false);
                    enemyTile.ClearData();
                    furthestTile.SetEnemy(enemy);
                    enemyTiles[enemy] = furthestTile;
                    HighlightActiveCharacter(furthestTile, true);
                    yield return new WaitForSeconds(1.0f);
                }

                // Mesmo se moveu, tenta atacar se possível
                var newTargets = GetAttackableTargets(enemy, enemyTiles[enemy]);
                if (newTargets.Count > 0)
                {
                    yield return AttackTargets(enemy, enemyTiles[enemy], newTargets);
                }
                HighlightActiveCharacter(enemyTiles[enemy], false);
            }

            HighlightActiveCharacter(enemyTile, false);
            yield return new WaitForSeconds(1.0f);
        }
        if (isCombatOver) yield break;
        // Volta para os personagens do jogador
        isPlayerTurn = true;
        ResetCharactersForNewTurn();
        currentCharacterIndex = 0;
        activeCharacter = selectedCharactersInOrder[currentCharacterIndex];
        activeCharacter.card.numberOfTurns++;
        ResetActionButtons(true);
        UpdateSpecialHability();
        UpdateSpecialButton();
        HighlightActiveCharacter(activeCharacter.currentTile, true);
        combatText.text = "Turno de " + activeCharacter.card.cardName;
        cancelButtonCombat.gameObject.SetActive(false);
        characterOptions.SetActive(true);
    }

    private IEnumerator TurretTurnCoroutine()
    {
        foreach (var turret in itemTiles.Keys.ToList())
        {
            if (turret.currenthealth <= 0)
                continue;

            Tile turretTile = itemTiles[turret];
            combatText.text = "Turno de Torreta";
            HighlightActiveCharacter(turretTile,true);
            yield return new WaitForSeconds(1.0f);
            var targets = GetAttackableTurretTargets(turret, turretTile);
            if (targets.Count > 0)
            {
                (EnemyCardData enemy, Tile tile) = targets[0];
                enemy.currenthealth -= turret.damage;
                AudioManager.Instance.PlaySFX(turret.attackAudio);
                yield return PlayDamageAnimation(tile);
                CheckIfEnemyDied(enemy, tile);
                HighlightActiveCharacter(turretTile,false);
                if (isCombatOver) yield break;
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                HighlightActiveCharacter(turretTile,false);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void ResetCharactersForNewTurn()
    {
        foreach (var character in selectedCharactersInOrder)
        {
            character.usedMovement = 0;
            character.usedAttacks = 0;
        }
    }

    private CharacterInCombat GetClosestCharacter(Tile fromTile)
    {
        CharacterInCombat closest = null;
        int minDistance = int.MaxValue;

        foreach (var character in selectedCharactersInOrder)
        {
            if (character.card.currenthealth <= 0 || character.currentTile == null || character.card.isInvisible)
                continue;

            int dist = GetDistance(fromTile.gridPosition, character.currentTile.gridPosition);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = character;
            }
        }

        return closest;
    }

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    private List<(CardData card, Tile tile)> GetAttackableTargets(EnemyCardData enemy, Tile enemyTile)
    {
        var targets = new List<(CardData, Tile)>();

        foreach (var pair in characterTiles)
        {
            var character = pair.Key;
            var tile = pair.Value;

            int distance = GetDistance(enemyTile.gridPosition, tile.gridPosition);
            if (distance <= enemy.range && !character.isInvisible)
            {
                targets.Add((character, tile));
            }
        }

        return targets;
    }

    private List<(EnemyCardData card, Tile tile)> GetAttackableTurretTargets(ItemCardData turret, Tile enemyTile)
    {
        var targets = new List<(EnemyCardData, Tile)>();

        foreach (var pair in enemyTiles)
        {
            var enemy = pair.Key;
            if(enemy.isInvisible) continue;
            var tile = pair.Value;

            int distance = GetDistance(enemyTile.gridPosition, tile.gridPosition);
            if (distance <= turret.range && enemy.currenthealth > 0)
            {
                targets.Add((enemy, tile));
            }
        }

        return targets;
    }

    private IEnumerator AttackTargets(EnemyCardData attacker, Tile attackerTile, List<(CardData card, Tile tile)> targets)
    {
        if(attacker.specialProperties!="Recarregar" || !attacker.needRecharge){

            int attacksLeft = attacker.specialProperties =="Golpe Duplo" ? 2 : 1;
            var attackedThisTurn = new HashSet<CardData>();

            while (attacksLeft > 0 && targets.Count > 0)
            {
                // Prioriza personagens ainda não atacados
                var target = targets.FirstOrDefault(t => !attackedThisTurn.Contains(t.card));
                if (target.card == null)
                {
                    target = targets[0]; // Todos já foram atacados, repete algum
                }

                attackedThisTurn.Add(target.card);
                AudioManager.Instance.PlaySFX(attacker.attackAudio);
                yield return PlayDamageAnimation(target.tile);
                if (protectedTiles.TryGetValue(target.tile, out int shieldHp))
                {
                    shieldHp -= attacker.damage;
                    if(shieldHp > 0){
                        protectedTiles[target.tile] = shieldHp;
                    } else {
                        protectedTiles.Remove(target.tile);
                        Image tileImage = target.tile.GetComponent<Image>();
                        tileImage.color = Color.white;
                    }
                }
                else
                {
                    var damage = attacker.isSupported?attacker.damage*2:attacker.damage;
                    target.card.currenthealth -= target.card.specialProperties == "Defesa Robusta"?damage-1:damage;
                }
                

                CheckIfCharacterDied(target.card, target.tile);
                attacksLeft--;
                if (isCombatOver) yield break;
            }
            if(attacker.specialProperties=="Recarregar"){
                attacker.needRecharge = true;
            }
            yield return new WaitForSeconds(0.5f);
        } else {
            attacker.needRecharge = false;
            yield return new WaitForSeconds(0.5f);
        }



    }

    public void OpenItemMenu()
    {
        characterOptions.SetActive(false);
        itemOptions.SetActive(true);
        UpdateItemOptions();

    }

    private IEnumerator BlinkAddItemText()
    {
        addItemText.gameObject.SetActive(true);
        Color originalColor = addItemText.color;

        for (int i = 0; i < 3; i++)
        {
            // Fade out
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.4f;
                float alpha = Mathf.Lerp(1f, 0f, t);
                addItemText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            // Fade in
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.4f;
                float alpha = Mathf.Lerp(0f, 1f, t);
                addItemText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
         float t2 = 0f;
            while (t2 < 1f)
            {
                t2 += Time.deltaTime / 0.4f;
                float alpha = Mathf.Lerp(1f, 0f, t2);
                addItemText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        // Garantir que termine com a opacidade original
        addItemText.color = originalColor;
        addItemText.gameObject.SetActive(false);
    }

    private void UpdateItemOptions(){
        ClearTileHighlights(true);
        ClearGrenadeHighlight();
        RemoveGrenadeOnMouseOver();
        HighlightActiveCharacter(activeCharacter.currentTile,true);
        if(isCombatOver) return;
        combatText.text = "Turno de " + activeCharacter.card.cardName;
        cancelItemButton.onClick.AddListener(CancelItemMenu);
        foreach (ItemOption itemOption in itemOptionButtons)
        {
            var card = itemOption.card;
            var icon = itemOption.option.transform.Find("Icon").GetComponent<Image>();
            var nameText = itemOption.option.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            var quantityText = itemOption.option.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
            var button = itemOption.option.transform.Find("Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = false;

            if (!card.discovered)
            {
                icon.color = Color.black;
                nameText.text = "????";
                quantityText.text = "X0";
                continue;
            }

            icon.color = Color.white;
            nameText.text = card.itemName;
            quantityText.text = "x" + card.quantity;
            if (card.quantity > 0)
            {
                button.interactable = true;
                if (card == healingPotion)
                    button.onClick.AddListener(() => OnHealPotion(itemOption));
                else if (card == lightning)
                    button.onClick.AddListener(() => OnLightning(itemOption));
                else if (card == medKit)
                    button.onClick.AddListener(() => OnMedKit(itemOption));
                else if (card == grenade)
                    button.onClick.AddListener(() => OnGrenade(itemOption));
                else if (card == holographicShield)
                    button.onClick.AddListener(() => OnHolographicShield(itemOption));
                else if (card == turret)
                    button.onClick.AddListener(() => OnTurret(itemOption));
            }
        }
    }

    private void DisableItemOptions(){
        cancelItemButton.onClick.RemoveAllListeners();
        cancelItemButton.onClick.AddListener(UpdateItemOptions);
        foreach (ItemOption option in itemOptionButtons)
        {
            var button = option.option.transform.Find("Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.interactable = false;
        }
    }

    private void OnTurret(ItemOption option)
    {
        combatText.text = "Selecione onde posicionar a Torreta.";
        DisableItemOptions();
        List<Tile> spawnableTiles = new List<Tile>();

        foreach (var tile in gridManager.tiles.Values)
        {
            if (!tile.HasCard())
            {
                tile.GetComponent<Image>().color = Color.green;

                var button = tile.transform.Find("Button").GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(true);
                spawnableTiles.Add(tile);

                // Cópia local da variável tile
                Tile selectedTile = tile;

                button.onClick.AddListener(() =>
                {
                    foreach (var t in spawnableTiles)
                    {
                        var tButton = t.transform.Find("Button")?.GetComponent<Button>();
                        tButton.onClick.RemoveAllListeners();
                        button.gameObject.SetActive(false);
                        t.GetComponent<Image>().color = protectedTiles.ContainsKey(t)? Color.blue : Color.white;
                    }
                    AudioManager.Instance.PlaySFX(option.card.useAudio);
                    // Cria a instância da torreta
                    ItemCardData turretInstance = ScriptableObject.CreateInstance<ItemCardData>();
                    turretInstance.cardName = "Torreta";
                    turretInstance.health = 6;
                    turretInstance.currenthealth = 6;
                    turretInstance.damage = 4;
                    turretInstance.movement = 0;
                    turretInstance.range = 4;
                    turretInstance.specialProperties = "Nenhuma";
                    turretInstance.cardImage = option.card.cardImage;
                    turretInstance.thumbnailImage = option.card.thumbnailImage;
                    turretInstance.attackAudio = option.card.attackAudio;

                    selectedTile.SetItem(turretInstance); // Usando selectedTile corretamente
                    itemTiles.Add(turretInstance, selectedTile);

                    option.card.quantity--;
                    UpdateItemOptions();
                });
            }
        }
    }

    private void OnHolographicShield(ItemOption option)
    {
        combatText.text = "Selecione onde o Escudo Holográfico.";
        DisableItemOptions();
        List<Tile> spawnableTiles = new List<Tile>();

        foreach (var tile in gridManager.tiles.Values)
        {
            if (!protectedTiles.ContainsKey(tile))
            {
                tile.GetComponent<Image>().color = Color.green;

                var button = tile.transform.Find("Button").GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(true);
                spawnableTiles.Add(tile);

                Tile selectedTile = tile;

                button.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySFX(option.card.useAudio);
                    protectedTiles[selectedTile] = 5;
                    foreach (var t in spawnableTiles)
                    {
                        var tButton = t.transform.Find("Button")?.GetComponent<Button>();
                        tButton.onClick.RemoveAllListeners();
                        button.gameObject.SetActive(false);
                        t.GetComponent<Image>().color = selectedTile==t?Color.blue:(protectedTiles.ContainsKey(t)?Color.blue:Color.white);
                    }

                    option.card.quantity--;
                    UpdateItemOptions();
                });
            }
        }
    }

    private void OnGrenade(ItemOption option)
    {
        combatText.text = "Selecione onde jogar a Granada.";
        DisableItemOptions();

        foreach (var tile in gridManager.tiles.Values)
        {
            var button = tile.transform.Find("Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(true);

            Tile selectedTile = tile;

            // Clicou e lançou a granada
            button.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySFX(option.card.useAudio);
                ClearGrenadeHighlight();
                RemoveGrenadeOnMouseOver();
                foreach (var t in gridManager.tiles.Values)
                {
                    int dx = Mathf.Abs(t.gridPosition[0] - selectedTile.gridPosition[0]);
                    int dy = Mathf.Abs(t.gridPosition[1] - selectedTile.gridPosition[1]);

                    if (dx + dy <= 3)
                    {
                        // Aplica dano
                        if (characterTiles.ContainsValue(t))
                        {
                            var character = characterTiles.FirstOrDefault(x => x.Value == t).Key;
                            if (protectedTiles.TryGetValue(t, out int shieldHp))
                            {
                                shieldHp -= 10;
                                if(shieldHp > 0){
                                    protectedTiles[t] = shieldHp;
                                } else {
                                    protectedTiles.Remove(t);
                                    Image tileImage = t.GetComponent<Image>();
                                    tileImage.color = Color.white;
                                }
                            }
                            else
                            {
                                character.currenthealth -= character.specialProperties == "Defesa Robusta"?10-1:10;
                            }
                            StartCoroutine(PlayDamageAnimation(t));
                            CheckIfCharacterDied(character, t);
                        }
                        else if (enemyTiles.ContainsValue(t))
                        {
                            var enemy = enemyTiles.FirstOrDefault(x => x.Value == t).Key;
                            if (protectedTiles.TryGetValue(t, out int shieldHp))
                            {
                                shieldHp -= 10;
                                if(shieldHp > 0){
                                    protectedTiles[t] = shieldHp;
                                } else {
                                    protectedTiles.Remove(t);
                                    Image tileImage = t.GetComponent<Image>();
                                    tileImage.color = Color.white;
                                }
                            }
                            else
                            {
                                enemy.currenthealth -= enemy.specialProperties == "Defesa Robusta"?10-1:10;
                            }
                            StartCoroutine(PlayDamageAnimation(t));
                            CheckIfEnemyDied(enemy, t);
                        }
                        else if (itemTiles.ContainsValue(t))
                        {
                            var item = itemTiles.FirstOrDefault(x => x.Value == t).Key;
                            if (protectedTiles.TryGetValue(t, out int shieldHp))
                            {
                                shieldHp -= 10;
                                if(shieldHp > 0){
                                    protectedTiles[t] = shieldHp;
                                } else {
                                    protectedTiles.Remove(t);
                                    Image tileImage = t.GetComponent<Image>();
                                    tileImage.color = Color.white;
                                }
                            }
                            else
                            {
                                item.currenthealth -= 10;
                            }
                            StartCoroutine(PlayDamageAnimation(t));
                            if (item.currenthealth <= 0)
                            {
                                t.ClearData();
                                itemTiles.Remove(item);
                            }
                        }
                    }
                }
                option.card.quantity--;
                UpdateItemOptions();
            });

            var eventTrigger = tile.GetComponent<EventTrigger>();
            if (eventTrigger == null) eventTrigger = tile.gameObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.RemoveAll(e => e.eventID == EventTriggerType.PointerEnter && 
                e.callback.GetPersistentEventCount() > 0 && 
                e.callback.GetPersistentMethodName(0) == "HighlightGrenadeArea");

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => HighlightGrenadeArea(selectedTile));
            eventTrigger.triggers.Add(entry);
        }
    }

    private void ClearGrenadeHighlight()
    {
        foreach (var tile in gridManager.tiles.Values)
        {
            if (tile.GetComponent<Image>().color == Color.red)
            {
                tile.GetComponent<Image>().color = protectedTiles.ContainsKey(tile)? Color.blue : Color.white;
            }
        }
    }

    private void HighlightGrenadeArea(Tile centerTile, int range = 3)
    {
        ClearGrenadeHighlight();

        foreach (var tile in gridManager.tiles.Values)
        {
            int dx = Mathf.Abs(tile.gridPosition[0] - centerTile.gridPosition[0]);
            int dy = Mathf.Abs(tile.gridPosition[1] - centerTile.gridPosition[1]);
            if (dx + dy <= range)
            {
                tile.GetComponent<Image>().color = Color.red;
            }
        }
    }

    private void RemoveGrenadeOnMouseOver(){
        foreach (var t in gridManager.tiles.Values)
        {
            var trigger = t.GetComponent<EventTrigger>();
            if (trigger == null) trigger = t.gameObject.AddComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger.triggers.RemoveAll(e =>
                    e.eventID == EventTriggerType.PointerEnter
                );
            }
        }
    }

    private void OnMedKit(ItemOption option)
    {
        combatText.text = "Selecione em quem aplicar o Kit.";
        DisableItemOptions();

        foreach (var tile in gridManager.tiles.Values)
        {
            var button = tile.transform.Find("Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false); // Oculta todos inicialmente

            // Verifica se o tile possui um personagem com vida abaixo do máximo
            var characterEntry = characterTiles.FirstOrDefault(x => x.Value == tile);
            if (characterEntry.Key != null && characterEntry.Key.currenthealth < characterEntry.Key.health)
            {
                tile.GetComponent<Image>().color = Color.green;
                button.gameObject.SetActive(true);
                var character = characterEntry.Key;
                var selectedTile = tile;

                button.onClick.AddListener(() =>
                {
                    // Cura com limite
                    AudioManager.Instance.PlaySFX(option.card.useAudio);
                    character.currenthealth = Mathf.Min(character.currenthealth + 7, character.health);

                    tile.GetComponent<Image>().color = Color.white;

                    option.card.quantity--;
                    UpdateItemOptions();
                });
            }
        }
    }

    private void OnLightning(ItemOption option)
    {
        combatText.text = "Selecione em quem lançar o Relâmpago.";
        DisableItemOptions();

        foreach (var tile in gridManager.tiles.Values)
        {
            var button = tile.transform.Find("Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false); // Oculta todos os botões inicialmente

            var enemyEntry = enemyTiles.FirstOrDefault(x => x.Value == tile);
            if (enemyEntry.Key != null && !enemyEntry.Key.isInvisible)
            {
                var enemy = enemyEntry.Key;
                if (enemy.currenthealth > 0)
                {
                    tile.GetComponent<Image>().color = Color.green;
                    button.gameObject.SetActive(true);

                    var selectedTile = tile;

                    button.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySFX(option.card.useAudio);
                        if (protectedTiles.TryGetValue(selectedTile, out int shieldHp))
                            {
                                shieldHp -= 10;
                                if(shieldHp > 0){
                                    protectedTiles[selectedTile] = shieldHp;
                                } else {
                                    protectedTiles.Remove(selectedTile);
                                    Image tileImage = selectedTile.GetComponent<Image>();
                                    tileImage.color = Color.white;
                                }
                            }
                            else
                            {
                                enemy.currenthealth -= 15;
                                CheckIfEnemyDied(enemy, selectedTile);
                            }
                        
                        StartCoroutine(PlayDamageAnimation(selectedTile));

                        tile.GetComponent<Image>().color = Color.white;
                        option.card.quantity--;
                        // Limpa destaque dos outros tiles
                        foreach (var t in gridManager.tiles.Values)
                        {
                            t.GetComponent<Image>().color = Color.white;
                            t.transform.Find("Button").gameObject.SetActive(false);
                        }
                        UpdateItemOptions();
                    });
                }
            }
        }
    }

    private void OnHealPotion(ItemOption option)
    {
        combatText.text = "Selecione em quem aplicar a Poção.";
        DisableItemOptions();

        foreach (var tile in gridManager.tiles.Values)
        {
            var button = tile.transform.Find("Button").GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false); // Oculta todos inicialmente

            // Verifica se o tile possui um personagem com vida abaixo do máximo
            var characterEntry = characterTiles.FirstOrDefault(x => x.Value == tile);
            if (characterEntry.Key != null && characterEntry.Key.currenthealth < characterEntry.Key.health)
            {
                tile.GetComponent<Image>().color = Color.green;
                button.gameObject.SetActive(true);
                var character = characterEntry.Key;
                var selectedTile = tile;

                button.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySFX(option.card.useAudio);
                    // Cura com limite
                    character.currenthealth = Mathf.Min(character.currenthealth + 9, character.health);

                    tile.GetComponent<Image>().color = Color.white;

                    option.card.quantity--;
                    UpdateItemOptions();
                });
            }
        }
    }

    private void CancelItemMenu()
    {
        itemOptions.SetActive(false);
        characterOptions.SetActive(true);
    }

    private List<Vector2Int> GetAdjacentPositions(Vector2Int origin)
    {
        return new List<Vector2Int>
        {
            origin + Vector2Int.up,
            origin + Vector2Int.down,
            origin + Vector2Int.left,
            origin + Vector2Int.right,
            origin + new Vector2Int(-1, 1),
            origin + new Vector2Int(1, 1),
            origin + new Vector2Int(-1, -1),
            origin + new Vector2Int(1, -1)
        };
    }

    private void CheckIfCharacterDied(CardData card, Tile tile)
    {
        if (card.currenthealth <= 0)
        {
            if(card.specialProperties=="Mecha"){
                if(card.isMechaCard){
                    foreach(var characterInCombat in selectedCharactersInOrder){
                        if(characterInCombat.card==card){
                            if(card.isSupported){
                                var supporter = card.supporterCharacter;
                                card.isSupported = false;
                                card.nonMechaCard.isSupported = true;
                                card.nonMechaCard.supporterCharacter = supporter;
                                supporter.supportedCharacter = card.nonMechaCard;
                                card.supporterCharacter = null;
                            }
                            characterInCombat.card = card.nonMechaCard;
                            if (characterTiles.ContainsKey(card))
                            {
                                characterTiles.Remove(card);
                                characterTiles.Add(characterInCombat.card,tile);
                            }
                            tile.ClearData();
                            tile.SetCard(card.nonMechaCard);
                        }
                    }
                } else {
                    tile.ClearData();

                    card.mechaCard.died = true;

                    if (characterTiles.ContainsKey(card))
                    {
                        characterTiles.Remove(card);
                    }

                    if(card.isSupported){
                        card.isSupported = false;
                        card.supporterCharacter.supportedCharacter = null;
                        card.supporterCharacter = null;
                    }

                    selectedCharactersInOrder.RemoveAll(c => c.card == card);

                    if(activeCharacter.card==card && isPlayerTurn){
                        EndTurn();
                    }
                }
            } else {
                tile.ClearData();

                if(card.isTransformed){
                    card.nonTransformedCard.died = true;
                } else {
                    card.died = true;
                }

                if(card.supportedCharacter!=null){
                    card.supportedCharacter.isSupported = false;
                    card.supportedCharacter.supporterCharacter = null;
                    card.supportedCharacter = null;
                }
                if(card.isSupported){
                    card.isSupported = false;
                    card.supporterCharacter.supportedCharacter = null;
                    card.supporterCharacter = null;
                }

                if (characterTiles.ContainsKey(card))
                {
                    characterTiles.Remove(card);
                }

                selectedCharactersInOrder.RemoveAll(c => c.card == card);

                if(activeCharacter.card==card && isPlayerTurn){
                    //yield return new WaitForSeconds(1.0f);
                    EndTurn();
                }
            }
            

            CheckEndOfCombat();
        }
    }

    private void CheckIfEnemyDied(EnemyCardData enemy, Tile tile)
    {
        if (enemy.currenthealth <= 0)
        {
            tile.ClearData();
            enemyTiles.Remove(enemy);
            CheckEndOfCombat();
            if(enemy.specialProperties=="Telepatia Apoiadora" && enemy.supportedCharacter!=null){
                enemy.supportedCharacter.supporterCharacter = null;
                enemy.supportedCharacter.isSupported = false;
                enemy.supportedCharacter = null;
            }
            if(enemy.isSupported){
                enemy.isSupported = false;
                enemy.supporterCharacter.supportedCharacter = null;
                enemy.supporterCharacter = null;
            }
            if (enemy.specialProperties == "Bomba")
            {
                AudioManager.Instance.PlaySFX(enemy.deathAudio);
                var adjacentTiles = GetAdjacentPositions(tile.gridPosition);
                foreach (var v in adjacentTiles)
                {
                    // Aplica dano
                    if(!gridManager.tiles.TryGetValue(v, out Tile adjacentTile)) continue;
                    var t = gridManager.tiles[v];
                    if (characterTiles.ContainsValue(t))
                    {
                        var character = characterTiles.FirstOrDefault(x => x.Value == t).Key;
                        if (protectedTiles.TryGetValue(t, out int shieldHp))
                        {
                            shieldHp -= 8;
                            if(shieldHp > 0){
                                protectedTiles[t] = shieldHp;
                            } else {
                                protectedTiles.Remove(t);
                                Image tileImage = t.GetComponent<Image>();
                                tileImage.color = Color.white;
                            }
                        }
                        else
                        {
                            character.currenthealth -= character.specialProperties == "Defesa Robusta"?8-1:8;
                        }
                        StartCoroutine(PlayDamageAnimation(t));
                        CheckIfCharacterDied(character, t);
                    }
                    else if (enemyTiles.ContainsValue(t))
                    {
                        var e = enemyTiles.FirstOrDefault(x => x.Value == t).Key;
                        if (protectedTiles.TryGetValue(t, out int shieldHp))
                        {
                            shieldHp -= 10;
                            if(shieldHp > 0){
                                protectedTiles[t] = shieldHp;
                            } else {
                                protectedTiles.Remove(t);
                                Image tileImage = t.GetComponent<Image>();
                                tileImage.color = Color.white;
                            }
                        }
                        else
                        {
                            e.currenthealth -= e.specialProperties == "Defesa Robusta"?8-1:8;
                        }
                        StartCoroutine(PlayDamageAnimation(t));
                        CheckIfEnemyDied(e, t);
                    }
                    else if (itemTiles.ContainsValue(t))
                    {
                        var item = itemTiles.FirstOrDefault(x => x.Value == t).Key;
                        if (protectedTiles.TryGetValue(t, out int shieldHp))
                        {
                            shieldHp -= 10;
                            if(shieldHp > 0){
                                protectedTiles[t] = shieldHp;
                            } else {
                                protectedTiles.Remove(t);
                                Image tileImage = t.GetComponent<Image>();
                                tileImage.color = Color.white;
                            }
                        }
                        else
                        {
                            item.currenthealth -= 10;
                        }
                        StartCoroutine(PlayDamageAnimation(t));
                        if (item.currenthealth <= 0)
                        {
                            t.ClearData();
                            itemTiles.Remove(item);
                        }
                    }
                }
            }
        }
    }

    public void UpdateCombatText(int selectedCount)
    {
        if (combatText != null)
        {
            combatText.text = $"Selecione os Personagens: {selectedCount}/{maxCharacters}";
        }
    }

    private void HighlightSpawnableTiles(bool highlight)
    {
        foreach (var tile in gridManager.tiles.Values)
        {
            if (tile.spawnable && !tile.occupied)
            {
                tile.GetComponent<Image>().color = highlight ? Color.green : Color.white;
            }
        }
    }

    private void HideHighlightSpawnableTiles()
    {
        foreach (var tile in gridManager.tiles.Values)
        {
            tile.GetComponent<Image>().color = protectedTiles.ContainsKey(tile)? Color.blue : Color.white;
        }
    }
}
