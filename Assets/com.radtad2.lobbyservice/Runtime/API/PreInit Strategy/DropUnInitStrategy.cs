using System;

namespace LobbyService
{
    public class DropUnInitStrategy : IUnInitStrategy
    {
        public void Handle(Action action)
        {
            // No-op
        }
    }
}