using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SoDWhoHasThisEssence.Patches
{
    // Scoreboard outline (DESIGN.md "Scoreboard"). UpdateInfo sets a slot's icon on enable, on show and on every
    // LogicUpdate while the scoreboard shows, so a postfix keeps the ring live.
    //
    // A slot ("Gem Item", 40x40, round) holds a "Border" ring image and then the "Icon" image, both stretched over it.
    // The green ring goes between them, a little larger than the slot: it surrounds the slot's own border and is drawn
    // before the icon, so it never covers or tints it.
    [HarmonyPatch(typeof(UI_InGame_Scoreboard_PlayerItem_Skill_Gem), "UpdateInfo")]
    internal static class ScoreboardGemPatch
    {
        private const string RingName = "WHTE Duplicate Outline";
        private const float Overhang = 3f; // UI units outside the slot on each side
        private const float Thickness = 4f; // UI units
        private static bool _loggedLayout;

        private static void Postfix(UI_InGame_Scoreboard_PlayerItem_Skill_Gem __instance,
            UI_InGame_Scoreboard_PlayerItem_Skill ____skill, UI_InGame_Scoreboard_PlayerItem ____item)
        {
            try
            {
                if (__instance.icon == null) return;
                var show = ShouldOutline(__instance, ____skill, ____item);
                var ring = __instance.icon.transform.parent.Find(RingName);
                if (ring == null)
                {
                    if (!show) return;
                    ring = CreateRing(__instance);
                }
                if (ring.gameObject.activeSelf != show) ring.gameObject.SetActive(show);
            }
            catch (Exception e)
            {
                AddedUi.Report("Scoreboard outline", e);
            }
        }

        private static bool ShouldOutline(UI_InGame_Scoreboard_PlayerItem_Skill_Gem slot,
            UI_InGame_Scoreboard_PlayerItem_Skill skill, UI_InGame_Scoreboard_PlayerItem item)
        {
            if (!slot.icon.gameObject.activeSelf || skill == null || item == null) return false;
            var hero = item.hero;
            if (hero.IsNullOrInactive() || hero.Skill == null) return false;
            var gem = hero.Skill.GetGem(new GemLocation(skill.type, slot.index));
            return gem != null && Duplicates.IsOutlined(gem.GetType(), WhoHasThisEssenceMod.ShowOtherDuplicates);
        }

        private static Transform CreateRing(UI_InGame_Scoreboard_PlayerItem_Skill_Gem slot)
        {
            var go = new GameObject(RingName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                typeof(LayoutElement));
            go.layer = slot.gameObject.layer;
            AddedUi.Track(go);

            var rt = (RectTransform)go.transform;
            var icon = slot.icon.transform;
            var parent = icon.parent;
            rt.SetParent(parent, false);
            rt.SetSiblingIndex(icon.GetSiblingIndex()); // just before the icon: drawn under it
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(-Overhang, -Overhang);
            rt.offsetMax = new Vector2(Overhang, Overhang);
            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var image = go.GetComponent<Image>();
            var radius = ((RectTransform)parent).rect.width / 2f + Overhang;
            image.sprite = AddedUi.RingSprite(radius > 0 ? Mathf.Clamp01(Thickness / radius) : 0.17f);
            image.color = AddedUi.DuplicateColor;
            image.raycastTarget = false; // keep the slot's tooltip and ping hover working

            if (!_loggedLayout)
            {
                _loggedLayout = true;
                Log.Info($"Scoreboard outline added under {AddedUi.PathOf(parent)} " +
                         $"(slot {((RectTransform)parent).rect.size}, sibling {rt.GetSiblingIndex()} of {parent.childCount})");
            }
            return rt;
        }
    }
}
