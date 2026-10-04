# Who Has This Essence? — Design

A client-side **Shape of Dreams** mod for co-op. It answers one question for the player standing over an essence:

> Should I equip or dismantle this essence myself, or give it to a teammate who already has it equipped and can
> combine it?

Picking up an essence that you already have equipped merges the two (vanilla), so handing a spare copy to the teammate
who has it frees a slot for a different essence. The mod only *shows* information. It changes no gameplay.

This document is authoritative. Where code and this document disagree, this document wins.

## Scope

Two UI additions and one setting, nothing else:

1. **Scoreboard** (the hold-to-show player overview): a green outline on every essence icon that is a duplicate you're
   part of, or, with the setting on, every duplicate.
2. **Essence pickup prompt** (shown over an essence on the ground): a green line with the names of teammates who have
   that essence equipped.

Out of scope: the skill bar, the edit-memory screen, shops, shrine choices, the results screen, the lobby, other
settings, merge/quality previews, and any change to merging, dropping or picking up.

## Definitions

- **Hero**: every hero the scoreboard shows (`NetworkedManagerBase<ActorManager>.instance.allHeroes`), including heroes
  that are knocked out or dead. A disconnected player's hero is removed by the game and no longer counts.
- **Has an essence**: the hero has an essence of that **type** (`gem.GetType()`) slotted in any memory, in any slot
  (`hero.Skill.gems`). This is the same test the game uses to decide a pickup merges
  (`HeroSkill.TryGetEquippedGemOfSameType`). Quality and rarity don't matter. Essences held in hand or lying on the
  ground don't count.
- **Duplicate**: an essence type that **two or more different heroes** have. Two copies on the same hero (possible only
  with other mods, such as Controlled Merge) don't make it a duplicate on their own.
- **Your duplicate**: a duplicate that your own hero (`DewPlayer.local.hero`) has.

## Setting

One config field, shown in the mod manager's config screen:

- **Show duplicates between other players** (`bool`, default **off**). Description: "Also outline essences that two or
  more teammates share when you don't have them."

It affects only the scoreboard. The pickup prompt is always about what *you* do with the essence in front of you, so it
ignores the setting.

## Scoreboard

- Which essence types are outlined:
  - Setting **off** (default): your duplicates only. Every icon of such a type is outlined: your copy and each
    teammate's copy. A type that only teammates share isn't outlined.
  - Setting **on**: every duplicate, including types shared only between teammates.
- Every essence icon on the scoreboard whose type is outlined gets a **bright green outline, `#3CFF6E`**.
- Changing the setting takes effect the next time the scoreboard updates; no restart.
- One color for every duplicate. Which icons match each other is clear from the icons themselves.
- The outline must not cover or tint the icon itself. Green was chosen because no rarity color is green (rarity colors:
  `Dew.CommonColor` … `Dew.UniqueColor`).
- Non-duplicate icons and empty slots look exactly as in vanilla.
- The outline updates live while the scoreboard is open (a teammate equips, unequips, swaps or merges).

## Pickup prompt

The vanilla prompt (`UI_InGame_Interact_Gem`) shows the quality and name, an optional short description, and the
Equip/Combine, Dismantle and lock indicators.

The mod adds **one line directly under the essence's name line**:

```
(120%) Rare Essence of Lava
(Bo, Cy)                      <- #3CFF6E
Adds Fire damage...
[F] Equip   [Hold F] Dismantle
```

- Content: the **player names** (`hero.owner.playerName`, as on the scoreboard rows) of every *other* hero that has
  this essence type, separated by `", "`, inside one pair of parentheses. No other words, so there is nothing to
  translate. Order: the order of `ActorManager.allHeroes`.
- Color: `#3CFF6E`, the scoreboard outline color.
- The line is shown only when **all** of these hold. Otherwise it is hidden and the prompt looks exactly as in vanilla.
  1. At least one *other* hero has the essence (knocked-out and dead teammates count).
  2. **You don't have it equipped.** If you do, vanilla already shows "Combine", and the essence is yours to merge.
  3. **The essence isn't locked for you** (`gem.isLocked` is false). A locked essence (dropped by a teammate with
     "share items when dropped" off) can't be equipped, dismantled or handed over by you, so the question doesn't
     apply. The dropper still sees the line on their own client.
- The line updates live while the prompt is shown (teammates equipping or unequipping, you equipping a copy, the lock
  changing).

## Behavior and constraints

- **Client-side only.** Every client already receives every hero's equipped essences (Mirror `SyncDictionary` in
  `HeroSkill`), so no networking is needed. It works when only you have the mod; teammates don't need it.
- **Not gameplay-altering:** leave `instance.isAlteringGameplay` false (no MOD badge on lobbies).
- **One setting** (see "Setting"). Nothing else is configurable; to turn the mod off, disable it.
- Solo play: there are no duplicates, so nothing changes.
- No interaction with other mods is planned. Mods that add essence slots (More Gem Slots extends the scoreboard) are
  best effort.
- Must survive unload and live reload: `OnDestroy` unpatches Harmony and removes or hides anything the mod added to UI
  objects.

## Distribution

- Mod name **Who Has This Essence?**, metadata author `Chainfire` (the Workshop author name of the earlier Archipelago
  mod), mod id `com.fruityshuffles.whohasthisessence`, assembly `SoDWhoHasThisEssence.dll`, first version `0.1.0`.
- Steam Workshop (the in-game uploader) plus a public MIT repo, `FruityShuffles/SoDWhoHasThisEssence` on GitHub.
- Player-facing text (`about/description.txt`) says only what the mod shows; it assumes the reader knows the game.
