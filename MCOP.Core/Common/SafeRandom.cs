using System.Security.Cryptography;

namespace MCOP.Core.Common
{
    public sealed class SafeRandom : IDisposable
    {
        private readonly RandomNumberGenerator rng;


        public SafeRandom()
        {
            rng = RandomNumberGenerator.Create();
        }

        ~SafeRandom()
        {
            rng.Dispose();
        }


        public void Dispose()
            => rng.Dispose();

        public bool NextBool(int trueRatio = 1)
        {
            if (trueRatio <= 0)
                throw new ArgumentOutOfRangeException(nameof(trueRatio), "Ratio must be positive.");

            return Next() % (trueRatio + 1) > 0;
        }

        public byte[] GetBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must get at least 1 byte.");

            byte[] bytes = new byte[count];
            rng.GetBytes(bytes);
            return bytes;
        }

        public void GetBytes(int count, out byte[] bytes)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must get at least 1 byte.");

            bytes = new byte[count];
            rng.GetBytes(bytes);
        }

        public byte GetU8()
            => GetBytes(1)[0];

        public sbyte GetS8()
            => (sbyte)GetBytes(1)[0];

        public ushort GetU16()
            => BitConverter.ToUInt16(GetBytes(2), 0);

        public short GetS16()
            => BitConverter.ToInt16(GetBytes(2), 0);

        public uint GetU32()
            => BitConverter.ToUInt32(GetBytes(4), 0);

        public int GetS32()
            => BitConverter.ToInt32(GetBytes(4), 0);

        public ulong GetU64()
            => BitConverter.ToUInt64(GetBytes(8), 0);

        public long GetS64()
            => BitConverter.ToInt64(GetBytes(8), 0);

        public int Next()
            => Next(0, int.MaxValue);

        public int Next(int maxExcluded)
            => Next(0, maxExcluded);

        public int Next(int min, int max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum needs to be greater than minimum.");

            int offset = 0;
            if (min < 0)
                offset = -min;

            min += offset;
            max += offset;

            return Math.Abs(GetS32()) % (max - min) + min - offset;
        }

        public T ChooseRandomElement<T>(IEnumerable<T> collection)
            => collection.ElementAt(Next(collection.Count()));

        public KeyValuePair<T, K> ChooseRandomElement<T, K>(IDictionary<T, K> collection)
            => collection.ElementAt(Next(collection.Count));

        public char ChooseRandomChar(string str)
            => str[Next(0, str.Length)];

        public T? ChooseRandomEnumValue<T>() where T : Enum
        {
            Array v = Enum.GetValues(typeof(T));
            return (T?)v.GetValue(Next(v.Length));
        }
    }

}
