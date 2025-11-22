using System;

namespace LobbyService
{
    public interface IPreInitStrategy
    {
        public void Handle(Action call);
    }
}