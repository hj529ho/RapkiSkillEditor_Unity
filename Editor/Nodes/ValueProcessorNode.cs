using System.Collections.Generic;
using System.Linq;
using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class ValueProcessorNode : BaseNode
    {
        public List<Port> Inputs = new List<Port>();
        public Port Output;
        
        public string ProcessorName;
        private PopupField<string> _dropdown;

        public ValueProcessorNode(SkillGraphView graphView) 
            : base(graphView, "Processor", new Color(0.5f, 0.6f, 0.7f))
        {
            Output = CreateOutputPort("Result", typeof(float), Port.Capacity.Multi, Color.gray);
            
            var processors = GetProcessorNames();
            if (processors.Count > 0)
            {
                ProcessorName = processors[0];
                _dropdown = new PopupField<string>("Processor", processors, 0);
                _dropdown.RegisterValueChangedCallback(evt =>
                {
                    ProcessorName = evt.newValue;
                    RebuildInputPorts();
                    UpdateTitle();
                });
                mainContainer.Add(_dropdown);
                
                RebuildInputPorts();
                UpdateTitle();
            }
            else
            {
                mainContainer.Add(new Label("No processors registered"));
            }

            RefreshExpandedState();
            RefreshPorts();
        }
        
        private List<string> GetProcessorNames()
        {
            return ValueProcessorRegistry.Instance.Infos.Keys.ToList();
        }
        
        private void RebuildInputPorts()
        {
            // 기존 입력 포트 제거
            foreach (var port in Inputs)
            {
                inputContainer.Remove(port);
            }
            Inputs.Clear();
            
            // 새 입력 포트 생성
            if (ValueProcessorRegistry.Instance.Infos.TryGetValue(ProcessorName, out var info))
            {
                for (int i = 0; i < info.InputCount; i++)
                {
                    var portName = info.InputNames != null && i < info.InputNames.Length 
                        ? info.InputNames[i] 
                        : ((char)('A' + i)).ToString();
                    
                    var port = CreateInputPort(portName, typeof(float), Port.Capacity.Single, Color.gray);
                    Inputs.Add(port);
                }
                
                // 노드 색상 업데이트
                titleContainer.style.backgroundColor = new StyleColor(info.Color * 0.7f);
            }
            
            RefreshExpandedState();
            RefreshPorts();
        }
        
        public void SetProcessor(string processorName)
        {
            ProcessorName = processorName;
            if (_dropdown != null)
            {
                _dropdown.value = processorName;
            }
            RebuildInputPorts();
            UpdateTitle();
        }
        
        private void UpdateTitle()
        {
            title = $"Process: {ProcessorName}";
        }
    }
}
