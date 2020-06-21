using System.Threading;
using Raven.Client;
using Raven.Imports.Newtonsoft.Json;

namespace XDiscordBotLib.Database
{
    public abstract class DatastoreModel
    {
        [JsonIgnore]
        protected abstract string DatastoreKey { get; }

        public virtual void Save(IDocumentSession session)
        {
            if (DatastoreKey == null)
                session.Store(this);
            else
                session.Store(this, DatastoreKey);
        }

        public virtual void SaveAsync(IAsyncDocumentSession session, CancellationToken token = default(CancellationToken))
        {
            if (DatastoreKey == null)
                session.StoreAsync(this, token);
            else
                session.StoreAsync(this, DatastoreKey, token);
        }
    }
}