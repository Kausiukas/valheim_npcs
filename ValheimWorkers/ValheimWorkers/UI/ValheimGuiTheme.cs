using System;
using UnityEngine;

namespace ValheimWorkers.UI
{
    public static class ValheimGuiTheme
    {
        private static bool _initialized = false;
        private static float _currentScale = 1.15f;
        private static Font _norseTitleFont;
        private static Font _averiaBoldFont;
        private static Font _averiaRegularFont;

        public static float FontScale => _currentScale;

        // Procedural Vanilla-Matched Textures
        private static Texture2D _texWoodWindowBg;
        private static Texture2D _texWoodPanelInset;
        private static Texture2D _texStoneButtonNormal;
        private static Texture2D _texStoneButtonHover;
        private static Texture2D _texStoneButtonActive;
        private static Texture2D _texTabNormal;
        private static Texture2D _texTabActive;
        private static Texture2D _texListRowNormal;
        private static Texture2D _texListRowSelected;
        private static Texture2D _texCraftButtonNormal;
        private static Texture2D _texCraftButtonHover;
        private static Texture2D _texDangerNormal;
        private static Texture2D _texDangerHover;
        private static Texture2D _texInputBg;
        private static Texture2D _texSliderTrack;
        private static Texture2D _texSliderThumb;
        private static Texture2D _texLogBoxBg;
        private static Texture2D _texSlotBoxBg;
        private static Texture2D _texSlotBoxEquipped;

        // Styles
        public static GUIStyle WindowStyle;
        public static GUIStyle TabInactiveStyle;
        public static GUIStyle TabActiveStyle;
        public static GUIStyle CardStyle;
        public static GUIStyle LogBoxStyle;
        public static GUIStyle SlotBoxStyle;
        public static GUIStyle SlotBoxEquippedStyle;
        public static GUIStyle SlotCountStyle;
        public static GUIStyle CardHeaderStyle;
        public static GUIStyle SubHeaderStyle;
        public static GUIStyle LabelParchment;
        public static GUIStyle LabelMuted;
        public static GUIStyle LabelGold;
        public static GUIStyle LabelLogEntry;
        public static GUIStyle ButtonNormal;
        public static GUIStyle ButtonSelected;
        public static GUIStyle ButtonListRow;
        public static GUIStyle ButtonListRowSelected;
        public static GUIStyle ButtonPrimary;
        public static GUIStyle ButtonDanger;
        public static GUIStyle TextFieldStyle;
        public static GUIStyle SliderStyle;
        public static GUIStyle SliderThumbStyle;

        // Vanilla Valheim Palette
        public static readonly Color ColorValheimGold = new Color(1.0f, 0.75f, 0.12f, 1.0f);       // #FFBF1F (Torch / Craft gold)
        public static readonly Color ColorAmberHighlight = new Color(0.92f, 0.56f, 0.05f, 1.0f);    // #EB8F0D (Orange selection bar & active tab border)
        public static readonly Color ColorParchment = new Color(0.94f, 0.91f, 0.86f, 1.0f);         // Clean readable off-white
        public static readonly Color ColorSilverMuted = new Color(0.74f, 0.70f, 0.65f, 1.0f);       // Inactive tab / secondary text
        public static readonly Color ColorWoodLight = new Color(0.27f, 0.17f, 0.10f, 0.98f);        // Warm brown oak plank
        public static readonly Color ColorWoodDark = new Color(0.18f, 0.11f, 0.07f, 0.98f);         // Deep recessed wood grain
        public static readonly Color ColorWoodBorder = new Color(0.12f, 0.07f, 0.04f, 1.0f);        // Outer carved dark wood edge
        public static readonly Color ColorStoneTop = new Color(0.21f, 0.19f, 0.17f, 0.98f);         // Vanilla dark stone button top
        public static readonly Color ColorStoneBottom = new Color(0.12f, 0.11f, 0.10f, 0.98f);      // Vanilla dark stone button bottom
        public static readonly Color ColorStoneBorder = new Color(0.32f, 0.28f, 0.24f, 1.0f);       // Subtle stone bevel
        public static readonly Color ColorMeadowsGreen = new Color(0.45f, 0.78f, 0.32f, 1.0f);
        public static readonly Color ColorCrimson = new Color(0.92f, 0.35f, 0.32f, 1.0f);

