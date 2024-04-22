using System.Buffers;

using GenHTTP.Api.Protocol;

using PooledAwait;

namespace GenHTTP.Engine.Protocol.Parser
{

    internal sealed class RequestScanner
    {

        internal RequestToken Current { get; private set; }

        internal ReadOnlySequence<byte> Value { get; private set; }

        internal ScannerMode Mode { get; set; }

        internal RequestScanner()
        {
            Current = RequestToken.None;
            Mode = ScannerMode.Words;
        }

        internal async PooledValueTask<bool> Next(RequestBuffer buffer, RequestToken expectedToken)
        {
            var read = await Next(buffer, forceRead: false);

            if (read != expectedToken)
            {
                throw new ProtocolException($"Unexpected token '{read}' (expected '{expectedToken}')");
            }

            return true;
        }

        internal async PooledValueTask<RequestToken> Next(RequestBuffer buffer, bool forceRead = false)
        {
            // ensure we have data to be scanned
            if (await Fill(buffer, forceRead))
            {
                var found = ScanData(buffer);

                if (found != null)
                {
                    return Current = found.Value;
                }
            }

            // did not recognize any tokens, probably due to missing input data
            if (!forceRead)
            {
                return await Next(buffer, forceRead: true);
            }
            else
            {
                throw new ProtocolException("No more data to be available to be parsed");
            }
        }

        private RequestToken? ScanData(RequestBuffer buffer)
        {
            if (Mode == ScannerMode.Words)
            {
                if (ReadTo(buffer, ' ', boundary: '\r'))
                {
                    return RequestToken.Word;
                }
                else if (ReadTo(buffer, '\r', skipAdditionally: 1))
                {
                    return RequestToken.Word;
                }
            }
            else if (Mode == ScannerMode.Path)
            {
                if (ReadTo(buffer, '?', boundary: '\r'))
                {
                    return RequestToken.PathWithQuery;
                }
                else if (ReadTo(buffer, ' ', boundary: '\r'))
                {
                    return RequestToken.Path;
                }
            }
            else if (Mode == ScannerMode.HeaderKey)
            {
                if (IsNewLine(buffer))
                {
                    return RequestToken.NewLine;
                }
                else if (ReadTo(buffer, ':', boundary: '\r', skipAdditionally: 1))
                {
                    return RequestToken.Word;
                }
            }
            else if (Mode == ScannerMode.HeaderValue)
            {
                if (ReadTo(buffer, '\r', skipAdditionally: 1))
                {
                    return RequestToken.Word;
                }
            }

            return null;
        }

        private static bool IsNewLine(RequestBuffer buffer)
        {
            if (buffer.Data.First.Span[0] == (byte)'\r')
            {
                buffer.Advance(2);
                return true;
            }

            return false;
        }

        private bool ReadTo(RequestBuffer buffer, char delimiter, char? boundary = null, byte skipAdditionally = 0)
        {
            var reader = new SequenceReader<byte>(buffer.Data);

            if (reader.TryReadTo(out ReadOnlySequence<byte> value, (byte)delimiter, true))
            {
                if (boundary != null)
                {
                    var boundaryReader = new SequenceReader<byte>(buffer.Data);

                    if (boundaryReader.TryReadTo(out ReadOnlySequence<byte> boundaryData, (byte)boundary))
                    {
                        if (boundaryData.Length < value.Length)
                        {
                            return false;
                        }
                    }
                }

                Value = value;
                buffer.Advance(value.Length + 1 + skipAdditionally);

                return true;
            }

            return false;
        }

        private static async PooledValueTask<bool> Fill(RequestBuffer buffer, bool force = false)
        {
            if (buffer.ReadRequired || force)
            {
                await buffer.Read(force).ConfigureAwait(false);
            }

            return !buffer.Data.IsEmpty;
        }

    }

}
