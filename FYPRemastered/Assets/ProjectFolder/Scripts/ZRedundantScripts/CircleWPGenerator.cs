using UnityEngine;

[ExecuteInEditMode]
public class CircleWPGenerator : MonoBehaviour
{
    public int count = 12;
    public float radius = 5f;
    public GameObject _wpPrefab;

    [ContextMenu("Generate")]
    void Generate()
    {
        // Clear old waypoints
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

            var wp = GameObject.Instantiate(_wpPrefab);//new GameObject($"Waypoint_{i}");
            wp.name = ($"Waypoint_{i}");
            wp.transform.SetParent(transform);
            wp.transform.localPosition = pos;
        }
    }
}
