---
name: Moyva AI Council Orchestration
alwaysApply: true
---

# Moyva AI Council

The Moyva AI Council is available through MCP tools.

The Council specialist models do not independently browse the whole repository.

Before calling a specialist:

1. inspect current production source with Continue tools;
2. identify directly relevant files;
3. collect compact verified evidence;
4. pass only the minimum relevant files to the Council.

Primary source of truth:

Assets/Moyva/Scripts/

Do not use these as current implementation evidence:

- .codex-backups/
- .codex-audit/
- .moyva_optimization_backups/
- Library/
- Temp/
- Logs/
- obj/
- old reports
- generated audits
- old patches


## Council roles

### Qwen 7B Scout

Use for:

- quick source inspection;
- dependencies;
- usages;
- responsibilities;
- evidence mapping;
- small mechanical refactors.


### GPT-OSS Reasoner

Use for:

- skeptical verification;
- architecture reasoning;
- challenging assumptions;
- comparing alternatives;
- disproving weak findings.


### Devstral Engineer

Use for:

- multi-file refactoring;
- implementation planning;
- integration impact;
- software engineering review.


### Qwen3 Coder 30B Architect

Use for:

- difficult L3 architecture;
- unresolved disagreements;
- ownership changes;
- networking boundaries;
- persistence boundaries;
- highest-risk refactors.

Do NOT invoke Qwen3 30B routinely.


## Routing policy

Routine task:

Use the main Continue model.

Small or medium investigation:

Use consult_qwen7b when additional review is useful.

Uncertain architecture claim:

Use consult_gpt_oss.

Important multi-file refactor:

Use review_with_council with mode=refactor.

Architecture / ownership / persistence / networking change:

Gather source evidence first.

Then use review_with_council mode=fast.

Escalate to mode=full only when:

- important models disagree;
- confidence remains low;
- the task is L3/high-risk;
- or the user explicitly asks for full Council review.


## Context minimization

Prefer approximately 3-12 directly relevant production files.

Never send the whole repository to a Council model.

Never send large stale audit documents as source evidence.

If a model requires more evidence, inspect the requested exact files first.


## Paid-model minimization

Use prepare_paid_ai_capsule when an expensive external model is genuinely needed.

The paid model should receive only:

- exact goal;
- verified current behavior;
- verified problem;
- minimal relevant file list;
- important symbols;
- constraints;
- local analysis;
- unresolved questions;
- exact task.

Do not make a paid model repeat repository discovery already completed locally.