namespace SkillEditor.Core
{
    public enum VariableType { Int, Float }
    
    [System.Serializable]
    public class SkillVariable
    {
        public string name;
        public VariableType type;
        
        // 기본값 (그래프에서 설정)
        public int defaultIntValue;
        public float defaultFloatValue;
        
        // 실제값 (SO/그래프에서 수정)
        public int intValue;
        public float floatValue;
        
        public SkillVariable(string name, VariableType type = VariableType.Float)
        {
            this.name = name;
            this.type = type;
        }
        
        public float GetValueAsFloat() => type switch
        {
            VariableType.Int => intValue,
            VariableType.Float => floatValue,
            _ => 0
        };
        
        public void SetValue(float value)
        {
            if (type == VariableType.Int)
                intValue = (int)value;
            else
                floatValue = value;
        }
        
        public void ResetToDefault()
        {
            intValue = defaultIntValue;
            floatValue = defaultFloatValue;
        }
    }
}
