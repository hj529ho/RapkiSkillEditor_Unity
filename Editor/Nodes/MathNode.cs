using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class MathNode : BaseNode
    {
        public Port InputA;
        public Port InputB;
        public Port Output;
        public EnumField EnumField;
        public MathType MathType = MathType.Add;
        
        public MathNode(SkillGraphView graphView) 
            : base(graphView, "Math", new Color(0.5f, 0.5f, 0.8f))
        {
            InputA = CreateInputPort("A", typeof(float), Port.Capacity.Single, Color.gray);
            InputB = CreateInputPort("B", typeof(float), Port.Capacity.Single, Color.gray);
            Output = CreateOutputPort("Result", typeof(float), Port.Capacity.Multi, Color.gray);
            
            EnumField= new EnumField("Type", MathType.Add);
            EnumField.RegisterValueChangedCallback(evt =>
            {
                MathType = (MathType)evt.newValue;
                UpdateTitle();
            });
            mainContainer.Add(EnumField);
            
            UpdateTitle();
            RefreshExpandedState();
            RefreshPorts();
        }
        
        public void SetMathType(MathType type)
        {
            MathType = type;
            UpdateTitle();
            EnumField.value = type;
        }
        
        private void UpdateTitle()
        {
            var symbol = MathType switch
            {
                MathType.Add => "+",
                MathType.Subtract => "-",
                MathType.Multiply => "×",
                MathType.Divide => "÷",
                _ => "?"
            };
            title = $"Math ({symbol})";
        }
    }
}
