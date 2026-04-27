# Surface Grammar

## Great Hall

Purpose:
- monthly decision room
- primary review surface

Should foreground:
- lead notice
- current pressure cluster
- bounded next actions

Should background:
- long historical recap
- decorative lore blocks

Good anchors:
- head notice
- memorial stack
- side notice tray
- bounded action cluster
- one timeline or pressure band only if it supports the current review

## Ancestral Hall

Purpose:
- lineage memory
- branch and heir context

Should foreground:
- branch pressure
- elder memory
- inheritance context

Good anchors:
- lineage panel
- branch list
- ancestor or memorial cues
- family command receipts

## Desk Sandbox

Purpose:
- local-world board
- routes, estates, markets, gates, ferries, hotspots
- mutable sand-table surface for local terrain, route, and node change

Should foreground:
- current county pressure
- manipulable nodes
- route risk
- terrain or node changes that alter play

Good anchors:
- node field
- route field
- notice pins
- one active local hotspot
- raised terrain, route strings, marker flags, repair tokens, flood or drought stains, damaged bridge/market pieces, and settlement rank pieces

## County, Regional, and Realm Map

Purpose:
- large-scale pressure reading
- county, prefecture, regional, frontier, and realm-wide route awareness
- showing how state, market, force, public opinion, and historical momentum travel
- showing durable geographic, route, and settlement-node changes caused by disaster, waterworks, war, migration, trade growth, or administrative change
- giving the player a taskful reason to read the map now: pressure objective, reachable locus, route cost, visibility boundary, risk band, or next drill-down

Should foreground:
- meaningful nodes and routes
- current pressure overlays
- the player's actual influence reach
- route delay, transport burden, and safety
- court, border, market, disaster, or rebellion pressure when it matters
- scale switching: local or hall, county, prefecture or circuit, frontier, realm
- overlay switching: household strain, lineage influence, market flow, yamen pressure, public rumor, route safety, military pressure, disaster exposure, reform pressure, legitimacy
- mutable node and route state: flooded, dry, silted, blocked, damaged, abandoned, growing, rebuilt, militarized, reclassified
- visibility and freshness: known, rumored, stale, hidden, disputed, or report-delayed

Good anchors:
- county seats and prefecture seats
- market towns, ferries, bridges, canal junctions, passes, granaries, academies, yamen, garrisons
- route overlays for grain, tax, petitions, exams, markets, military movement, rumor, and refugees
- scale toggles that preserve causality rather than hiding it
- overlay toggles that keep the same selected node, route, or pressure chain visible across views
- terrain-state tokens for floodwater, drought, silt, broken embankment, repaired embankment, washed road, closed pass, bridge collapse, war burn, refugee camp, new market, abandoned hamlet, rebuilt granary, or new garrison
- moving or placed tokens for caravan, courier, grain cart, tax packet, refugee band, military column, rumor slip, storm/flood front, and repair crew
- reach marker showing where the player's current influence, route knowledge, office access, or message chain ends

Switching rules:
- scale switches answer "how wide is this pressure"
- overlay switches answer "which social chain is carrying it"
- switching should preserve selected nodes, active notices, and cause traces where possible
- unavailable scales should be locked by information reach, office access, route knowledge, military contact, or public rumor, not by arbitrary UI gating
- when scale changes, persistent terrain scars and node-rank changes should remain visible at an appropriate abstraction level
- switching should never erase the current task: if the player opened the map from a notice, the notice's pressure chain remains selected until dismissed or replaced

Mutable map rules:
- terrain changes are not pure cosmetics when they alter route time, harvest, disease exposure, settlement safety, tax reach, military movement, trade access, or migration
- node changes should use clear status states such as thriving, strained, damaged, abandoned, recovering, fortified, militarized, upgraded, downgraded, or newly founded
- route changes should use clear states such as open, delayed, unsafe, blocked, seasonal, washed out, silted, repaired, guarded, or toll-heavy
- map changes should produce receipts in notices, ledgers, repair queues, public rumor, or command windows

Avoid:
- decorative geography with scattered icons
- modern road-map language
- a grand-strategy map that ignores household, market, office, route, and legitimacy consequences
- showing a realm map before the player's information reach, public reputation, office access, or conflict stake makes that scale meaningful
- one overloaded all-on map where every layer competes at once
- permanent terrain transformation without an owning state change, save data, or visible consequence
- empty overmap choice with no route cost, visibility rule, event pressure, reachable objective, or downstream consequence
- perfect omniscience over distant maps when the player's information reach is thin

## Notice Tray

Purpose:
- separate urgent, consequential, and background signals
- preserve the true cause and locus of the pressure without turning the notice into a fake rule

Should foreground:
- urgency and consequence ordering
- the affected locus and, when available, the cause family such as corvee, amnesty, market, disaster, war, migration, or public rumor

Good anchors:
- stacked notices
- small but readable consequence cues
- cause chips or short receipts only when backed by event metadata, read model state, or a traceable upstream chain

Avoid:
- writing a cause-specific sentence for a generic event unless the cause is present in the data
- using notice copy to complete a chain that the scheduler or module state has not completed

## Conflict Vignette

Purpose:
- aftermath
- damage
- consequence

Should foreground:
- who was hit
- what changed
- what pressure rises next

Avoid:
- tactics HUD sprawl
- battle-map framing

## Campaign-lite Board

Purpose:
- route, front, posture, supply, and aftermath

Should foreground:
- board state
- front pressure
- supply or route risk
- command posture

Avoid:
- universalizing this surface into every conflict
