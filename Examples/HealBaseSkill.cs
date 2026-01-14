using SkillEditor.Core;
using UnityEngine;

namespace SkillEditor.Examples
{
    [SkillBehaviour("회복", "#7BFF93", "체력을 회복한다")]
    public class HealBaseSkill : BaseSkillBehaviour
    {
        public override string Name => "회복";
        public override string Description => "체력을 회복한다";
        public override Color NodeColor => new Color(0.48f, 1f, 0.58f);
        
        public override void Execute(ISkillContext context, int value)
        {
            Debug.Log($"[회복] 대상이 {value} 회복");
        }
    }
}
