namespace ServiceLocator.JSON
{
    public interface IRegistrationRegistry
    {
        string Class { get; set; }
        string Factory { get; set; }
        string FactoryMethod { get; set; }
        string Interface { get; set; }
        bool Multiple { get; set; }
    }
}