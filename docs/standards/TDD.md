# TDD: Модульна архітектура Unity (Zenject + asmdef)

## Мета документа

Цей документ фіксує технічний стандарт для проєкту Moyva:

- архітектура Package by Feature;
- ізоляція модулів через API/Runtime/Editor;
- керування залежностями через Zenject;
- контроль кордонів між збірками через Assembly Definitions.

Документ призначений для щоденної розробки, code review та онбордингу.

---

## Чому Package by Feature

Для ігрового проєкту, який активно росте по фічах, підхід Package by Feature дає:

- локалізовані зміни в межах однієї фічі;
- менше випадкових залежностей між несуміжними частинами коду;
- простіше тестування та безпечніший рефакторинг;
- швидшу навігацію по репозиторію.

---

## Базова структура

```text
Assets/
└── Moyva/
    └── Scripts/
        ├── Bootstrap/
        │   └── Runtime/
        └── Features/
            ├── <FeatureName>/
            │   ├── API/
            │   ├── Runtime/
            │   └── Editor/
            └── ...
```

---

## Призначення шарів

| Шар | Вміст | Правила |
|---|---|---|
| API | Інтерфейси, DTO, enum, сигнали | Жодної бізнес-реалізації |
| Runtime | Реалізації сервісів, інсталери Zenject | Зовнішні модулі працюють через API |
| Editor | UnityEditor tooling, custom inspectors, вікна | Не має потрапляти в runtime-білд |

---

## Правила залежностей (обов'язково)

1. Будь-яка фіча експортує назовні лише `API`.
2. Інші модулі не залежать від чужого `Runtime` напряму.
3. `Bootstrap` зв'язує модулі через DI-контейнер, а не через прямі new-залежності.
4. Cross-feature комунікація йде через:
   - інтерфейси API;
   - сигнали;
   - мінімальні DTO.

---

## Стандарт для asmdef

Рекомендовано мати щонайменше окремі збірки:

- `Kruty1918.Moyva.<Feature>` для фічі;
- `Kruty1918.Moyva.<Feature>.Editor` (де є editor-код);
- `Kruty1918.Moyva.Tests.<Feature>` для тестів.

Ключові вимоги:

- забороняти зайві references;
- уникати циклічних залежностей;
- підключати тести до API/дозволених runtime-точок через asmdef-посилання;
- тримати `overrideReferences` і `defineConstraints` контрольованими.

---

## Zenject-патерн модулів

Кожна фіча має власний installer і явно декларує контракти.

```csharp
public sealed class FeatureInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IFeatureService>()
            .To<FeatureService>()
            .AsSingle();
    }
}
```

Практика:

- якщо сервіс потрібен назовні, споживач бачить тільки `IFeatureService`;
- concrete-типи не використовуються з інших фіч напряму.

---

## Інкапсуляція реалізацій

`internal` для runtime-реалізацій залишається базовим правилом, якщо немає окремої причини відкрити клас.

Допускаються винятки:

- інтеграційні тести в іншій збірці;
- інфраструктурні обмеження контейнера/плагіна;
- технічний борг з чітким TODO.

Рекомендований шлях для тестів: залишати runtime-реалізації `internal` і відкривати доступ
конкретній тестовій збірці через `InternalsVisibleTo`, замість підняття класу до `public`.

У таких випадках рішення має бути зафіксоване в PR/документації.

---

## Шаблон перевірки для PR

Перед мерджем перевіряємо:

1. Нова фіча має `API/Runtime` (і `Editor`, якщо потрібно).
2. Зовнішні модулі не використовують конкретні runtime-класи фічі.
3. asmdef-залежності мінімальні, циклів немає.
4. Реєстрація залежностей відбувається через installer.
5. Публічні контракти описані в docs/systems.

---

## Стандарт структури сцен (Scenes)

```text
Assets/
└── Moyva/
    └── Scenes/
        ├── Main/
        │   └── GameScene.unity          — основна ігрова сцена
        ├── UI/
        │   └── UIOverlay.unity          — UI-оверлей (additive)
        ├── Bootstrap/
        │   └── BootstrapScene.unity     — entry-point, DI-контейнер
        └── Testing/
            └── TestScene.unity          — сцена для play-mode тестів
```

Правила:

1. Кожна сцена має чітку відповідальність (одна сцена ≠ одна фіча, але і не "все в одному").
2. `BootstrapScene` — єдиний entry-point, завантажує інші адитивно.
3. UI-сцени завантажуються адитивно і не залежать від ігрової логіки напряму.
4. Тестові сцени (play-mode) ізольовані в `Testing/` і не входять у білд.
5. Заборонено зберігати скрипти всередині `.unity` файлів — лише MonoBehaviour-посилання на prefab/asmdef.

---

## Стандарт для аудіо (Audio)

```text
Assets/
└── Moyva/
    └── Audio/
        ├── Music/
        │   ├── Ambient/           — фонова музика
        │   └── Combat/            — бойова музика
        ├── SFX/
        │   ├── UI/                — натиски кнопок, переходи
        │   ├── Units/             — кроки, атаки, голоси
        │   ├── Environment/       — вітер, вода, вогонь
        │   └── Construction/      — будівництво, руйнування
        └── Configs/
            └── AudioMixerMain.mixer
```

Правила:

