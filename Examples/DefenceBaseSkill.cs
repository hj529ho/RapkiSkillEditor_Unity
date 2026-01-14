using SkillEditor.Core;
using UnityEngine;

namespace SkillEditor.Examples
{
    [SkillBehaviour("방어도", "#7BC8FF", "방어도를 얻는다")]
    public class DefenceBaseSkill : BaseSkillBehaviour
    {
        public override string Name => "방어도";
        public override string Description => "방어도를 얻는다";
        public override Color NodeColor => new Color(0.48f, 0.78f, 1f);
        
        public override void Execute(ISkillContext context, int value)
        {
            Debug.Log($"[방어도] 시전자가 {value} 방어도 획득");
        }
    }
}
