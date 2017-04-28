namespace ServiceLocator.JSON.Registries
{
    public interface IResolverRegistry
    {
        string Class { get; set; }
        string Interface { get; set; }
        bool Multiple { get; set; }
    }
}