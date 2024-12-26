using System;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Lexy.Poc
{
    public abstract class ScopedServicesTestFixture
    {
        private IServiceScope serviceScope;

        protected IServiceScope ServiceScope
        {
            get
            {
                if (serviceScope == null)
                {
                    throw new InvalidOperationException("ServiceScope not set");
                }

                return serviceScope;
            }
        }

        protected IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

        [SetUp]
        public void SetUp()
        {
            serviceScope = TestServiceProvider.CreateScope();
        }

        [TearDown]
        public void TearDown()
        {
            serviceScope?.Dispose();
            serviceScope = null;
        }

        public T GetService<T>() => ServiceProvider.GetRequiredService<T>();
    }
}