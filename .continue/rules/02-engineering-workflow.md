---
name: Moyva Engineering Workflow
alwaysApply: true
---

# Engineering workflow

For complex tasks:

1. Understand the requested goal.
2. Inspect current implementation.
3. Search call sites and dependencies.
4. Form a concrete plan before major edits.
5. Keep changes minimal and cohesive.
6. Re-read modified code.
7. Inspect resulting diff.
8. Check for compile/runtime implications.
9. Report remaining uncertainty explicitly.

For architecture findings always provide exact source evidence.

For dead-code analysis remember that Unity references may exist through:
- serialized scenes
- prefabs
- ScriptableObjects
- inspector fields
- reflection

Do not classify a performance concern as a measured hotspot without
profiling evidence.

Do not change unrelated systems just to make architecture look cleaner.