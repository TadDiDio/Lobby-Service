using System;
using System.Collections.Generic;

namespace LobbyService
{
    public class PreInitWrapper
    {
        private IPreInitStrategy _strategy;
        private Queue<Action> _callList = new();

        public PreInitWrapper(IPreInitStrategy strategy)
        {
            SetStrategy(strategy);
        }
        
        public void SetStrategy(IPreInitStrategy strategy)
        {
            _strategy = strategy;
        }

        public void RegisterAction(Action call)
        {
            _callList.Enqueue(call);
        }
        
        public void Flush()
        {
            while (_callList.Count > 0)
            {
                _strategy.Handle(_callList.Dequeue());
            }
        }

        public void Reset(IPreInitStrategy strategy)
        {
            _strategy = strategy;
            _callList.Clear();
        }
    }
}