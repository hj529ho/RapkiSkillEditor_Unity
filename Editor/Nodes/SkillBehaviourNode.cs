using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class SkillBehaviourNode : BaseNode
    {
        public string BehaviourName;
        public int Value;
        
        // Input/Output 포트 (패스스루)
        public Port SelfInput;
        public Port TargetInput;
        public Port ValueInput;
        public Port SelfOutput;
        public Port TargetOutput;
        
        // 현재 소스 추적
        public EntitySource SelfSource = EntitySource.Self;
        public EntitySource TargetSource = EntitySource.Target;

        public IntegerField ValueField;
        private Label _selfSourceLabel;
        private Label _targetSourceLabel;
        
        private bool _isValueConnected;

        private ISkillEditorConfig _config;

        public SkillBehaviourNode(SkillGraphView graphView, string behaviourName) 
            : base(graphView, behaviourName, GetNodeColor(behaviourName))
        {
            _config = SkillEditorConfig.Default;
            BehaviourName = behaviourName;

            // Input 포트
            SelfInput = CreateInputPort(_config.SelfPortName, typeof(Entity), Port.Capacity.Single, Color.gray);
            TargetInput = CreateInputPort(_config.TargetPortName, typeof(Entity), Port.Capacity.Single, Color.gray);
            ValueInput = CreateInputPort("Value", typeof(float), Port.Capacity.Single, Color.gray);
            
            // Output 포트 (패스스루)
            SelfOutput = CreateOutputPort(_config.SelfPortName, typeof(Entity), Port.Capacity.Multi, Color.gray);
            TargetOutput = CreateOutputPort(_config.TargetPortName, typeof(Entity), Port.Capacity.Multi, Color.gray);

            // Value 필드 (연결 없을 때만 표시)
            ValueField = new IntegerField("Value") { value = 0 };
            ValueField.RegisterValueChangedCallback(evt =>
            {
                Value = evt.newValue;
                title = $"{BehaviourName} ({Value})";
            });
            mainContainer.Add(ValueField);
            
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

        private static Color GetNodeColor(string behaviourName)
        {
            if (SkillBehaviourRegistry.Instance.Infos.TryGetValue(behaviourName, out var info))
                return info.Color * 0.7f;
            return Color.gray;
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
        
        public void SetValueConnected(bool connected)
        {
            _isValueConnected = connected;
            ValueField.style.display = connected ? DisplayStyle.None : DisplayStyle.Flex;
            
            if (connected)
                title = $"{BehaviourName} (→)";
            else
                title = $"{BehaviourName} ({Value})";
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
