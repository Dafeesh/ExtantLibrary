using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Extant.Util;

namespace Extant.Logging
{
    public interface IByteRecord
    {
        ILongTally InboundBytes { get; }
        ILongTally OutboundBytes { get; }
    }

    public interface IByteRecording
    {
        IByteRecord ByteRecord { get; }
    }

    /// <summary>
    /// Used to record byte usage and returns analytics about the data.
    /// </summary>
    public class ByteRecorder : IByteRecord
    {
        private LongTally _inboundBytes;
        private LongTally _outboundBytes;

        public ByteRecorder()
            : this(0, 0)
        { }

        public ByteRecorder(long initialInboundCount, long initialOutboundCount)
        {
            this._inboundBytes = new LongTally(initialInboundCount);
            this._outboundBytes = new LongTally(initialOutboundCount);
        }

        public long AddInboundBytes(long amount)
        {
            return _inboundBytes.Add(amount);
        }

        public long AddOutboundBytes(long amount)
        {
            return _outboundBytes.Add(amount);
        }

        public ILongTally InboundBytes
        {
            get
            {
                return _inboundBytes;
            }
        }

        public ILongTally OutboundBytes
        {
            get
            {
                return _outboundBytes;
            }
        }
    }
}
