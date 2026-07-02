using System.Runtime.CompilerServices;

// Відкриває internal FogOfWar runtime API для спеціалізованого тестового asmdef.
// Це потрібно лише для тестового доступу до runtime helper-ів і не змінює gameplay/runtime поведінку.
[assembly: InternalsVisibleTo("Kruty1918.Moyva.Tests.FogOfWar")]
