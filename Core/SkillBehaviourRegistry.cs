using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// 스킬 행동 레지스트리 인터페이스
    /// </summary>
    public interface ISkillBehaviourRegistry
    {
        IReadOnlyDictionary<string, ISkillBehaviour> All { get; }
        void Register(ISkillBehaviour behaviour);
        ISkillBehaviour Get(string name);
        bool Contains(string name);
    }
    
    /// <summary>
    /// 기본 레지스트리 구현체 (Attribute 자동 등록 지원)
    /// </summary>
    public class SkillBehaviourRegistry : ISkillBehaviourRegistry
    {
        private static SkillBehaviourRegistry _instance;
        public static SkillBehaviourRegistry Instance => _instance ??= new SkillBehaviourRegistry();
        
        private Dictionary<string, ISkillBehaviour> _behaviours;
        private Dictionary<string, SkillBehaviourInfo> _infos;
        
        public IReadOnlyDictionary<string, ISkillBehaviour> All
        {
            get
            {
                if (_behaviours == null) BuildRegistry();
                return _behaviours;
            }
        }
        
        public IReadOnlyDictionary<string, SkillBehaviourInfo> Infos
        {
            get
            {
                if (_infos == null) BuildRegistry();
                return _infos;
            }
        }
        
        private void BuildRegistry()
        {
            _behaviours = new Dictionary<string, ISkillBehaviour>();
            _infos = new Dictionary<string, SkillBehaviourInfo>();
            
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ISkillBehaviour).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<SkillBehaviourAttribute>() != null);

            foreach (var type in types)
            {
                try
                {
                    var attr = type.GetCustomAttribute<SkillBehaviourAttribute>();
                    var instance = (ISkillBehaviour)Activator.CreateInstance(type);
                    ColorUtility.TryParseHtmlString(attr.Color, out var color);
                    
                    _behaviours[attr.Name] = instance;
                    _infos[attr.Name] = new SkillBehaviourInfo
                    {
                        Name = attr.Name,
                        Type = type,
                        Color = color,
                        Instance = instance
                    };
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to register {type.Name}: {e.Message}");
                }
            }
        }
        
        public void Register(ISkillBehaviour behaviour)
        {
            if (behaviour == null) return;
            if (_behaviours == null) BuildRegistry();
            
            _behaviours[behaviour.Name] = behaviour;
            _infos[behaviour.Name] = new SkillBehaviourInfo
            {
                Name = behaviour.Name,
                Type = behaviour.GetType(),
                Color = behaviour.NodeColor,
                Instance = behaviour
            };
        }
        
        public ISkillBehaviour Get(string name)
        {
            return All.TryGetValue(name, out var b) ? b : null;
        }
        
        public bool Contains(string name) => All.ContainsKey(name);
        public void Refresh() { _behaviours = null; _infos = null; }
    }
    
    /// <summary>
    /// 스킬 메타 정보
    /// </summary>
    public class SkillBehaviourInfo
    {
        public string Name;
        public Type Type;
        public Color Color;
        public ISkillBehaviour Instance;
        public ISkillBehaviour CreateInstance() => (ISkillBehaviour)Activator.CreateInstance(Type);
    }
}
