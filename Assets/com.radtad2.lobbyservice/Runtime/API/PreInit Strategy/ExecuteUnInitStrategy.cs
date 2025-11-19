using System;

namespace LobbyService
{
    public class ExecuteUnInitStrategy : IUnInitStrategy
    {
        public void Handle(Action call)
        {
            call.Invoke();
        }
    }
}