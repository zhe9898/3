# Pressure Chains and Causality

## Default chain

1. world or household pressure appears
2. a module resolves it during the monthly pass
3. downstream modules inherit consequences
4. structured diffs are produced
5. projection builds notices, summaries, prompts, and surfaces
6. the player reviews and issues bounded commands
7. next month absorbs those choices

## Implementation guardrails

Use these guardrails when turning a pressure chain into code or reviewing a "chain completed" claim. They are intentionally narrow: they protect causality without forcing every chain to become heavy infrastructure.

- Decide the cadence: either follow-on effects resolve within the same month, or they are deliberately delayed to the next month. Do not leave this implicit.
- If follow-on effects resolve in the same month, use deterministic bounded event draining. A finite cap such as `MaxEventDrainRounds` is a safety valve against event feedback loops, not a game balance value.
- Process only fresh events per drain round. Replaying the whole month event list every round can duplicate effects and hide loops.
- Keep module order stable during event handling. If the same seed and same state produce a different event order, the chain is not deterministic enough for Zongzu.
- Every event emitted by a module must be present in that module's `PublishedEvents`.
- Every event actually handled by a module must be present in that module's `ConsumedEvents`.
- Event names used across modules should live in contract event-name classes, not private strings duplicated in handlers or tests.
- A chain test can use focused handler tests for formula edges, but an end-to-end claim needs either a real scheduler test or an explicit note that the tested path is only manual handler wiring.
- Classify every pressure event's scope before implementing it: global, settlement, route, household, clan, person, office, campaign, or court-wide. Scope is part of the contract.
- Put the scope in stable metadata such as `EntityKey` whenever a consumer needs to know the affected locus. Symbolic keys are fine for global phases; typed-id strings are preferred for concrete entities.
- If an upstream event summarizes several axes, carry the changed axis in metadata or make the consumer compare against prior state. A handler should respond to "amnesty wave newly crossed threshold," not merely "some imperial rhythm changed while amnesty is high."
- Use a persisted watermark, processed band, or edge flag when the same high-pressure state can be observed repeatedly across months or sibling events. This is a determinism and idempotency guard, not extra simulation depth.
- Consumers must filter before mutation. A settlement-scoped grain, disaster, route, yamen, or campaign event should not mutate unrelated households, clans, markets, public-life nodes, or forces.
- If a pressure is intentionally global, say so in the rule or test. Do not get a global effect accidentally from a missing or ignored `EntityKey`.
- For scoped chains, include an off-scope negative assertion: prove the affected entity changed and a comparable unrelated entity did not.
- A visible shell or notice result should be asserted from the final `DomainEvent`, structured diff, read model, or module-owned state after scheduler resolution. Do not let UI text prove the chain.
- Generic downstream events need cause discipline. Either carry `cause`, `sourceEvent`, or equivalent metadata, or keep narrative/UI guidance cause-neutral until the player drills into the upstream chain.
- Thin slices must say what they are replacing or omitting. If a slice uses `DisorderSpike` before the fuller `DisorderLevelChanged` summary exists, document that substitution in the spec, integration rules, and exec plan.
- If a chain hits the drain cap, prefer carrying the remaining pressure into next month with a traceable state flag over silently continuing recursive handling.

## Good pressure examples

- failed harvest -> livelihood strain -> debt or migration pressure -> family support allocation -> clan memory and resentment
- route insecurity -> trade loss -> market strain -> office attention or private force response -> settlement security shift
- death of a family elder -> inheritance pressure -> branch tension -> memory update -> hall guidance and council prompts
- campaign fallout -> fatigue and readiness loss -> civilian strain -> family standing shift -> next-month vulnerability
- court reform signal -> county quota pressure -> yamen reinterpretation -> market and household cost shift -> public rumor and faction heat -> player intervention window
- unclear succession or imperial weakness -> official caution -> local magnate bargaining -> protection-market growth -> legitimacy thinning -> coalition or disorder pressure
- famine and tax arrears -> relief failure -> debt flight -> bandit concentration -> private force or militia response -> rebel governance or county recovery branch
- accumulated force, grain, office ties, public legitimacy, and faction support -> succession challenge, usurpation attempt, restoration bid, or dynasty repair pressure

## Design checks

- where does the pressure start
- who carries it
- when does the player first see it
- what can the player do
- what changes next month if ignored
- what higher-level institution or historical trend is being touched, if any
- what backlash, memory, or legitimacy scar remains
- which part should stay ambient rather than interactive
- what event contract and scheduler cadence make the chain real in code
