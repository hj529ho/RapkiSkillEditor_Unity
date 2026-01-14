using System;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 값을 가공하는 프로세서 인터페이스
    /// </summary>
    public interface IValueProcessor
    {
        string Name { get; }
        Color NodeColor { get; }
        int InputCount { get; }
        float Process(params float[] inputs);
    }
    
    /// <summary>
    /// ValueProcessor 자동 등록용 Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ValueProcessorAttribute : Attribute
    {
        public string Name { get; }
        public string Color { get; }
        public int InputCount { get; }
        public string[] InputNames { get; }
        
        public ValueProcessorAttribute(string name, string color = "#8888CC", int inputCount = 1, params string[] inputNames)
        {
            Name = name;
            Color = color;
            InputCount = inputCount;
            InputNames = inputNames.Length > 0 ? inputNames : null;
        }
    }
}