1. Аудіофайли організовані за контекстом використання, а не за форматом.
2. Назви файлів: `category_action_variant.wav` (наприклад `unit_attack_sword_01.wav`).
3. Формати: `.wav` для SFX (без стиснення в редакторі), `.ogg` для Music (streaming).
4. AudioMixer має групи: Master → Music, SFX, UI — з окремим контролем гучності.
5. Заборонено хардкодити шляхи до аудіо — використовувати `AudioClip` через SO або Addressables.
6. Кожен SFX-файл повинен мати нормалізований рівень гучності (peak −3 dB).

---

## Стандарт для мистецтва (Art Assets)

```text
Assets/
└── Moyva/
    └── Art/
        ├── Sprites/
        │   ├── Units/             — спрайти юнітів
        │   ├── Buildings/         — спрайти будівель
        │   ├── Tiles/             — тайли карти
        │   ├── UI/                — іконки, фони, елементи UI
        │   └── Effects/           — VFX-спрайти, частинки
        ├── Animations/
        │   ├── Units/             — анімації юнітів
        │   └── Buildings/         — анімації будівель
        ├── Materials/
        │   ├── Sprites/           — материали для спрайтів
        │   └── Shaders/           — кастомні шейдери
        └── Prefabs/
            ├── Units/             — префаби юнітів
            ├── Buildings/         — префаби будівель
            └── Effects/           — VFX-префаби
```

Правила:

1. Спрайти групуються за ігровим доменом (Units, Buildings, Tiles), а не за технічним типом.
2. Naming: `domain_objectType_variant_size.png` (наприклад `unit_warrior_idle_64.png`).
3. Для 2D: Pixels Per Unit єдиний на проєкт (32 або 64), налаштовується в Sprite Import Settings.
4. Анімації зберігаються поруч із відповідними спрайтами або в `Animations/<Domain>/`.
5. Prefab-и мають мінімум компонентів; логіка — у скриптах з asmdef, не в prefab-інспекторі.
6. Заборонено змішувати 3D-моделі та 2D-спрайти в одних теках.
7. Кожен мистецький ассет має бути оптимізований: стиснення ASTC для Android, Crunch для Desktop.

---

## Стандарт структури тек (Folder Convention)

```text
Assets/
└── Moyva/
    ├── Scripts/
    │   ├── Bootstrap/             — entry-point, DI-контейнер
    │   ├── Features/              — фіча-модулі (API/Runtime/Editor)
    │   └── Tests/                 — editor & play-mode тести
    ├── Scenes/                    — Unity-сцени (див. вище)
    ├── Audio/                     — звук (див. вище)
    ├── Art/                       — спрайти, анімації, матеріали (див. вище)
    ├── Configs/                   — ScriptableObject конфіги, JSON-параметри
    ├── Plugins/                   — сторонні бібліотеки (Zenject, DOTween, тощо)
    ├── Resources/                 — МІНІМУМ: лише те, що потрібно Runtime.Load
    └── StreamingAssets/           — файли, що копіюються as-is у білд
```

Правила:

1. **Жодних файлів у кореневому `Assets/`** — все під `Assets/Moyva/`.
2. Кожна тека першого рівня (`Scripts`, `Scenes`, `Audio`, `Art`, `Configs`) має чіткий scope.
3. `Resources/` тримати мінімальним — перевага Addressables або прямих посилань.
4. `Plugins/` — сторонній код; не редагувати вручну, тримати версії.
5. `Configs/` — SO-ассети для runtime-конфігурації (EconomyRulesConfigSO, CalendarConfig, тощо).
6. Нові фіча-модулі створюються лише через стандартну структуру: `Features/<Name>/API + Runtime + (Editor)`.
7. Тести розміщуються в `Tests/<Feature>/` з окремим asmdef (Editor-only platform).
8. Заборонено дублювати ассети між теками (один ассет — одне місце).

---

## Стандарт тестування (Testing Convention)

```text
Assets/
└── Moyva/
    └── Scripts/
        └── Tests/
            ├── <Feature>/
            │   ├── Kruty1918.Moyva.Tests.<Feature>.asmdef
            │   ├── <Feature>Tests.cs
            │   └── <Feature>ExtendedTests.cs   — (за потреби)
            └── ...
```

Правила:

1. Кожен модуль має мінімум один тестовий файл з editor-тестами.
2. asmdef тестів:
   - `includePlatforms: ["Editor"]`;
   - references лише на API тестованої фічі + Zenject (якщо потрібно);
   - `overrideReferences: true`, `precompiledReferences: ["nunit.framework.dll"]`.
3. Два патерни тестування:
   - **ZenjectUnitTestFixture** — для сервісів із SignalBus;
   - **Пряме створення** — для простих сервісів без DI.
4. Фейки (mock): лише **hand-written sealed fakes** (без NSubstitute/Moq).
5. Naming: `Method_ShouldBehavior_WhenCondition`.
6. Доступ до `internal` типів через `typeof(IInterface).Assembly.GetType(...)` або `InternalsVisibleTo`.
7. Мінімальне покриття: кожен публічний метод API має щонайменше один тест.
8. Play-mode тести (якщо потрібні) — в окремому asmdef з `defineConstraints: ["UNITY_INCLUDE_TESTS"]`.

---

## Застосування до Moyva

Поточний стан проєкту відповідає цій моделі:

- модулі в `Assets/Moyva/Scripts/Features` розділені по доменах;
- для ключових систем є окремі API контракти;
- документація систем зібрана в `docs/systems`;
- TDD використовується як стандарт архітектурних рішень;
- тести покривають 759 сценаріїв (540 нових) по 16+ модулях;
- структура тек, сцен, аудіо та арту стандартизована.

