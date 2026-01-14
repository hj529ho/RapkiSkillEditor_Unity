using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class ComparisonNode : BaseNode
    {
        public Port InputA;
        public Port InputB;
        public Port Output;
        
        public CompareType CompareType;
        public EnumField CompareTypeField;

        public ComparisonNode(SkillGraphView graphView) : base(graphView, "Compare", Color.yellow)
        {
            var floatColor = Color.gray;
            var boolColor = new Color(0.9f, 0.9f, 0.5f);

            InputA = CreateInputPort("A", typeof(float), Port.Capacity.Single, floatColor);
            InputB = CreateInputPort("B", typeof(float), Port.Capacity.Single, floatColor);
            Output = CreateOutputPort("Result", typeof(bool), Port.Capacity.Multi, boolColor);

            CompareTypeField = new EnumField("Type", CompareType.Equal);
            CompareTypeField.RegisterValueChangedCallback(evt =>
            {
                CompareType = (CompareType)evt.newValue;
            });
            mainContainer.Add(CompareTypeField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
