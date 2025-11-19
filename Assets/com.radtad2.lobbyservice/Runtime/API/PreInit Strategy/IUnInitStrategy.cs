using System;

namespace LobbyService
{
    public interface IUnInitStrategy
    {
        public void Handle(Action call);
    }
}