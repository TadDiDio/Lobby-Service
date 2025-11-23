using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyService
{
    public class ModuleProxy<T> : DispatchProxy
    {
        private readonly Dictionary<string, object> _propertyBag = new();
        
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
            // Allow getters and setters to pass through 
            if (targetMethod.IsSpecialName)
            {
                return HandleProperty(targetMethod, args);
            }
            
            if (_target != null) return targetMethod.Invoke(_target, args);

            if (Lobby.AllowingActions)
            {
                if (Lobby.Rules.WarnOnPreInitCommands)
                {
                    Debug.LogWarning($"Received a call before initialization");
                }
                _strategyWrapper.RegisterAction(() => targetMethod.Invoke(_target, args));
            }
            return GetDefaultReturnValue(targetMethod.ReturnType);
        }
        
        private object HandleProperty(MethodInfo method, object[] args)
        {
            string name = method.Name;

            if (name.StartsWith("set_"))
            {
                var prop = name.Substring(4);
                _propertyBag[prop] = args[0];
                return null;
            }

            if (name.StartsWith("get_"))
            {
                var prop = name.Substring(4);

                return _propertyBag.TryGetValue(prop, out var value)
                    ? value
                    : GetDefaultReturnValue(method.ReturnType);
            }

            return null;
        }
        
        private object GetDefaultReturnValue(Type returnType)
        {
            if (returnType == typeof(void)) return null;
            if (typeof(Task).IsAssignableFrom(returnType)) return Task.CompletedTask;
            return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
        }
    }
}