using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public abstract class BaseNode : Node
    {
        public string Guid { get; set; }
        protected SkillGraphView GraphView { get; }

        protected BaseNode(SkillGraphView graphView, string title, Color color)
        {
            GraphView = graphView;
            Guid = System.Guid.NewGuid().ToString();
            this.title = title;
            
            titleContainer.style.backgroundColor = new StyleColor(color);
        }

        protected Port CreateInputPort(string name, Type type, Port.Capacity capacity, Color color)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            port.portName = name;
            port.portColor = color;
            
            // Edge Connector 추가 (드래그해서 놓으면 서치 윈도우)
            var listener = new SkillEdgeConnectorListener(GraphView, GraphView.SearchWindow);
            port.AddManipulator(new EdgeConnector<Edge>(listener));
            
            inputContainer.Add(port);
            return port;
        }

        protected Port CreateOutputPort(string name, Type type, Port.Capacity capacity, Color color)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, type);
            port.portName = name;
            port.portColor = color;
            
            // Edge Connector 추가
            var listener = new SkillEdgeConnectorListener(GraphView, GraphView.SearchWindow);
            port.AddManipulator(new EdgeConnector<Edge>(listener));
            
            outputContainer.Add(port);
            return port;
        }
    }
}