        public static int Scaled(int baseSize)
        {
            return Mathf.Max(10, Mathf.RoundToInt(baseSize * _currentScale));
        }

        public static float ScaledF(float baseVal)
        {
            return baseVal * _currentScale;
        }

        public static void SetFontScale(float newScale)
        {
            float clamped = Mathf.Clamp(newScale, 0.85f, 1.65f);
            if (Mathf.Abs(clamped - _currentScale) < 0.01f && _initialized) return;

            _currentScale = clamped;
            if (ValheimWorkersPlugin.UiFontScale != null)
            {
                ValheimWorkersPlugin.UiFontScale.Value = _currentScale;
            }
            if (_initialized)
            {
                BuildStyles();
            }
        }

        public static void EnsureInitialized()
        {
            if (ValheimWorkersPlugin.UiFontScale != null)
            {
                _currentScale = Mathf.Clamp(ValheimWorkersPlugin.UiFontScale.Value, 0.85f, 1.65f);
            }

            if (_initialized && _texWoodWindowBg != null) return;

            FindValheimFonts();
            CreateVanillaTextures();
            BuildStyles();

            _initialized = true;
        }

        private static void FindValheimFonts()
        {
            try
            {
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (var f in fonts)
                {
                    if (f == null || string.IsNullOrEmpty(f.name)) continue;

                    string lower = f.name.ToLowerInvariant();
                    if (lower.Contains("norsebold") || lower == "norse")
                    {
                        if (_norseTitleFont == null || lower.Contains("bold"))
                            _norseTitleFont = f;
                    }
                    else if (lower.Contains("averiaseriflibre-bold"))
                    {
                        _averiaBoldFont = f;
                    }
                    else if (lower.Contains("averiaseriflibre-regular") || lower.Contains("averiaserif"))
                    {
                        _averiaRegularFont = f;
                    }
                }

                if (_averiaBoldFont == null && _averiaRegularFont != null) _averiaBoldFont = _averiaRegularFont;
                if (_averiaRegularFont == null && _averiaBoldFont != null) _averiaRegularFont = _averiaBoldFont;
            }
            catch
            {
                // Fallback to default Unity font
            }
        }

        private static Texture2D CreateWoodPlankTexture(int w, int h, Color baseLight, Color baseDark, Color outerBorder, Color innerTrim, int borderWidth)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float grain = Mathf.Sin(y * 0.55f) * 0.35f + Mathf.Cos(y * 1.3f) * 0.25f + 0.5f;
                bool plankSeam = (y % 16 == 0);
                Color rowWood = Color.Lerp(baseDark, baseLight, Mathf.Clamp01(grain));
                if (plankSeam) rowWood = Color.Lerp(rowWood, outerBorder, 0.35f);

