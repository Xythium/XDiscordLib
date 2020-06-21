using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using XDiscordBotLib.Utils;

namespace XDiscordBotLib.Database
{
    public class DataStore
    {
        private readonly IDocumentStore documentStore;

        public DataStore(string name) : this(name, "http://localhost") { }

        public DataStore(string name, string ip) : this(name, ip, 8888) { }

        public DataStore(string name, string ip, int port)
        {
            Singleton<DataStore>.SetInstance(this);
            documentStore = new DocumentStore
            {
                Url = $"{ip}:{port}",
                DefaultDatabase = name
            }.Initialize();
            documentStore.Conventions.MaxNumberOfRequestsPerSession = 1000;
            IndexCreation.CreateIndexes(typeof(DataStore).Assembly, documentStore);
            Logging.Console.Verbose($"Opened connection to {documentStore.Url}");
        }

        public IAsyncDocumentSession OpenAsyncSession() => documentStore.OpenAsyncSession();

        public IDocumentSession OpenSession() => documentStore.OpenSession();

        public IDocumentStore GetStore() => documentStore;

        public bool DocumentExists(string key) { return documentStore.DatabaseCommands.Get(key) != null; }
    }
}