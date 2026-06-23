using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Reflection;
using System.IO;

namespace SVS_ExpandCharacterRoster.Utils
{
    internal class ResourcesLoader
    {
        /*public enum ResourcesName
        {
            RosterBackground = 0,
            Icon12 = 1,
            Icon12Hover = 2,
            IconMax = 3,
            IconMaxHover = 4,
            SliderBG = 5,
            SliderFill = 6,
            SliderHandle = 7,
        }*/
        public static Sprite GetSprite(int type)
        {
            var text = LoadSprite(type);
            if (text == null) return null;
            Sprite newSprite = Sprite.Create(text, new Rect(0.0f, 0.0f, text.width, text.height), new Vector2(0f, 1f), 100.0f);

            switch (type)
            {
                case 0: newSprite.name = "background";
                    break;
                case 1: newSprite.name = "Icon12";
                    break;
                case 2: newSprite.name = "Icon12_Hover";
                    break;
                case 3: newSprite.name = "IconMax";
                    break;
                case 4: newSprite.name = "IconMax_Hover";
                    break;
            }
            return newSprite;
        }
        public static Sprite GetSpriteWithCustomBorders(int type)
        {
            var text = LoadSprite(type);
            if (text == null) return null;
            Sprite newSprite = null;
            switch (type)
            {
                case 5:
                    // Border values: X=Left, Y=Bottom, Z=Right, W=Top
                    newSprite = Sprite.Create(text, new Rect(0.0f, 0.0f, text.width, text.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, new Vector4(6f,11f,6f,11f));
                    newSprite.name = "slider_bg";
                    break;
                case 6:
                    newSprite = Sprite.Create(text, new Rect(0.0f, 0.0f, text.width, text.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, new Vector4(6f, 11f, 6f, 11f));
                    newSprite.name = "slider_fill";
                    break;
                case 7:
                    newSprite = Sprite.Create(text, new Rect(0.0f, 0.0f, text.width, text.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, new Vector4(10f, 12f, 10f, 8f));
                    newSprite.name = "slider_handle";
                    break;
            }
            return newSprite;
        }
        public static Texture2D LoadSprite(int type)
        {
            Assembly moreoutfits = Assembly.GetExecutingAssembly();
            string resourceSprite = "";

            switch (type)
            {
                case 0://Roster Background
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.RosterBG.png";
                    break;
                case 1://Roster Background
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.icon12_selected.png";
                    break;
                case 2://Roster Background
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.icon12_hover.png";
                    break;
                case 3://Roster Background
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.iconMax_selected.png";
                    break;
                case 4://Roster Background
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.iconMax_hover.png";
                    break;
                case 5://Slider Background
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.slider_bg.png";
                    break;
                case 6://Slider Fill
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.slider_fill.png";
                    break;
                case 7://Slider Handle
                    resourceSprite = "SVS_ExpandCharacterRoster.Resources.slider_handle.png";
                    break;
            }

            Texture2D tex = new Texture2D(2, 2, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, 1, TextureCreationFlags.None);

            byte[] embeded = GetPngResourceAsByteArray(resourceSprite);
            if (embeded.Length > 0)
            {
                tex.LoadImage(embeded);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            }
            return null;
        }

        private static byte[] GetPngResourceAsByteArray(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string fullResourceName = resourceName;

            using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{fullResourceName}' not found.");
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
