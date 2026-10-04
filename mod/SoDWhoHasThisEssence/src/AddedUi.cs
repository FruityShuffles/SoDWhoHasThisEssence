using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SoDWhoHasThisEssence
{
    // Everything this mod adds to the game's UI, so OnDestroy can remove it on unload. All of it is built at runtime
    // (no game assets).
    internal static class AddedUi
    {
        public static readonly Color DuplicateColor = new Color32(0x3C, 0xFF, 0x6E, 0xFF);

        private static readonly List<GameObject> Created = new List<GameObject>();
        private static readonly HashSet<string> Reported = new HashSet<string>();
        private static Texture2D _ringTexture;
        private static Sprite _ringSprite;

        public static void Track(GameObject go)
        {
            Created.RemoveAll(g => g == null);
            Created.Add(go);
        }

        public static void DestroyAll()
        {
            foreach (var go in Created)
            {
                if (go == null) continue;
                // Destroy is deferred to the end of the frame; rename now so a reloaded mod's lookup by name can't find it.
                go.name = "WHTE (removed)";
                go.SetActive(false);
                UnityEngine.Object.Destroy(go);
            }
            Created.Clear();
            if (_ringSprite != null) UnityEngine.Object.Destroy(_ringSprite);
            if (_ringTexture != null) UnityEngine.Object.Destroy(_ringTexture);
            _ringSprite = null;
            _ringTexture = null;
        }

        // A white anti-aliased ring filling the sprite: outer radius at the edge, `thickness` as a fraction of the radius.
        public static Sprite RingSprite(float thickness)
        {
            if (_ringSprite != null) return _ringSprite;
            const int size = 128;
            _ringTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "WHTE ring",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[size * size];
            float outer = size / 2f - 0.5f, inner = outer * (1f - thickness), c = (size - 1) / 2f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                var a = Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - inner + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255));
            }
            _ringTexture.SetPixels32(pixels);
            _ringTexture.Apply(false, true);
            _ringSprite = Sprite.Create(_ringTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _ringSprite.name = "WHTE ring";
            _ringSprite.hideFlags = HideFlags.HideAndDontSave;
            return _ringSprite;
        }

        // For the one-time layout log lines: "Root/.../Leaf".
        public static string PathOf(Transform t)
        {
            var sb = new StringBuilder(t.name);
            for (var p = t.parent; p != null; p = p.parent) sb.Insert(0, p.name + "/");
            return sb.ToString();
        }

        // Our postfixes must never break the game's UI update: log each failing spot once and carry on.
        public static void Report(string where, Exception e)
        {
            if (Reported.Add(where)) Log.Warn($"{where} failed: {e}");
        }
    }
}
