using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoDWhoHasThisEssence
{
    // Which heroes have each essence type equipped (DESIGN.md "Definitions"). Every client receives every hero's
    // equipped essences, so this is read straight from HeroSkill.gems. Rebuilt at most once per frame, on demand, which
    // keeps the scoreboard and the prompt live without subscribing to gem events.
    internal static class Duplicates
    {
        private static readonly Dictionary<Type, List<Hero>> Owners = new Dictionary<Type, List<Hero>>();
        private static readonly List<Hero> None = new List<Hero>();
        private static int _builtFrame = -1;

        public static Hero LocalHero => DewPlayer.local != null ? DewPlayer.local.hero : null;

        // The heroes that have `type` equipped, in ActorManager.allHeroes order. Don't keep the list.
        public static List<Hero> OwnersOf(Type type)
        {
            Refresh();
            return Owners.TryGetValue(type, out var list) ? list : None;
        }

        // Scoreboard rule: a duplicate (2+ different heroes) that you have, or any duplicate if `includeOthers`.
        public static bool IsOutlined(Type type, bool includeOthers)
        {
            var owners = OwnersOf(type);
            if (owners.Count < 2) return false;
            if (includeOthers) return true;
            var local = LocalHero;
            return local != null && owners.Contains(local);
        }

        private static void Refresh()
        {
            if (_builtFrame == Time.frameCount) return;
            _builtFrame = Time.frameCount;
            foreach (var list in Owners.Values) list.Clear();

            var actors = NetworkedManagerBase<ActorManager>.instance;
            if (actors == null) return;
            // Knocked-out and dead heroes stay active; a disconnected player's hero is destroyed.
            foreach (var hero in actors.allHeroes)
            {
                if (hero.IsNullOrInactive() || hero.Skill == null) continue;
                foreach (var entry in hero.Skill.gems)
                {
                    // An entry can be null on a client until the gem's network object has spawned.
                    var gem = entry.Value;
                    if (gem == null) continue;
                    var type = gem.GetType();
                    if (!Owners.TryGetValue(type, out var owners)) Owners[type] = owners = new List<Hero>();
                    if (!owners.Contains(hero)) owners.Add(hero);
                }
            }
        }
    }
}
