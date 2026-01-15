using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 노드 데이터 베이스 클래스
    /// </summary>
    [System.Serializable]
    public abstract class BaseNodeData
    {
        public string guid;
        public Vector2 position;
    }
    
    [System.Serializable]
    public class SkillInfoNodeData : BaseNodeData { }
    
    [System.Serializable]
    public class ContextNodeData : BaseNodeData { }
    
    [System.Serializable]
    public class SkillBehaviourNodeData : BaseNodeData
    {
        public string behaviourName;
        public int value;
    }
    
    [System.Serializable]
    public class BranchNodeData : BaseNodeData { }
    
    [System.Serializable]
    public class LogicalNodeData : BaseNodeData
    {
        public ConditionType conditionType;
    }
    
    [System.Serializable]
    public class ComparisonNodeData : BaseNodeData
    {
        public CompareType compareType;
    }
    
    [System.Serializable]
    public class ConstantNodeData : BaseNodeData
    {
        public float value;
    }
    
    [System.Serializable]
    public class VariableNodeData : BaseNodeData
    {
        public string variableName;
        public VariableType variableType;
        public int defaultIntValue;
        public float defaultFloatValue;
        public int intValue;
        public float floatValue;
    }
    
    /// <summary>
    /// 엣지 데이터
    /// </summary>
    [System.Serializable]
    public class EdgeData
    {
        public string outputNodeGuid;
        public string outputPortName;
        public string inputNodeGuid;
        public string inputPortName;
    }
}
