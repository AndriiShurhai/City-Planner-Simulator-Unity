using UnityEngine;
using UnityEditor;

public class PivotSetter : Editor
{
    // Adjust this pivot as needed (0.5, 0) = bottom-center
    private static readonly Vector2 newPivot = new Vector2(0.5f, 0f);

    [MenuItem("Tools/Set Pivot For Selected Sprites")]
    private static void SetPivotForSelectedSprites()
    {
        // Get all selected assets in the Project
        Object[] selectedAssets = Selection.objects;
        bool anyProcessed = false;

        foreach (Object asset in selectedAssets)
        {
            // Get the path of the selected asset
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                continue;

            // Try to get a TextureImporter for this asset
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null)
                continue;

            // We only want to modify sprites in Multiple mode
            if (textureImporter.textureType == TextureImporterType.Sprite
                && textureImporter.spriteImportMode == SpriteImportMode.Multiple)
            {
                // Get all sub-sprites (the "slices")
                SpriteMetaData[] spriteSheet = textureImporter.spritesheet;

                // Check if we have any sprites to modify
                if (spriteSheet.Length > 0)
                {
                    // Track if we made changes
                    bool madeChanges = false;

                    // Modify each slice's pivot
                    for (int i = 0; i < spriteSheet.Length; i++)
                    {
                        SpriteMetaData smd = spriteSheet[i];
                        // Only change if needed
                        if (smd.alignment != (int)SpriteAlignment.Custom || smd.pivot != newPivot)
                        {
                            smd.alignment = (int)SpriteAlignment.Custom;  // Use custom pivot
                            smd.pivot = newPivot;                        // Set to our new pivot
                            spriteSheet[i] = smd;
                            madeChanges = true;
                        }
                    }

                    // Only save if we made changes
                    if (madeChanges)
                    {
                        // Set sprite sheet data back to the importer
                        textureImporter.spritesheet = spriteSheet;

                        // Force Unity to recognize the changes
                        EditorUtility.SetDirty(textureImporter);

                        // Apply and reimport
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        textureImporter.SaveAndReimport();

                        Debug.Log($"Pivot changed for {asset.name} at {assetPath}");
                        anyProcessed = true;
                    }
                }
            }
        }

        if (!anyProcessed)
        {
            Debug.Log("No sprites processed. Make sure you've selected sprite sheets using Multiple mode.");
        }
    }
}