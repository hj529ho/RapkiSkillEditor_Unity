using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class AndNode : BaseNode
    {
        public Port InputA;
        public Port InputB;
        public Port Output;

        public AndNode(SkillGraphView graphView) : base(graphView, "AND", new Color(0.9f, 0.9f, 0.5f))
        {
            var boolColor = new Color(0.9f, 0.9f, 0.5f);

            InputA = CreateInputPort("A", typeof(bool), Port.Capacity.Single, boolColor);
            InputB = CreateInputPort("B", typeof(bool), Port.Capacity.Single, boolColor);
            Output = CreateOutputPort("Result", typeof(bool), Port.Capacity.Multi, boolColor);

            mainContainer.Add(new Label("A && B") { style = { fontSize = 10 } });

            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class OrNode : BaseNode
    {
        public Port InputA;
        public Port InputB;
        public Port Output;

        public OrNode(SkillGraphView graphView) : base(graphView, "OR", new Color(0.9f, 0.9f, 0.5f))
        {
            var boolColor = new Color(0.9f, 0.9f, 0.5f);

            InputA = CreateInputPort("A", typeof(bool), Port.Capacity.Single, boolColor);
            InputB = CreateInputPort("B", typeof(bool), Port.Capacity.Single, boolColor);
            Output = CreateOutputPort("Result", typeof(bool), Port.Capacity.Multi, boolColor);

            mainContainer.Add(new Label("A || B") { style = { fontSize = 10 } });

            RefreshExpandedState();
            RefreshPorts();
        }
    }

    public class NotNode : BaseNode
    {
        public Port Input;
        public Port Output;

        public NotNode(SkillGraphView graphView) : base(graphView, "NOT", new Color(0.9f, 0.9f, 0.5f))
        {
            var boolColor = new Color(0.9f, 0.9f, 0.5f);

            Input = CreateInputPort("A", typeof(bool), Port.Capacity.Single, boolColor);
            Output = CreateOutputPort("Result", typeof(bool), Port.Capacity.Multi, boolColor);

            mainContainer.Add(new Label("!A") { style = { fontSize = 10 } });

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
