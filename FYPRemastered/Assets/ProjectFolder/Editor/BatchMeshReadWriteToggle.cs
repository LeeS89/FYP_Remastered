using UnityEditor;
using UnityEngine;

public class BatchMeshReadWriteToggle : EditorWindow
{
    [MenuItem("Tools/Batch Toggle Mesh Read/Write")]
    public static void ShowWindow()
    {
        GetWindow<BatchMeshReadWriteToggle>("Batch Mesh Read/Write Toggle");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Enable/Disable Read/Write", EditorStyles.boldLabel);

        if (GUILayout.Button("Enable Read/Write on Selected Meshes"))
        {
            ToggleReadWriteOnSelected(true);
        }

        if (GUILayout.Button("Disable Read/Write on Selected Meshes"))
        {
            ToggleReadWriteOnSelected(false);
        }
    }

    private void ToggleReadWriteOnSelected(bool enable)
    {
        var selected = Selection.objects;

        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null)
            {
                importer.isReadable = enable;
                importer.SaveAndReimport();
                Debug.Log($"{(enable ? "Enabled" : "Disabled")} Read/Write on {obj.name}");
            }
        }

        Debug.Log($"✅ Finished processing {selected.Length} selected items.");
    }
}
