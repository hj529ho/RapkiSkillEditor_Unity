using SkillEditor.Core;

namespace SkillEditor.Examples
{
    /// <summary>
    /// 예제: HP 접근자
    /// 실제 게임에서는 자신의 Entity 타입에 맞게 구현
    /// </summary>
    [PropertyAccessor("HP", "Status")]
    public class HPAccessor : IPropertyAccessor
    {
        public string Name => "HP";
        public string Category => "Status";
        
        public float GetValue(object entity)
        {
            // 예: return ((YourEntity)entity).HP;
            return 100f; // 더미값
        }
        
        public void SetValue(object entity, float value)
        {
            // 예: ((YourEntity)entity).HP = value;
        }
    }
    
    [PropertyAccessor("MaxHP", "Status")]
    public class MaxHPAccessor : IPropertyAccessor
    {
        public string Name => "MaxHP";
        public string Category => "Status";
        
        public float GetValue(object entity)
        {
            return 100f;
        }
        
        public void SetValue(object entity, float value) { }
    }
    
    [PropertyAccessor("Attack", "Combat")]
    public class AttackAccessor : IPropertyAccessor
    {
        public string Name => "Attack";
        public string Category => "Combat";
        
        public float GetValue(object entity)
        {
            return 10f;
        }
        
        public void SetValue(object entity, float value) { }
    }
    
    [PropertyAccessor("Defense", "Combat")]
    public class DefenseAccessor : IPropertyAccessor
    {
        public string Name => "Defense";
        public string Category => "Combat";
        
        public float GetValue(object entity)
        {
            return 5f;
        }
        
        public void SetValue(object entity, float value) { }
    }
}
