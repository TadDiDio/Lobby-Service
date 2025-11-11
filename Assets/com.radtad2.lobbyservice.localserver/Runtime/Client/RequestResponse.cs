namespace LobbyService.LocalServer
{
    public class RequestResponse<T> where T : IResponse
    {
        public Error Error;
        public T Response;
    }
}