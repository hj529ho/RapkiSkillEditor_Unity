using System.Collections.Generic;
using System.Linq;
using SkillEditor.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    public class SkillGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private SkillGraphView _graphView;
        private Port _draggedPort;

        public void Initialize(SkillGraphView graphView)
        {
            _graphView = graphView;
        }
        
        public void SetDraggedPort(Port port)
        {
            _draggedPort = port;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("노드 생성"), 0),
                
                // Context
                new SearchTreeGroupEntry(new GUIContent("컨텍스트"), 1),
                new SearchTreeEntry(new GUIContent("Context")) { level = 2, userData = "Context" },
                
                // 값
                new SearchTreeGroupEntry(new GUIContent("값"), 1),
                new SearchTreeEntry(new GUIContent("Number")) { level = 2, userData = "Number" },
                new SearchTreeEntry(new GUIContent("Get Property")) { level = 2, userData = "GetProperty" },
                new SearchTreeEntry(new GUIContent("Math (+−×÷)")) { level = 2, userData = "Math" },
                new SearchTreeEntry(new GUIContent("Processor")) { level = 2, userData = "Processor" },
                
                // 조건
                new SearchTreeGroupEntry(new GUIContent("조건"), 1),
                new SearchTreeEntry(new GUIContent("Branch")) { level = 2, userData = "Branch" },
                new SearchTreeEntry(new GUIContent("Compare")) { level = 2, userData = "Compare" },
                new SearchTreeEntry(new GUIContent("AND")) { level = 2, userData = "AND" },
                new SearchTreeEntry(new GUIContent("OR")) { level = 2, userData = "OR" },
                new SearchTreeEntry(new GUIContent("NOT")) { level = 2, userData = "NOT" },
                
                // 유틸
                new SearchTreeGroupEntry(new GUIContent("유틸"), 1),
                new SearchTreeEntry(new GUIContent("Comment")) { level = 2, userData = "Comment" },
                
                // 효과
                new SearchTreeGroupEntry(new GUIContent("효과"), 1),
            };

            // 등록된 스킬들 추가
            foreach (var info in SkillBehaviourRegistry.Instance.Infos.Values)
            {
                tree.Add(new SearchTreeEntry(new GUIContent(info.Name))
                {
                    level = 2,
                    userData = $"Skill:{info.Name}"
                });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var worldMousePos = context.screenMousePosition - _graphView.Window.position.position;
            var localMousePos = _graphView.contentViewContainer.WorldToLocal(worldMousePos);

            var data = entry.userData as string;
            if (data == null) return false;

            BaseNode node = null;

            if (data.StartsWith("Skill:"))
            {
                var skillName = data.Substring(6);
                node = new SkillBehaviourNode(_graphView, skillName);
            }
            else
            {
                node = data switch
                {
                    "Context" => new ContextNode(_graphView),
                    "Branch" => new BranchNode(_graphView),
                    "AND" => new AndNode(_graphView),
                    "OR" => new OrNode(_graphView),
                    "NOT" => new NotNode(_graphView),
                    "Compare" => new ComparisonNode(_graphView),
                    "Number" => new ConstantNode(_graphView),
                    "GetProperty" => new GetPropertyNode(_graphView),
                    "Math" => new MathNode(_graphView),
                    "Processor" => new ValueProcessorNode(_graphView),
                    "Comment" => new CommentNode(_graphView),
                    _ => null
                };
            }

            if (node != null)
            {
                node.SetPosition(new Rect(localMousePos, Vector2.zero));
                _graphView.AddElement(node);
                
                // 드래그로 생성한 경우 자동 연결
                if (_draggedPort != null)
                {
                    TryAutoConnect(node);
                    _draggedPort = null;
                }
                
                return true;
            }

            return false;
        }
        
        private void TryAutoConnect(BaseNode node)
        {
            Port targetPort = null;
            
            // Context 노드면 Pipeline 연결
            if (node is ContextNode contextNode && _draggedPort.portType == typeof(Pipeline))
            {
                targetPort = contextNode.PipelineInput;
            }
            // SkillBehaviour 노드면 Entity 연결
            else if (node is SkillBehaviourNode skillNode && _draggedPort.portType == typeof(Entity))
            {
                if (_draggedPort.direction == Direction.Output)
                    targetPort = skillNode.SelfInput;
                else
                    targetPort = skillNode.SelfOutput;
            }
            // Branch 노드
            else if (node is BranchNode branchNode && _draggedPort.portType == typeof(Entity))
            {
                if (_draggedPort.direction == Direction.Output)
                    targetPort = branchNode.SelfInput;
                else
                    targetPort = branchNode.SelfOutput;
            }
            // GetProperty 노드
            else if (node is GetPropertyNode propNode && _draggedPort.portType == typeof(Entity))
            {
                targetPort = propNode.EntityInput;
            }
            
            if (targetPort == null) return;
            
            // 기존 연결 제거 (Single 포트인 경우)
            if (_draggedPort.capacity == Port.Capacity.Single && _draggedPort.connected)
            {
                var olds = _draggedPort.connections.ToList();
                foreach (var old in olds)
                {
                    _graphView.DeleteElements(new List<GraphElement> { old });
                }
            }
            
            // 새 연결 생성
            var edge = _draggedPort.direction == Direction.Output
                ? _draggedPort.ConnectTo(targetPort)
                : targetPort.ConnectTo(_draggedPort);
            
            _graphView.AddElement(edge);
            _graphView.NotifyEdgeCreated(edge);
        }
    }
}
