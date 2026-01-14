using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 스킬 행동을 정의하는 인터페이스
    /// </summary>
    public interface ISkillBehaviour
    {
        string Name { get; }
        string Description { get; }
        Color NodeColor { get; }
        void Execute(ISkillContext context, int value);
    }
}
