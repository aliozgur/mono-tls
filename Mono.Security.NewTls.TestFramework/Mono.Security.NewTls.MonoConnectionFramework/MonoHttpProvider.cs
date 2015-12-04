//
// MonoHttpProvider.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MSI = Mono.Security.Interface;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Portable;
using Xamarin.WebTests.ConnectionFramework;
using Xamarin.WebTests.HttpFramework;
using Xamarin.WebTests.Portable;
using Xamarin.WebTests.Providers;
using Xamarin.WebTests.HttpClient;
using Xamarin.WebTests.Server;

namespace Mono.Security.NewTls.MonoConnectionFramework
{
	using TestFramework;

	class MonoHttpProvider : IHttpProvider
	{
		readonly MonoConnectionProvider connectionProvider;

		internal MonoHttpProvider (MonoConnectionProvider connectionProvider)
		{
			this.connectionProvider = connectionProvider;
		}

		public bool SupportsWebRequest {
			get { return true; }
		}

		public HttpWebRequest CreateWebRequest (Uri uri)
		{
			return MSI.MonoTlsProviderFactory.CreateHttpsRequest (uri, connectionProvider.MonoTlsProvider);
		}

		public bool SupportsHttpClient {
			get { return false; }
		}

		public IHttpClientHandler CreateHttpClient ()
		{
			throw new InvalidOperationException ();
		}

		public bool SupportsPerRequestCertificateValidator {
			get { return true; }
		}
	}
}