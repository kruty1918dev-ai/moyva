# Feature Toggles for Risky Features

Мета: дати швидкий runtime rollback для ризикових фіч без гарячих патчів та без перекомпіляції.

## Правило

Ризикові фічі мають мати explicit toggle у конфігурації, який читається на старті та застосовується до runtime-flow.

## Що вважати ризиковою фічею

- інтеграції з зовнішніми SDK/API, де можливі runtime-злами;
- flow, що впливають на доступність сесії (join/host/connectivity);
- складні recovery-механізми з високим UX-ризиком у продакшені.

## Практичний контракт

1. Toggle зберігається в `MultiplayerConfig` (або доменному config модуля).
2. Default значення для existing прод-поведінки: `true` (feature увімкнена).
3. Store має бути backward-compatible (старі schema читаються з safe default).
4. Runtime сприймає toggle як authoritative policy і виконує deterministic fallback.
5. Toggle має бути доступний у editor-config інструменті.

## Поточне застосування

- `EnableRelayProvider`:
  - вимикає Relay як ризиковий провайдер і переводить систему на fallback provider;
  - застосовується у `MultiplayerInstaller` і `NetworkProviderFactory`.
- `EnableHostMigration`:
  - при вимкненні сесія завершується при відпадінні хоста;
  - застосовується у `SessionManager`.

## Вимога rollback

Зміна toggle у конфігу має бути достатньою для rollback-поведінки без hotfix-патчів runtime-коду.
