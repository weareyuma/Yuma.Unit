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

using AutoFixture.AutoMoq;
using MicroElements.AutoFixture.NodaTime;
using Moq;
using NodaTime;

namespace Yuma.AutoFixture.Xunit2;

public class InlineAutoDataAttributeFixture
{
	[Theory]
	[InlineAutoData<NodaTimeCustomization>(1.2)]
	public void AllowInlineDataWith1AutoDataCustomization(decimal number, LocalDate date)
	{
		number.Should()
			.Be(expected: 1.2m);

		date.Should()
			.BeOfType<LocalDate>()
			.And.NotBeNull();
	}

	[Theory]
	[InlineAutoData<NodaTimeCustomization, AutoMoqCustomization>(1.2)]
	public void AllowInlineDataWith2AutoDataCustomization(decimal number, LocalDate date, IClock clock)
	{
		number.Should()
			.Be(expected: 1.2m);

		date.Should()
			.BeOfType<LocalDate>()
			.And.NotBeNull();

		Mock.Get(clock)
			.Should()
			.BeOfType<Mock<IClock>>()
			.And.NotBeNull();
	}
}
