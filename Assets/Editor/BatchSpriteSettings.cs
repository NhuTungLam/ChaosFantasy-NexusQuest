using UnityEngine;
using UnityEditor;
using System.IO;

public class BatchSpriteSettings
{
    [MenuItem("Tools/Apply Sprite Settings")]
    static void ApplySettings()
    {
        string spriteFolder = "Assets/Sprite/Map";
        string[] spriteFiles = Directory.GetFiles(spriteFolder, "*.png", SearchOption.AllDirectories);

        foreach (string path in spriteFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        Debug.Log("? Batch apply sprite settings completed.");
    }
    [MenuItem("Tools/Apply Sprite Multiple")]
    static void ApplySettings1()
    {
        string spriteFolder = "Assets/Sprite/Multiple";
        string[] spriteFiles = Directory.GetFiles(spriteFolder, "*.png", SearchOption.AllDirectories);

        foreach (string path in spriteFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.SaveAndReimport();
            }
        }

        Debug.Log("? Batch apply sprite settings completed.");
    }
    [MenuItem("Tools/Apply Sprite Single")]
    static void ApplySettings2()
    {
        string spriteFolder = "Assets/Sprite/Single";
        string[] spriteFiles = Directory.GetFiles(spriteFolder, "*.png", SearchOption.AllDirectories);

        foreach (string path in spriteFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        Debug.Log("? Batch apply sprite settings completed.");
    }
}
