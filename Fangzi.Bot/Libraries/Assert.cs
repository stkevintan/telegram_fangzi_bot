using System;
using System.Diagnostics.CodeAnalysis;

namespace Fangzi.Bot.Libraries {
	public static class Assert
	{
		public static void NotNull([NotNull] object? o, string message = "null reference encounter")
		{
			if (o is null) throw new InvalidOperationException(message);
		}
	}
}