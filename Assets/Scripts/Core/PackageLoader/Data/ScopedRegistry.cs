namespace Core.PackageLoader
{
    [System.Serializable]
    public class ScopedRegistry
    {
        public string name;
        public string url;
        public string[] scopes;
    }

}