using System;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoDWhoHasThisEssence.Patches
{
    // Pickup prompt line (DESIGN.md "Pickup prompt"). OnActivate fills the name and then calls UpdateActions, which also
    // runs on every LogicUpdate while the prompt shows, so one postfix there both sets the line up and keeps it live.
    //
    // The prompt root has a VerticalLayoutGroup: "Name Container" (title banner with the name and the lock icon),
    // "Short Desc Container", "Action Group". The line is a new text row inserted right after the name container.
    [HarmonyPatch(typeof(UI_InGame_Interact_Gem), "UpdateActions")]
    internal static class PickupPromptPatch
    {
        private const string LineName = "WHTE Teammates With Essence";
        private const float SizeFactor = 0.8f; // of the name's font size
        private static readonly StringBuilder Text = new StringBuilder();
        private static bool _loggedLayout;

        private static void Postfix(UI_InGame_Interact_Gem __instance)
        {
            try
            {
                if (__instance.nameText == null) return;
                var text = LineText(__instance.interactable as Gem);
                var anchor = RowOf(__instance);
                var line = FindLine(anchor);
                if (text == null)
                {
                    if (line != null && line.gameObject.activeSelf) line.gameObject.SetActive(false);
                    return;
                }
                if (line == null) line = CreateLine(__instance, anchor);
                if (line.text != text) line.text = text;
                if (!line.gameObject.activeSelf) line.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                AddedUi.Report("Pickup prompt line", e);
            }
        }

        // "(Bo, Cy)": every other hero with this essence type equipped, or null when the line must be hidden.
        private static string LineText(Gem gem)
        {
            if (gem == null || gem.isLocked) return null;
            var local = Duplicates.LocalHero;
            if (local == null) return null;
            var owners = Duplicates.OwnersOf(gem.GetType());
            if (owners.Contains(local)) return null; // vanilla shows Combine
            Text.Clear();
            foreach (var hero in owners)
            {
                Text.Append(Text.Length == 0 ? "(" : ", ");
                Text.Append(hero.owner != null ? hero.owner.playerName : hero.name);
            }
            if (Text.Length == 0) return null;
            return Text.Append(')').ToString();
        }

        // The prompt root's child that holds the name line; the new line goes right after it.
        private static Transform RowOf(UI_InGame_Interact_Gem prompt)
        {
            var row = prompt.nameText.transform;
            while (row.parent != null && row.parent != prompt.transform) row = row.parent;
            return row.parent == prompt.transform ? row : prompt.nameText.transform;
        }

        private static TextMeshProUGUI FindLine(Transform anchor)
        {
            var parent = anchor.parent;
            var next = anchor.GetSiblingIndex() + 1;
            if (next < parent.childCount)
            {
                var t = parent.GetChild(next);
                if (t.name == LineName) return t.GetComponent<TextMeshProUGUI>();
            }
            var found = parent.Find(LineName);
            return found != null ? found.GetComponent<TextMeshProUGUI>() : null;
        }

        private static TextMeshProUGUI CreateLine(UI_InGame_Interact_Gem prompt, Transform anchor)
        {
            var go = new GameObject(LineName, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = prompt.gameObject.layer;
            go.SetActive(false);
            AddedUi.Track(go);

            var rt = (RectTransform)go.transform;
            rt.SetParent(anchor.parent, false);
            rt.SetSiblingIndex(anchor.GetSiblingIndex() + 1);

            var name = prompt.nameText;
            var line = go.AddComponent<TextMeshProUGUI>();
            line.font = name.font;
            line.fontSharedMaterial = name.fontSharedMaterial; // keeps the name's outline/shadow for readability
            line.fontSize = name.fontSize * SizeFactor;
            line.fontStyle = name.fontStyle;
            line.alignment = TextAlignmentOptions.Center;
            line.color = AddedUi.DuplicateColor;
            line.richText = false; // player names are shown as typed
            line.textWrappingMode = TextWrappingModes.NoWrap;
            line.raycastTarget = false;

            // Size to the text whether or not the layout group controls its children's size.
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (!_loggedLayout)
            {
                _loggedLayout = true;
                var group = anchor.parent.GetComponent<LayoutGroup>();
                Log.Info($"Prompt line added under {AddedUi.PathOf(anchor.parent)} after '{anchor.name}' " +
                         $"(layout {(group != null ? group.GetType().Name : "none")}, name font size {name.fontSize})");
            }
            return line;
        }
    }
}
