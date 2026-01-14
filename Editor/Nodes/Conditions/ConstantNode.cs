using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class ConstantNode : BaseNode
    {
        public Port Output;
        public float Value;
        public FloatField ValueField;

        public ConstantNode(SkillGraphView graphView) : base(graphView, "Constant", Color.gray)
        {
            Output = CreateOutputPort("Value", typeof(float), Port.Capacity.Multi, Color.gray);

            ValueField = new FloatField("Value") { value = 0 };
            ValueField.RegisterValueChangedCallback(evt => Value = evt.newValue);
            mainContainer.Add(ValueField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
