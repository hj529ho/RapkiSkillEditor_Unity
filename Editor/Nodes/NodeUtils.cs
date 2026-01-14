using SkillEditor.Core;
using UnityEngine;

namespace SkillEditor.Editor
{
    public static class NodeUtils
    {
        public static Color GetSourceColor(EntitySource source)
        {
            return source switch
            {
                EntitySource.Self => new Color(0.3f, 0.8f, 1f),    // 시전자 - 하늘색
                EntitySource.Target => new Color(1f, 0.4f, 0.4f), // 대상 - 빨간색
                _ => Color.gray
            };
        }

        public static string GetSourceText(EntitySource source)
        {
            return source switch
            {
                EntitySource.Self => "Self",
                EntitySource.Target => "Target",
                _ => "(연결 없음)"
            };
        }
    }
}
