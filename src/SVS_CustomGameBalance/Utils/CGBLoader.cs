using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SVS_CustomGameBalance
{
    internal class CGBLoader
    {
        public static Texture2D LoadSprite()
        {
            var resourceOutfits = "SVS_CustomGameBalance.Resources.btn_switch.png";

            var tex = new Texture2D(2, 2, GraphicsFormat.R8G8B8A8_SRGB, 1, TextureCreationFlags.None);

            var embeded = GetPngResourceAsByteArray(resourceOutfits);
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
            var assembly = Assembly.GetExecutingAssembly();

            // Construct the full resource name (Namespace.FolderName.FileName.Extension)
            // Example: "YourProjectNamespace.Images.MyImage.png"
            // You can get all resource names using assembly.GetManifestResourceNames() for debugging.
            var fullResourceName = resourceName;

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{fullResourceName}' not found.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
