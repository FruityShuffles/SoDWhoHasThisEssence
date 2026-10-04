# Who Has This Essence?

A small client-side co-op mod for [Shape of Dreams](https://store.steampowered.com/app/2444750/), loaded by the game's
official mod loader.

- **Scoreboard:** essences you share with a teammate get a green outline (optionally also those shared only between
  other players).
- **Essence pickup prompt:** a green line under the name lists the teammates who have that essence equipped, so you can
  hand a spare copy to the one who can combine it.

Only you need the mod. It changes no gameplay. See [DESIGN.md](DESIGN.md) for the exact behavior.

## Building

Needs the .NET 8 SDK and the game installed.

```
dotnet build mod/SoDWhoHasThisEssence.sln
```

This deploys to `<game>/Mods/SoDWhoHasThisEssence/` (`-p:DeployToGame=false` skips that). If the game isn't in the
default Steam folder, set `SOD_GAME_DIR` or copy `mod/GamePath.user.props.example` to `mod/GamePath.user.props`.
The art in `about/` is rendered by `tools/make_mod_art.py` (Pillow).

## License

MIT, see [LICENSE](LICENSE).
