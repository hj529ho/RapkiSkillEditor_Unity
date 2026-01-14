using SkillEditor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class SkillGraphWindow : EditorWindow
    {
        private SkillGraphView _graphView;
        private SkillGraphData _currentData;
        
        public SkillGraphView GraphView => _graphView;
        public SkillGraphData CurrentData => _currentData;
        
        [MenuItem("Tools/Skill Editor")]
        public static void Open()
        {
            var window = GetWindow<SkillGraphWindow>();
            window.titleContent = new GUIContent("Skill Graph Editor");
        }
        
        public static void Open(SkillGraphData data)
        {
            var window = GetWindow<SkillGraphWindow>();
            window.titleContent = new GUIContent("Skill Graph Editor");
            window.LoadGraph(data);
        }

        private void OnEnable()
        {
            CreateGraphView();
            CreateToolbar();
        }

        private void OnDisable()
        {
            if (_graphView != null)
                rootVisualElement.Remove(_graphView);
        }

        public void CreateGraphView()
        {
            _graphView = new SkillGraphView(this)
            {
                name = "Skill Graph"
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void CreateToolbar()
        {
            var toolbar = new Toolbar();
            
            var saveButton = new ToolbarButton(() => Save()) { text = "Save" };
            var loadButton = new ToolbarButton(() => Load()) { text = "Load" };
            
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            
            rootVisualElement.Add(toolbar);
        }

        private void Save()
        {
            if (_currentData == null)
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Skill Graph", "NewSkillGraph", "asset", "");
                if (string.IsNullOrEmpty(path)) return;
                
                _currentData = ScriptableObject.CreateInstance<SkillGraphData>();
                AssetDatabase.CreateAsset(_currentData, path);
            }
            
            SkillGraphSaveLoad.Save(_graphView, _currentData);
        }

        private void Load()
        {
            var path = EditorUtility.OpenFilePanel("Load Skill Graph", "Assets", "asset");
            if (string.IsNullOrEmpty(path)) return;
            
            path = "Assets" + path.Substring(Application.dataPath.Length);
            var data = AssetDatabase.LoadAssetAtPath<SkillGraphData>(path);
            
            if (data != null)
                LoadGraph(data);
        }

        public void LoadGraph(SkillGraphData data)
        {
            _currentData = data;
            SkillGraphSaveLoad.Load(_graphView, data);
        }
    }
}
