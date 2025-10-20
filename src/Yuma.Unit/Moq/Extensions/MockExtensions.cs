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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.Moq;
using Moq;

namespace Yuma.Moq.Extensions;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
public static class MockExtensions
{
	public static DbSet<T> AsDbSetMock<T>(this IEnumerable<T> source)
		where T : class
	{
		var data = source as ICollection<T> ?? source.ToList();
		return data.BuildMockDbSet()
			.Object;
	}

	public static IQueryable<T> AsQueryableMock<T>(this IEnumerable<T> source)
		where T : class
	{
		var data = source as ICollection<T> ?? source.ToList();
		return data.BuildMock();
	}

	public static Mock<T> AsMock<T>(this T t)
		where T : class
	{
		return Mock.Get(t);
	}
}
