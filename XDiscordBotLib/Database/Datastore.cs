using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using XDiscordBotLib.Utils;

namespace XDiscordBotLib.Database
{
    public class DataStore
    {
        private readonly IDocumentStore documentStore;

        public DataStore(string name, string ip = "http://localhost", int port = 8888)
        {
            documentStore = new DocumentStore
            {
                Url = $"{ip}:{port}",
                DefaultDatabase = name
            }.Initialize();
            IndexCreation.CreateIndexes(typeof(DataStore).Assembly, documentStore);
            Logging.Console.Verbose("Opened connection to {Url}", documentStore.Url);
        }

        public IAsyncDocumentSession OpenAsyncSession() => documentStore.OpenAsyncSession();

        public IDocumentSession OpenSession() => documentStore.OpenSession();

        public IDocumentStore GetStore() => documentStore;

        public bool DocumentExists(string key) { return documentStore.DatabaseCommands.Get(key) != null; }
    }
}