                for (int x = 0; x < w; x++)
                {
                    bool isOuter = (x < borderWidth || x >= w - borderWidth || y < borderWidth || y >= h - borderWidth);
                    bool isInnerTrim = !isOuter && (x == borderWidth || x == w - borderWidth - 1 || y == borderWidth || y == h - borderWidth - 1);

                    if (isOuter)
                        pixels[y * w + x] = outerBorder;
                    else if (isInnerTrim)
                        pixels[y * w + x] = innerTrim;
                    else
                        pixels[y * w + x] = rowWood;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        private static Texture2D CreateStoneSlabTexture(int w, int h, Color topCol, Color bottomCol, Color borderCol, int borderWidth, bool roundedCorners = true)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / Mathf.Max(1, h - 1);
                Color grad = Color.Lerp(bottomCol, topCol, t);

                for (int x = 0; x < w; x++)
                {
                    if (roundedCorners && ((x == 0 && (y == 0 || y == h - 1)) || (x == w - 1 && (y == 0 || y == h - 1))))
                    {
                        pixels[y * w + x] = new Color(0f, 0f, 0f, 0f);
                        continue;
                    }

                    bool isBorder = (x < borderWidth || x >= w - borderWidth || y < borderWidth || y >= h - borderWidth);
                    pixels[y * w + x] = isBorder ? borderCol : grad;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        private static Texture2D CreateLeftAccentRowTexture(int w, int h, Color fillCol, Color accentCol, int accentWidth)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < accentWidth)
                        pixels[y * w + x] = accentCol;
                    else if (y == 0 || y == h - 1)
                        pixels[y * w + x] = Color.Lerp(fillCol, accentCol, 0.35f);
                    else
                        pixels[y * w + x] = fillCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        private static void CreateVanillaTextures()
        {
            _texWoodWindowBg = CreateWoodPlankTexture(
                32, 64,
                ColorWoodLight,
                ColorWoodDark,
                ColorWoodBorder,
                new Color(0.42f, 0.28f, 0.16f, 1.0f),
                3
            );

            _texWoodPanelInset = CreateWoodPlankTexture(
                24, 48,
                new Color(0.19f, 0.12f, 0.08f, 0.95f),
                new Color(0.14f, 0.09f, 0.05f, 0.95f),
                new Color(0.10f, 0.06f, 0.03f, 1.0f),
                new Color(0.32f, 0.21f, 0.13f, 0.85f),
                2
            );

            _texLogBoxBg = CreateStoneSlabTexture(
                16, 16,
                new Color(0.09f, 0.07f, 0.05f, 0.96f),
                new Color(0.07f, 0.05f, 0.04f, 0.96f),
                new Color(0.28f, 0.19f, 0.12f, 1.0f),
                1,
                false
            );

            _texStoneButtonNormal = CreateStoneSlabTexture(20, 24, ColorStoneTop, ColorStoneBottom, ColorStoneBorder, 1, true);
            _texStoneButtonHover = CreateStoneSlabTexture(20, 24, new Color(0.28f, 0.24f, 0.20f, 1f), new Color(0.17f, 0.14f, 0.12f, 1f), ColorValheimGold, 1, true);
            _texStoneButtonActive = CreateStoneSlabTexture(20, 24, new Color(0.26f, 0.21f, 0.16f, 1f), new Color(0.15f, 0.12f, 0.09f, 1f), ColorAmberHighlight, 2, true);

            _texTabNormal = CreateStoneSlabTexture(24, 28, new Color(0.20f, 0.18f, 0.16f, 0.96f), new Color(0.11f, 0.10f, 0.09f, 0.96f), new Color(0.28f, 0.25f, 0.22f, 1f), 1, true);
            _texTabActive = CreateStoneSlabTexture(24, 28, new Color(0.27f, 0.23f, 0.19f, 1f), new Color(0.15f, 0.13f, 0.10f, 1f), ColorAmberHighlight, 2, true);

            _texListRowNormal = CreateStoneSlabTexture(16, 20, new Color(0.16f, 0.11f, 0.07f, 0.85f), new Color(0.13f, 0.09f, 0.06f, 0.85f), new Color(0.24f, 0.16f, 0.10f, 0.7f), 1, false);
            _texListRowSelected = CreateLeftAccentRowTexture(24, 24, new Color(0.32f, 0.21f, 0.11f, 0.96f), ColorAmberHighlight, 6);

            _texCraftButtonNormal = CreateStoneSlabTexture(24, 32, new Color(0.24f, 0.22f, 0.20f, 1f), new Color(0.12f, 0.11f, 0.10f, 1f), ColorAmberHighlight, 2, true);
            _texCraftButtonHover = CreateStoneSlabTexture(24, 32, new Color(0.32f, 0.28f, 0.23f, 1f), new Color(0.18f, 0.15f, 0.12f, 1f), ColorValheimGold, 2, true);

            _texDangerNormal = CreateStoneSlabTexture(20, 24, new Color(0.28f, 0.11f, 0.10f, 1f), new Color(0.16f, 0.06f, 0.06f, 1f), new Color(0.55f, 0.22f, 0.20f, 1f), 1, true);
            _texDangerHover = CreateStoneSlabTexture(20, 24, new Color(0.40f, 0.15f, 0.14f, 1f), new Color(0.24f, 0.08f, 0.08f, 1f), ColorCrimson, 2, true);

            _texInputBg = CreateStoneSlabTexture(16, 20, new Color(0.10f, 0.08f, 0.06f, 1f), new Color(0.13f, 0.10f, 0.08f, 1f), new Color(0.36f, 0.27f, 0.18f, 1f), 1, true);
            _texSliderTrack = CreateStoneSlabTexture(12, 8, new Color(0.08f, 0.06f, 0.04f, 1f), new Color(0.14f, 0.10f, 0.07f, 1f), new Color(0.32f, 0.22f, 0.14f, 1f), 1, false);
            _texSliderThumb = CreateStoneSlabTexture(12, 16, ColorValheimGold, ColorAmberHighlight, new Color(1f, 0.9f, 0.5f, 1f), 1, true);
            _texSlotBoxBg = CreateStoneSlabTexture(24, 24, new Color(0.12f, 0.09f, 0.07f, 0.95f), new Color(0.08f, 0.06f, 0.04f, 0.95f), new Color(0.24f, 0.17f, 0.11f, 1f), 1, false);
            _texSlotBoxEquipped = CreateStoneSlabTexture(24, 24, new Color(0.22f, 0.15f, 0.08f, 0.98f), new Color(0.14f, 0.09f, 0.05f, 0.98f), ColorAmberHighlight, 2, true);
        }

        public static void BuildStyles()
        {
            Font bodyBold = _averiaBoldFont ?? GUI.skin.font;
            Font bodyRegular = _averiaRegularFont ?? GUI.skin.font;
            Font bannerFont = _norseTitleFont ?? bodyBold;

            // 1. Window Frame
            WindowStyle = new GUIStyle(GUI.skin.window)
            {
                font = bannerFont,
                fontSize = Scaled(19),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(16, 16, Scaled(34), 14),
                border = new RectOffset(6, 6, 6, 6)
            };
            WindowStyle.normal.background = _texWoodWindowBg;
            WindowStyle.normal.textColor = ColorValheimGold;
            WindowStyle.onNormal.background = _texWoodWindowBg;
            WindowStyle.onNormal.textColor = ColorValheimGold;

            // 2. Top Navigation Tabs
            TabInactiveStyle = new GUIStyle(GUI.skin.button)
            {
                font = bodyBold,
                fontSize = Scaled(14),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(10, 10, 6, 6)
            };
            TabInactiveStyle.normal.background = _texTabNormal;
            TabInactiveStyle.normal.textColor = ColorSilverMuted;
            TabInactiveStyle.hover.background = _texStoneButtonHover;
            TabInactiveStyle.hover.textColor = ColorParchment;
            TabInactiveStyle.active.background = _texTabActive;
            TabInactiveStyle.active.textColor = ColorValheimGold;

            TabActiveStyle = new GUIStyle(TabInactiveStyle);
            TabActiveStyle.normal.background = _texTabActive;
            TabActiveStyle.normal.textColor = ColorValheimGold;
            TabActiveStyle.hover.background = _texTabActive;
            TabActiveStyle.hover.textColor = ColorValheimGold;

            // 3. Recessed Wood Panels / Cards
            CardStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 4, 6)
            };
            CardStyle.normal.background = _texWoodPanelInset;

            LogBoxStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(8, 8, 6, 6),
                margin = new RectOffset(0, 0, 4, 4)
            };
            LogBoxStyle.normal.background = _texLogBoxBg;

            SlotBoxStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(2, 2, 2, 2)
            };
            SlotBoxStyle.normal.background = _texSlotBoxBg;

            SlotBoxEquippedStyle = new GUIStyle(SlotBoxStyle);
            SlotBoxEquippedStyle.normal.background = _texSlotBoxEquipped;

            SlotCountStyle = new GUIStyle(GUI.skin.label)
            {
                font = bodyBold,
                fontSize = Scaled(11),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight,
                padding = new RectOffset(0, 3, 0, 1)
            };
            SlotCountStyle.normal.textColor = ColorParchment;

            // 4. Headers
            CardHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                font = bannerFont,
                fontSize = Scaled(17),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 0, 4)
            };
            CardHeaderStyle.normal.textColor = ColorValheimGold;

            SubHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                font = bodyBold,
                fontSize = Scaled(13),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 4, 2)
            };
            SubHeaderStyle.normal.textColor = ColorValheimGold;

            // 5. Readable Body Labels
            LabelParchment = new GUIStyle(GUI.skin.label)
            {
                font = bodyRegular,
                fontSize = Scaled(13),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                richText = true
            };
            LabelParchment.normal.textColor = ColorParchment;

            LabelMuted = new GUIStyle(GUI.skin.label)
            {
                font = bodyRegular,
                fontSize = Scaled(12),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                richText = true
            };
            LabelMuted.normal.textColor = ColorSilverMuted;

            LabelGold = new GUIStyle(GUI.skin.label)
            {
                font = bodyBold,
                fontSize = Scaled(14),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            LabelGold.normal.textColor = ColorValheimGold;

            LabelLogEntry = new GUIStyle(GUI.skin.label)
            {
                font = GUI.skin.font,
                fontSize = Scaled(12),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                richText = true,
                margin = new RectOffset(0, 0, 1, 1)
            };
            LabelLogEntry.normal.textColor = new Color(0.90f, 0.87f, 0.80f, 1f);

            // 6. Stone Pill Buttons
            ButtonNormal = new GUIStyle(GUI.skin.button)
            {
                font = bodyBold,
                fontSize = Scaled(13),
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(8, 8, 5, 5)
            };
            ButtonNormal.normal.background = _texStoneButtonNormal;
            ButtonNormal.normal.textColor = ColorParchment;
            ButtonNormal.hover.background = _texStoneButtonHover;
            ButtonNormal.hover.textColor = ColorValheimGold;
            ButtonNormal.active.background = _texStoneButtonActive;
            ButtonNormal.active.textColor = ColorValheimGold;

            ButtonSelected = new GUIStyle(ButtonNormal)
            {
                fontStyle = FontStyle.Bold
            };
            ButtonSelected.normal.background = _texStoneButtonActive;
            ButtonSelected.normal.textColor = ColorValheimGold;
            ButtonSelected.hover.background = _texStoneButtonActive;
            ButtonSelected.hover.textColor = ColorValheimGold;

            // 7. Roster List Row Buttons
            ButtonListRow = new GUIStyle(ButtonNormal)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 8, 6, 6)
            };
            ButtonListRow.normal.background = _texListRowNormal;
            ButtonListRow.normal.textColor = ColorParchment;

            ButtonListRowSelected = new GUIStyle(ButtonListRow)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(14, 8, 6, 6)
            };
            ButtonListRowSelected.normal.background = _texListRowSelected;
            ButtonListRowSelected.normal.textColor = ColorValheimGold;
            ButtonListRowSelected.hover.background = _texListRowSelected;
            ButtonListRowSelected.hover.textColor = ColorValheimGold;

            // 8. Primary 'Craft'-Style Button
            ButtonPrimary = new GUIStyle(ButtonNormal)
            {
                font = bodyBold,
                fontSize = Scaled(15),
                fontStyle = FontStyle.Bold
            };
            ButtonPrimary.normal.background = _texCraftButtonNormal;
            ButtonPrimary.normal.textColor = ColorValheimGold;
            ButtonPrimary.hover.background = _texCraftButtonHover;
            ButtonPrimary.hover.textColor = Color.white;
            ButtonPrimary.active.background = _texCraftButtonHover;
            ButtonPrimary.active.textColor = ColorValheimGold;

            // 9. Danger Button
            ButtonDanger = new GUIStyle(ButtonNormal);
            ButtonDanger.normal.background = _texDangerNormal;
            ButtonDanger.normal.textColor = new Color(1f, 0.72f, 0.70f, 1f);
            ButtonDanger.hover.background = _texDangerHover;
            ButtonDanger.hover.textColor = Color.white;
            ButtonDanger.active.background = _texDangerHover;
            ButtonDanger.active.textColor = Color.white;

            // 10. Inputs & Sliders
            TextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                font = bodyRegular,
                fontSize = Scaled(14),
                alignment = TextAnchor.MiddleLeft,
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(8, 8, 4, 4)
            };
            TextFieldStyle.normal.background = _texInputBg;
            TextFieldStyle.normal.textColor = ColorParchment;
            TextFieldStyle.focused.background = _texInputBg;
            TextFieldStyle.focused.textColor = ColorValheimGold;

            SliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                fixedHeight = 8f,
                border = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(2, 2, 6, 6)
            };
            SliderStyle.normal.background = _texSliderTrack;

            SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                fixedWidth = 14f,
                fixedHeight = 16f,
                border = new RectOffset(2, 2, 2, 2)
            };
            SliderThumbStyle.normal.background = _texSliderThumb;
            SliderThumbStyle.hover.background = _texSliderThumb;
            SliderThumbStyle.active.background = _texSliderThumb;
        }

        public static void DrawDivider(float thickness = 1.5f, Color? color = null)
        {
            Color c = color ?? new Color(0.85f, 0.52f, 0.08f, 0.55f);
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(thickness + 6f), GUILayout.ExpandWidth(true));
            r.y += 3f;
            r.height = thickness;
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        public static void DrawHealthBar(float current, float max, float width = 130f, float height = 14f)
        {
            float pct = Mathf.Clamp01(current / Mathf.Max(1f, max));
            Rect r = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));

            Color prev = GUI.color;
            GUI.color = new Color(0.14f, 0.05f, 0.05f, 1.0f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            Rect fillRect = new Rect(r.x + 1, r.y + 1, Mathf.Max(0f, (r.width - 2) * pct), r.height - 2);
            GUI.color = new Color(0.82f, 0.24f, 0.20f, 1.0f);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

            GUI.color = new Color(0.55f, 0.38f, 0.20f, 1.0f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.yMax - 1, r.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, 1, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax - 1, r.y, 1, r.height), Texture2D.whiteTexture);

            GUI.color = prev;
        }

        public static void DrawProgressBar(float fraction, float width = 130f, float height = 14f, Color? fillColor = null)
        {
            float pct = Mathf.Clamp01(fraction);
            Rect r = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));

            Color prev = GUI.color;
            GUI.color = new Color(0.10f, 0.08f, 0.06f, 1.0f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            Rect fillRect = new Rect(r.x + 1, r.y + 1, Mathf.Max(0f, (r.width - 2) * pct), r.height - 2);
            GUI.color = fillColor ?? ColorValheimGold;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

            GUI.color = new Color(0.35f, 0.25f, 0.15f, 1.0f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.yMax - 1, r.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, 1, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax - 1, r.y, 1, r.height), Texture2D.whiteTexture);

            GUI.color = prev;
        }
    }
}
