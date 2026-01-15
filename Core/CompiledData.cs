using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 컴파일된 파이프라인
    /// </summary>
    [System.Serializable]
    public class CompiledPipeline
    {
        public List<CompiledAction> actions = new List<CompiledAction>();
    }
    
    /// <summary>
    /// 컴파일된 액션
    /// </summary>
    [System.Serializable]
    public class CompiledAction
    {
        public ActionType actionType;
        public string behaviourName;
        
        [SerializeReference]
        public CompiledValue value;
        
        public EntitySource selfSource;
        public EntitySource targetSource;
        
        [SerializeReference] 
        public CompiledCondition condition;
        public int jumpIfFalse;
    }
    
    /// <summary>
    /// 컴파일된 조건
    /// </summary>
    [System.Serializable]
    public class CompiledCondition
    {
        public ConditionType conditionType;
        public CompareType compareType;
        
        [SerializeReference] public CompiledValue valueA;
        [SerializeReference] public CompiledValue valueB;
        [SerializeReference] public CompiledCondition left;
        [SerializeReference] public CompiledCondition right;
    }
    
    /// <summary>
    /// 컴파일된 값
    /// </summary>
    [System.Serializable]
    public class CompiledValue
    {
        public ValueType type;
        public float constantValue;
        public EntitySource entitySource;
        public string propertyName;
        
        // Math 연산용
        public MathType mathType;
        
        // ValueProcessor용
        public string processorName;
        
        // Variable용
        public string variableName;
        
        // 입력값들 (Math, Processor 공용)
        [SerializeReference] public CompiledValue[] inputs;
    }
    
    public enum ActionType { SkillBehaviour, Branch }
    public enum EntitySource { Self, Target }
    public enum ConditionType { AlwaysTrue, AlwaysFalse, And, Or, Not, Compare }
    public enum CompareType { Equal, Greater, Less, GreaterOrEqual, LessOrEqual }
    public enum ValueType { Constant, EntityProperty, Math, Processor, Variable }
}
