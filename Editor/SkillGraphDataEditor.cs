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
            var data = (SkillGraphData)target;
            serializedObject.Update();



            var sprite = data.icon;
       

            if (sprite != null)
            {
                float maxWidth = 200f;
                float maxHeight = 300f;
    
                float spriteWidth = sprite.textureRect.width;
                float spriteHeight = sprite.textureRect.height;
                float spriteAspect = spriteWidth / spriteHeight;
    
                float boxAspect = maxWidth / maxHeight;
    
                float drawWidth, drawHeight;
    
                if (spriteAspect > boxAspect)
                {
                    drawWidth = maxWidth;
                    drawHeight = maxWidth / spriteAspect;
                }
                else
                {
                    drawHeight = maxHeight;
                    drawWidth = maxHeight * spriteAspect;
                }
    
                // 중앙 정렬
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
    
                Rect rect = GUILayoutUtility.GetRect(
                    drawWidth, drawHeight,
                    GUILayout.Width(drawWidth),
                    GUILayout.Height(drawHeight)
                );
    
                Rect texCoords = new Rect(
                    sprite.textureRect.x / sprite.texture.width,
                    sprite.textureRect.y / sprite.texture.height,
                    sprite.textureRect.width / sprite.texture.width,
                    sprite.textureRect.height / sprite.texture.height
                );
    
                GUI.DrawTextureWithTexCoords(rect, sprite.texture, texCoords);
    
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pipelineCount"));
            
            EditorGUILayout.Space();
            
            // Variables - 필드처럼 표시
            if (data.variables != null && data.variables.Count > 0)
            {
                EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                foreach (var variable in data.variables)
                {
                    EditorGUI.BeginChangeCheck();
                    
                    if (variable.type == VariableType.Int)
                    {
                        int newVal = EditorGUILayout.IntField(variable.name, variable.intValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(data, "Change Variable");
                            variable.intValue = newVal;
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        float newVal = EditorGUILayout.FloatField(variable.name, variable.floatValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(data, "Change Variable");
                            variable.floatValue = newVal;
                            EditorUtility.SetDirty(data);
                        }
                    }
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            // Buttons
            if (GUILayout.Button("Open in Skill Editor", GUILayout.Height(30)))
            {
                SkillGraphWindow.Open(data);
            }

            if (GUILayout.Button("Recompile"))
            {
                SkillGraphCompiler.Compile(data);
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
