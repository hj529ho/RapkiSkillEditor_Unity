using System.Collections.Generic;
using System.Linq;
using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class GetPropertyNode : BaseNode
    {
        public Port EntityInput;
        public Port ValueOutput;
        
        public string PropertyName;
        public EntitySource EntitySource = EntitySource.Self;
        public PopupField<string> PropertyDropdown;
        
        private Label _entitySourceLabel;

        public GetPropertyNode(SkillGraphView graphView) 
            : base(graphView, "Get Property", new Color(0.6f, 0.4f, 0.8f))
        {
            // Entity 입력 (Self 또는 Target에서 연결)
            EntityInput = CreateInputPort("Entity", typeof(Entity), Port.Capacity.Single, Color.gray);
            
            // Value 출력 (Compare 등에 연결)
            ValueOutput = CreateOutputPort("Value", typeof(float), Port.Capacity.Multi, Color.gray);

            // Property 선택 드롭다운
            var properties = GetPropertyNames();
            if (properties.Count > 0)
            {
                PropertyName = properties[0];
                PropertyDropdown = new PopupField<string>("Property", properties, 0);
                PropertyDropdown.RegisterValueChangedCallback(evt =>
                {
                    PropertyName = evt.newValue;
                    UpdateTitle();
                });
                mainContainer.Add(PropertyDropdown);
            }
            else
            {
                mainContainer.Add(new Label("No properties registered"));
            }
            
            // 소스 라벨
            _entitySourceLabel = new Label("Entity: (연결 없음)");
            _entitySourceLabel.style.fontSize = 10;
            _entitySourceLabel.style.color = Color.gray;
            mainContainer.Add(_entitySourceLabel);
            
            UpdateTitle();
            RefreshExpandedState();
            RefreshPorts();
        }
        
        private List<string> GetPropertyNames()
        {
            return PropertyAccessorRegistry.Instance.All.Keys.ToList();
        }
        
        public void SetProperty(string propertyName)
        {
            PropertyName = propertyName;
            if (PropertyDropdown != null)
            {
                PropertyDropdown.value = propertyName;
            }
            UpdateTitle();
        }
        
        public void UpdateEntitySource(EntitySource source)
        {
            EntitySource = source;
            UpdatePortColors();
            UpdateLabels();
            UpdateTitle();
        }
        
        private void UpdatePortColors()
        {
            EntityInput.portColor = NodeUtils.GetSourceColor(EntitySource);
        }
        
        private void UpdateLabels()
        {
            _entitySourceLabel.text = $"Entity: {NodeUtils.GetSourceText(EntitySource)}";
            _entitySourceLabel.style.color = NodeUtils.GetSourceColor(EntitySource);
        }
        
        private void UpdateTitle()
        {
            var sourceText = EntitySource == EntitySource.Self ? "Self" : "Target";
            title = $"Get: {sourceText}.{PropertyName}";
        }
    }
}
