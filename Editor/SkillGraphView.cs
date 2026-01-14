using System;
using System.Collections.Generic;
using System.Linq;
using SkillEditor.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class SkillGraphView : GraphView
    {
        public SkillGraphWindow Window { get; }
        public SkillInfoNode InfoNode { get; private set; }
        
        private SkillGraphSearchWindow _searchWindow;
        public SkillGraphSearchWindow SearchWindow => _searchWindow;

        public SkillGraphView(SkillGraphWindow window)
        {
            Window = window;
            
            // 배경 그리드
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            
            // 기본 조작
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            // 스타일
            var style = Resources.Load<StyleSheet>("SkillGraphStyle");
            if (style != null)
                styleSheets.Add(style);
            
            // 검색 윈도우
            _searchWindow = ScriptableObject.CreateInstance<SkillGraphSearchWindow>();
            _searchWindow.Initialize(this);
            
            // 스페이스바로 검색창 열기
            this.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Space)
                    OpenSearchWindow();
            });
            
            nodeCreationRequest = ctx =>
            {
                UnityEditor.Experimental.GraphView.SearchWindow.Open(
                    new SearchWindowContext(ctx.screenMousePosition), _searchWindow);
            };
            
            // 우클릭 메뉴
            var menuManipulator = new ContextualMenuManipulator(BuildContextMenu);
            this.AddManipulator(menuManipulator);
            
            graphViewChanged += OnGraphViewChanged;
            
            // 기본 노드 생성
            CreateDefaultNodes();
        }
        
        private void OpenSearchWindow()
        {
            _searchWindow.SetDraggedPort(null);
            var screenPos = GUIUtility.GUIToScreenPoint(Event.current?.mousePosition ?? Vector2.zero);
            UnityEditor.Experimental.GraphView.SearchWindow.Open(
                new SearchWindowContext(screenPos), _searchWindow);
        }
        
        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("노드 추가", action =>
            {
                var screenPos = GUIUtility.GUIToScreenPoint(action.eventInfo.mousePosition);
                UnityEditor.Experimental.GraphView.SearchWindow.Open(
                    new SearchWindowContext(screenPos), _searchWindow);
            });
            
            // 스킬 목록
            foreach (var info in SkillBehaviourRegistry.Instance.Infos.Values)
            {
                evt.menu.AppendAction($"효과/{info.Name}", action =>
                {
                    var node = new SkillBehaviourNode(this, info.Name);
                    var mousePos = action.eventInfo.localMousePosition;
                    node.SetPosition(new Rect(mousePos.x, mousePos.y, 180, 150));
                    AddElement(node);
                });
            }
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            evt.menu.AppendAction("노드 추가", action =>
            {
                var screenPos = GUIUtility.GUIToScreenPoint(action.eventInfo.mousePosition);
                UnityEditor.Experimental.GraphView.SearchWindow.Open(
                    new SearchWindowContext(screenPos), _searchWindow);
            });
        }

        private void CreateDefaultNodes()
        {
            InfoNode = new SkillInfoNode(this);
            InfoNode.SetPosition(new Rect(100, 200, 280, 400));
            AddElement(InfoNode);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node &&
                p.portType == startPort.portType
            ).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    NotifyEdgeCreated(edge);
                }
            }
            return change;
        }

        public void NotifyEdgeCreated(Edge edge)
        {
            UpdateSourceFromEdge(edge);
        }
        
        public void NotifyEdgeRemoved(Edge edge)
        {
            ClearSourceFromEdge(edge);
        }
        
        private void UpdateSourceFromEdge(Edge edge)
        {
            var inputPort = edge.input;
            var outputPort = edge.output;
            
            var source = GetSourceFromOutputPort(outputPort);
            
            if (inputPort?.node is SkillBehaviourNode skillNode)
            {
                if (inputPort == skillNode.SelfInput)
                    skillNode.UpdateSelfSource(source);
                else if (inputPort == skillNode.TargetInput)
                    skillNode.UpdateTargetSource(source);
                else if (inputPort == skillNode.ValueInput)
                    skillNode.SetValueConnected(true);
                    
                PropagateSourceUpdate(skillNode);
            }
            else if (inputPort?.node is BranchNode branchNode)
            {
                if (inputPort == branchNode.SelfInput)
                    branchNode.UpdateSelfSource(source);
                else if (inputPort == branchNode.TargetInput)
                    branchNode.UpdateTargetSource(source);
                    
                PropagateSourceUpdateFromBranch(branchNode);
            }
            else if (inputPort?.node is GetPropertyNode propNode)
            {
                if (inputPort == propNode.EntityInput)
                    propNode.UpdateEntitySource(source);
            }
        }
        
        private void ClearSourceFromEdge(Edge edge)
        {
            var inputPort = edge.input;
            
            if (inputPort?.node is SkillBehaviourNode skillNode)
            {
                if (inputPort == skillNode.SelfInput)
                    skillNode.UpdateSelfSource(Core.EntitySource.Self);
                else if (inputPort == skillNode.TargetInput)
                    skillNode.UpdateTargetSource(Core.EntitySource.Target);
                else if (inputPort == skillNode.ValueInput)
                    skillNode.SetValueConnected(false);
                    
                PropagateSourceUpdate(skillNode);
            }
            else if (inputPort?.node is BranchNode branchNode)
            {
                if (inputPort == branchNode.SelfInput)
                    branchNode.UpdateSelfSource(Core.EntitySource.Self);
                else if (inputPort == branchNode.TargetInput)
                    branchNode.UpdateTargetSource(Core.EntitySource.Target);
                    
                PropagateSourceUpdateFromBranch(branchNode);
            }
            else if (inputPort?.node is GetPropertyNode propNode)
            {
                if (inputPort == propNode.EntityInput)
                    propNode.UpdateEntitySource(Core.EntitySource.Self);
            }
        }
        
        private Core.EntitySource GetSourceFromOutputPort(Port outputPort)
        {
            if (outputPort == null) return Core.EntitySource.Self;
            
            if (outputPort.node is ContextNode contextNode)
            {
                if (outputPort == contextNode.SelfOutput) return Core.EntitySource.Self;
                if (outputPort == contextNode.TargetOutput) return Core.EntitySource.Target;
            }
            else if (outputPort.node is SkillBehaviourNode skillNode)
            {
                if (outputPort == skillNode.SelfOutput) return skillNode.SelfSource;
                if (outputPort == skillNode.TargetOutput) return skillNode.TargetSource;
            }
            else if (outputPort.node is BranchNode branchNode)
            {
                if (outputPort == branchNode.SelfOutput) return branchNode.SelfSource;
                if (outputPort == branchNode.TargetOutput) return branchNode.TargetSource;
            }
            
            return Core.EntitySource.Self;
        }
        
        private void PropagateSourceUpdate(SkillBehaviourNode node)
        {
            foreach (var edge in edges.ToList())
            {
                if (edge.output?.node != node) continue;
                UpdateSourceFromEdge(edge);
            }
        }
        
        private void PropagateSourceUpdateFromBranch(BranchNode node)
        {
            foreach (var edge in edges.ToList())
            {
                if (edge.output?.node != node) continue;
                UpdateSourceFromEdge(edge);
            }
        }

        public void ClearGraph()
        {
            foreach (var node in nodes.ToList())
            {
                if (node != InfoNode)
                    RemoveElement(node);
            }
            foreach (var edge in edges.ToList())
            {
                RemoveElement(edge);
            }
        }
    }
}
