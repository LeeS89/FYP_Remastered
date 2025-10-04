using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[CustomPropertyDrawer(typeof(FireRateParams))]
public class FireRateEditor : PropertyDrawer
{
    const float Pad = 2f;
    float Line => EditorGUIUtility.singleLineHeight;
    float Row => EditorGUIUtility.singleLineHeight + Pad;

    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label)
    {
        // Indent & foldout header
        r = EditorGUI.IndentedRect(r);
        prop.isExpanded = EditorGUI.Foldout(new Rect(r.x, r.y, r.width, Line), prop.isExpanded, label, true);
        if (!prop.isExpanded) return;

        EditorGUI.indentLevel++;

        // Sub-properties (names must match your fields)
        var fireModeProp = prop.FindPropertyRelative("_fireRate");          // enum FireRate
        var intervalProp = prop.FindPropertyRelative("_interval");          // float
        var useFixedIntervalProp = prop.FindPropertyRelative("_useFixedInterval");  // bool
        var minIntervalProp = prop.FindPropertyRelative("_minFireInterval");   // float
        var maxIntervalProp = prop.FindPropertyRelative("_maxFireInterval");   // float

        var rapidIntervalProp = prop.FindPropertyRelative("_rapidInterval");

        // Row: Fire mode
        r.y += Row;
        EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, Line), fireModeProp);

        // Draw branch by mode
        switch ((FireRate)fireModeProp.enumValueIndex)
        {
            case FireRate.Single:
                // no extra fields
                break;

            case FireRate.SingleAutomatic:
                // toggle
                r.y += Row;
                EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, Line), useFixedIntervalProp);

                if (useFixedIntervalProp.boolValue)
                {
                    r.y += Row;
                    EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, Line), intervalProp);
                }
                else
                {
                    r.y += Row;
                    EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, Line), minIntervalProp);
                    r.y += Row;
                    EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, Line), maxIntervalProp);
                }
                break;

            case FireRate.Burst:
                // add burst fields here when you have them
                break;

            case FireRate.FullAutomatic:
                r.y += Row;
                EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, Line), rapidIntervalProp);
                break;
        }

        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float h = Line; // foldout line only when collapsed
        if (!prop.isExpanded) return h;

        h += Row; // fire mode row

        var mode = (FireRate)prop.FindPropertyRelative("_fireRate").enumValueIndex;

        switch (mode)
        {
            case FireRate.Single:
                // nothing more
                break;

            case FireRate.SingleAutomatic:
                {
                    h += Row; // toggle
                    bool fixedInterval = prop.FindPropertyRelative("_useFixedInterval").boolValue;
                    h += fixedInterval ? Row       // one line for _interval
                                       : Row * 2;  // two lines for min/max
                    break;
                }

            case FireRate.Burst:
                // add to height when you add fields
                break;

            case FireRate.FullAutomatic:
                h += Row; // interval
                break;
        }

        return h;
    }



    /* const float Pad = 2f;
     float Line => EditorGUIUtility.singleLineHeight;
     float LineWithPad => EditorGUIUtility.singleLineHeight + Pad;
 */

    /*  SerializedProperty fireModeProp;
      SerializedProperty fullyAutoFireProp;
      SerializedProperty singleAutoFireProp;*/

    /* private void OnEnable()
     {
         fireModeProp *//*serializedObject.FindProperty("_fireRate");*//* = serializedObject.FindProperty(nameof(FireRateParams._fireRate));
         fullyAutoFireProp = serializedObject.FindProperty("_interval");
     }*/

    /*  public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
      {
          serializedObject.Update();

          EditorGUILayout.PropertyField(fireModeProp);

          switch ((FireRate)fireModeProp.enumValueIndex)
          {
              case FireRate.Single:
                  //EditorGUILayout.PropertyField(singleRecoilProp);
                  break;

              case FireRate.Burst:
                 // EditorGUILayout.PropertyField(burstCountProp);
                 // EditorGUILayout.PropertyField(burstIntervalProp);
                  break;

              case FireRate.FullAutomatic:
                  EditorGUILayout.PropertyField(fullyAutoFireProp);
                  break;
          }
          Debug.LogError("Editor called");
          serializedObject.ApplyModifiedProperties();

          // Optional: quick proof the custom inspector is active
          EditorGUILayout.Space();
          if (GUILayout.Button("Debug: Custom Inspector Active"))
              Debug.Log("WeaponEditor running for: " + target, target as Object);
      }*/

    /*public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        // Draw a foldout header for the whole block
        pos = EditorGUI.IndentedRect(pos);
        property.isExpanded = EditorGUI.Foldout(
            new Rect(pos.x, pos.y, pos.width, Line),
            property.isExpanded, label, true);

        if (!property.isExpanded) return;

        EditorGUI.indentLevel++;

        // Find sub-properties
        var fireModeProp = property.FindPropertyRelative("_fireRate");
        var fireIntervalProp = property.FindPropertyRelative("_interval");
        var useFixedIntervalProp = property.FindPropertyRelative("_useFixedInterval");
        var fireMinIntervalProp = property.FindPropertyRelative("_minFireInterval");
        var fireMaxIntervalProp = property.FindPropertyRelative("_maxFireInterval");
      

        // Row 1: fire mode
        pos.y += LineWithPad;
        EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), fireModeProp);

        // Branch on enum
        var mode = (FireRate)fireModeProp.enumValueIndex;

        switch (mode)
        {
            case FireRate.Single:
                pos.y += LineWithPad;
               // EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), singleShotRecoilProp);
                break;
            case FireRate.SingleAutomatic:
                pos.y += LineWithPad;
                EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), useFixedIntervalProp);

                pos.y += LineWithPad;

                if (useFixedIntervalProp.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), fireIntervalProp);
                }
                else
                {
                    pos.y += LineWithPad;
                    EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), fireMinIntervalProp);
                    pos.y += LineWithPad;
                    EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), fireMaxIntervalProp);
                }
                    break;

            case FireRate.Burst:
                pos.y += LineWithPad;
               // EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), burstCountProp);
                pos.y += LineWithPad;
               // EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), burstIntervalProp);
                break;

            case FireRate.FullAutomatic:
                pos.y += LineWithPad;
                EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, Line), fireIntervalProp);
                break;
        }

        // Optional: a tiny debug button to prove the drawer is active
        pos.y += LineWithPad;
        var btnRect = new Rect(pos.x, pos.y, 200f, Line);
        if (GUI.Button(btnRect, "Debug: Drawer Active"))
            Debug.Log("FireRateParamsDrawer active for: " + property.serializedObject.targetObject);

        EditorGUI.indentLevel--;
    }

    // Tell Unity how tall our custom UI is
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return Line;

        // 1 line for foldout + 1 for mode + fields by mode + optional button + padding
        var fireMode = property.FindPropertyRelative("_fireRate").enumValueIndex;
        int fieldLines = fireMode switch
        {
            (int)FireRate.Single => 1,   // singleShotRecoil
            (int)FireRate.Burst => 2,   // burstCount, burstInterval
            (int)FireRate.FullAutomatic => 1,   // autoRps
            _ => 0
        };

        int totalLines = 1 *//*foldout*//* + 1 *//*mode*//* + fieldLines + 1 *//*button*//*;
        return totalLines * LineWithPad;
    }*/
}
