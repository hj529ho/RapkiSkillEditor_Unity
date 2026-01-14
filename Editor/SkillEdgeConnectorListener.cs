using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 엣지 드래그 시 동작 처리
    /// - 빈 공간에 드롭하면 서치 윈도우 열림
    /// - 포트에 드롭하면 연결
    /// </summary>
    public class SkillEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly SkillGraphView _view;
        private readonly SkillGraphSearchWindow _search;

        public SkillEdgeConnectorListener(SkillGraphView view, SkillGraphSearchWindow search)
        {
            _view = view;
            _search = search;
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            // Single input이면 기존 연결 먼저 제거
            if (edge.input != null && edge.input.capacity == Port.Capacity.Single)
            {
                var olds = edge.input.connections.ToList();
                foreach (var old in olds)
                {
                    if (old == edge) continue;
                    graphView.DeleteElements(new List<GraphElement> { old });
                }
            }
            
            // 포트 연결 갱신
            edge.output?.Connect(edge);
            edge.input?.Connect(edge);
            
            // 엣지 추가
            graphView.AddElement(edge);
            
            // 소스 갱신
            if (graphView is SkillGraphView view)
                view.NotifyEdgeCreated(edge);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            // 빈 공간에 드롭하면 서치 윈도우 열기
            var world = _view.contentViewContainer.LocalToWorld(position);
            var screen = GUIUtility.GUIToScreenPoint(world);

            _search.SetDraggedPort(edge.output ?? edge.input);
            SearchWindow.Open(new SearchWindowContext(screen), _search);
        }
    }
}
