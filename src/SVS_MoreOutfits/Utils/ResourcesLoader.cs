using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SVS_MoreOutfits
{
    internal class ResourcesLoader
    {
        public static Texture2D LoadSprite(int Outfit, int type)
        {
            Assembly moreoutfits = Assembly.GetExecutingAssembly();
            string[] resourceOutfits = new string[3];

            switch (Outfit)
            {
                case 0://Weekend
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.01_Weekend_default.png",
                        "SVS_MoreOutfits.Resources.01_Weekend_hovered.png",
                        "SVS_MoreOutfits.Resources.01_Weekend_selected.png"];
                    break;
                    
                case 1://Night
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.02_Night_default.png",
                        "SVS_MoreOutfits.Resources.02_Night_hovered.png",
                        "SVS_MoreOutfits.Resources.02_Night_selected.png"];
                    break;
                    
                case 2://Lewd
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.03_Lewd_default.png",
                        "SVS_MoreOutfits.Resources.03_Lewd_hovered.png",
                        "SVS_MoreOutfits.Resources.03_Lewd_selected.png"];
                    break;

                case 3://Costume
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.04_Costume_default.png",
                        "SVS_MoreOutfits.Resources.04_Costume_hovered.png",
                        "SVS_MoreOutfits.Resources.04_Costume_selected.png"];
                    break;
                    
                case 4://Sports
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.05_Sports_default.png",
                        "SVS_MoreOutfits.Resources.05_Sports_hovered.png",
                        "SVS_MoreOutfits.Resources.05_Sports_selected.png"];
                    break;
                    
                case 5://Bath
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.06_Bath_default.png",
                        "SVS_MoreOutfits.Resources.06_Bath_hovered.png",
                        "SVS_MoreOutfits.Resources.06_Bath_selected.png"];
                    break;

                case 6://Camping
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.07_Camping_default.png",
                        "SVS_MoreOutfits.Resources.07_Camping_hovered.png",
                        "SVS_MoreOutfits.Resources.07_Camping_selected.png"];
                    break;

                case 7://Home
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.08_Home_default.png",
                        "SVS_MoreOutfits.Resources.08_Home_hovered.png",
                        "SVS_MoreOutfits.Resources.08_Home_selected.png"];
                    break;

                default:
                    resourceOutfits = [
                        "SVS_MoreOutfits.Resources.00_Unused_default.png",
                        "SVS_MoreOutfits.Resources.00_Unused_hovered.png",
                        "SVS_MoreOutfits.Resources.00_Unused_selected.png"];
                    break;
            }

            Texture2D tex = new Texture2D(2, 2, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,1, TextureCreationFlags.None);

            byte[] embeded = GetPngResourceAsByteArray(resourceOutfits[type]);
            if (embeded.Length > 0)
            {
                tex.LoadImage(embeded);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            }
            return null;
        }
        public static Texture2D LoadPNG(string filePath, int type)
        {
            Texture2D tex = new Texture2D(2, 2); ;
            byte[] fileData;

            string spritePath = null;

            switch (type)
            {
                case 0:
                    spritePath = System.IO.Path.Combine(filePath, "default.png");
                    break;
                case 1:
                    spritePath = System.IO.Path.Combine(filePath, "hovered.png");
                    break;
                case 2:
                    spritePath = System.IO.Path.Combine(filePath, "selected.png");
                    break;
            }

            if (File.Exists(spritePath))
            {
                fileData = File.ReadAllBytes(spritePath);
                tex.LoadImage(fileData);
                tex.filterMode = FilterMode.Bilinear;
                tex.Compress(true);
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
