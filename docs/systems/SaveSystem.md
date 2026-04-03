## SaveSystem — архітектура з двома сервісами

За замистю використовується модульна система збереження з чітким розділенням:

### 1. **SaveService** — ігрові слоти
- **Файли:** `slot00.mvs` до `slot99.mvs`
- **Призначення:** персональні дані гравця (здоров'я, позиція, інвентар)
- **Сигнали:** реагує на `SaveRequestedSignal`, `LoadRequestedSignal`

### 2. **ConfigService** — глобальний конфіг
- **Файл:** `config.mvs` (один)
- **Призначення:** налаштування, локалізація, переваги модів
- **API:** явні методи `SaveConfig()`, `LoadConfig()`

---

## Використання

### 1. Структура модулів — ігрові дані

Модуль реалізує `ISaveModule` для збереження/завантаження одного компонента дан:

```csharp
using Kruty1918.Moyva.SaveSystem.API;

// Приклад: збереження даних гравця
public class PlayerDataModule : ISaveModule
{
    private int _health;
    private int _mana;
    private Vector2Int _position;

    public PlayerDataModule(PlayerController player)
    {
        _health = player.Health;
        _mana = player.Mana;
        _position = player.Position;
    }

    public void OnSave(ISaveContext ctx)
    {
        ctx.Writer.Write(_health);
        ctx.Writer.Write(_mana);
        ctx.Writer.Write(_position.x);
        ctx.Writer.Write(_position.y);
        Debug.Log($"[Save] Гравець збережений: HP={_health}, Mana={_mana}, Pos={_position}");
    }

    public void OnLoad(ISaveContext ctx)
    {
        _health = ctx.Reader.ReadInt32();
        _mana = ctx.Reader.ReadInt32();
        int x = ctx.Reader.ReadInt32();
        int y = ctx.Reader.ReadInt32();
        _position = new Vector2Int(x, y);
        Debug.Log($"[Load] Гравець завантажений: HP={_health}, Mana={_mana}, Pos={_position}");
    }

    // Публічні сеттери для операцій після завантаження
    public void ApplyToPlayer(PlayerController player)
    {
        player.SetHealth(_health);
        player.SetMana(_mana);
        player.SetPosition(_position);
    }
}

// Приклад: збереження інвентаря
public class InventoryModule : ISaveModule
{
    private List<ItemStack> _items = new();

    public InventoryModule(Inventory inventory)
    {
        _items = inventory.GetItems();
    }

    public void OnSave(ISaveContext ctx)
    {
        ctx.Writer.Write(_items.Count);
        foreach (var item in _items)
        {
            ctx.Writer.Write(item.ItemId);
            ctx.Writer.Write(item.Quantity);
        }
    }

    public void OnLoad(ISaveContext ctx)
    {
        int count = ctx.Reader.ReadInt32();
        _items.Clear();
        for (int i = 0; i < count; i++)
        {
            string itemId = ctx.Reader.ReadString();
            int qty = ctx.Reader.ReadInt32();
            _items.Add(new ItemStack(itemId, qty));
        }
    }

    public void ApplyToInventory(Inventory inventory)
    {
        inventory.Clear();
        foreach (var item in _items)
            inventory.AddItem(item.ItemId, item.Quantity);
    }
}

// Приклад: збереження стану рівня (FOW, об'єкти, враги)
public class LevelStateModule : ISaveModule
{
    private bool[,] _exploredTiles;
    private Dictionary<string, EnemyState> _enemies = new();
    private int _completedObjectives;

    public LevelStateModule(LevelManager level, FogOfWarService fog)
    {
        _exploredTiles = fog.GetExploredSnapshot();
        _enemies = level.GetEnemyStates();
        _completedObjectives = level.CompletedObjectives;
    }

    public void OnSave(ISaveContext ctx)
    {
        // FOW
        ctx.Writer.Write(_exploredTiles.GetLength(0));
        ctx.Writer.Write(_exploredTiles.GetLength(1));
        for (int x = 0; x < _exploredTiles.GetLength(0); x++)
            for (int y = 0; y < _exploredTiles.GetLength(1); y++)
                ctx.Writer.Write(_exploredTiles[x, y]);

        // Враги
        ctx.Writer.Write(_enemies.Count);
        foreach (var kvp in _enemies)
        {
            ctx.Writer.Write(kvp.Key);
            ctx.Writer.Write(kvp.Value.Health);
            ctx.Writer.Write(kvp.Value.Position.x);
            ctx.Writer.Write(kvp.Value.Position.y);
        }

        ctx.Writer.Write(_completedObjectives);
    }

    public void OnLoad(ISaveContext ctx)
    {
        int w = ctx.Reader.ReadInt32();
        int h = ctx.Reader.ReadInt32();
        _exploredTiles = new bool[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                _exploredTiles[x, y] = ctx.Reader.ReadBoolean();

        int enemyCount = ctx.Reader.ReadInt32();
        _enemies.Clear();
        for (int i = 0; i < enemyCount; i++)
        {
            string id = ctx.Reader.ReadString();
            int health = ctx.Reader.ReadInt32();
            int px = ctx.Reader.ReadInt32();
            int py = ctx.Reader.ReadInt32();
            _enemies[id] = new EnemyState(health, new Vector2Int(px, py));
        }

        _completedObjectives = ctx.Reader.ReadInt32();
    }

    public void ApplyToLevel(LevelManager level, FogOfWarService fog)
    {
        fog.LoadFromSnapshot(_exploredTiles);
        level.RestoreEnemyStates(_enemies);
        level.SetCompletedObjectives(_completedObjectives);
    }
}
```

### 2. Структура модулів — конфіг користувача

```csharp
// Збереження аудіо налаштувань
public class AudioSettingsModule : ISaveModule
{
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.8f;
    public float SFXVolume { get; set; } = 0.9f;
    public bool IsMuted { get; set; }

    public void OnSave(ISaveContext ctx)
    {
        ctx.Writer.Write(MasterVolume);
        ctx.Writer.Write(MusicVolume);
        ctx.Writer.Write(SFXVolume);
        ctx.Writer.Write(IsMuted);
    }

    public void OnLoad(ISaveContext ctx)
    {
        MasterVolume = ctx.Reader.ReadSingle();
        MusicVolume = ctx.Reader.ReadSingle();
        SFXVolume = ctx.Reader.ReadSingle();
        IsMuted = ctx.Reader.ReadBoolean();
    }

    public void ApplyToAudioManager(AudioManager audioManager)
    {
        audioManager.SetMasterVolume(MasterVolume);
        audioManager.SetMusicVolume(MusicVolume);
        audioManager.SetSFXVolume(SFXVolume);
        audioManager.SetMuted(IsMuted);
    }
}

// Збереження графіки
public class GraphicsSettingsModule : ISaveModule
{
    public int ResolutionWidth { get; set; } = 1920;
    public int ResolutionHeight { get; set; } = 1080;
    public int Quality { get; set; } = 3;  // 0-5
    public bool VSync { get; set; } = true;
    public bool Fullscreen { get; set; } = true;

    public void OnSave(ISaveContext ctx)
    {
        ctx.Writer.Write(ResolutionWidth);
        ctx.Writer.Write(ResolutionHeight);
        ctx.Writer.Write(Quality);
        ctx.Writer.Write(VSync);
        ctx.Writer.Write(Fullscreen);
    }

    public void OnLoad(ISaveContext ctx)
    {
        ResolutionWidth = ctx.Reader.ReadInt32();
        ResolutionHeight = ctx.Reader.ReadInt32();
        Quality = ctx.Reader.ReadInt32();
        VSync = ctx.Reader.ReadBoolean();
        Fullscreen = ctx.Reader.ReadBoolean();
    }

    public void ApplyToGraphics()
    {
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, Fullscreen);
        QualitySettings.SetQualityLevel(Quality);
        QualitySettings.vSyncCount = VSync ? 1 : 0;
    }
}

// Збереження мови та локалізації
public class LocalizationModule : ISaveModule
{
    public string CurrentLanguage { get; set; } = "uk";

    public void OnSave(ISaveContext ctx)
    {
        ctx.Writer.Write(CurrentLanguage ?? "uk");
    }

    public void OnLoad(ISaveContext ctx)
    {
        CurrentLanguage = ctx.Reader.ReadString();
    }

    public void ApplyToGame(LocalizationService locService)
    {
        locService.SetLanguage(CurrentLanguage);
    }
}
```

### 3. Повна інтеграція — збереження ігрових даних

```csharp
/// <summary>
/// Менеджер ігрових сейвів. Коордлює збереження/завантаження слотів.
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    [Inject] private ISaveService _saveService;
    [Inject] private PlayerController _player;
    [Inject] private Inventory _inventory;
    [Inject] private LevelManager _levelManager;
    [Inject] private FogOfWarService _fogOfWar;

    // Сигнал при завершенні
    public signal SaveCompletedSignal SaveCompleted { get; set; }
    public signal LoadCompletedSignal LoadCompleted { get; set; }

    /// <summary>
    /// Збереження поточної гри в слот
    /// </summary>
    public void SaveGameToSlot(int slot)
    {
        if (slot < 0 || slot > 99)
        {
            Debug.LogError($"Invalid slot: {slot}. Range: 0-99");
            return;
        }

        try
        {
            // Створити модулі з поточного стану
            var modules = new List<ISaveModule>
            {
                new PlayerDataModule(_player),
                new InventoryModule(_inventory),
                new LevelStateModule(_levelManager, _fogOfWar)
            };

            // Запустити збереження в фоновому потоці
            _saveService.Save(slot);
            Debug.Log($"[SaveManager] Гра збережена в слот {slot}");

            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Помилка при збереженні: {ex.Message}");
        }
    }

    /// <summary>
    /// Завантаження гри зі слота
    /// </summary>
    public void LoadGameFromSlot(int slot)
    {
        if (slot < 0 || slot > 99)
        {
            Debug.LogError($"Invalid slot: {slot}. Range: 0-99");
            return;
        }

        try
        {
            var modules = new List<ISaveModule>
            {
                new PlayerDataModule(_player),
                new InventoryModule(_inventory),
                new LevelStateModule(_levelManager, _fogOfWar)
            };

            // Завантажити
            _saveService.Load(slot);
            Debug.Log($"[SaveManager] Гра завантажена зі слота {slot}");

            LoadCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Помилка при завантаженні: {ex.Message}");
        }
    }

    /// <summary>
    /// Видалить слот (опціонально: залишить .bak backup)
    /// </summary>
    public void DeleteSlot(int slot)
    {
        string slotFile = GetSlotPath(slot);
        if (File.Exists(slotFile))
        {
            File.Delete(slotFile);
            Debug.Log($"[SaveManager] Слот {slot} видалений");
        }
    }

    /// <summary>
    /// Отримати інформацію про слот (час збереження, розмір)
    /// </summary>
    public SaveInfo GetSlotInfo(int slot)
    {
        return _saveService.GetSlotInfo(slot);
    }

    private string GetSlotPath(int slot)
        => Path.Combine(Application.persistentDataPath, "saves", $"slot{slot:D2}.mvs");
}
```

### 4. Повна інтеграція — конфіг користувача

```csharp
/// <summary>
/// Менеджер налаштувань гри. Синглтон для доступу до конфіга.
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    [Inject] private IConfigService _configService;

    public AudioSettingsModule AudioSettings { get; private set; }
    public GraphicsSettingsModule GraphicsSettings { get; private set; }
    public LocalizationModule LocalizationSettings { get; private set; }

    // Сигнали
    public signal SettingsSavedSignal SettingsSaved { get; set; }
    public signal SettingsLoadedSignal SettingsLoaded { get; set; }

    private void Start()
    {
        // Автоматично завантажити конфіг при запуску
        LoadSettings();
    }

    private void OnDestroy()
    {
        // Автоматично зберегти конфіг при вимиканні
        SaveSettings();
    }

    /// <summary>
    /// Завантажити всі налаштування з config.mvs
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            AudioSettings = new AudioSettingsModule();
            GraphicsSettings = new GraphicsSettingsModule();
            LocalizationSettings = new LocalizationModule();

            var modules = new List<ISaveModule>
            {
                AudioSettings,
                GraphicsSettings,
                LocalizationSettings
            };

            _configService.LoadConfig(modules);

            // Застосувати налаштування до систем
            AudioSettings.ApplyToAudioManager(FindObjectOfType<AudioManager>());
            GraphicsSettings.ApplyToGraphics();
            LocalizationSettings.ApplyToGame(FindObjectOfType<LocalizationService>());

            Debug.Log("[Settings] Конфіг завантажений успішно");
            SettingsLoaded?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Settings] Помилка при завантаженні конфіга: {ex.Message}. Используються значення за замовчанням.");
            InitializeDefaults();
        }
    }

    /// <summary>
    /// Зберегти всі налаштування в config.mvs
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            var modules = new List<ISaveModule>
            {
                AudioSettings ?? new AudioSettingsModule(),
                GraphicsSettings ?? new GraphicsSettingsModule(),
                LocalizationSettings ?? new LocalizationModule()
            };

            _configService.SaveConfig(modules);
            Debug.Log("[Settings] Конфіг збережений успішно");
            SettingsSaved?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Settings] Помилка при збереженні конфіга: {ex.Message}");
        }
    }

    /// <summary>
    /// Отримати інформацію про конфіг (розмір, час модифікації)
    /// </summary>
    public SaveInfo GetConfigInfo()
    {
        return _configService.GetConfigInfo();
    }

    /// <summary>
    /// Видалити конфіг та використовувати дефолти
    /// </summary>
    public void ResetToDefaults()
    {
        _configService.DeleteConfig();
        InitializeDefaults();
        Debug.Log("[Settings] Конфіг скинутий на значення за замовчанням");
    }

    /// <summary>
    /// Перевірити, чи існує конфіг файл
    /// </summary>
    public bool HasSavedConfig()
    {
        return _configService.HasConfig();
    }

    private void InitializeDefaults()
    {
        AudioSettings = new AudioSettingsModule();
        GraphicsSettings = new GraphicsSettingsModule();
        LocalizationSettings = new LocalizationModule();
    }
}
```

### 5. GUI приклад — меню збереження

```csharp
/// <summary>
/// UI для вибору слота при збереженні / завантаженні
/// </summary>
public class SaveMenuUI : MonoBehaviour
{
    [Inject] private GameSaveManager _gameSaveManager;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private SaveSlotUI _slotPrefab;

    private List<SaveSlotUI> _slotUIs = new();

    private void Start()
    {
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        foreach (var slotUI in _slotUIs)
            Destroy(slotUI.gameObject);
        _slotUIs.Clear();

        // Показати 10 слотів (0-9)
        for (int i = 0; i < 10; i++)
        {
            var slotUI = Instantiate(_slotPrefab, _slotContainer);
            var info = _gameSaveManager.GetSlotInfo(i);

            slotUI.SetSlotNumber(i);
            if (info != null)
            {
                slotUI.SetSaveTime(info.SaveTime);
                slotUI.SetFileSize(info.FileSize);
                slotUI.SetHasData(true);
            }
            else
            {
                slotUI.SetHasData(false);
            }

            slotUI.OnSelected += () => OnSlotSelected(i);
            _slotUIs.Add(slotUI);
        }
    }

    private void OnSlotSelected(int slot)
    {
        // Запустити корутину для збереження/завантаження
        StartCoroutine(SaveOrLoadCoroutine(slot));
    }

    private IEnumerator SaveOrLoadCoroutine(int slot)
    {
        _gameSaveManager.SaveGameToSlot(slot);
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Гра збережена в слот {slot}");
        RefreshSlots();
    }
}

/// <summary>
/// UI елемент для одного слота
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private Text _slotNumberText;
    [SerializeField] private Text _saveTimeText;
    [SerializeField] private Text _fileSizeText;
    [SerializeField] private Button _selectButton;

    public event System.Action OnSelected;

    private void Start()
    {
        _selectButton.onClick.AddListener(() => OnSelected?.Invoke());
    }

    public void SetSlotNumber(int slot)
    {
        _slotNumberText.text = $"Слот {slot}";
    }

    public void SetSaveTime(System.DateTime time)
    {
        _saveTimeText.text = time.ToString("dd.MM.yyyy HH:mm");
    }

    public void SetFileSize(long bytes)
    {
        string sizeText = bytes > 1024 * 1024 
            ? $"{bytes / (1024 * 1024.0):F1} MB"
            : $"{bytes / 1024.0:F1} KB";
        _fileSizeText.text = sizeText;
    }

    public void SetHasData(bool hasData)
    {
        _saveTimeText.text = hasData ? _saveTimeText.text : "пусто";
        _fileSizeText.text = hasData ? _fileSizeText.text : "";
        _selectButton.interactable = hasData;
    }
}
```

### 6. Відновлення при збої

```csharp
/// <summary>
/// Сценарій відновлення при загруженні сейва з пошкодженням
/// </summary>
public class SaveRecoveryManager : MonoBehaviour
{
    [Inject] private ISaveService _saveService;

    /// <summary>
    /// Спроба завантажити слот, при невдачі — використати backup
    /// </summary>
    public bool TryLoadWithFallback(int slot)
    {
        try
        {
            var modules = new List<ISaveModule>
            {
                new PlayerDataModule(null),
                new InventoryModule(null),
                new LevelStateModule(null, null)
            };

            _saveService.Load(slot);
            Debug.Log($"[Recovery] Слот {slot} завантажений успішно");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Recovery] Помилка при завантаженні {slot}: {ex.Message}");
            Debug.Log($"[Recovery] Спроба відновити з backup...");

            // Backup буде автоматично використаний SaveService,
            // якщо основний файл пошкджен
            return false;
        }
    }

    /// <summary>
    /// Отримати список резервних копій для слота
    /// </summary>
    public List<SaveInfo> GetAvailableBackups(int slot)
    {
        var backups = new List<SaveInfo>();
        string slotPath = GetSlotPath(slot);
        string backupPath = slotPath + ".bak";

        if (File.Exists(slotPath))
            backups.Add(GetFileInfo(slotPath, "основний"));

        if (File.Exists(backupPath))
            backups.Add(GetFileInfo(backupPath, "backup"));

        return backups;
    }

    private string GetSlotPath(int slot)
        => Path.Combine(Application.persistentDataPath, "saves", $"slot{slot:D2}.mvs");

    private SaveInfo GetFileInfo(string path, string label)
    {
        var fi = new FileInfo(path);
        return new SaveInfo
        {
            FilePath = path,
            Label = label,
            FileSize = fi.Length,
            SaveTime = fi.LastWriteTime
        };
    }
}
```

---

## Шляхи до файлів

```
$PERSISTENT_DATA_PATH/saves/
├── slot00.mvs         ← гравець 1 (PlayerData + Inventory + Stats)
├── slot01.mvs         ← гравець 2
├── slot02.mvs         ← гравець 3
│
└── config.mvs         ← конфіг (Audio + Graphics + Localization)
```

На різних платформах:
- **Android:** `/data/data/com.company.game/files/saves/`
- **iOS:** `Documents/saves/`
- **Windows:** `%AppData%/DefaultCompany/GameName/saves/`
- **Editor:** `~/.config/unity3d/DefaultCompany/ProjectName/saves/`

---

## Формат .mvs

Обидва сервіси (SaveService та ConfigService) використовують один формат:

```
File: slot00.mvs або config.mvs
├─ Magic (4b):     "MVSA"
├─ Version (2b):   1
├─ BlockCount(4b): количество модулей
├─ Blocks (N шт):
│  ├─ BlockId (4b)
│  ├─ BlockSize (4b)
│  ├─ BlockCrc32 (4b)
│  └─ Payload (BlockSize b)
└─ GlobalCrc32 (4b): CRC всього файлу
```

Кожен блок відповідає одному модулю. Формат дозволяє:
- Добавляти нові модулі без порушення старих файлів
- Пропускати невідомі модулі при завантаженні
- Верифікувати цілісність дан через CRC

---

## Атомарні операції

Обидва сервіси гарантують **atomic writes**:

1. Запис у `.tmp` файл
2. Верифікація (magic + size + CRC)
3. Створення `.bak` (backup старого)
4. Атомарний rename `.tmp` → фінальний

Завдяки цьому не втрачаються дані при збої або відключенні.
