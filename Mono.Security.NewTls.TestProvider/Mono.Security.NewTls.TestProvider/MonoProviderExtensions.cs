﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Mono.Security.Interface;
using Mono.Security.Providers.NewTls;

namespace Mono.Security.NewTls.TestProvider
{
	using MonoConnectionFramework;

	class MonoProviderExtensions : IMonoProviderExtensions
	{
		MonoTlsProvider provider;
		NewTlsProvider newTls;

		public MonoProviderExtensions (MonoTlsProvider provider)
		{
			this.provider = provider;
			global::System.Console.WriteLine ("PROVIDER: {0} {1}", provider.ID, provider);
			newTls = provider as NewTlsProvider;
		}

		public MonoTlsProvider Object {
			get { return provider; }
		}

		public bool IsNewTls {
			get { return newTls != null; }
		}

		public bool SupportsMonoExtensions {
			get { return newTls != null; }
		}

		public IMonoNewTlsStream GetStreamExtension (IMonoSslStream stream)
		{
			var monoNewTlsStream = stream as MonoNewTlsStream;
			if (monoNewTlsStream == null)
				throw new InvalidOperationException ();
			return new MonoNewTlsStreamWrapper (monoNewTlsStream);
		}

		public MonoTlsConnectionInfo GetConnectionInfo ()
		{
			throw new NotImplementedException ();
		}

		public bool SupportsInstrumentation {
			get { return newTls != null; }
		}

		class MonoNewTlsStreamWrapper : IMonoNewTlsStream
		{
			MonoNewTlsStream stream;

			public MonoNewTlsStreamWrapper (MonoNewTlsStream stream)
			{
				this.stream = stream;
			}

			public MonoTlsConnectionInfo GetConnectionInfo ()
			{
				return stream.GetConnectionInfo ();
			}

			public Task Shutdown ()
			{
				return stream.Shutdown ();
			}

			public Task RequestRenegotiation ()
			{
				return stream.RequestRenegotiation ();
			}

			public Exception LastError {
				get { return stream.LastError; }
			}

			public bool SupportsConnectionInfo {
				get { return true; }
			}

			public bool SupportsRenegotiation {
				get { return true; }
			}
		}
	}
}

