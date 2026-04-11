# Economy Documentation Index

## Призначення
Цей розділ описує економічну підсистему та редакторний інструмент `Economy Designer`.

Документація покриває:
- загальну концепцію економіки;
- структуру даних (ScriptableObject);
- повний API-контракт класів/методів;
- роботу вкладок інструмента;
- валідацію, автоправки, міграцію схеми;
- детермінований simulation preview;
- покрокові туторіали.

## Структура розділу
- [Economy Handbook](economy-handbook.md)
- [Tutorial: Add Resource](tutorial-add-resource.md)
- [Economy API Files Reference](economy-api-files-reference.md)
- [Complete Guide](economy-complete-guide.md)
- [Tutorials](tutorials.md)
- [Economy Designer (Quick Start)](economy-designer.md)
- [Economy Runtime Logic Plan](economy-runtime-logic-plan.md)
- [Economy Runtime Implementation](economy-runtime-implementation.md)

## Для кого
- геймдизайнери, які налаштовують економіку;
- технічні дизайнери, які підтримують баланс та консистентність;
- розробники, які розширюють модуль Economy.

## Область дії (поточний стан)
Поточна реалізація покриває:
- editor-side конфігурацію і валідацію;
- runtime тік-логіку поселень (population/workers/production);
- owner-scoped агрегацію ресурсів для multiplayer/bot сценаріїв;
- summary UI для Food/Materials через centralized formatting параметри.

Поза поточним scope (етап extensibility):
- повністю автономна AI-економіка;
- повна бойова інтеграція караванів без заглушок;
- фінальна модель складної логістики маршрутів.
