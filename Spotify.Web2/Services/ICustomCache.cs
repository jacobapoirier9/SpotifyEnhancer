namespace Spotify.Web.Services
{
    public interface ICustomCache
    {
        public T Get<T>(string username);
        public T Get<T>(string username, string key);

        public bool HasKey<T>(string username);

        public bool HasKey(string username, string key);

        public void Save<T>(string username, T dto);
        public void Save<T>(string username, string key, T dto);
    }
}
