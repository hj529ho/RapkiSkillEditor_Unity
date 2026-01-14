using System;

namespace SkillEditor.Core
{
    /// <summary>
    /// 엔티티 속성 접근 인터페이스
    /// </summary>
    public interface IPropertyAccessor
    {
        string Name { get; }
        string Category { get; }
        float GetValue(object entity);
        void SetValue(object entity, float value);
    }
    
    /// <summary>
    /// PropertyAccessor 자동 등록용 Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyAccessorAttribute : Attribute
    {
        public string Name { get; }
        public string Category { get; }
        
        public PropertyAccessorAttribute(string name, string category = "Default")
        {
            Name = name;
            Category = category;
        }
    }
}
