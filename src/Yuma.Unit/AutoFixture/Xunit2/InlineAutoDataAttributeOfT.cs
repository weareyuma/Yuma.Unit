#region Copyright & License

// Copyright © 2024 - 2025 Yuma
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.Xunit2;

namespace Yuma.AutoFixture.Xunit2;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T>(), values)
	where T : ICustomization, new();

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T1, T2>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T1, T2>(), values)
	where T1 : ICustomization, new()
	where T2 : ICustomization, new();

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T1, T2, T3>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T1, T2, T3>(), values)
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new();

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T1, T2, T3, T4>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T1, T2, T3, T4>(), values)
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new();

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T1, T2, T3, T4, T5>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T1, T2, T3, T4, T5>(), values)
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
	where T5 : ICustomization, new();

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T1, T2, T3, T4, T5, T6>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T1, T2, T3, T4, T5, T6>(), values)
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
	where T5 : ICustomization, new()
	where T6 : ICustomization, new();

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
public sealed class InlineAutoDataAttribute<T1, T2, T3, T4, T5, T6, T7>(params object[] values) : InlineAutoDataAttribute(new AutoDataAttribute<T1, T2, T3, T4, T5, T6, T7>(), values)
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
	where T5 : ICustomization, new()
	where T6 : ICustomization, new()
	where T7 : ICustomization, new();
