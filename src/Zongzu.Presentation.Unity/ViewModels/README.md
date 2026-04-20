# ViewModel Conventions

View models in this folder are grouped by shell surface so the presentation layer can be scanned the same way the player sees it.

## Surface groups

- `Shared/`
  - shell-wide DTOs such as command affordances, command receipts, and the composed root shell model
- `Debug/`
  - development-only diagnostics panels and summary cards
- `Family/`
  - lineage, clan, and family-council surfaces
- `GreatHall/`
  - great hall dashboard and hall docket items
- `DeskSandbox/`
  - county and settlement-focused desk sandbox surfaces
- `Notifications/`
  - notice tray and notification-center items
- `Office/`
  - governance-lite office projections
- `Warfare/`
  - campaign-lite boards, route summaries, and warfare shell surfaces

## Guardrails

- keep namespaces as `Zongzu.Presentation.Unity`
- keep view models passive; they describe shell state and never hold authority logic
- when adding a new surface model, place it next to the surface that renders it instead of growing another flat root list
