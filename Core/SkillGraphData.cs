using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 스킬 그래프 데이터 (ScriptableObject)
    /// 게임별로 상속받아 확장 가능
    /// </summary>
    [CreateAssetMenu(fileName = "SkillGraphData", menuName = "SkillEditor/SkillGraphData")]
    public class SkillGraphData : ScriptableObject
    {
        [Header("Skill Info")]
        public string skillName;
        public string description;
        public Sprite icon;
        // public int defaultValue;
        [Range(1, 3)] public int pipelineCount = 1;
        
        [Header("Editor Data")]
        [SerializeReference]
        public List<BaseNodeData> nodes = new List<BaseNodeData>();
        public List<EdgeData> edges = new List<EdgeData>();
        
        [Header("Compiled Data")]
        public CompiledPipeline pipeline1;
        public CompiledPipeline pipeline2;
        public CompiledPipeline pipeline3;
        
        public CompiledPipeline GetPipeline(int index)
        {
            return index switch
            {
                0 => pipeline1,
                1 => pipeline2,
                2 => pipeline3,
                _ => null
            };
        }
        
        /// <summary>
        /// 파이프라인 실행
        /// </summary>
        public void Execute(int pipelineIndex, ISkillContext context, ISkillBehaviourRegistry registry = null)
        {
            var pipeline = GetPipeline(pipelineIndex);
            PipelineExecutor.Execute(pipeline, context, registry ?? SkillBehaviourRegistry.Instance);
        }
    }
}
