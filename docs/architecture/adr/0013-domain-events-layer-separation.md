# 0013 Domain Events Layer Separation

- Status: Superseded — rolled back 2026-09-20; the `DomainEvents` mirror and `SignalDomainEventBridge` are deleted. Canonical gameplay `*Signal` types are the single event layer; former subscribers (`GameplayAudioFeedbackService`, `AmbientWorldAudioService`) consume them directly.
- Date: 2026-05-10

## Context

Signals модуль містив одночасно:
- UI-сигнали (запит/закриття панелей, preview-події);
- gameplay/domain сигнали (рух юнітів, економічні тики, зміни стану гри).

Це створювало нечіткі межі відповідальності: UI і доменна логіка були зв'язані через один плоский шар сигналів.

## Decision

Вводимо окремий шар `Kruty1918.Moyva.Signals.DomainEvents` для подій домену.

Додатково:
- у `SignalBusInstaller` розділяємо декларації legacy/UI і domain events;
- додаємо перехідний `SignalDomainEventBridge`, який транслює ключові legacy gameplay signals у domain events.

## Consequences

Плюси:
- чіткі межі UI vs domain;
- простіше додавати нові gameplay інтеграції без залежності від UI-сигналів;
- міграція без breaking changes.

Мінуси:
- тимчасово існує дублювання подій (legacy + domain);
- додатковий перехідний компонент у runtime.

## Rollback / Alternative

Rollback:
- прибрати domain event декларації та `SignalDomainEventBridge`;
- повернутися до одного legacy шару сигналів.

Alternative:
- повний одноетапний rename/перенесення сигналів без bridge.
  - Відхилено через високий ризик регресій у великій кількості підписників.
