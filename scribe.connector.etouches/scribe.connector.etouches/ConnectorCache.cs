using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Caching;
namespace Scribe.Connector.etouches
{
    public static class ConnectorCache
    {
        static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        public static T GetCachedData<T>(string cacheKey)
        {
            //First we do a read lock to see if it already exists, this allows multiple readers at the same time.
            cacheLock.EnterReadLock();
            try
            {
                //Returns null if the string does not exist, prevents a race condition where the cache invalidates between the contains check and the retreival.
                T cachedValue = (T)MemoryCache.Default.Get(cacheKey, null);

                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

            //Only one UpgradeableReadLock can exist at one time, but it can co-exist with many ReadLocks
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                //We need to check again to see if the string was created while we where waiting to enter the EnterUpgradeableReadLock
                var cachedValue = (T)MemoryCache.Default.Get(cacheKey, null);

                if (cachedValue != null)
                {
                    return cachedValue;
                }

                return default(T);
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        public static void StoreData(string cacheKey, object data)
        {
            cacheLock.EnterWriteLock();
            try
            {
                int ttl;
                if (!int.TryParse(Connector.TTL, out ttl))
                    ttl = 20;
                CacheItemPolicy cip = new CacheItemPolicy()
                {
                     
                    AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(ttl))
                };
                MemoryCache.Default.Set(cacheKey, data, cip);                
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
    }

    
}
