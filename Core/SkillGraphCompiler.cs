using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 스킬 그래프 컴파일러
    /// </summary>
    public static class SkillGraphCompiler
    {
        private static HashSet<string> _visitedNodes;
        private static ISkillEditorConfig _config;
        private static ISkillEditorConfig Config => _config ??= SkillEditorConfig.Default;

        public static void Compile(SkillGraphData data, ISkillEditorConfig config = null)
        {
            _config = config ?? SkillEditorConfig.Default;
            
            var infoNode = data.nodes.OfType<SkillInfoNodeData>().FirstOrDefault();
            if (infoNode == null)
            {
                Debug.LogWarning("SkillInfo 노드가 없습니다.");
                return;
            }

            data.pipeline1 = CompilePipeline(data, infoNode.guid, Config.GetPipelineSlotName(0));
            data.pipeline2 = data.pipelineCount >= 2
                ? CompilePipeline(data, infoNode.guid, Config.GetPipelineSlotName(1))
                : null;
            data.pipeline3 = data.pipelineCount >= 3
                ? CompilePipeline(data, infoNode.guid, Config.GetPipelineSlotName(2))
                : null;

            Debug.Log($"컴파일 완료: {data.skillName} (Pipeline: {data.pipelineCount}개)");
        }

        private static CompiledPipeline CompilePipeline(SkillGraphData data, string infoGuid, string portName)
        {
            var pipeline = new CompiledPipeline();
            _visitedNodes = new HashSet<string>();
            
            var slotEdge = data.edges.FirstOrDefault(e =>
                e.outputNodeGuid == infoGuid && e.outputPortName == portName);
            if (slotEdge == null)
            {
                Debug.LogWarning($"{portName}에 연결된 노드가 없습니다.");
                return pipeline;
            }
            
            var contextNode = data.nodes.FirstOrDefault(n => n.guid == slotEdge.inputNodeGuid);
            if (contextNode is not ContextNodeData)
            {
                Debug.LogWarning($"{portName}에 연결된 노드가 Context가 아닙니다.");
                return pipeline;
            }
            
            CompileFromContext(data, contextNode.guid, pipeline);
            return pipeline;
        }

        private static void CompileFromContext(SkillGraphData data, string contextGuid, CompiledPipeline pipeline)
        {
            var outEdges = data.edges.Where(e => e.outputNodeGuid == contextGuid).ToList();
            foreach (var edge in outEdges)
            {
                CompileNode(data, edge.inputNodeGuid, pipeline);
            }
        }

        private static void CompileNode(SkillGraphData data, string nodeGuid, CompiledPipeline pipeline)
        {
            if (_visitedNodes.Contains(nodeGuid)) return;
            _visitedNodes.Add(nodeGuid);

            var node = data.nodes.FirstOrDefault(n => n.guid == nodeGuid);
            if (node == null) return;

            switch (node)
            {
                case SkillBehaviourNodeData skill:
                    CompileSkillBehaviour(data, skill, pipeline);
                    break;
                case BranchNodeData branch:
                    CompileBranch(data, branch, pipeline);
                    break;
            }
        }

        private static void CompileSkillBehaviour(SkillGraphData data, SkillBehaviourNodeData skill, CompiledPipeline pipeline)
        {
            pipeline.actions.Add(new CompiledAction
            {
                actionType = ActionType.SkillBehaviour,
                behaviourName = skill.behaviourName,
                value = CompileSkillValue(data, skill),
                selfSource = TraceEntitySource(data, skill.guid, Config.SelfPortName),
                targetSource = TraceEntitySource(data, skill.guid, Config.TargetPortName)
            });
            PropagateToNext(data, skill.guid, pipeline);
        }
        
        private static CompiledValue CompileSkillValue(SkillGraphData data, SkillBehaviourNodeData skill)
        {
            // Value 포트에 연결된 엣지 찾기
            var edge = data.edges.FirstOrDefault(e =>
                e.inputNodeGuid == skill.guid && e.inputPortName == "Value");
            
            // 연결 없으면 상수 사용
            if (edge == null)
                return new CompiledValue { type = ValueType.Constant, constantValue = skill.value };
            
            // 연결된 노드에서 값 가져오기
            var sourceNode = data.nodes.FirstOrDefault(n => n.guid == edge.outputNodeGuid);
            
            if (sourceNode is ConstantNodeData constant)
                return new CompiledValue { type = ValueType.Constant, constantValue = constant.value };
            
            if (sourceNode is GetPropertyNodeData prop)
            {
                var entitySource = TraceEntitySource(data, prop.guid, "Entity");
                return new CompiledValue
                {
                    type = ValueType.EntityProperty,
                    entitySource = entitySource,
                    propertyName = prop.propertyName
                };
            }
            
            if (sourceNode is MathNodeData math)
            {
                return new CompiledValue
                {
                    type = ValueType.Math,
                    mathType = math.mathType,
                    inputs = new[]
                    {
                        CompileValue(data, math.guid, "A"),
                        CompileValue(data, math.guid, "B")
                    }
                };
            }
            
            if (sourceNode is ValueProcessorNodeData proc)
            {
                return CompileProcessorValue(data, proc);
            }
            
            return new CompiledValue { type = ValueType.Constant, constantValue = skill.value };
        }

        private static void CompileBranch(SkillGraphData data, BranchNodeData branch, CompiledPipeline pipeline)
        {
            var branchAction = new CompiledAction
            {
                actionType = ActionType.Branch,
                condition = CompileCondition(data, branch.guid),
                selfSource = TraceEntitySource(data, branch.guid, Config.SelfPortName),
                targetSource = TraceEntitySource(data, branch.guid, Config.TargetPortName),
                jumpIfFalse = -1
            };
            
            int branchIndex = pipeline.actions.Count;
            pipeline.actions.Add(branchAction);
            PropagateToNext(data, branch.guid, pipeline);
            pipeline.actions[branchIndex].jumpIfFalse = pipeline.actions.Count;
        }

        private static EntitySource TraceEntitySource(SkillGraphData data, string guid, string inputPortName)
        {
            var edge = data.edges.FirstOrDefault(e =>
                e.inputNodeGuid == guid && e.inputPortName == inputPortName);
            if (edge == null) return EntitySource.Self;
            return TraceToContext(data, edge.outputNodeGuid, edge.outputPortName);
        }

        private static EntitySource TraceToContext(SkillGraphData data, string guid, string outPortName)
        {
            var node = data.nodes.FirstOrDefault(n => n.guid == guid);
            if (node is ContextNodeData)
                return outPortName == Config.ContextSelfPortName ? EntitySource.Self : EntitySource.Target;
            
            var inputPort = outPortName == Config.SelfPortName ? Config.SelfPortName : 
                           outPortName == Config.TargetPortName ? Config.TargetPortName : outPortName;
            
            var edge = data.edges.FirstOrDefault(e =>
                e.inputNodeGuid == guid && e.inputPortName == inputPort);
            return edge != null ? TraceToContext(data, edge.outputNodeGuid, edge.outputPortName) : EntitySource.Self;
        }

        private static void PropagateToNext(SkillGraphData data, string guid, CompiledPipeline pipeline)
        {
            foreach (var edge in data.edges.Where(e => e.outputNodeGuid == guid))
            {
                CompileNode(data, edge.inputNodeGuid, pipeline);
            }
        }

        private static CompiledCondition CompileCondition(SkillGraphData data, string guid)
        {
            var edge = data.edges.FirstOrDefault(e =>
                e.inputNodeGuid == guid && e.inputPortName == Config.ConditionPortName);
            
            return edge == null 
                ? new CompiledCondition { conditionType = ConditionType.AlwaysTrue }
                : CompileConditionNode(data, edge.outputNodeGuid);
        }

        private static CompiledCondition CompileConditionNode(SkillGraphData data, string guid)
        {
            var node = data.nodes.FirstOrDefault(n => n.guid == guid);
            if (node == null)
                return new CompiledCondition { conditionType = ConditionType.AlwaysTrue };

            return node switch
            {
                LogicalNodeData logical => new CompiledCondition
                {
                    conditionType = logical.conditionType,
                    left = CompileConditionInput(data, guid, "A"),
                    right = CompileConditionInput(data, guid, "B")
                },
                ComparisonNodeData comparison => new CompiledCondition
                {
                    conditionType = ConditionType.Compare,
                    compareType = comparison.compareType,
                    valueA = CompileValue(data, guid, "A"),
                    valueB = CompileValue(data, guid, "B")
                },
                _ => new CompiledCondition { conditionType = ConditionType.AlwaysTrue }
            };
        }

        private static CompiledCondition CompileConditionInput(SkillGraphData data, string guid, string portName)
        {
            var edge = data.edges.FirstOrDefault(e =>
                e.inputNodeGuid == guid && e.inputPortName == portName);
            return edge == null 
                ? new CompiledCondition { conditionType = ConditionType.AlwaysTrue }
                : CompileConditionNode(data, edge.outputNodeGuid);
        }

        private static CompiledValue CompileValue(SkillGraphData data, string guid, string portName)
        {
            var edge = data.edges.FirstOrDefault(e =>
                e.inputNodeGuid == guid && e.inputPortName == portName);
            
            if (edge == null)
                return new CompiledValue { type = ValueType.Constant, constantValue = 0 };

            var sourceNode = data.nodes.FirstOrDefault(n => n.guid == edge.outputNodeGuid);
            
            if (sourceNode is ConstantNodeData constant)
                return new CompiledValue { type = ValueType.Constant, constantValue = constant.value };
            
            if (sourceNode is GetPropertyNodeData prop)
            {
                var entitySource = TraceEntitySource(data, prop.guid, "Entity");
                return new CompiledValue
                {
                    type = ValueType.EntityProperty,
                    entitySource = entitySource,
                    propertyName = prop.propertyName
                };
            }
            
            if (sourceNode is MathNodeData math)
            {
                return new CompiledValue
                {
                    type = ValueType.Math,
                    mathType = math.mathType,
                    inputs = new[]
                    {
                        CompileValue(data, math.guid, "A"),
                        CompileValue(data, math.guid, "B")
                    }
                };
            }
            
            if (sourceNode is ValueProcessorNodeData proc)
            {
                return CompileProcessorValue(data, proc);
            }
            
            return new CompiledValue { type = ValueType.Constant, constantValue = 0 };
        }
        
        private static CompiledValue CompileProcessorValue(SkillGraphData data, ValueProcessorNodeData proc)
        {
            if (!ValueProcessorRegistry.Instance.Infos.TryGetValue(proc.processorName, out var info))
                return new CompiledValue { type = ValueType.Constant, constantValue = 0 };
            
            var inputs = new CompiledValue[info.InputCount];
            for (int i = 0; i < info.InputCount; i++)
            {
                var portName = info.InputNames != null && i < info.InputNames.Length 
                    ? info.InputNames[i] 
                    : ((char)('A' + i)).ToString();
                inputs[i] = CompileValue(data, proc.guid, portName);
            }
            
            return new CompiledValue
            {
                type = ValueType.Processor,
                processorName = proc.processorName,
                inputs = inputs
            };
        }
    }
}
