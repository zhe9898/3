# PLANS.md

Use an **ExecPlan** for any task that touches more than a tiny local bug.

## ExecPlan requirements
Each plan must be a Markdown file under `docs/exec-plans/active/`.

### Required sections
- Goal
- Scope in / out
- Affected modules
- Save/schema impact
- Determinism risk
- Milestones
- Tests to add/update
- Rollback / fallback plan
- Open questions

### Naming
`YYYY-MM-DD_short-name.md`

### Completion
When a plan is complete:
- move it to `docs/exec-plans/archive/`
- add a short result note at the top
- link any follow-up plan if needed

## Rule
No large refactor or new feature pack should start without an ExecPlan.
