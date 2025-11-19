using System;
using System.Collections.Generic;

namespace LobbyService
{
    public class UnInitWrapper
    {
        private IUnInitStrategy _strategy;
        private Queue<Action> _callList = new();

        public UnInitWrapper(IUnInitStrategy strategy)
        {
            SetStrategy(strategy);
        }
        
        public void SetStrategy(IUnInitStrategy strategy)
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

        public void Reset(IUnInitStrategy strategy)
        {
            _strategy = strategy;
            _callList.Clear();
        }
    }
}