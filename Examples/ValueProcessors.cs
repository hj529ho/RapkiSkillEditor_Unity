using SkillEditor.Core;
using UnityEngine;

namespace SkillEditor.Examples
{
    /// <summary>
    /// 퍼센트 계산: value * percent / 100
    /// </summary>
    [ValueProcessor("Percent", "#CC8888", 2, "Value", "Percent")]
    public class PercentProcessor : IValueProcessor
    {
        public string Name => "Percent";
        public Color NodeColor => new Color(0.8f, 0.5f, 0.5f);
        public int InputCount => 2;
        
        public float Process(params float[] inputs)
        {
            if (inputs.Length < 2) return 0;
            return inputs[0] * inputs[1] / 100f;
        }
    }
    
    /// <summary>
    /// 클램프: value를 min~max 범위로 제한
    /// </summary>
    [ValueProcessor("Clamp", "#88CC88", 3, "Value", "Min", "Max")]
    public class ClampProcessor : IValueProcessor
    {
        public string Name => "Clamp";
        public Color NodeColor => new Color(0.5f, 0.8f, 0.5f);
        public int InputCount => 3;
        
        public float Process(params float[] inputs)
        {
            if (inputs.Length < 3) return inputs.Length > 0 ? inputs[0] : 0;
            return Mathf.Clamp(inputs[0], inputs[1], inputs[2]);
        }
    }
    
    /// <summary>
    /// 반올림
    /// </summary>
    [ValueProcessor("Round", "#8888CC", 1, "Value")]
    public class RoundProcessor : IValueProcessor
    {
        public string Name => "Round";
        public Color NodeColor => new Color(0.5f, 0.5f, 0.8f);
        public int InputCount => 1;
        
        public float Process(params float[] inputs)
        {
            if (inputs.Length < 1) return 0;
            return Mathf.Round(inputs[0]);
        }
    }
    
    /// <summary>
    /// 최소값
    /// </summary>
    [ValueProcessor("Min", "#CCCC88", 2, "A", "B")]
    public class MinProcessor : IValueProcessor
    {
        public string Name => "Min";
        public Color NodeColor => new Color(0.8f, 0.8f, 0.5f);
        public int InputCount => 2;
        
        public float Process(params float[] inputs)
        {
            if (inputs.Length < 2) return inputs.Length > 0 ? inputs[0] : 0;
            return Mathf.Min(inputs[0], inputs[1]);
        }
    }
    
    /// <summary>
    /// 최대값
    /// </summary>
    [ValueProcessor("Max", "#88CCCC", 2, "A", "B")]
    public class MaxProcessor : IValueProcessor
    {
        public string Name => "Max";
        public Color NodeColor => new Color(0.5f, 0.8f, 0.8f);
        public int InputCount => 2;
        
        public float Process(params float[] inputs)
        {
            if (inputs.Length < 2) return inputs.Length > 0 ? inputs[0] : 0;
            return Mathf.Max(inputs[0], inputs[1]);
        }
    }
}
