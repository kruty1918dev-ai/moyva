# Semantic Folders for Editor Tools

Мета: зробити пошук інструментів для дизайнерів швидким і передбачуваним через єдину структуру меню.

## Структура

- `Moyva/Tools/Designers`
- `Moyva/Tools/Validation`
- `Moyva/Tools/Diagnostics`

## Принципи

- Нові пункти меню є semantic-alias і відкривають уже існуючі інструменти.
- Legacy-шляхи не видаляються одразу, щоб не ламати поточний workflow.
- Нові editor-інструменти додаються у відповідну semantic-групу з першого дня.

## Де реалізовано

- `Assets/Moyva/Scripts/Editor/MoyvaSemanticToolsMenu.cs`

## Міграція

- Етап 1: паралельне існування legacy та semantic меню.
- Етап 2: оновлення документації/гайдів на semantic-шляхи.
- Етап 3: поступове згортання legacy-шляхів (опціонально).
