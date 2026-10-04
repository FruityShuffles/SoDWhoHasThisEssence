# Who Has This Essence? — In-game tests

Testing is done in a real co-op session with a friend. Only you need the mod; the friend plays vanilla (that also
covers "teammates don't need it"). A third player helps with test 4 but isn't required.

Before each session: `dotnet build` (deploys to `<game>/Mods/SoDWhoHasThisEssence/`), enable the mod in the Mod
Manager, then check `Player.log` (`%USERPROFILE%\AppData\LocalLow\Lizard Smoothie\Shape of Dreams\`) for the mod's
`[WHTE]` lines and for exceptions after each test.

Record results here (date, game version, pass/fail, notes).

## Script

1. **Load.** The mod loads with no exceptions. The hosted lobby shows no MOD badge.
2. **No duplicates.** Different essences on each hero: the scoreboard looks vanilla, and a ground essence that only you
   could want shows no extra line.
3. **Scoreboard outline.** Both of you equip the same essence (different memories/slots on purpose). Hold the
   scoreboard: both icons are outlined `#3CFF6E`, the icons themselves are unchanged, other icons aren't outlined.
   Keep it open while the friend unequips the copy: both outlines disappear live. Re-equip: they come back.
4. **Pair without you** (needs 3 players). Two teammates share an essence you don't have. Setting **off** (default):
   not outlined. Turn **Show duplicates between other players** on in the mod config: both of their icons are now
   outlined (no restart). Turn it off again: the outlines go. Your own duplicates stay outlined throughout.
5. **Prompt line.** The friend has essence X equipped and you don't. Stand over a ground X: a green `(FriendName)` line
   appears directly under the name line. With two teammates who have X: `(Name1, Name2)`.
6. **You have it.** Equip your own X, then stand over another ground X: the prompt shows vanilla "Combine" and no line.
7. **Live update.** While standing over a ground X, the friend unequips X: the line disappears without moving away.
8. **Knocked out.** The friend has X and is knocked out (or dead awaiting revive): the line still lists them.
9. **Locked.** An X locked to someone else (dropped by a teammate with "share items when dropped" off) while another
   teammate has X: the lock icon shows and there is no line. The teammate it's locked to sees the line on their client
   (only checkable if they also run the mod).
10. **Disconnect.** The friend leaves mid-run: their names and outlines disappear.
11. **Unload/reload.** Disable and re-enable the mod in the Mod Manager (title screen), then play test 3 again: no
    exceptions, no doubled outlines or lines.

## Results

(none yet)
