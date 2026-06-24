using System.IO;
using System.Reflection;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SVS_SixthSense.Utils
{
    internal class SixthSenseLoader
    {
        public static T GetResource<T>(string name) where T : Object
        {
            var objs = Resources.FindObjectsOfTypeAll(Il2CppType.Of<T>());
            for (var i = objs.Length - 1; i >= 0; --i)
            {
                var obj = objs[i];
                if (obj != null && obj.name == name)
                {
                    var ret = obj.TryCast<T>();
                    return ret;
                }
            }
            return null;
        }
        public static Sprite GetMoodSprites(int moodType)
        {
            Texture2D moodIcon = LoadSprite(moodType);
            Sprite newSprite = Sprite.Create(moodIcon, new Rect(0.0f, 0.0f, moodIcon.width, moodIcon.height), new Vector2(0f, 1f), 100.0f);
            switch (moodType)
            {
                case 0:// Happy
                    newSprite.name = "HappySp";
                    break;
                case 1://Romantic
                    newSprite.name = "RomanticSp";
                    break;
                case 2://Jealousy
                    newSprite.name = "JealousySp";
                    break;
                case 3://Anger
                    newSprite.name = "AngerSp";
                    break;
                case 4://Sad
                    newSprite.name = "SadSp";
                    break;
                case 5://Calm
                    newSprite.name = "CalmSp";
                    break;
                case 6://Horny
                    newSprite.name = "HornySp";
                    break;
                case 7://Serious
                    newSprite.name = "SeriousSp";
                    break;
                case 8://Agressive
                    newSprite.name = "AgressiveSp";
                    break;
                case 9://Normal
                    newSprite.name = "NormalSp";
                    break;
                default:
                    newSprite.name = "Unknown";
                    break;
            }
            return newSprite;
        }
        public static Texture2D LoadSprite(int type)
        {
            string resourceSprite;
            switch (type)
            {
                case 0:// Happy
                    resourceSprite = "SVS_SixthSense.Resources.00_happy.png";
                    break;
                case 1://Romantic
                    resourceSprite = "SVS_SixthSense.Resources.01_shy.png";
                    break;
                case 2://Jealousy
                    resourceSprite = "SVS_SixthSense.Resources.02_jealous.png";
                    break;
                case 3://Anger
                    resourceSprite = "SVS_SixthSense.Resources.03_angry.png";
                    break;
                case 4://Sad
                    resourceSprite = "SVS_SixthSense.Resources.04_sad.png";
                    break;
                case 5://Calm
                    resourceSprite = "SVS_SixthSense.Resources.05_calm.png";
                    break;
                case 6://Horny
                    resourceSprite = "SVS_SixthSense.Resources.06_horny.png";
                    break;
                case 7://Serious
                    resourceSprite = "SVS_SixthSense.Resources.07_serious.png";
                    break;
                case 8://Agressive
                    resourceSprite = "SVS_SixthSense.Resources.08_tension.png";
                    break;
                case 9://Normal
                    resourceSprite = "SVS_SixthSense.Resources.09_normal.png";
                    break;
                default:
                    resourceSprite = "SVS_SixthSense.Resources._unknown.png";
                    break;
            }

            Texture2D tex = new Texture2D(2, 2, GraphicsFormat.R8G8B8A8_SRGB, 1, TextureCreationFlags.None);

            byte[] embeded = GetPngResourceAsByteArray(resourceSprite);
            if (embeded.Length > 0)
            {
                tex.LoadImage(embeded);
                tex.filterMode = FilterMode.Bilinear;
                return tex;
            }
            return null;
        }

        private static byte[] GetPngResourceAsByteArray(string resourceName)
        {
            // Get the assembly containing the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Construct the full resource name (Namespace.FolderName.FileName.Extension)
            // Example: "YourProjectNamespace.Images.MyImage.png"
            // You can get all resource names using assembly.GetManifestResourceNames() for debugging.
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
