using System;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 스킬 행동 Attribute (자동 등록용)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SkillBehaviourAttribute : Attribute
    {
        public string Name { get; }
        public string Color { get; }
        public string Description { get; }

        public SkillBehaviourAttribute(string name, string color = "#FFFFFF", string description = "")
        {
            Name = name;
            Color = color;
            Description = description;
        }
    }
    
    /// <summary>
    /// 스킬 행동 베이스 클래스
    /// </summary>
    public abstract class BaseSkillBehaviour : ISkillBehaviour
    {
        public abstract string Name { get; }
        public virtual string Description => "";
        public virtual Color NodeColor => Color.white;
        public abstract void Execute(ISkillContext context, int value);
    }
}
