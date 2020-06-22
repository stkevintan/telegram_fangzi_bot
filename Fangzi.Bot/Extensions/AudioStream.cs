using System;
using System.IO;
using Microsoft.CognitiveServices.Speech;

namespace Fangzi.Bot.Extensions
{
    public class AudioStream : Stream
    {
        public static implicit operator AudioStream(AudioDataStream dataStream) {
            return new AudioStream(dataStream);
        }
        AudioDataStream dataStream;
        public AudioStream(AudioDataStream stream)
        {
            dataStream = stream;
        }

        public override long Position
        {
            get
            {
                return (long)dataStream.GetPosition();
            }
            set
            {
                dataStream.SetPosition((uint)value);
            }
        }
        public override long Length
        {
            get
            {
                return Position;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return (int)dataStream.ReadData(buffer);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dataStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}