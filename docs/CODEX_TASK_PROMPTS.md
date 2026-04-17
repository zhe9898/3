# CODEX_TASK_PROMPTS

## Bootstrap repo
```text
Read AGENTS.md and docs/README.md first.
Then read PRODUCT_SCOPE.md, MVP_SCOPE.md, VERSION_ALIGNMENT.md, TECH_STACK.md, ENGINEERING_RULES.md, ARCHITECTURE.md, MODULE_BOUNDARIES.md, EXTENSIBILITY_MODEL.md, MODULE_INTEGRATION_RULES.md, SCHEMA_NAMESPACE_RULES.md, DATA_SCHEMA.md, and IMPLEMENTATION_PHASES.md.
Create an ExecPlan for Phase M0.
Bootstrap the repository skeleton exactly as specified.
Implement kernel/contracts/scheduler/persistence shells and feature manifest support.
Add deterministic replay-hash and save-roundtrip tests.
Do not introduce Unity dependencies into authority layers.
```

## Add a new module
```text
Read AGENTS.md, ARCHITECTURE.md, MODULE_BOUNDARIES.md, EXTENSIBILITY_MODEL.md, MODULE_INTEGRATION_RULES.md, SCHEMA_NAMESPACE_RULES.md, and DATA_SCHEMA.md.
Create an ExecPlan.
Add the module with owned state, schema namespace, registration, tests, and projections.
Do not mutate foreign module state directly.
Update docs and acceptance tests.
```

## Implement lite exams
```text
Read AGENTS.md, MVP_SCOPE.md, SOCIAL_STRATA_AND_PATHWAYS.md, MODULE_BOUNDARIES.md, DATA_SCHEMA.md, and SIMULATION.md.
Implement EducationAndExams.Lite only.
Support study state, exam attempt, pass/fail, explanation traces, and events.
Do not implement office careers in this task.
```

## Implement trade lite
```text
Read AGENTS.md, MVP_SCOPE.md, SOCIAL_STRATA_AND_PATHWAYS.md, MODULE_BOUNDARIES.md, DATA_SCHEMA.md, and SIMULATION.md.
Implement TradeAndIndustry.Lite only.
Support local estates/shops/routes, cash-grain-debt pressure, and explanation traces.
Do not implement full guild networks or campaign logistics in this task.
```

## Implement local conflict lite
```text
Read AGENTS.md, CONFLICT_AND_FORCE.md, MODULE_BOUNDARIES.md, SIMULATION.md, and VERSION_ALIGNMENT.md.
Implement ConflictAndForce.Lite and optional OrderAndBanditry.Lite.
Add abstract conflict resolution only.
Do not add tactical battle maps, unit micro, or detached combat scenes.
```

## Implement campaign sandbox
```text
Read AGENTS.md, POST_MVP_SCOPE.md, VERSION_ALIGNMENT.md, CONFLICT_AND_FORCE.md, MODULE_BOUNDARIES.md, EXTENSIBILITY_MODEL.md, and VISUAL_FORM_AND_INTERACTION.md.
Implement WarfareCampaign as a campaign-level desk sandbox extension.
Use authority, mobilization, command, supply, morale, and aftermath.
Do not replace local conflict foundations.
Do not build RTS/tactics control.
```
