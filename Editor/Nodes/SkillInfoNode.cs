using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public sealed class SkillInfoNode : BaseNode
    {
        // 스킬 정보
        public string SkillName;
        public string Description;
        public Sprite Icon;
        // public int DefaultValue;
        public int PipelineCount = 1;

        // 파이프라인 포트
        public Port Pipeline1Port;
        public Port Pipeline2Port;
        public Port Pipeline3Port;

        private VisualElement _pipeline2Container;
        private VisualElement _pipeline3Container;
        
        private ISkillEditorConfig _config;

        public SkillInfoNode(SkillGraphView graphView) : base(graphView, "📋 Skill Info", Color.cornsilk)
        {
            _config = SkillEditorConfig.Default;
            
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;

            // 아이콘 프리뷰
            var preview = new Image
            {
                style =
                {
                    width = 200,
                    height = 200,
                    marginBottom = 10,
                    alignSelf = Align.Center
                },
                scaleMode = ScaleMode.ScaleToFit
            };
            mainContainer.Add(preview);

            // 아이콘 필드
            var iconField = new ObjectField("Icon") { objectType = typeof(Sprite) };
            iconField.RegisterValueChangedCallback(evt =>
            {
                Icon = evt.newValue as Sprite;
                preview.image = Icon?.texture;
            });
            mainContainer.Add(iconField);

            // 이름
            var nameField = new TextField("Name") { value = "" };
            nameField.RegisterValueChangedCallback(evt => SkillName = evt.newValue);
            mainContainer.Add(nameField);

            // 설명
            var descField = new TextField("Description") { value = "", multiline = true };
            descField.style.minHeight = 40;
            descField.RegisterValueChangedCallback(evt => Description = evt.newValue);
            mainContainer.Add(descField);

            // 기본값
            // var valueField = new IntegerField("Default Value") { value = 0 };
            // valueField.RegisterValueChangedCallback(evt => DefaultValue = evt.newValue);
            // mainContainer.Add(valueField);

            // 파이프라인 개수
            var rangeSlider = new SliderInt("Pipelines", 1, _config.MaxPipelines) { value = 1 };
            rangeSlider.RegisterValueChangedCallback(evt =>
            {
                PipelineCount = evt.newValue;
                UpdatePipelineVisibility();
            });
            mainContainer.Add(rangeSlider);

            // 구분선
            mainContainer.Add(new VisualElement
            {
                style = { height = 2, backgroundColor = Color.gray, marginTop = 10, marginBottom = 10 }
            });

            mainContainer.Add(new Label("Pipelines") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            // 파이프라인 포트들
            var pipelineColor = new Color(0.5f, 1f, 0.5f);
            var listener = new SkillEdgeConnectorListener(graphView, graphView.SearchWindow);
            
            Pipeline1Port = CreateOutputPort(_config.GetPipelineSlotName(0), typeof(Pipeline), Port.Capacity.Single, pipelineColor);
            
            _pipeline2Container = new VisualElement { style = { display = DisplayStyle.None } };
            Pipeline2Port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Pipeline));
            Pipeline2Port.portName = _config.GetPipelineSlotName(1);
            Pipeline2Port.portColor = pipelineColor;
            Pipeline2Port.AddManipulator(new EdgeConnector<Edge>(listener));
            _pipeline2Container.Add(Pipeline2Port);
            outputContainer.Add(_pipeline2Container);
            
            _pipeline3Container = new VisualElement { style = { display = DisplayStyle.None } };
            Pipeline3Port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Pipeline));
            Pipeline3Port.portName = _config.GetPipelineSlotName(2);
            Pipeline3Port.portColor = pipelineColor;
            Pipeline3Port.AddManipulator(new EdgeConnector<Edge>(listener));
            _pipeline3Container.Add(Pipeline3Port);
            outputContainer.Add(_pipeline3Container);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void UpdatePipelineVisibility()
        {
            _pipeline2Container.style.display = PipelineCount >= 2 ? DisplayStyle.Flex : DisplayStyle.None;
            _pipeline3Container.style.display = PipelineCount >= 3 ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshPorts();
        }

        public void LoadData(SkillGraphData data)
        {
            SkillName = data.skillName;
            Description = data.description;
            Icon = data.icon;
            // DefaultValue = data.defaultValue;
            PipelineCount = data.pipelineCount;
            UpdatePipelineVisibility();
        }
    }

    // 파이프라인 타입 마커
    public class Pipeline { }
}
