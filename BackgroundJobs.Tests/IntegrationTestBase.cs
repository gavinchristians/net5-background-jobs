using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static BackgroundJobs.Tests.SliceFixture;

namespace BackgroundJobs.Tests
{
    public class IntegrationTestBase : IAsyncLifetime
    {
        private static readonly AsyncLock Mutex = new AsyncLock();

        private static bool _initialized;

        public virtual async Task InitializeAsync()
        {
            if (_initialized)
                return;

            using (await Mutex.LockAsync())
            {
                if (_initialized)
                    return;

                if (!connectionString.Contains("Server=(localdb)\\mssqllocaldb;"))
                {
                    throw new Exception(TestMessages.DbWarning);
                }
                else
                {
                    await SliceFixture.ResetCheckpoint();
                }

                _initialized = true;
            }
        }

        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}
