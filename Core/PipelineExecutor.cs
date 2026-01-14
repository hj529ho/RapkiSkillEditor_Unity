using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 컴파일된 파이프라인 실행기
    /// </summary>
    public static class PipelineExecutor
    {
        public static void Execute(CompiledPipeline pipeline, ISkillContext context, ISkillBehaviourRegistry registry)
        {
            if (pipeline?.actions == null || pipeline.actions.Count == 0)
            {
                Debug.LogWarning("Pipeline is empty or null");
                return;
            }
            
            for (int i = 0; i < pipeline.actions.Count; i++)
            {
                var action = pipeline.actions[i];
                switch (action.actionType)
                {
                    case ActionType.SkillBehaviour:
                        ExecuteSkillBehaviour(action, context, registry);
                        break;
                    case ActionType.Branch:
                        if (!EvaluateCondition(action.condition, context))
                            i = action.jumpIfFalse - 1;
                        break;
                }
            }
        }
        
        private static void ExecuteSkillBehaviour(CompiledAction action, ISkillContext context, ISkillBehaviourRegistry registry)
        {
            var execContext = new SkillContext
            {
                Self = action.selfSource == EntitySource.Self ? context.Self : context.Target,
                Target = action.targetSource == EntitySource.Self ? context.Self : context.Target
            };
            
            var behaviour = registry.Get(action.behaviourName);
            if (behaviour != null)
            {
                var value = (int)GetValue(action.value, context);
                behaviour.Execute(execContext, value);
            }
            else
                Debug.LogWarning($"SkillBehaviour '{action.behaviourName}' not found");
        }
        
        private static bool EvaluateCondition(CompiledCondition cond, ISkillContext context)
        {
            if (cond == null) return true;
            
            return cond.conditionType switch
            {
                ConditionType.AlwaysTrue => true,
                ConditionType.AlwaysFalse => false,
                ConditionType.And => EvaluateCondition(cond.left, context) && EvaluateCondition(cond.right, context),
                ConditionType.Or => EvaluateCondition(cond.left, context) || EvaluateCondition(cond.right, context),
                ConditionType.Not => !EvaluateCondition(cond.left, context),
                ConditionType.Compare => EvaluateCompare(cond, context),
                _ => true
            };
        }
        
        private static bool EvaluateCompare(CompiledCondition cond, ISkillContext context)
        {
            var a = GetValue(cond.valueA, context);
            var b = GetValue(cond.valueB, context);
            
            return cond.compareType switch
            {
                CompareType.Equal => Mathf.Approximately(a, b),
                CompareType.Greater => a > b,
                CompareType.Less => a < b,
                CompareType.GreaterOrEqual => a >= b,
                CompareType.LessOrEqual => a <= b,
                _ => false
            };
        }
        
        private static float GetValue(CompiledValue val, ISkillContext context)
        {
            if (val == null) return 0f;
            
            return val.type switch
            {
                ValueType.Constant => val.constantValue,
                ValueType.EntityProperty => GetEntityProperty(val, context),
                ValueType.Math => EvaluateMath(val, context),
                ValueType.Processor => EvaluateProcessor(val, context),
                _ => 0f
            };
        }
        
        private static float GetEntityProperty(CompiledValue val, ISkillContext context)
        {
            var entity = val.entitySource == EntitySource.Self ? context.Self : context.Target;
            return PropertyAccessorRegistry.Instance.GetValue(val.propertyName, entity);
        }
        
        private static float EvaluateMath(CompiledValue val, ISkillContext context)
        {
            if (val.inputs == null || val.inputs.Length < 2) return 0f;
            
            var a = GetValue(val.inputs[0], context);
            var b = GetValue(val.inputs[1], context);
            
            return val.mathType switch
            {
                MathType.Add => a + b,
                MathType.Subtract => a - b,
                MathType.Multiply => a * b,
                MathType.Divide => b != 0 ? a / b : 0f,
                _ => 0f
            };
        }
        
        private static float EvaluateProcessor(CompiledValue val, ISkillContext context)
        {
            var processor = ValueProcessorRegistry.Instance.Get(val.processorName);
            if (processor == null) return 0f;
            
            var inputs = new float[val.inputs?.Length ?? 0];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = GetValue(val.inputs[i], context);
            }
            
            return processor.Process(inputs);
        }
    }
}
