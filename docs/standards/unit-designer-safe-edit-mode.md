# Unit Designer Safe Edit Mode

Мета: зробити масові операції в Unit Designer контрольованими через preview-before-apply, щоб уникати випадкових руйнівних змін у registry.

## Що вважається mass-операцією

У поточній реалізації Safe Edit Mode покриває:

1. Масове застосування combat presets (`Створити/оновити всі`).
2. Масову автогенерацію animation clips (`Автостворити набір`).
3. Масове очищення animation clips (`Очистити всі`).

## Поведінка

1. Якщо Safe Edit Mode = OFF:
- Поведінка лишається legacy (операція застосовується одразу).

2. Якщо Safe Edit Mode = ON:
- Операція спочатку формує preview (що саме буде створено/оновлено/видалено).
- Операція потрапляє у pending state.
- Designer підтверджує `Застосувати` або `Скасувати`.
- Лише після `Застосувати` дані реально мутуються.

## UX-контракт

1. Safe toggle доступний у toolbar (`Safe`).
2. Для pending mass-операції рендериться окрема preview-панель.
3. Preview-панель містить:
- Назву операції.
- Короткий summary змін.
- Список змін по пунктах.
- Кнопки `Застосувати` і `Скасувати`.

## Технічні вимоги

1. Статус Safe Edit Mode зберігається в `EditorPrefs`.
2. Pending-операція має містити:
- title;
- summary;
- preview lines;
- apply callback.
3. Після apply викликаються `SerializedObject.Update/ApplyModifiedProperties` і dirty-mark для registry.
4. Якщо таргет-юніт змінився/зник, apply має fail-safe поведінку (не кидати exception, не псувати чужі дані).

## Розширення

Будь-яка нова mass-операція в Unit Designer має використовувати цей самий flow:
1. Побудувати preview.
2. Поставити pending-операцію.
3. Дати користувачу explicit confirm.
