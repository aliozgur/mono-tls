﻿//
// MonoClientAndServerTestTypeAttribute.cs
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
using System.Collections.Generic;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.WebTests.ConnectionFramework;

namespace Mono.Security.NewTls.TestFeatures
{
	using TestFramework;

	public class MonoClientAndServerTestTypeAttribute : TestParameterAttribute, ITestParameterSource<MonoClientAndServerTestType>
	{
		public bool IncludeVersionTests {
			get; set;
		}

		public MonoClientAndServerTestType? TestType {
			get; set;
		}

		public MonoClientAndServerTestTypeAttribute (string filter = null)
			: base (filter)
		{
		}

		public MonoClientAndServerTestTypeAttribute (MonoClientAndServerTestType type)
		{
			TestType = type;
		}

		public IEnumerable<MonoClientAndServerTestType> GetParameters (TestContext ctx, string filter)
		{
			if (TestType != null) {
				yield return TestType.Value;
				yield break;
			}

			yield return MonoClientAndServerTestType.Simple;
			yield return MonoClientAndServerTestType.CheckDefaultCipher;

			if (IncludeVersionTests) {
				yield return MonoClientAndServerTestType.SimpleTls10;
				yield return MonoClientAndServerTestType.SimpleTls11;
				yield return MonoClientAndServerTestType.SimpleTls12;
			}
		}
	}
}

