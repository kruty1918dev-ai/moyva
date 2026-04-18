# Документація Economy

## Що Це
Розділ описує економічну систему гри: runtime-логіку, API, інтеграційні точки, редакторний Economy Hub і типові робочі сценарії.

## Швидка Навігація
1. Старт і огляд:
- [Economy Handbook](economy-handbook.md)
- [Economy Runtime Implementation](economy-runtime-implementation.md)

2. Вкладка API:
- [Economy API Tab](economy-api-tab.md)
- [Economy API Files Reference](economy-api-files-reference.md)
- [Каталог Інтерфейсів Economy](economy-interface-catalog.md)

3. Практика і задачі:
- [Economy Designer (Quick Start)](economy-designer.md)
- [Tutorial: Add Resource](tutorial-add-resource.md)
- [Tutorials](tutorials.md)

4. Архітектура і планування:
- [Complete Guide](economy-complete-guide.md)
- [Economy Runtime Logic Plan](economy-runtime-logic-plan.md)

5. Швидкі відповіді:
- [Top 100 Питань По Economy](economy-top-100-qa.md)

## Для Кого
- Геймдизайнери: налаштування балансу, ресурсів і правил.
- Технічні дизайнери: валідація конфігів, контроль консистентності.
- Розробники: інтеграція через API/сигнали, розширення runtime-шару.

## Поточний Scope
Покрито:
- runtime-тік економіки (населення, воркери, виробництво, споживання);
- owner-scoped і settlement-scoped агрегація ресурсів;
- інтеграція через сигнали та DI;
- редакторний Economy Hub з валідацією/автофіксами/міграціями.

Ще не фіналізовано:
- повністю автономна AI-економіка;
- повна логістика караванів без заглушок;
- розширені бойові сценарії перехоплення караванів.
