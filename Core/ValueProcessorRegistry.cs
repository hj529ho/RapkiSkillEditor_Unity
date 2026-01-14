using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// ValueProcessor 레지스트리
    /// </summary>
    public class ValueProcessorRegistry
    {
        private static ValueProcessorRegistry _instance;
        public static ValueProcessorRegistry Instance => _instance ??= new ValueProcessorRegistry();
        
        private Dictionary<string, IValueProcessor> _processors;
        private Dictionary<string, ValueProcessorInfo> _infos;
        
        public IReadOnlyDictionary<string, IValueProcessor> All
        {
            get
            {
                if (_processors == null) BuildRegistry();
                return _processors;
            }
        }
        
        public IReadOnlyDictionary<string, ValueProcessorInfo> Infos
        {
            get
            {
                if (_infos == null) BuildRegistry();
                return _infos;
            }
        }
        
        private void BuildRegistry()
        {
            _processors = new Dictionary<string, IValueProcessor>();
            _infos = new Dictionary<string, ValueProcessorInfo>();
            
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IValueProcessor).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<ValueProcessorAttribute>() != null);

            foreach (var type in types)
            {
                try
                {
                    var attr = type.GetCustomAttribute<ValueProcessorAttribute>();
                    var instance = (IValueProcessor)Activator.CreateInstance(type);
                    ColorUtility.TryParseHtmlString(attr.Color, out var color);
                    
                    _processors[attr.Name] = instance;
                    _infos[attr.Name] = new ValueProcessorInfo
                    {
                        Name = attr.Name,
                        Type = type,
                        Color = color,
                        InputCount = attr.InputCount,
                        InputNames = attr.InputNames,
                        Instance = instance
                    };
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to register ValueProcessor {type.Name}: {e.Message}");
                }
            }
        }
        
        public IValueProcessor Get(string name)
        {
            return All.TryGetValue(name, out var p) ? p : null;
        }
        
        public void Refresh()
        {
            _processors = null;
            _infos = null;
        }
    }
    
    /// <summary>
    /// ValueProcessor 메타 정보
    /// </summary>
    public class ValueProcessorInfo
    {
        public string Name;
        public Type Type;
        public Color Color;
        public int InputCount;
        public string[] InputNames;
        public IValueProcessor Instance;
    }
}
