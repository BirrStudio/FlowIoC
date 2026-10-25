namespace Modules.LocalSaveModule.Services
{
    /// <summary>
    /// The service a Root with IsTest ticked binds instead of the real one. Reporting no key for
    /// everything is what stops the module restoring anything, and writing nothing is what keeps
    /// a test run out of the developer's save file. A reset still puts the assets back in memory:
    /// that is the model's own copy, and only the writing of it ends here.
    /// </summary>
    public class DummyLocalSaveService : ILocalSaveService
    {
        public void BeginSession(string password) { }

        public bool Has(string key) => false;

        public void LoadInto(string key, object target) { }

        public void Save(string key, object value) { }

        public void Clear() { }

        public void Flush() { }
    }
}
