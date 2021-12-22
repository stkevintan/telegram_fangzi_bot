using System;
using System.Diagnostics.CodeAnalysis;

namespace Fangzi.Bot.Libraries {
	public static class Assert
	{
		public static void NotNull([NotNull] object? o)
		{
			if (o is null) throw new InvalidOperationException("It was null!");
		}
	}
}