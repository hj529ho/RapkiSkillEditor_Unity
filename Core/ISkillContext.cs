namespace SkillEditor.Core
{
    /// <summary>
    /// 스킬 실행 시 필요한 컨텍스트
    /// </summary>
    public interface ISkillContext
    {
        object Self { get; }
        object Target { get; }
        T GetSelf<T>() where T : class;
        T GetTarget<T>() where T : class;
    }
    
    /// <summary>
    /// 기본 컨텍스트 구현체
    /// </summary>
    public class SkillContext : ISkillContext
    {
        public object Self { get; set; }
        public object Target { get; set; }
        
        public SkillContext() { }
        
        public SkillContext(object self, object target)
        {
            Self = self;
            Target = target;
        }
        
        public T GetSelf<T>() where T : class => Self as T;
        public T GetTarget<T>() where T : class => Target as T;
    }
}
