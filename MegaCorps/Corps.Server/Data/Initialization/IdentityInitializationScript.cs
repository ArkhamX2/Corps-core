namespace Corps.Server.Data.Initialization
{
    public class IdentityInitializationScript(IdentityContext context) : IInitializationScript
    {
        private IdentityContext IdentityContext { get; } = context;

        public async Task Run()
        {
            if (!await IdentityContext.TryInitializeAsync())
            {
                throw new Exception("Data initialization failed!");
            }
        }
    }
}
