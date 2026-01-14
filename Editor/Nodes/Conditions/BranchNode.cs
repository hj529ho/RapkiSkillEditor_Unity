using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class BranchNode : BaseNode
    {
        // Input/Output 포트 (패스스루)
        public Port SelfInput;
        public Port TargetInput;
        public Port SelfOutput;
        public Port TargetOutput;
        public Port ConditionInput;
        
        // 현재 소스 추적
        public EntitySource SelfSource = EntitySource.Self;
        public EntitySource TargetSource = EntitySource.Target;

        private Label _selfSourceLabel;
        private Label _targetSourceLabel;
        private ISkillEditorConfig _config;

        public BranchNode(SkillGraphView graphView) : base(graphView, "Branch", new Color(0.4f, 0.7f, 0.4f))
        {
            _config = SkillEditorConfig.Default;

            var conditionColor = new Color(0.9f, 0.9f, 0.5f);

            // Input 포트
            SelfInput = CreateInputPort(_config.SelfPortName, typeof(Entity), Port.Capacity.Single, Color.gray);
            TargetInput = CreateInputPort(_config.TargetPortName, typeof(Entity), Port.Capacity.Single, Color.gray);
            ConditionInput = CreateInputPort(_config.ConditionPortName, typeof(bool), Port.Capacity.Single, conditionColor);
            
            // Output 포트 (패스스루)
            SelfOutput = CreateOutputPort(_config.SelfPortName, typeof(Entity), Port.Capacity.Multi, Color.gray);
            TargetOutput = CreateOutputPort(_config.TargetPortName, typeof(Entity), Port.Capacity.Multi, Color.gray);

            // 소스 라벨
            _selfSourceLabel = new Label($"{_config.SelfPortName}: (연결 없음)");
            _selfSourceLabel.style.fontSize = 10;
            _selfSourceLabel.style.color = Color.gray;
            mainContainer.Add(_selfSourceLabel);
            
            _targetSourceLabel = new Label($"{_config.TargetPortName}: (연결 없음)");
            _targetSourceLabel.style.fontSize = 10;
            _targetSourceLabel.style.color = Color.gray;
            mainContainer.Add(_targetSourceLabel);

            RefreshExpandedState();
            RefreshPorts();
        }
        
        public void UpdateSelfSource(EntitySource source)
        {
            SelfSource = source;
            UpdatePortColors();
            UpdateLabels();
        }

        public void UpdateTargetSource(EntitySource source)
        {
            TargetSource = source;
            UpdatePortColors();
            UpdateLabels();
        }

        private void UpdatePortColors()
        {
            SelfInput.portColor = NodeUtils.GetSourceColor(SelfSource);
            SelfOutput.portColor = NodeUtils.GetSourceColor(SelfSource);
            TargetInput.portColor = NodeUtils.GetSourceColor(TargetSource);
            TargetOutput.portColor = NodeUtils.GetSourceColor(TargetSource);
        }

        private void UpdateLabels()
        {
            _selfSourceLabel.text = $"{_config.SelfPortName}: {NodeUtils.GetSourceText(SelfSource)}";
            _selfSourceLabel.style.color = NodeUtils.GetSourceColor(SelfSource);
            
            _targetSourceLabel.text = $"{_config.TargetPortName}: {NodeUtils.GetSourceText(TargetSource)}";
            _targetSourceLabel.style.color = NodeUtils.GetSourceColor(TargetSource);
        }
    }
}
