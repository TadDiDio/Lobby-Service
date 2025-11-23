using UnityEngine;

namespace LobbyService.LocalServer
{
    public static class RegistryInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Init()
        {
            MessageTypeRegistry.RegisterMessageTypes();
        }
    }
}