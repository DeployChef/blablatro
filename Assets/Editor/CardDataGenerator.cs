using System.IO;
using UnityEditor;
using UnityEngine;

public static class CardDataGenerator
{
    [MenuItem("Tools/Cards/Generate Standard Deck")]
    public static void GenerateStandardDeck()
    {
        string path = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                var card = ScriptableObject.CreateInstance<CardData>();
                card.suit = suit;
                card.rank = rank;

                string fileName = $"Card_{rank}_{suit}.asset";
                string assetPath = Path.Combine(path, fileName);

                AssetDatabase.CreateAsset(card, assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Deck generated in folder: {path}");
    }

    private static string GetSelectedFolderPath()
    {
        // если выделена папка
        if (Selection.activeObject != null)
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                return selectedPath;
            }

            // если выделен ассет внутри папки
            if (!string.IsNullOrEmpty(selectedPath))
            {
                string dir = Path.GetDirectoryName(selectedPath);
                if (!string.IsNullOrEmpty(dir) && AssetDatabase.IsValidFolder(dir))
                    return dir;
            }
        }

        return "Assets";
    }
}