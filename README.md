# CursedRelics
Slay the spire 2 mod, some relics may appear cursed, which has a stronger effect with attached downsides.

## Design Outline

Relics, when generated, have a 10% chance to appear as their cursed variants(Gives players the choice to pick them or not).
IMPORTANT: for the initial release, we ignore ancient rewards for now, effects might be beyond the scope of my capabilities.
- Visualization: sts2 already has relic outlines(as seen in the compendium), add red outlines around all cursed relics + change name color to be cursed.
- Downsides: rolls from a pool of possible downsides, each individual effect has a % chance to hit, essentially rarity to downsides(for example, -1 strength can be a common downsides, while -1 cards drawn at the start of combat can be a rare one)
 - Upsides: focus on numerical adjustments right now, custom effects can be a later feature.

## technical stuff

TEST GENERIC WORKABLE VERSION WITH SINGLEPLAYER FIRST, WORK ON FIXING MULTIPLAYER SYNC AFTER(USE THE CORRECT RNG CALL)
In multiplayer, the shared grab bag has a single cursed state(all players must have the mod installed)
 - Modifier relics with dynamicVars/canonnicalvars property first then look at other relic that are hardcoded


