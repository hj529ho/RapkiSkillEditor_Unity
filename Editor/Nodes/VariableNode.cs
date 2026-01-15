using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class VariableNode : BaseNode
    {
        public Port Output;
        public string VariableName = "NewVariable";
        public VariableType VariableType = VariableType.Float;
        
        public int DefaultIntValue;
        public float DefaultFloatValue;
        public int IntValue;
        public float FloatValue;
        
        private TextField _nameField;
        private EnumField _typeField;
        private VisualElement _defaultContainer;
        private VisualElement _valueContainer;

        public VariableNode(SkillGraphView graphView) 
            : base(graphView, "$Variable", new Color(0.4f, 0.6f, 0.4f))
        {
            Output = CreateOutputPort("Value", typeof(float), Port.Capacity.Multi, Color.gray);
            
            _nameField = new TextField("Name") { value = VariableName };
            _nameField.RegisterValueChangedCallback(evt =>
            {
                VariableName = evt.newValue;
                UpdateTitle();
            });
            mainContainer.Add(_nameField);
            
            _typeField = new EnumField("Type", VariableType.Float);
            _typeField.RegisterValueChangedCallback(evt =>
            {
                VariableType = (VariableType)evt.newValue;
                UpdateFields();
                UpdateTitle();
            });
            mainContainer.Add(_typeField);
            
            _defaultContainer = new VisualElement();
            mainContainer.Add(_defaultContainer);
            
            _valueContainer = new VisualElement();
            mainContainer.Add(_valueContainer);
            
            UpdateFields();
            UpdateTitle();
            RefreshExpandedState();
            RefreshPorts();
        }
        
        private void UpdateFields()
        {
            _defaultContainer.Clear();
            _valueContainer.Clear();
            
            if (VariableType == VariableType.Int)
            {
                var defaultField = new IntegerField("Default") { value = DefaultIntValue };
                defaultField.RegisterValueChangedCallback(evt => DefaultIntValue = evt.newValue);
                _defaultContainer.Add(defaultField);
                
                var valueField = new IntegerField("Value") { value = IntValue };
                valueField.RegisterValueChangedCallback(evt => IntValue = evt.newValue);
                _valueContainer.Add(valueField);
            }
            else
            {
                var defaultField = new FloatField("Default") { value = DefaultFloatValue };
                defaultField.RegisterValueChangedCallback(evt => DefaultFloatValue = evt.newValue);
                _defaultContainer.Add(defaultField);
                
                var valueField = new FloatField("Value") { value = FloatValue };
                valueField.RegisterValueChangedCallback(evt => FloatValue = evt.newValue);
                _valueContainer.Add(valueField);
            }
        }
        
        private void UpdateTitle()
        {
            var typeStr = VariableType == VariableType.Int ? "int" : "float";
            title = $"${VariableName} ({typeStr})";
        }
        
        public void SetVariable(string name, VariableType type, int defaultInt, float defaultFloat, int intVal, float floatVal)
        {
            VariableName = name;
            VariableType = type;
            DefaultIntValue = defaultInt;
            DefaultFloatValue = defaultFloat;
            IntValue = intVal;
            FloatValue = floatVal;
            
            _nameField.value = name;
            _typeField.value = type;
            
            UpdateFields();
            UpdateTitle();
        }
    }
}
