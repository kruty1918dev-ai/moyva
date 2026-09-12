---
name: Moyva Source of Truth
alwaysApply: true
---

# Moyva source rules

Primary active production source:

Assets/Moyva/Scripts/

Secondary evidence when relevant:

- Packages/manifest.json
- ProjectSettings/
- active tests

Do NOT use these as proof of current implementation:

- .codex-backups/
- .codex-audit/
- .moyva_optimization_backups/
- Library/
- Temp/
- Logs/
- obj/
- generated reports
- old patches
- old audit files

Assets/Plugins/ is third-party code and should only be inspected when
understanding an external API is required.

Never invent:
- classes
- methods
- constructors
- interfaces
- signals
- dependencies
- Unity APIs
- Zenject APIs

Verify important claims against current source code.