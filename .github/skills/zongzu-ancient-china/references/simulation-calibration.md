# Simulation Calibration

Use this reference when the task needs believable timing, travel bands, message delay, market cadence, levy response, settlement scale, or other practical calibration instead of fake precision.

## Core Rule

Prefer believable bands over invented exact numbers.

When unsure, choose:
- ranges
- named bands
- rough ordering
- explicit confidence labels

## Good Zongzu Abstractions

- `travel_days_band`
- `message_delay_band`
- `market_cadence`
- `muster_delay`
- `granary_buffer`
- `fiscal_pressure_band`
- `settlement_scale_band`

## Practical Heuristics

- Same settlement or nearby village: hours to same day
- Within a county core: same day to a few days depending on road, water access, and urgency
- County to prefectural center: often multi-day rather than instant
- Official notice is slower to originate but stronger when it arrives
- Rumor is faster but dirtier than documentary proof
- Water routes can be high-capacity and strategic even when departure timing is irregular
- Market rhythm and festival rhythm should create pulses rather than continuous flat activity
- Muster, escort, relief, and tax response should feel delayed, uneven, and capacity-bound

## Design Guidance

- A believable model feels delayed, partial, and locally uneven.
- Use calibration to support gameplay readability, not to simulate every li and every coin.
- If a user asks for exact numbers, state whether you are giving a gameplay band, a historical estimate, or a placeholder pending verification.
- Calibration is strongest when it changes map pressure, player foresight, and the cost of late response.

## Suggested Module Mapping

- `Kernel and contracts`
- `WorldSettlements`
- `PopulationAndHouseholds`
- `ConflictAndForce and WarfareCampaign`
- `Presentation.Unity`
