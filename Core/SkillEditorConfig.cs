using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 에디터 설정 인터페이스
    /// </summary>
    public interface ISkillEditorConfig
    {
        string SelfPortName { get; }
        string TargetPortName { get; }
        string ConditionPortName { get; }
        string ContextSelfPortName { get; }
        string ContextTargetPortName { get; }
        int MaxPipelines { get; }
        string GetPipelineSlotName(int index);
    }
    
    /// <summary>
    /// 기본 설정 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "SkillEditorConfig", menuName = "SkillEditor/Config")]
    public class SkillEditorConfig : ScriptableObject, ISkillEditorConfig
    {
        [Header("Port Names")]
        [SerializeField] private string selfPortName = "시전자";
        [SerializeField] private string targetPortName = "대상";
        [SerializeField] private string conditionPortName = "조건";
        [SerializeField] private string contextSelfPortName = "Self";
        [SerializeField] private string contextTargetPortName = "Target";
        
        [Header("Pipeline")]
        [SerializeField] private int maxPipelines = 3;
        [SerializeField] private string pipelineSlotFormat = "슬롯 {0}";
        
        public string SelfPortName => selfPortName;
        public string TargetPortName => targetPortName;
        public string ConditionPortName => conditionPortName;
        public string ContextSelfPortName => contextSelfPortName;
        public string ContextTargetPortName => contextTargetPortName;
        public int MaxPipelines => maxPipelines;
        public string GetPipelineSlotName(int index) => string.Format(pipelineSlotFormat, index + 1);
        
        private static SkillEditorConfig _default;
        public static ISkillEditorConfig Default => _default ??= CreateInstance<SkillEditorConfig>();
    }
}
