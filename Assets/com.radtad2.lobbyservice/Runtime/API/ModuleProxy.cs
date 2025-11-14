using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LobbyService
{
    public class ModuleProxy<T> : DispatchProxy
    {
        private IPreInitStrategy _strategy;
        private T _target;
        
        public void Initialize(IPreInitStrategy strategy)
        {
            _strategy = strategy;
        }
        
        public void AttachTarget(T target)
        {
            _target = target;
        }
        
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (_target != null) return targetMethod.Invoke(_target, args);
            
            _strategy.Handle(() => targetMethod.Invoke(_target, args));
            return GetDefaultReturnValue(targetMethod.ReturnType);
        }
        
        private object GetDefaultReturnValue(Type returnType)
        {
            if (returnType == typeof(void)) return null;
            if (typeof(Task).IsAssignableFrom(returnType)) return Task.CompletedTask;
            return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
        }
    }
}