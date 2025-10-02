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
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Yuma.AutoFixture;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
public class AutoDbContextCustomization : ICustomization
{
	#region ICustomization Members

	[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated by AutoFixture.")]
	public void Customize(IFixture fixture)
	{
		fixture.Customize<DbContext>(static composer => composer.FromFactory(static () => new Mock<DbContext>().Object));
	}

	#endregion
}
