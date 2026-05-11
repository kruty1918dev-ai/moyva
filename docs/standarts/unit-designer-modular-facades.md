# Unit Designer Modular Facades

Мета: розбити великий Unit Designer на ізольовані підмодулі з окремими фасадами, щоб зменшити зв'язаність та спростити підтримку.

## Підмодулі

1. Identity
- TypeId, Role, валідація і danger-операції запису.

2. Prefab
- Призначення prefab/sprite, перевірка SpriteRenderer/Animator, quick actions.

3. Animation
- Path animation settings, AnimationClips, auto-generation tools.

4. Preview
- Симуляція preview, відображення стану і візуальні підказки.

5. Combat
- Compact combat block у загальному editor flow і окремий Combat workspace.

## Контракт фасадів

`UnitDesignerWindow` взаємодіє з підмодулями тільки через фасадні інтерфейси:
- `IUnitDesignerIdentityFacade`
- `IUnitDesignerPrefabFacade`
- `IUnitDesignerAnimationFacade`
- `IUnitDesignerPreviewFacade`
- `IUnitDesignerCombatFacade`

Фасад інкапсулює модульну межу і дозволяє поетапно переносити реалізацію без масових змін у головному вікні.

## Принципи

1. Головне вікно не повинно знати внутрішні деталі підмодуля.
2. Нова логіка додається у відповідний підмодуль/фасад, а не в загальний `UnitDesignerWindow`.
3. Зміни одного підмодуля не повинні вимагати правок у решті підмодулів, окрім контракту фасаду.
4. Combat workspace лишається окремим доменом, але підключається через `IUnitDesignerCombatFacade`.
