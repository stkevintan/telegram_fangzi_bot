using System;
using System.IO;
using OpenCvSharp;
using HeyRed.Mime;
using Fangzi.Bot.Libraries;

namespace Fangzi.Bot.Services
{
	public class AvatarService : IDisposable
	{
		readonly string _mimeType;
		
		Mat _src;

		static Random _rnd = new Random();

		public string MimeType => _mimeType;

		public static AvatarService? FromStream(Stream stream)
		{
			stream.Position = 0;
			var mimeType = MimeGuesser.GuessMimeType(stream);
			stream.Position = 0;
			var tmp = mimeType.Split("/", 2);
			Mat? src = (tmp[0], tmp[1]) switch
			{
				("image", "gif") or ("video", _) => MatFromVideo(stream),
				("image", _) => Mat.FromStream(stream, ImreadModes.Unchanged),
				_ => null
			};
			return src is null ? null : new AvatarService(src, mimeType);
		}

		static Mat? MatFromVideo(Stream stream)
		{
			using var tmpStream = new TempStream(stream);
			var src = new Mat();
			var error = true;
			try
			{
				using var capture = new VideoCapture(tmpStream.Location);
				var count = Convert.ToInt32(capture.Get(VideoCaptureProperties.FrameCount));
				var frameIndex = _rnd.Next(count);
				capture.Set(VideoCaptureProperties.PosFrames, frameIndex);

				if (capture.Read(src))
				{
					error = false;
					return src;
				}
				return null;
			}
			finally
			{
				if (error) src.Dispose();
			}

		}

		AvatarService(Mat src, string mimeType)
		{
			MimeGuesser.MagicFilePath = "/usr/lib/file/magic.mgc";
			_mimeType = mimeType;
			_src = src;
		}


		public void Dispose()
		{
			_src.Dispose();
		}

		public AvatarService Resize(double baseSize = 2048.0)
		{
			var rows = _src.Rows;
			var cols = _src.Cols;
			var scaleFactor = baseSize / Math.Min(rows, cols);
			var dst = new Mat();
			var error = true;
			try
			{
				Cv2.Resize(
					_src,
					dst,
					dsize: Size.Zero,
					fx: scaleFactor,
					fy: scaleFactor,
					interpolation: InterpolationFlags.Area
				);
				error = false;
				return updateSrc(dst);
			}
			finally
			{
				if (error) dst.Dispose();
			}
		}

		public AvatarService AddImageBackground(in (int, int, int) color)
		{
			// if not rgba
			if (_src.Channels() != 4)
			{
				return this;
			}
			(int r, int g, int b) = color;
			var dst = new Mat(new Size(_src.Width, _src.Height), MatType.CV_8UC3);
			var error = true;
			try
			{
				var sIndexer = _src.GetGenericIndexer<Vec4b>();
				var dIndexer = dst.GetGenericIndexer<Vec3b>();

				for (int y = 0; y < _src.Height; y++)
				{
					for (int x = 0; x < _src.Width; x++)
					{
						Vec4b sColor = sIndexer[y, x];
						Vec3b dColor = dIndexer[y, x];
						var alpha = sColor.Item3 / 255.0;
						dColor.Item0 = Convert.ToByte((1 - alpha) * b + alpha * sColor.Item0);
						dColor.Item1 = Convert.ToByte((1 - alpha) * g + alpha * sColor.Item1);
						dColor.Item2 = Convert.ToByte((1 - alpha) * r + alpha * sColor.Item2);
						// Don't forget set it back
						dIndexer[y, x] = dColor;
					}
				}
				error = false;
				return updateSrc(dst);
			}
			finally
			{
				if (error) dst.Dispose();
			}
		}

		public Stream toStream()
		{
			var output = new MemoryStream();
			var error = true;
			try
			{
				_src.WriteToStream(output);
				output.Position = 0;
				error = false;
				return output;
			}
			finally
			{
				if (error) output.Dispose();
			}
		}

		AvatarService updateSrc(in Mat dst)
		{
			_src.Dispose();
			_src = dst;
			return this;
		}
	}
}