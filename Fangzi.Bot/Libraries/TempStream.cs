using System;
using System.IO;
namespace Fangzi.Bot.Libraries;

public class TempStream : IDisposable
{
	public string Location { get; }
	public TempStream(Stream stream, string prefix = "bin")
	{
		Location = Path.Join(Path.GetTempPath(), $"{prefix}-{System.Guid.NewGuid()}.tmp");
		stream.Position = 0;
		using (var fs = File.Create(Location))
		{
			stream.CopyTo(fs);
		}
		stream.Position = 0;
	}

	public void Dispose()
	{
		try
		{
			System.IO.File.Delete(Location);
		}
		catch { }
	}
}