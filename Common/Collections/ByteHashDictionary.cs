using MCOP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Common.Collections
{
    public class ByteHashDictionary
    {
        public ConcurrentDictionary<ulong, List<byte[]>> Data { get; set; }
        public ByteHashDictionary()
        {
            Data = new ConcurrentDictionary<ulong, List<byte[]>>();
        }

        public bool ContainsKey(ulong id)
        {
            return Data.ContainsKey(id);
        }

        public void Add(ulong id, List<byte[]> bytes)
        {
            Data.TryAdd(id, bytes);
        }

        public void AddHash(ulong id, byte[] bytes)
        {
            if (ContainsKey(id))
            {
                Data[id].Add(bytes);
            }
            else
            {
                List<byte[]> list = new List<byte[]>();
                list.Add(bytes);
                Data.TryAdd(id, list);
            }
        }

        public bool RemoveHash(ulong id, byte[] bytes)
        {
            if (ContainsKey(id))
            {
                Data[id].Remove(bytes);
                return true;
            }
            return false;
        }

        public bool Remove(ulong id)
        {
            return Data.TryRemove(id, out _);
        }

        public bool TryCheckHash(byte[] hash, double minProcent, out ulong id, out double procent)
        {
            foreach (var item in Data)
            {
                foreach (var hs in item.Value)
                {
                    double percentage = ImageProcessorService.GetPercentageDifference(hash, hs);
                    if (percentage >= minProcent)
                    {
                        id = item.Key;
                        procent = percentage;
                        return true;
                    }
                }
            }
            procent = default;
            id = default;
            return false;
        }

        public void SerializeAsJson(string path)
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public void DeserializeFromJson(string path)
        {
            var json = string.Empty;

            using (var fs2 = File.OpenRead(path))
            {
                using (var sr2 = new StreamReader(fs2, new UTF8Encoding(false)))
                {
                    json = sr2.ReadToEnd();
                }
            }
            Data = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, List<byte[]>>>(json) ?? new();
        }

    }
}
