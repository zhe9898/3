# ART_AND_AUDIO_ASSET_SOURCING

This document defines how Zongzu should source, stage, and track low-cost art and audio assets.

The goal is not to build a big-budget asset pipeline.
The goal is to keep the project visually coherent, legally safe, and cheap enough to maintain.

## Product fit

This project does not need:
- open-world environment art
- high-volume character animation
- tactics-map unit libraries
- expensive cinematic asset sets

This project does need:
- room-state changes
- object-anchored UI
- stylized portrait modules
- short conflict or funeral vignettes
- ambient sound layers

## Asset lanes

### Lane 1. Shell surfaces

Use for:
- great hall
- ancestral hall
- desk sandbox table
- office or yamen shell
- campaign board shell later

Preferred asset type:
- static or lightly layered 2.5D backgrounds
- props, desk objects, papers, seals, shelves, lamps, map boards
- decorative overlays instead of large scene-production cost

### Lane 2. UI object pack

Use for:
- buttons
- markers
- icons
- node decorations
- ledgers
- seals
- notice boards

Preferred asset type:
- flat or lightly shaded icons
- UI ornaments
- modular panel pieces

### Lane 3. Portrait and figure pack

Use for:
- stylized portraits
- visitor cards
- household or office figure accents

Preferred asset type:
- modular portrait parts
- painterly busts
- paper-cut or ink-wash-adjacent silhouettes

### Lane 4. Vignette pack

Use for:
- injury
- raid aftermath
- funeral
- wounded return
- field dispatch

Preferred asset type:
- short reusable scene plates
- composited stills
- layered 2.5D moment art

### Lane 5. Ambient audio pack

Use for:
- hall ambience
- market ambience
- rain, ferry, gate, street, workshop, temple, mourning layers
- notice, paper, seal, and command feedback sounds

Preferred asset type:
- loops
- one-shots
- low-complexity ambient beds

## Source tiers

### Tier A. Safe default sources

Use these first when free or open material is enough:

- `Kenney`
  Best for neutral UI pieces, icons, placeholder props, and low-friction CC0 asset intake.

- `OpenGameArt`
  Best for broad free/open-source asset discovery.
  Only intake assets after checking the exact per-asset license.

- `Freesound`
  Best for ambience and SFX.
  Only intake files whose license is acceptable for this project.

- `The Met Open Access` and similar museum open-access collections
  Best for ornamental reference, public-domain pattern extraction, object reference, and visual grounding.
  Treat these as reference or source material for derived assets, not as a substitute for cohesive game art direction.

### Tier B. Discovery-only sources

These can be used to find candidates, but must be reviewed carefully before download or import:

- `itch.io` free asset listings
- `Wikimedia Commons`
- general web search results

Do not bulk-download from discovery sources into the repo.

### Tier C. Avoid by default

- unclear blog reposts
- Pinterest mirrors
- scraped asset aggregators
- AI slop packs with unclear origin
- any asset page with no clear license

## License gates

### Accept by default

- `CC0`
- clear public-domain release
- `CC-BY` when attribution is easy to track

### Review carefully before using

- `CC-BY-SA`
  Use only if the share-alike consequences are understood and acceptable.

### Reject by default

- `CC-BY-NC`
- no-derivatives licenses
- unknown or custom licenses that are not plainly reusable
- anything with unclear provenance

## Download rules

1. Do not download directly into runtime-facing folders first.
2. Stage third-party originals in `content/authoring`.
3. Record every accepted asset in the asset-source ledger.
4. Derive normalized or edited outputs into `content/generated`.
5. Keep attribution and license notes with the source record, not only in memory.

## Repository staging rules

Use these locations:

- `content/authoring/art/third-party/`
  Raw third-party visual assets and downloaded source packages.

- `content/authoring/audio/third-party/`
  Raw third-party sound and ambience packages.

- `content/authoring/reference/`
  Museum or historical reference images that inform derived art.

- `content/generated/art/`
  Cropped, normalized, palette-adjusted, composited, or converted visual assets.

- `content/generated/audio/`
  Trimmed, normalized, looped, or converted audio assets.

If a path does not exist yet, create it as part of the intake change.

## Intake checklist

Before accepting an asset, answer:

- what surface is this for
- is it shell, UI, portrait, vignette, or audio
- is the license acceptable
- is attribution required
- is the style coherent with the project
- does it feel Northern-Song-inspired or at least compatible with the project's grounded baseline
- is it cheap to reuse across multiple surfaces

If the answer to any of these is unclear, do not import it yet.

## Style rules

- prefer stylized, restrained assets over faux-AAA realism
- prefer reusable modular pieces over one-off hero art
- prefer grounded material cues over fantasy symbols
- prefer room, object, paper, wood, fabric, seal, lamp, rain, dust, and crowd signals over excessive architecture production

## What "guofeng" should mean here

For this project, "guofeng" should not mean generic fantasy-China packaging.

It should mean:
- grounded Chinese-ancient material cues
- room and desk atmosphere
- paper, wood, cloth, seal, shelf, lamp, courtyard, gate, ferry, market, archive, and yamen texture
- restrained ornament
- readable silhouettes and calm palettes
- Northern-Song-inspired literati and county-life influence rather than xianxia spectacle

It should not mean:
- mobile-game red-and-gold overload
- dragon-and-cloud wallpaper pasted everywhere
- wuxia or xianxia VFX-first spectacle
- generic "East Asian" fantasy assets with no county, desk, or hall grounding
- ornate costume drama excess on every surface

## Search vocabulary guidance

Do not search only for `guofeng` or `Chinese style`.
Those searches often return the wrong aesthetic for this repo.

Prefer search terms built from surface and material:
- `ink wash ui`
- `paper texture`
- `wood desk props`
- `seal stamp`
- `scholar studio`
- `courtyard interior`
- `county yamen`
- `market lane`
- `ferry wharf`
- `posted notice board`
- `rain ambience`
- `teahouse ambience`
- `street market ambience`
- `funeral ambience`

When you do use `guofeng`, pair it with concrete constraints:
- `guofeng ui paper wood`
- `guofeng scholar desk`
- `guofeng county yamen`
- `guofeng market ambience`

## Historical-grounding rules for art

- do not confuse visual reference with strict documentary reconstruction
- avoid generic fantasy-East-Asia packs
- avoid obviously Japanese, wuxia-fantasy, or xianxia-only silhouettes unless deliberately intended
- use museum or public-domain reference to improve texture and object plausibility, not to force literal replica art

## Audio rules

- ambient layers should support lived-space fantasy, not dominate it
- loops should be short, stable, and easy to layer
- avoid over-scored cinematic music during normal monthly review surfaces
- prioritize room tone, crowd, paper, gate, rain, ferry, and workshop textures

## Attribution and source ledger

Every accepted asset must be logged in:
- `content/authoring/ASSET_SOURCES.md`

Minimum fields:
- asset id
- source site
- source url
- creator
- license
- attribution requirement
- intended use
- raw path
- generated path
- notes

## Implementation workflow

1. Identify the asset lane.
2. Search safe sources first.
3. Check license before download.
4. Stage raw files in `content/authoring`.
5. Register the asset in the ledger.
6. Normalize or derive into `content/generated`.
7. Only then wire it into Unity/presentation work.

## What Codex should do

When asked to fetch free assets for this repo:
- prefer safe default sources first
- prefer assets that fit shell, UI, portrait, vignette, or ambient-audio lanes
- reject unclear licensing
- log what was downloaded
- avoid mixing unrelated art styles in one batch
- default to placeholders and reusable packs before hunting bespoke hero art
