using System;

namespace LobbyService
{
    public class ExecutePreInitStrategy : IPreInitStrategy
    {
        public void Handle(Action call)
        {
            call.Invoke();
        }
    }
}