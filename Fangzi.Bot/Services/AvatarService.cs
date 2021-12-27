using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Linq;
using System.IO;
using OpenCvSharp;
using System.Text.RegularExpressions;
using Fangzi.Bot.Interfaces;
using HeyRed.Mime;

namespace Fangzi.Bot.Services
{

    public record class AvatarResult: IDisposable
    {
        public static AvatarResult NotFound = new AvatarResult { ErrorMessage = "图呢？？？" };
        public static AvatarResult TooBig = new AvatarResult { ErrorMessage = "太大了，换一个！" };
        public static AvatarResult NoImageType = new AvatarResult { ErrorMessage = "不支持的文件类型QaQ" };
        public string? ErrorMessage;
        public Stream? Stream;

		public void Dispose()
		{
            Stream?.Dispose();
		}

		public Task Match(Func<string, Task> onError, Func<Stream, Task> onOk)
        {
            if (ErrorMessage is string message)
            {
                return onError(message);
            }
            return onOk(Stream!);
        }
    }

    public class AvatarService
    {
        static int _max_size = 15 * (1 << 20);
        static Regex colorRegex = new Regex(@"(?:\s|^)\/c(?:olor)?\s*=\s*#([0-9a-zA-Z]{3}|[0-9a-zA-Z]{6})\b");
        static Regex faceRegex = new Regex(@"(?:\s|^)\/face\s*=\s*(\d+)\b");

        ITelegramBotClient _bot;

        public AvatarService(ITelegramBotClient bot)
        {
            _bot = bot;
            MimeGuesser.MagicFilePath = "/usr/lib/file/magic.mgc";
        }

        public async Task<AvatarResult> FromPhotoAsync(ISession session)
        {
            var photo = session.Message.ReplyToMessage?.Photo?.MaxBy(p => p.FileSize);
            return await DownloadImage(photo, session, nameof(photo), "image");
        }

        public async Task<AvatarResult> FromChatPhotoChangedAsync(ISession session)
        {
            var photo = session.Message.ReplyToMessage?.NewChatPhoto?.MaxBy(p => p.FileSize);
            return await DownloadImage(photo, session, nameof(photo), "image");
        }

        public async Task<AvatarResult> FromStickerAsync(ISession session)
        {
            var sticker = session.Message.ReplyToMessage?.Sticker;
            return await DownloadImage(sticker, session, nameof(sticker), sticker!.IsAnimated ? null : "image");
        }

        public async Task<AvatarResult> FromDocumentAsync(ISession session)
        {
            var doc = session.Message.ReplyToMessage?.Document;
            return await DownloadImage(doc, session, nameof(doc));

        }

        public async Task<AvatarResult> DownloadImage(FileBase? File, ISession session, string prefix, string? mimeType = null)
        {
            if (File is null)
            {
                return AvatarResult.NotFound;
            }

            if (File.FileSize > _max_size)
            {
                return AvatarResult.TooBig;
            }
            using var stream = new MemoryStream();
            await _bot.GetInfoAndDownloadFileAsync(File.FileId, stream);
            // reset Position
            stream.Position = 0;
            var MimeType = mimeType ?? MimeGuesser.GuessMimeType(stream);
            // reset position
            stream.Position = 0;
            if(MimeType == "application/gzip")
            {
               // TODO 
            } 
            if (!checkMimeType(MimeType))
            {
                return AvatarResult.NoImageType;
            }
            // TODO: snap a shot on that video
            (Scalar bg, int face) = parseText(session.Content);
            using var src = Mat.FromStream(stream, ImreadModes.Unchanged);
            using var src1 = addImageBackground(src, bg);
            using var dst = resize(src1);
            var output = new MemoryStream();
            dst.WriteToStream(output);
            // reset position
            output.Position = 0;
            return new AvatarResult { Stream = output };
        }

        bool checkMimeType(string mimeType)
        {
            return mimeType.Split("/")[0] switch
            {
                "image" => true,
                "video" => true,
                _ => false
            };
        }

        (Scalar, int) parseText(string text)
        {
            // /color=<hex> /face=<int>
            var bg = new Scalar(255, 255, 255);
            var face = 0;
            var colorRet = colorRegex.Match(text);
            if (colorRet.Success && colorRet.Groups[1] != null)
            {
                var hex = colorRet.Groups[1]!.Value;
                var indexer = hex.Length switch
                {
                    3 => new int[,] { { 0, 0 }, { 1, 1 }, { 2, 2 } },
                    _ => new int[,] { { 0, 1 }, { 2, 3 }, { 4, 5 } }
                };
                var c3 = Enumerable.Range(0, 3)
                    .Select(i => (indexer[i, 0], indexer[i, 1]))
                    .Select(p => $"{hex[p.Item1]}{hex[p.Item2]}")
                    .Select(s => Convert.ToInt32(s, 16))
                    .ToList();

                bg = Scalar.FromRgb(c3[0], c3[1], c3[2]);
            }
            var faceRet = faceRegex.Match(text);
            if (faceRet.Success && faceRet.Groups[1] != null)
            {
                face = Convert.ToInt32(faceRet.Groups[1]!.Value);
            }
            return (bg, face);
        }

        Mat resize(in Mat src)
        {
            var rows = src.Rows;
            var cols = src.Cols;
            var scaleFactor = 2048.0 / Math.Min(rows, cols);
            var dst = new Mat();
            Cv2.Resize(
                src,
                dst,
                dsize: Size.Zero,
                fx: scaleFactor,
                fy: scaleFactor,
                interpolation: InterpolationFlags.Area
            );
            return dst;
        }

        Mat addImageBackground(in Mat src, Scalar color)
        {
            // if not rgba
            if (src.Channels() != 4)
            {
                return src;
            }
            (var b, var g, var r) = (color[0], color[1], color[2]);
            // var dst = new Mat<Vec3b>(src);
            var dst = new Mat(new Size(src.Width, src.Height), MatType.CV_8UC3);
            var sIndexer = src.GetGenericIndexer<Vec4b>();
            var dIndexer = dst.GetGenericIndexer<Vec3b>();

            for (int y = 0; y < dst.Height; y++)
            {
                for (int x = 0; x < dst.Width; x++)
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
            return dst;
        }

    }
}