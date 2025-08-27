using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace NTTaxi.Libraries.Extensions
{
    public class DDTP
    {
        private bool compressed;
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public DDTP() { }
        public DDTP(byte[] bytes) : this(new MemoryStream(bytes)) { }
        public DDTP(Stream stream, int timeoutMs = 12000) => LoadDDTP(stream);

        public void LoadDDTP(Stream stream)
        {
            byte[] header = new byte[4];
            if (stream.Read(header, 0, 4) != 4 || Encoding.UTF8.GetString(header) != "DDTP")
                throw new IOException("Invalid DDTP package");

            int compressFlag = stream.ReadByte();
            if (compressFlag < 0) throw new IOException("No data to read");
            compressed = compressFlag == 1;

            int count = ReadVarInt(stream);
            int dataLength = ReadVarInt(stream);

            byte[] dataBuf = new byte[dataLength];
            if (stream.Read(dataBuf, 0, dataLength) != dataLength)
                throw new IOException("Cannot read data");

            Stream dataStream = new MemoryStream(dataBuf);
            if (compressed)
                dataStream = new DeflateStream(dataStream, CompressionMode.Decompress);

            for (int i = 0; i < count; i++)
            {
                string key = ReadString(dataStream);
                object value = ReadObject(dataStream);
                data[key] = value;
            }
        }

        private int ReadVarInt(Stream stream)
        {
            int value = 0, count = 0;
            int b = stream.ReadByte();
            while (b >= 0)
            {
                if (count > 3 && (b & 0x7F) > 15) throw new IOException("Value out of range");
                count++;
                value = (value << 7) | (b & 0x7F);
                if ((b & 0x80) == 0) break;
                b = stream.ReadByte();
            }
            if (b < 0) throw new IOException("No data to read");
            return value;
        }

        private string ReadString(Stream stream)
        {
            int len = ReadVarInt(stream);
            byte[] buf = new byte[len];
            if (stream.Read(buf, 0, len) != len) throw new IOException("Cannot read string");
            return Encoding.UTF8.GetString(buf);
        }

        private object ReadObject(Stream stream)
        {
            int type = stream.ReadByte();
            if (type < 0) throw new IOException("No data to read");
            int len = ReadVarInt(stream);

            switch (type)
            {
                case 0:
                    byte[] strBuf = new byte[len];
                    stream.Read(strBuf, 0, len);
                    return Encoding.UTF8.GetString(strBuf);

                case 1:
                    var list = new List<object>();
                    for (int i = 0; i < len; i++)
                        list.Add(ReadObject(stream));
                    return list;

                case 2:
                    byte[] buf = new byte[len];
                    stream.Read(buf, 0, len);
                    return buf;

                default:
                    throw new IOException("Unsupported type");
            }
        }

        public Dictionary<string, object> GetData() => data;
        public string GetString(string key) => data.ContainsKey(key) ? data[key]?.ToString() : null;
        public byte[] GetByteArray(string key) => data.ContainsKey(key) && data[key] is byte[] b ? b : null;
        public List<object> GetList(string key) => data.ContainsKey(key) && data[key] is List<object> l ? l : null;

        public void SetString(string key, string value) => data[key] = value;
        public void SetByteArray(string key, byte[] value) => data[key] = value;
        public void SetList(string key, List<object> value) => data[key] = value;
    }
}