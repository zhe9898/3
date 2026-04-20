# Source Ladder

Use sources in this order unless the task explicitly needs something else.

## 1. Repo truth first

Start with Zongzu's own docs and code. The game's product goals, module boundaries, and accepted abstractions outrank raw historical imitation.

Use the repo to answer:
- what the game is trying to simulate
- which modules already own the relevant concept
- whether the product intentionally abstracts or compresses a practice

## 2. Local skill pack before open-ended lookup

Use the curated files inside this skill before reaching for open-ended external search.

This pack already encodes:
- topic routing
- anti-anachronism framing
- region and era prompts
- game-facing abstractions
- cross-topic combination hints

Use the local pack to answer:
- which topic file should lead
- which adjacent files should be combined
- whether the question is really about lineage, office, local culture, literacy, imperial-local bargaining, or another layer
- what should be modeled as system pressure versus flavor

Do not jump to broad internet search when the local pack already covers the question.
If the local pack is not enough, read [search-source-routing.md](search-source-routing.md) before choosing an external source.

## 3. Public primary-text corpus and historical lookup tools

Use these as the default external grounding set:

- Chinese Text Project: `https://ctext.org/`
  Best for classical passages, terminology, and seeing how a term appears in premodern texts.

- China Biographical Database (CBDB): `https://projects.iq.harvard.edu/cbdb`
  Best for people, office careers, networks, and biography-adjacent institutional context.

- CHGIS / China Historical GIS: `https://chgis.fas.harvard.edu/`
  Best for place names, administrative geography, and regional framing.

- Ministry of Education Revised Mandarin Dictionary: `https://dict.revised.moe.edu.tw/`
  Best for historical word senses, kinship terms, title wording, and lexical sanity checks.

- Digitized local gazetteers and local-history collections
  Best for county texture, temple and market geography, schools, ferries, public works, local custom, and place-specific social detail.

Route by question type rather than convenience:
- terminology and title wording -> dictionary first
- people and office careers -> CBDB first
- counties, prefectures, routes, regional grounding -> CHGIS first
- classical phrasing and term attestation -> Chinese Text Project first

## 4. Modern scholarship

Prefer modern scholarly synthesis when you need:
- contested interpretations
- dynasty-specific institutional summaries
- social practice rather than just normative text
- a cleaner explanation than raw primary text can provide

Prefer university presses, peer-reviewed scholarship, academic databases, and research institutions over blogs or unsourced summaries.

## 5. Community sources only as leads

Forums, Wikipedia, fan wikis, and general explainers can suggest search terms, but they should not be the sole authority for Zongzu-facing mechanics or terminology.

## Evidence labels

When substantial historical grounding matters, classify the result in plain language:
- `primary text attestation`
- `institutional summary`
- `biographical lookup`
- `geographic lookup`
- `secondary synthesis`
- `low-confidence lead`

## Minimal citation discipline

When changing docs, narrative-facing content, or naming conventions based on history, keep enough traceability that another contributor can audit the choice:
- source name
- era or dynasty
- region if relevant
- confidence

Do not pretend a claim is universal when the source is narrow.
