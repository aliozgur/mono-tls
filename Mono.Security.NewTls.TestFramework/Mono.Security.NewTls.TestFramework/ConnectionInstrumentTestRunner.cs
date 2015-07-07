﻿//
// ConnectionInstrumentTestRunner.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Constraints;
using Xamarin.WebTests.ConnectionFramework;
using Xamarin.WebTests.Resources;

namespace Mono.Security.NewTls.TestFramework
{
	public class ConnectionInstrumentTestRunner : InstrumentationTestRunner
	{
		new public ConnectionInstrumentParameters Parameters {
			get { return (ConnectionInstrumentParameters)base.Parameters; }
		}

		public ConnectionInstrumentType Type {
			get { return Parameters.Type; }
		}

		public ConnectionInstrumentTestRunner (IServer server, IClient client, ConnectionInstrumentParameters parameters, MonoConnectionFlags flags)
			: base (server, client, parameters, flags)
		{
		}

		public override Instrumentation CreateInstrument (TestContext ctx)
		{
			var instrumentation = new Instrumentation ();

			var settings = new UserSettings ();

			instrumentation.SettingsInstrument = new ConnectionInstrument (settings, ctx, this);
			if (Parameters.HandshakeInstruments != null)
				instrumentation.HandshakeInstruments.UnionWith (Parameters.HandshakeInstruments);
			return instrumentation;
		}

		public static IEnumerable<ConnectionInstrumentParameters> GetParameters (TestContext ctx, InstrumentationCategory category)
		{
			switch (category) {
			case InstrumentationCategory.ClientConnection:
				return ClientConnectionTypes.Select (t => Create (ctx, category, t));

			case InstrumentationCategory.ServerConnection:
				return ServerConnectionTypes.Select (t => Create (ctx, category, t));

			case InstrumentationCategory.Connection:
				return ConnectionTypes.Select (t => Create (ctx, category, t));

			case InstrumentationCategory.MartinTest:
				return MartinTestTypes.Select (t => Create (ctx, category, t));

			default:
				ctx.AssertFail ("Unsupported instrumentation category: '{0}'.", category);
				return null;
			}
		}

		internal static readonly ConnectionInstrumentType[] ClientConnectionTypes = {
			ConnectionInstrumentType.FragmentHandshakeMessages,
			ConnectionInstrumentType.SendBlobAfterReceivingFinish
		};

		internal static readonly ConnectionInstrumentType[] ServerConnectionTypes = {
			ConnectionInstrumentType.FragmentHandshakeMessages
		};

		internal static readonly ConnectionInstrumentType[] ConnectionTypes = {
			ConnectionInstrumentType.FragmentHandshakeMessages
		};

		internal static readonly ConnectionInstrumentType[] MartinTestTypes = {
			ConnectionInstrumentType.MartinTest
		};

		static ConnectionInstrumentParameters CreateParameters (InstrumentationCategory category, ConnectionInstrumentType type, params object[] args)
		{
			var sb = new StringBuilder ();
			sb.Append (type);
			foreach (var arg in args) {
				sb.AppendFormat (":{0}", arg);
			}
			var name = sb.ToString ();

			return new ConnectionInstrumentParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
				ClientCertificateValidator = AcceptAnyCertificate, ServerCertificateValidator = AcceptAnyCertificate,
				ProtocolVersion = ProtocolVersions.Tls12
			};
		}

		static ConnectionInstrumentParameters Create (TestContext ctx, InstrumentationCategory category, ConnectionInstrumentType type)
		{
			var parameters = CreateParameters (category, type);

			switch (type) {
			case ConnectionInstrumentType.FragmentHandshakeMessages:
				parameters.HandshakeInstruments = new HandshakeInstrumentType[] { HandshakeInstrumentType.FragmentHandshakeMessages };
				break;

			case ConnectionInstrumentType.SendBlobAfterReceivingFinish:
				parameters.HandshakeInstruments = new HandshakeInstrumentType[] { HandshakeInstrumentType.SendBlobAfterReceivingFinish };
				break;

			case ConnectionInstrumentType.MartinTest:
				parameters.HandshakeInstruments = new HandshakeInstrumentType[] { HandshakeInstrumentType.SendBlobAfterReceivingFinish };
				parameters.EnableDebugging = true;
				break;

			default:
				ctx.AssertFail ("Unsupported connection instrument: '{0}'.", type);
				break;
			}

			return parameters;
		}

		public bool HasInstrument (HandshakeInstrumentType type)
		{
			return Parameters.HandshakeInstruments != null && Parameters.HandshakeInstruments.Contains (type);
		}

		protected override Task MainLoop (TestContext ctx, CancellationToken cancellationToken)
		{
			switch (Parameters.Type) {
			case ConnectionInstrumentType.SendBlobAfterReceivingFinish:
				return RunMainLoopBlob (ctx, HandshakeInstrumentType.SendBlobAfterReceivingFinish, cancellationToken);

			case ConnectionInstrumentType.MartinTest:
				return RunMainLoopMartin (ctx, cancellationToken);

			default:
				return base.MainLoop (ctx, cancellationToken);
			}
		}

		async Task RunMainLoopBlob (TestContext ctx, HandshakeInstrumentType type, CancellationToken cancellationToken)
		{
			var expected = Instrumentation.GetTextBuffer (type).GetBuffer ();

			var buffer = new byte [4096];
			int ret = await Server.Stream.ReadAsync (buffer, 0, buffer.Length);
			ctx.Assert (ret, Is.EqualTo (expected.Length));

			buffer = new BufferOffsetSize (buffer, 0, ret).GetBuffer ();

			ctx.Assert (buffer, Is.EqualTo (expected), "blob");

			await Shutdown (ctx, SupportsCleanShutdown, cancellationToken);
		}

		async Task RunMainLoopMartin (TestContext ctx, CancellationToken cancellationToken)
		{
			ctx.LogMessage ("MAIN LOOP MARTIN");

			var buffer = new byte [4096];
			int ret = await Server.Stream.ReadAsync (buffer, 0, buffer.Length);
			ctx.LogMessage ("MAIN LOOP MARTIN #1: {0}", ret);
			DebugHelper.WriteBuffer ("READ", buffer, 0, ret);

			// await Server.Shutdown (ctx, true, true, cancellationToken);
			// ctx.LogMessage ("MAIN LOOP MARTIN #2");

			var secondRead = Server.Stream.ReadAsync (buffer, 0, buffer.Length);
			await Task.Delay (1500);
			await Client.Shutdown (ctx, true, cancellationToken);

			ret = await secondRead;
			ctx.LogMessage ("MAIN LOOP MARTIN #2: {0}", ret);
		}
	}
}

