# Similar-Game Map UI Calibration

Use this reference when checking Zongzu's map stack against comparable strategy, colony, and living-world games.

The point is not to copy another game's interface. The point is to avoid obvious map-surface failures: empty overmaps, omniscient views, decorative geography, noisy overlays, and maps that stop touching the lived simulation.

## Contents

- External calibration notes
- Cross-game pattern
- Zongzu map stack recommendation
- Sources

## External calibration notes

### Manor Lords

Relevant lesson:
- landscape, trade routes, soil fertility, animals, resources, seasons, and settlement growth are connected
- environmental exploitation can visibly affect the world
- battle deaths matter because each fighter is also a city person

Zongzu translation:
- terrain and routes should shape settlement growth, market opportunity, and supply pressure
- disaster, waterworks, war, migration, and trade should be allowed to change node appearance and rank
- campaign or conflict markers must point back to households, casualties, labor loss, public shame, and route repair

### RimWorld

Relevant lesson:
- the world map is not only a picture; caravans, settlements, faction bases, terrain tabs, travel times, roads, seasons, and local maps all connect
- the player selects a site or caravan, then gets route / terrain / interaction consequences

Zongzu translation:
- the big map needs selected actors or pressure carriers, not just static icons
- route time, terrain condition, message delay, and caravan/courier/grain-cart markers are first-class UI state
- drill-down from realm or regional map to county sand table should preserve the same selected pressure chain

### Songs of Syx

Relevant lesson:
- the world contains settlements, regions, natural resources, climate differences, trade carts, and even visible storm clouds
- resources and climate shape location choice and regional development

Zongzu translation:
- moving tokens are useful: trade cart, courier, military column, refugee group, storm/flood front, rumor slip
- regional profile should matter visually and mechanically: water-network county, dry-road county, mountain pass, frontier belt
- resource and ecology differences should not be hidden in text-only summaries

### Against the Storm

Relevant lesson:
- the world map needs structure, risk, goals, visibility, route planning, events, and modifiers
- the developers explicitly treated a previously flat meta layer as lacking purpose, thrill, stakes, and meaningful choices
- physical caravan position and route restrictions made the map layer deeper

Zongzu translation:
- a realm or regional sand table needs a reason to be open now: pressure objective, warning, reachable node, route risk, or historical front
- visibility and reach should be limited by information, office access, public rumor, route knowledge, and player influence footprint
- avoid a decorative overmap where the player simply chooses a place with no causal chain

### Old World

Relevant lesson:
- map overlays such as danger, trade network, roads, and rivers help players understand why movement or connection works
- roads and rivers materially change movement and logistics

Zongzu translation:
- overlay switching should answer one operational question at a time: grain route, tax/paperwork, public rumor, route safety, military pressure, disaster exposure, legitimacy
- roads, rivers, ferries, canals, passes, and bridges should change travel, message delay, trade, tax reach, and military posture

### Norland

Relevant lesson:
- family members and commoners have autonomy and can create political trouble
- the player acts through nobles and institutions rather than directly puppeteering everyone

Zongzu translation:
- maps should not imply omnipotent command
- the map needs a visible reach boundary: what the player can see, touch indirectly, command, or only hear about
- public discontent, family ambition, elite faction, and commoner unrest should appear as pressure carriers, not as raw color fill

## Zongzu map decision

The Zongzu map stack should be:
- a desk sand-table system at multiple scales
- taskful, not decorative
- state-driven, not flavor-driven
- bounded by visibility and influence reach
- connected to route cost, information delay, and pressure objectives
- able to show durable changes to terrain, routes, and settlement nodes
- always able to drill back down to household, market, yamen, public life, force, or memory consequences

## Hard rules

- Every map opening should have a current question: where is pressure, how does it travel, what changed, what can be reached, or what should be drilled into.
- Every map overlay should answer one social chain, not all chains at once.
- Every map scale switch should preserve the selected pressure chain when possible.
- Every persistent terrain or node change needs module-owned state, save data, or a read-model source.
- Every distant map should show uncertainty if the player's information reach is weak.
- Every major conflict, disaster, reform, or legitimacy marker should be able to return to notices, ledgers, route reports, public rumor, or command receipts.

## Source links for future re-check

- Manor Lords Steam page: https://store.steampowered.com/app/1363080/ManorLords/
- RimWorld world/menu and caravan wiki pages: https://www.rimworldwiki.com/wiki/Menus and https://rimworldwiki.com/wiki/Caravan
- Songs of Syx official wiki, World: https://www.songsofsyx.com/wiki/index.php/World
- Against the Storm official wiki, World Map: https://wiki.hoodedhorse.com/Against_the_Storm/World_Map
- Against the Storm Cycles Reforged design notes: https://eremitegames.com/cycles-reforged-update/
- Old World official manual: https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/597180/manuals/bdd2ff370f17bfe9ecb09483be0b8b22f5e62efe/Old_World-Official_User_Manual.pdf
- Norland Steam page: https://store.steampowered.com/app/1857090/Norland/
