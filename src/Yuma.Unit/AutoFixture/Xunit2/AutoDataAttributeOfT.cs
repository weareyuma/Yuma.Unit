#region Copyright & License

// Copyright © 2024-2025 Yuma
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
public sealed class AutoDataAttribute<T> : AutoDataAttribute
	where T : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T())) { }
}

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class AutoDataAttribute<T1, T2> : AutoDataAttribute
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T1())
		.Customize(new T2())) { }
}

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class AutoDataAttribute<T1, T2, T3> : AutoDataAttribute
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T1())
		.Customize(new T2())
		.Customize(new T3())) { }
}

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class AutoDataAttribute<T1, T2, T3, T4> : AutoDataAttribute
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T1())
		.Customize(new T2())
		.Customize(new T3())
		.Customize(new T4())) { }
}

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class AutoDataAttribute<T1, T2, T3, T4, T5> : AutoDataAttribute
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
	where T5 : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T1())
		.Customize(new T2())
		.Customize(new T3())
		.Customize(new T4())
		.Customize(new T5())) { }
}

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class AutoDataAttribute<T1, T2, T3, T4, T5, T6> : AutoDataAttribute
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
	where T5 : ICustomization, new()
	where T6 : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T1())
		.Customize(new T2())
		.Customize(new T3())
		.Customize(new T4())
		.Customize(new T5())
		.Customize(new T6())) { }
}

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public sealed class AutoDataAttribute<T1, T2, T3, T4, T5, T6, T7> : AutoDataAttribute
	where T1 : ICustomization, new()
	where T2 : ICustomization, new()
	where T3 : ICustomization, new()
	where T4 : ICustomization, new()
	where T5 : ICustomization, new()
	where T6 : ICustomization, new()
	where T7 : ICustomization, new()
{
	public AutoDataAttribute() : base(static () => new Fixture().Customize(new T1())
		.Customize(new T2())
		.Customize(new T3())
		.Customize(new T4())
		.Customize(new T5())
		.Customize(new T6())
		.Customize(new T7())) { }
}
