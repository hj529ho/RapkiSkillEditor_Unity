using SkillEditor.Core;
using UnityEngine;

namespace SkillEditor.Examples
{
    [SkillBehaviour("데미지", "#FF7B7B", "대상에게 피해를 준다")]
    public class DamageBaseSkill : BaseSkillBehaviour
    {
        public override string Name => "데미지";
        public override string Description => "대상에게 피해를 준다";
        public override Color NodeColor => new Color(1f, 0.48f, 0.48f);
        
        public override void Execute(ISkillContext context, int value)
        {
            Debug.Log($"[데미지] 대상에게 {value} 피해");
        }
    }
}
