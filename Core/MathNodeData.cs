namespace SkillEditor.Core
{
    [System.Serializable]
    public class MathNodeData : BaseNodeData
    {
        public MathType mathType;
    }
    
    [System.Serializable]
    public class ValueProcessorNodeData : BaseNodeData
    {
        public string processorName;
    }
}
