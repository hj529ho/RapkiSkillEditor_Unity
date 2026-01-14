using SkillEditor.Core;
using UnityEditor;
using UnityEngine;

namespace SkillEditor.Editor
{
    [CustomEditor(typeof(SkillGraphData))]
    public class SkillGraphDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open in Skill Editor", GUILayout.Height(30)))
            {
                SkillGraphWindow.Open((SkillGraphData)target);
            }

            if (GUILayout.Button("Recompile"))
            {
                SkillGraphCompiler.Compile((SkillGraphData)target);
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
