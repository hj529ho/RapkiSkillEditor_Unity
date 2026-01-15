using System.Collections.Generic;
using System.Linq;
using SkillEditor.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SkillEditor.Editor
{
    public static class SkillGraphSaveLoad
    {
        public static void Save(SkillGraphView graphView, SkillGraphData data)
        {
            data.nodes.Clear();
            data.edges.Clear();

            // 노드 저장
            foreach (var node in graphView.nodes)
            {
                BaseNodeData nodeData = node switch
                {
                    SkillInfoNode info => new SkillInfoNodeData { guid = info.Guid, position = info.GetPosition().position },
                    ContextNode ctx => new ContextNodeData { guid = ctx.Guid, position = ctx.GetPosition().position },
                    SkillBehaviourNode skill => new SkillBehaviourNodeData
                    {
                        guid = skill.Guid,
                        position = skill.GetPosition().position,
                        behaviourName = skill.BehaviourName,
                        value = skill.Value
                    },
                    BranchNode branch => new BranchNodeData { guid = branch.Guid, position = branch.GetPosition().position },
                    AndNode and => new LogicalNodeData { guid = and.Guid, position = and.GetPosition().position, conditionType = ConditionType.And },
                    OrNode or => new LogicalNodeData { guid = or.Guid, position = or.GetPosition().position, conditionType = ConditionType.Or },
                    NotNode not => new LogicalNodeData { guid = not.Guid, position = not.GetPosition().position, conditionType = ConditionType.Not },
                    ComparisonNode comp => new ComparisonNodeData { guid = comp.Guid, position = comp.GetPosition().position, compareType = comp.CompareType },
                    ConstantNode constant => new ConstantNodeData { guid = constant.Guid, position = constant.GetPosition().position, value = constant.Value },
                    VariableNode variable => new VariableNodeData 
                    { 
                        guid = variable.Guid, 
                        position = variable.GetPosition().position, 
                        variableName = variable.VariableName,
                        variableType = variable.VariableType,
                        defaultIntValue = variable.DefaultIntValue,
                        defaultFloatValue = variable.DefaultFloatValue,
                        intValue = variable.IntValue,
                        floatValue = variable.FloatValue
                    },
                    GetPropertyNode prop => new GetPropertyNodeData { guid = prop.Guid, position = prop.GetPosition().position, propertyName = prop.PropertyName },
                    MathNode math => new MathNodeData { guid = math.Guid, position = math.GetPosition().position, mathType = math.MathType },
                    ValueProcessorNode proc => new ValueProcessorNodeData { guid = proc.Guid, position = proc.GetPosition().position, processorName = proc.ProcessorName },
                    CommentNode comment => new CommentNodeData { guid = comment.Guid, position = comment.GetPosition().position, comment = comment.Comment },
                    _ => null
                };

                if (nodeData != null)
                    data.nodes.Add(nodeData);
            }

            // 엣지 저장
            foreach (var edge in graphView.edges)
            {
                var outputNode = edge.output.node as BaseNode;
                var inputNode = edge.input.node as BaseNode;

                if (outputNode != null && inputNode != null)
                {
                    data.edges.Add(new EdgeData
                    {
                        outputNodeGuid = outputNode.Guid,
                        outputPortName = edge.output.portName,
                        inputNodeGuid = inputNode.Guid,
                        inputPortName = edge.input.portName
                    });
                }
            }

            // Skill Info 저장
            var infoNode = graphView.InfoNode;
            data.skillName = infoNode.SkillName;
            data.description = infoNode.Description;
            data.icon = infoNode.Icon;
            // data.defaultValue = infoNode.DefaultValue;
            data.pipelineCount = infoNode.PipelineCount;
            
            // Variables 동기화
            data.variables.Clear();
            foreach (var node in graphView.nodes.OfType<VariableNode>())
            {
                data.variables.Add(new SkillVariable(node.VariableName, node.VariableType)
                {
                    defaultIntValue = node.DefaultIntValue,
                    defaultFloatValue = node.DefaultFloatValue,
                    intValue = node.IntValue,
                    floatValue = node.FloatValue
                });
            }

            // 컴파일
            SkillGraphCompiler.Compile(data);

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            Debug.Log($"저장 완료: {data.skillName}");
        }

        public static void Load(SkillGraphView graphView, SkillGraphData data)
        {
            graphView.ClearGraph();

            var nodeMap = new Dictionary<string, BaseNode>();

            // Info 노드 로드
            var infoData = data.nodes.OfType<SkillInfoNodeData>().FirstOrDefault();
            if (infoData != null)
            {
                graphView.InfoNode.Guid = infoData.guid;
                graphView.InfoNode.LoadData(data);
                nodeMap[infoData.guid] = graphView.InfoNode;
                graphView.InfoNode.SkillName = data.skillName;
                graphView.InfoNode.NameField.value = data.skillName;
                graphView.InfoNode.Description = data.description;
                graphView.InfoNode.DescriptionField.value = data.description;
                graphView.InfoNode.Icon = data.icon;
                graphView.InfoNode.IconField.value = data.icon;
                // graphView.InfoNode.DefaultValue = data.defaultValue;
                graphView.InfoNode.PipelineCount = data.pipelineCount;
                graphView.InfoNode.PipelineCountSlider.value = data.pipelineCount;
            }

            // 노드 생성
            foreach (var nodeData in data.nodes)
            {
                BaseNode node = nodeData switch
                {
                    SkillInfoNodeData => null, // 이미 처리함
                    ContextNodeData ctx => new ContextNode(graphView) { Guid = ctx.guid },
                    SkillBehaviourNodeData skill => new SkillBehaviourNode(graphView, skill.behaviourName)
                    {
                        Guid = skill.guid,
                        Value = skill.value
                    },
                    BranchNodeData branch => new BranchNode(graphView) { Guid = branch.guid },
                    LogicalNodeData logical => CreateLogicalNode(graphView, logical),
                    ComparisonNodeData comp => new ComparisonNode(graphView) { Guid = comp.guid, CompareType = comp.compareType },
                    ConstantNodeData constant => new ConstantNode(graphView) { Guid = constant.guid, Value = constant.value },
                    VariableNodeData variable => CreateVariableNode(graphView, variable),
                    GetPropertyNodeData prop => CreateGetPropertyNode(graphView, prop),
                    MathNodeData math => CreateMathNode(graphView, math),
                    ValueProcessorNodeData proc => CreateValueProcessorNode(graphView, proc),
                    CommentNodeData comment => CreateCommentNode(graphView, comment),
                    _ => null
                };

                if (node != null)
                {
                    node.SetPosition(new Rect(nodeData.position, Vector2.zero));
                    graphView.AddElement(node);
                    nodeMap[nodeData.guid] = node;
                }
            }

            // 엣지 생성
            foreach (var edgeData in data.edges)
            {
                if (!nodeMap.TryGetValue(edgeData.outputNodeGuid, out var outputNode)) continue;
                if (!nodeMap.TryGetValue(edgeData.inputNodeGuid, out var inputNode)) continue;

                var outputPort = FindPort(outputNode, edgeData.outputPortName, Direction.Output);
                var inputPort = FindPort(inputNode, edgeData.inputPortName, Direction.Input);

                if (outputPort != null && inputPort != null)
                {
                    var edge = outputPort.ConnectTo(inputPort);
                    graphView.AddElement(edge);
                    graphView.NotifyEdgeCreated(edge);
                }
            }
        }

        private static BaseNode CreateLogicalNode(SkillGraphView graphView, LogicalNodeData data)
        {
            BaseNode node = data.conditionType switch
            {
                ConditionType.And => new AndNode(graphView),
                ConditionType.Or => new OrNode(graphView),
                ConditionType.Not => new NotNode(graphView),
                _ => null
            };

            if (node != null)
                node.Guid = data.guid;

            return node;
        }
        
        private static BaseNode CreateGetPropertyNode(SkillGraphView graphView, GetPropertyNodeData data)
        {
            var node = new GetPropertyNode(graphView) { Guid = data.guid };
            node.SetProperty(data.propertyName);
            return node;
        }
        
        private static BaseNode CreateCommentNode(SkillGraphView graphView, CommentNodeData data)
        {
            var node = new CommentNode(graphView) { Guid = data.guid };
            node.SetComment(data.comment);
            return node;
        }
        
        private static BaseNode CreateMathNode(SkillGraphView graphView, MathNodeData data)
        {
            var node = new MathNode(graphView) { Guid = data.guid };
            node.SetMathType(data.mathType);
            return node;
        }
        
        private static BaseNode CreateValueProcessorNode(SkillGraphView graphView, ValueProcessorNodeData data)
        {
            var node = new ValueProcessorNode(graphView) { Guid = data.guid };
            node.SetProcessor(data.processorName);
            return node;
        }
        
        private static BaseNode CreateVariableNode(SkillGraphView graphView, VariableNodeData data)
        {
            var node = new VariableNode(graphView) { Guid = data.guid };
            node.SetVariable(data.variableName, data.variableType, 
                data.defaultIntValue, data.defaultFloatValue,
                data.intValue, data.floatValue);
            return node;
        }

        private static Port FindPort(Node node, string portName, Direction direction)
        {
            var container = direction == Direction.Output ? node.outputContainer : node.inputContainer;
            return container.Children().OfType<Port>().FirstOrDefault(p => p.portName == portName);
        }
    }
}
