using System.IO.Compression;
using System.Text;

namespace NTTaxi.Libraries.Extensions
{
    public class DDTP
    {
        private const string HEADER = "DDTP";
        private bool _compressData;
        private readonly Dictionary<string, object> _data = new();
        private bool _requireResponse;

        public DDTP(bool compressData = false)
        {
            _compressData = compressData;
            SetLong("$Var.RequestTime$", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            _requireResponse = true;
        }

        public DDTP(Stream stream, int timeoutMs = 12000)
        {
            Load(stream, timeoutMs);
        }

        #region Load / Store

        public void Load(Stream stream, int timeoutMs = 12000)
        {
            byte[] headerBytes = new byte[HEADER.Length];
            if (stream.Read(headerBytes, 0, HEADER.Length) != HEADER.Length ||
                Encoding.UTF8.GetString(headerBytes) != HEADER)
                throw new IOException("Invalid DDTP package");

            int compressFlag = stream.ReadByte();
            if (compressFlag < 0) throw new IOException("No data to read");
            _compressData = compressFlag == 1;

            int keyCount = ReadVarInt(stream);
            int bodyLength = ReadVarInt(stream);

            byte[] bodyBytes = new byte[bodyLength];
            int read = 0;
            while (read < bodyLength)
            {
                int n = stream.Read(bodyBytes, read, bodyLength - read);
                if (n <= 0) throw new IOException("No data to read");
                read += n;
            }

            using var bodyStream = new MemoryStream(bodyBytes);
            Stream dataStream = _compressData ? new DeflateStream(bodyStream, CompressionMode.Decompress) : bodyStream;

            for (int i = 0; i < keyCount; i++)
            {
                string key = ReadVarString(dataStream);
                object value = ReadValue(dataStream);
                _data[key] = value;
            }
        }

        public void Store(Stream stream, bool compress = false)
        {
            using var bodyStream = new MemoryStream();
            Stream dataStream = compress ? new DeflateStream(bodyStream, CompressionLevel.Optimal, true) : bodyStream;

            foreach (var kv in _data)
            {
                WriteVarString(dataStream, kv.Key);
                WriteValue(dataStream, kv.Value);
            }

            if (compress) dataStream.Dispose(); // flush deflate stream

            byte[] bodyBytes = bodyStream.ToArray();
            stream.Write(Encoding.UTF8.GetBytes(HEADER));
            stream.WriteByte((byte)(compress ? 1 : 0));
            WriteVarInt(stream, _data.Count);
            WriteVarInt(stream, bodyBytes.Length);
            stream.Write(bodyBytes, 0, bodyBytes.Length);
        }

        #endregion

        #region Read / Write Helpers

        private static int ReadVarInt(Stream stream)
        {
            int result = 0;
            int shift = 0;
            int b;
            do
            {
                b = stream.ReadByte();
                if (b < 0) throw new IOException("No data to read");
                result |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return result;
        }

        private static void WriteVarInt(Stream stream, int value)
        {
            while (true)
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value == 0)
                {
                    stream.WriteByte(b);
                    break;
                }
                stream.WriteByte((byte)(b | 0x80));
            }
        }
        private static string ReadVarString(Stream stream)
        {
            int len = stream.ReadByte();
            if (len < 0) throw new IOException("No data to read");
            byte[] buf = new byte[len];
            if (stream.Read(buf, 0, len) != len) throw new IOException("No data to read");
            return Encoding.UTF8.GetString(buf);
        }

        private static void WriteVarString(Stream stream, string value)
        {
            byte[] buf = Encoding.UTF8.GetBytes(value);
            stream.WriteByte((byte)buf.Length);
            stream.Write(buf, 0, buf.Length);
        }

        private static object ReadValue(Stream stream)
        {
            int type = stream.ReadByte();
            if (type < 0) throw new IOException("No data to read");
            int len = stream.ReadByte();
            if (len < 0) throw new IOException("No data to read");

            return type switch
            {
                0 => Encoding.UTF8.GetString(stream.ReadExactly(len)),
                1 => ReadVector(stream, len),
                2 => stream.ReadExactly(len),
                _ => throw new IOException("Unsupported type")
            };
        }

        private static void WriteValue(Stream stream, object value)
        {
            switch (value)
            {
                case string s:
                    stream.WriteByte(0);
                    WriteVarString(stream, s);
                    break;
                case byte[] b:
                    stream.WriteByte(2);
                    WriteVarInt(stream, b.Length);
                    stream.Write(b, 0, b.Length);
                    break;
                case List<object> list:
                    stream.WriteByte(1);
                    WriteVarInt(stream, list.Count);
                    foreach (var item in list) WriteValue(stream, item);
                    break;
                default:
                    string str = value?.ToString() ?? "";
                    stream.WriteByte(0);
                    WriteVarString(stream, str);
                    break;
            }
        }

        private static List<object> ReadVector(Stream stream, int count)
        {
            var list = new List<object>();
            for (int i = 0; i < count; i++)
            {
                list.Add(ReadValue(stream));
            }
            return list;
        }

        #endregion

        #region Data Access

        public void SetString(string key, string value) => _data[key] = value;
        public string GetString(string key) => _data.TryGetValue(key, out var v) ? v.ToString() : null;

        public void SetByteArray(string key, byte[] value) => _data[key] = value;
        public byte[] GetByteArray(string key) => _data.TryGetValue(key, out var v) ? v as byte[] : null;

        public void SetBoolean(string key, bool value) => _data[key] = value ? "true" : "false";
        public bool GetBoolean(string key) => _data.TryGetValue(key, out var v) && v?.ToString() == "true";

        public void SetLong(string key, long value) => _data[key] = value.ToString();
        public long GetLong(string key) => long.Parse(_data[key].ToString());

        public void SetVector(string key, List<object> list) => _data[key] = list;
        public List<object> GetVector(string key) => _data.TryGetValue(key, out var v) ? v as List<object> : null;

        public Dictionary<string, object> Data => _data;

        #endregion
    }

    public static class StreamExtensions
    {
        public static byte[] ReadExactly(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int read = 0;
            while (read < count)
            {
                int n = stream.Read(buffer, read, count - read);
                if (n <= 0) throw new IOException("Unexpected end of stream");
                read += n;
            }
            return buffer;
        }
    }
}
