using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    public class ModuleProxy<T> : DispatchProxy
    {
        private PreInitWrapper _strategyWrapper;
        private T _target;
        
        public void Initialize(PreInitWrapper strategyWrapper)
        {
            _strategyWrapper = strategyWrapper;
        }
        
        public void AttachTarget(T target)
        {
            _target = target;
        }

        public void DetachTarget()
        {
            _target = default;
        }
        
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (_target != null) return targetMethod.Invoke(_target, args);
            
            if (Lobby.AllowingActions) _strategyWrapper.RegisterAction(() => targetMethod.Invoke(_target, args));
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