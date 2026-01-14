using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SkillEditor.Editor
{
    public class ContextNode : BaseNode
    {
        public Port PipelineInput;
        public Port SelfOutput;
        public Port TargetOutput;

        private ISkillEditorConfig _config;

        public ContextNode(SkillGraphView graphView) : base(graphView, "Context", Color.cyan)
        {
            _config = SkillEditorConfig.Default;
            
            var pipelineColor = new Color(0.5f, 1f, 0.5f);
            var selfColor = new Color(0.3f, 0.8f, 1f);
            var targetColor = new Color(1f, 0.4f, 0.4f);

            PipelineInput = CreateInputPort("Pipeline", typeof(Pipeline), Port.Capacity.Single, pipelineColor);
            SelfOutput = CreateOutputPort(_config.ContextSelfPortName, typeof(Entity), Port.Capacity.Multi, selfColor);
            TargetOutput = CreateOutputPort(_config.ContextTargetPortName, typeof(Entity), Port.Capacity.Multi, targetColor);

            RefreshExpandedState();
            RefreshPorts();
        }
    }

    // Entity 타입 마커
    public class Entity { }
}
