# Camera — Система керування камерою

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/camera)

---

## Призначення

Система **Camera** забезпечує повне керування камерою гравця: плавне переміщення по карті, зум (наближення/віддалення) і автофокус на об'єкт. Реалізована через три незалежних сервіси + контролер вводу.

---

## Компоненти системи

| Клас / Інтерфейс | Роль |
|---|---|
| `ICameraMovement` / `CameraMovement` | Переміщення камери (панорамування) |
| `ICameraZoom` / `CameraZoom` | Зум камери (ортографічний або perspective) |
| `ICameraFocused` / `CameraFocused` | Форсований фокус на `Transform` |
| `CameraPlayerController` | Зчитує ввід гравця (Input System) і викликає сервіси |
| `CameraSettingsSO` | ScriptableObject з усіма параметрами камери |

---

## Як працює внутрішньо

### Переміщення (`CameraMovement`)

- На кожен `LateTick()` камера плавно рухається до `_targetPosition` через `Vector3.SmoothDamp`.
- `MoveCamera(delta)` перераховує піксельну дельту мишки/дотику у worldspace-одиниці: `unitsPerPixel = (orthographicSize * 2) / Screen.height`.
- Після `ForceMoveCameraToPosition` вмикається таймер `_forceBlockTimer`, який блокує ввід гравця на 1.5 с.

### Зум (`CameraZoom`)

- Підтримує обидва режими: `orthographic` (2D) та `perspective` (3D).
- `ZoomCamera(delta)` коригує `_targetZoom` у межах `[minZoom, maxZoom]` з `CameraSettingsSO`.
- На `LateTick()` використовується `Mathf.SmoothDamp` до `_targetZoom`.

### Фокус (`CameraFocused`)

- `Focus(Transform target)` викликає `ForceMoveCameraToPosition` і `ForceZoomCamera` зі середнім значенням зуму.

### Контролер гравця (`CameraPlayerController`)

- Читає `InputActionAsset` з map `"Player"`, actions `"Move"` і `"Zoom"`.
- На кожен `Tick()` передає вектор руху і дельту зуму відповідним сервісам.
- Реалізує `IDisposable` для вимкнення actions при знищенні.

---

## Публічний API

### `ICameraMovement`

```csharp
public interface ICameraMovement
{
    // Переміщує камеру на вектор delta (піксельна дельта мишки / дотику)
    void MoveCamera(Vector3 direction);

    // Миттєво встановлює нову ціль і блокує гравця на 1.5 с
    void ForceMoveCameraToPosition(Vector3 position);
}
```

### `ICameraZoom`

```csharp
public interface ICameraZoom
{
    // Зміщує зум на delta * zoomSpeed (scroll колеса або pinch)
    void ZoomCamera(float zoomAmount);

    // Форсований зум до заданого рівня
    void ForceZoomCamera(float zoomLevel);
}
```

### `ICameraFocused`

```csharp
public interface ICameraFocused
{
    // Центрує камеру на Transform і встановлює середній зум
    void Focus(Transform target);
}
```

### `CameraSettingsSO` — Налаштування

| Поле | Тип | Значення за замовч. | Опис |
|---|---|---|---|
| `moveSpeed` | `float` | 5.0 | Швидкість панорамування (≈1.0 = "приклеєна до курсора") |
| `smoothTime` | `float` | 0.3 | Час згладжування руху/зуму |
| `zoomSpeed` | `float` | 5.0 | Швидкість зуму |
| `minZoom` | `float` | 2.0 | Мінімальний ортографічний розмір |
| `maxZoom` | `float` | 10.0 | Максимальний ортографічний розмір |
| `defaultCameraZ` | `float` | -10.0 | Z-позиція камери (2D) |

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `MoveCamera` | `Vector3 delta` (піксельна дельта) | `void` |
| `ForceMoveCameraToPosition` | `Vector3 position` | `void` |
| `ZoomCamera` | `float delta` | `void` |
| `ForceZoomCamera` | `float zoomLevel` | `void` |
| `Focus` | `Transform target` | `void` |

---

## Залежності

| Залежність | Причина |
|---|---|
| `UnityEngine.Camera` | Зміна `orthographicSize` / `transform.position` |
| `CameraSettingsSO` | Всі параметри руху і зуму |
| `InputActionAsset` | Зчитування вводу гравця |

---

## Реєстрація в Zenject (`CameraInstaller`)

```csharp
public class CameraInstaller : MonoInstaller
{
    [SerializeField] private UnityEngine.Camera _sceneCamera;
    [SerializeField] private CameraSettingsSO   _cameraSettings;
    [SerializeField] private InputActionAsset   _cameraInputAsset;

    public override void InstallBindings()
    {
        var camera = _sceneCamera != null ? _sceneCamera : UnityEngine.Camera.main;
        Container.BindInstance(camera).AsSingle();
        Container.BindInstance(_cameraSettings).AsSingle();
        Container.BindInstance(_cameraInputAsset).AsSingle();

        // ITickable, IInitializable, ILateTickable підхоплюються автоматично
        Container.BindInterfacesAndSelfTo<CameraMovement>().AsSingle();
        Container.BindInterfacesAndSelfTo<CameraZoom>().AsSingle();
        Container.BindInterfacesTo<CameraFocused>().AsSingle();
        Container.BindInterfacesAndSelfTo<CameraPlayerController>().AsSingle();
    }
}
```

---

## Приклади використання

### Програмне переміщення камери

```csharp
[Inject] private ICameraMovement _cameraMovement;

// Перемістити камеру в точку (10, 5, -10)
_cameraMovement.ForceMoveCameraToPosition(new Vector3(10f, 5f, -10f));
```

### Фокус на юніті після його вибору

```csharp
[Inject] private ICameraFocused _cameraFocused;
[Inject] private IUnitService   _unitService;

if (_unitService.TryGetUnitPosition(unitId, out var pos))
{
    var unitObj = _unitService.GetUnitObject(unitId);
    _cameraFocused.Focus(unitObj.transform);
}
```

### Програмне масштабування

```csharp
[Inject] private ICameraZoom _cameraZoom;

// Наблизити до мінімального значення
_cameraZoom.ForceZoomCamera(2f);
```

### Обробка вводу (автоматично через `CameraPlayerController`)

```csharp
// Відбувається в Tick() — нічого додатково писати не треба
// Достатньо налаштувати InputActionAsset з map "Player" і actions "Move", "Zoom"
```

---

## Пов'язані системи

- [Signals](signals.md) — може підписуватись на `UnitCreatedSignal` для автофокусу (розширення)
- [Units](units.md) — `ICameraFocused.Focus` використовує `IUnitService.GetUnitObject`
