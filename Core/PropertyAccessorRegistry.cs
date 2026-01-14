using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SkillEditor.Core
{
    /// <summary>
    /// PropertyAccessor 레지스트리
    /// </summary>
    public class PropertyAccessorRegistry
    {
        private static PropertyAccessorRegistry _instance;
        public static PropertyAccessorRegistry Instance => _instance ??= new PropertyAccessorRegistry();
        
        private Dictionary<string, IPropertyAccessor> _accessors;
        private Dictionary<string, List<IPropertyAccessor>> _byCategory;
        
        public IReadOnlyDictionary<string, IPropertyAccessor> All
        {
            get
            {
                if (_accessors == null) BuildRegistry();
                return _accessors;
            }
        }
        
        public IReadOnlyDictionary<string, List<IPropertyAccessor>> ByCategory
        {
            get
            {
                if (_byCategory == null) BuildRegistry();
                return _byCategory;
            }
        }
        
        private void BuildRegistry()
        {
            _accessors = new Dictionary<string, IPropertyAccessor>();
            _byCategory = new Dictionary<string, List<IPropertyAccessor>>();
            
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IPropertyAccessor).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<PropertyAccessorAttribute>() != null);

            foreach (var type in types)
            {
                try
                {
                    var attr = type.GetCustomAttribute<PropertyAccessorAttribute>();
                    var instance = (IPropertyAccessor)Activator.CreateInstance(type);
                    
                    _accessors[attr.Name] = instance;
                    
                    if (!_byCategory.ContainsKey(attr.Category))
                        _byCategory[attr.Category] = new List<IPropertyAccessor>();
                    _byCategory[attr.Category].Add(instance);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to register PropertyAccessor {type.Name}: {e.Message}");
                }
            }
        }
        
        public IPropertyAccessor Get(string name)
        {
            return All.TryGetValue(name, out var accessor) ? accessor : null;
        }
        
        public float GetValue(string propertyName, object entity)
        {
            var accessor = Get(propertyName);
            return accessor?.GetValue(entity) ?? 0f;
        }
        
        public void Refresh()
        {
            _accessors = null;
            _byCategory = null;
        }
    }
}
