using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kv
{
    public class KvRecord : IEnumerable<KvRecord>
    {
        #region Constructor

        public KvRecord(string key, string value = null)
        {
            Key = key;
            _Value = value;
        }
        
        private KvRecord(KvRecord parent, string key, string value = null)
        {
            _Parent = parent;
            
            Key = key;
            _Value = value;
        }

        #endregion

        #region Clone
        
        /// <summary>
        /// Create deep copy of this object.
        /// </summary>
        /// <returns>New instance of <seealso cref="KvRecord"/> with same <see cref="Key"/>, <see cref="Value"/> and Child.</returns>
        public KvRecord Clone() => Clone(null);

        private KvRecord Clone(KvRecord parent)
        {
            KvRecord @new = new KvRecord(parent, Key, Value);

            if (HasChild)
            {
                if(@new._Child == null)
                    @new._Child = new Dictionary<string, KvRecord[]>();

                foreach (KeyValuePair<string, KvRecord[]> kvp in _Child)
                {
                    KvRecord[] arr = new KvRecord[kvp.Value.Length];

                    for (int i = 0; i < arr.Length; i++)
                        arr[i] = kvp.Value[i].Clone(@new);
                    
                    @new._Child[kvp.Key] = arr;
                }
            }

            return @new;
        }
        
        #endregion
        
        #region Data
        
        public string Key { get; }

        private string _Value;

        /// <summary>
        /// Value of this record.
        /// Records with child always have <see langref="null" /> as their value.
        /// </summary>
        public string Value
        {
            get => HasChild ? null : _Value;
            set
            {
                string oldValue = _Value;
                
                _Value = value;

                if (RecordValueChanged != null)
                    RecordValueChanged(this, oldValue);
            }
        }
        
        #endregion

        #region Escaped Data

        public string EscapedKey => Escape(Key);

        public string EscapedValue
        {
            get => Escape(Value);
            set => Value = Unescape(value);
        }

        public static string Escape(string input) => input?.Replace("\"", "\\\"");
        public static string Unescape(string input) => input?.Replace("\\\"", "\"");

        #endregion

        #region Events
        
        public delegate void RecordValueChangedHandler(KvRecord record, string oldValue);

        public event RecordValueChangedHandler RecordValueChanged;
        
        #endregion
        
        #region Children

        public IEnumerator<KvRecord> GetEnumerator()
        {
            foreach(KeyValuePair<string, KvRecord[]> kvp in _Child)
                foreach (KvRecord kv in kvp.Value)
                    yield return kv;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly KvRecord _Parent;
        public KvRecord Parent => _Parent;

        private Dictionary<string, KvRecord[]> _Child = null;

        public bool HasChild => _Child != null && _Child.Count > 0;

        /// <summary>
        /// Add new child.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Record with <paramref name="key"/> as <see cref="KvRecord.Key"/>.</returns>
        public KvRecord Add(string key, string value = null)
        {
            KvRecord kv = new KvRecord(this, key, value);

            if (_Child == null)
            {
                _Child = new Dictionary<string, KvRecord[]>
                {
                    [key] = new KvRecord[1] {kv}
                };

            }
            else
            {
                if (_Child.TryGetValue(key, out KvRecord[] childKv))
                {
                    int oldLen = childKv.Length;
                    Array.Resize(ref childKv, childKv.Length + 1);
                    childKv[oldLen] = kv;
                    
                    _Child[key] = childKv;
                }
                else
                    _Child[key] = new KvRecord[1] {kv};
            }

            return kv;
        }

        /// <summary>
        /// Add record (and its child).
        /// Uses <see cref="Clone()"/> to create copy before saving.
        /// </summary>
        /// <param name="record">Key-Value record to add.</param>
        /// <returns>Copy of <paramref name="record"/> which was inserted.</returns>
        public KvRecord Add(KvRecord record)
        {
            KvRecord kv = record.Clone(this);

            if (_Child == null)
            {
                _Child = new Dictionary<string, KvRecord[]>
                {
                    [kv.Key] = new KvRecord[1] {kv}
                };

            }
            else
            {
                if (_Child.TryGetValue(kv.Key, out KvRecord[] childKv))
                {
                    int oldLen = childKv.Length;
                    Array.Resize(ref childKv, childKv.Length + 1);
                    childKv[oldLen] = kv;
                    
                    _Child[kv.Key] = childKv;
                }
                else
                    _Child[kv.Key] = new KvRecord[1] {kv};
            }

            return kv;
        }

        /// <summary>
        /// Remove record based on <see cref="KvRecord.Key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Whenever there was a child with <paramref name="key"/> as <see cref="KvRecord.Key"/>.</returns>
        public bool RemoveAll(string key) => _Child.Remove(key);

        public KvRecord Get(string key) => _Child.TryGetValue(key, out KvRecord[] childKv) ? childKv[0] : null;

        public IEnumerable<KvRecord> GetAll(string key) => _Child.TryGetValue(key, out KvRecord[] childKv) ? childKv : null;

        #endregion

        #region Simple Get/Set
        
        private const NumberStyles _NumberStyles = NumberStyles.Number | NumberStyles.HexNumber;

        public string GetString() => Value;
        public byte GetByte(byte @default = 0) => byte.TryParse(GetString(), _NumberStyles, CultureInfo.InvariantCulture, out byte v) ? v : @default;
        public short GetShort(short @default = 0) => short.TryParse(GetString(), _NumberStyles, CultureInfo.InvariantCulture, out short v) ? v : @default;
        public int GetInt(int @default = 0) => int.TryParse(GetString(), _NumberStyles, CultureInfo.InvariantCulture, out int v) ? v : @default;
        public long GetLong(long @default = 0) => long.TryParse(GetString(), _NumberStyles, CultureInfo.InvariantCulture, out long v) ? v : @default;
        public float GetFloat(float @default = 0) => float.TryParse(GetString(), _NumberStyles | NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : @default;
        public double GetDouble(double @default = 0) => double.TryParse(GetString(), _NumberStyles | NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : @default;
        public decimal GetDecimal(decimal @default = 0) => decimal.TryParse(GetString(), _NumberStyles | NumberStyles.Float, CultureInfo.InvariantCulture, out decimal v) ? v : @default;
        public T GetEnum<T>(T @default) where T : struct, System.Enum => Enum.TryParse<T>(GetString(), true, out T e) ? e : @default;
        
        public void SetString(string val) => Value = val;
        public void SetByte(byte val) => SetString(val.ToString(CultureInfo.InvariantCulture));
        public void SetShort(short val) => SetString(val.ToString(CultureInfo.InvariantCulture));
        public void SetInt(int val) => SetString(val.ToString(CultureInfo.InvariantCulture));
        public void SetLong(long val) => SetString(val.ToString(CultureInfo.InvariantCulture));
        public void SetFloat(float val) => SetString(val.ToString("0.0##", CultureInfo.InvariantCulture));
        public void SetDouble(double val) => SetString(val.ToString("0.0##", CultureInfo.InvariantCulture));
        public void SetDecimal(decimal val) => SetString(val.ToString("0.0##", CultureInfo.InvariantCulture));
        public void SetEnum<T>(T val) where T : struct, System.Enum => SetString(Enum.GetName(typeof(T), val));

        #endregion

        #region Level-based Get/Set

        public int Levels
        {
            get
            {
                if (string.IsNullOrEmpty(Value))
                    return 0;
                return Value.Count((c) => c == ' ');
            }
        }

        public string GetStringByLevel(int level, string @default = null)
        {
            if (level <= 0)
                return @default;
            string[] split = Value.Split(' ');
            if (level >= split.Length)
                return split[split.Length - 1];
            else
                return split[level - 1];
        }
        public byte GetByteByLevel(int level, byte @default = 0) => byte.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles, CultureInfo.InvariantCulture, out byte v) ? v : @default;
        public short GetShortByLevel(int level, short @default = 0) => short.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles, CultureInfo.InvariantCulture, out short v) ? v : @default;
        public int GetIntByLevel(int level, int @default = 0) => int.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles, CultureInfo.InvariantCulture, out int v) ? v : @default;
        public long GetLongByLevel(int level, long @default = 0) => long.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles, CultureInfo.InvariantCulture, out long v) ? v : @default;
        public float GetFloatByLevel(int level, float @default = 0) => float.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles | NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : @default;
        public double GetDoubleByLevel(int level, double @default = 0) => double.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles | NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : @default;
        public decimal GetDecimalByLevel(int level, decimal @default = 0) => decimal.TryParse(GetStringByLevel(level, @default.ToString(CultureInfo.InvariantCulture)), _NumberStyles | NumberStyles.Float, CultureInfo.InvariantCulture, out decimal v) ? v : @default;
        public T GetEnumByLevel<T>(int level, T @default) where T : struct, System.Enum => Enum.TryParse<T>(GetStringByLevel(level, Enum.GetName(typeof(T), @default)), true, out T e) ? e : @default;

        public void SetStringByLevel(int level, string val)
        {
            if (level <= 0)
                return;
            if (string.IsNullOrEmpty(Value))
            {
                Value = val;
                return;
            }

            // Check current number of levels (creating split)
            string[] spl = Value.Split(' ');
            if (spl.Length <= level)
            {
                spl[level - 1] = val;
                Value = string.Join(" ", spl);
                return;
            }
            
            // Insert surfix values (when needed)
            int oldLen = spl.Length;
            string lastItem = spl[oldLen - 1];
            
            Array.Resize(ref spl, level);
            for (int i = oldLen; i < level - 1; i++)
                  spl[i] = lastItem;
            
            spl[level - 1] = val;

            // Join values back to single string
            Value = string.Join(" ", spl);
        }
        public void SetByteByLevel(int level, byte val) => SetStringByLevel(level, val.ToString(CultureInfo.InvariantCulture));
        public void SetShortByLevel(int level, short val) => SetStringByLevel(level, val.ToString(CultureInfo.InvariantCulture));
        public void SetIntByLevel(int level, int val) => SetStringByLevel(level, val.ToString(CultureInfo.InvariantCulture));
        public void SetLongByLevel(int level, long val) => SetStringByLevel(level, val.ToString(CultureInfo.InvariantCulture));
        public void SetFloatByLevel(int level, float val) => SetStringByLevel(level, val.ToString("0.0##", CultureInfo.InvariantCulture));
        public void SetDoubleByLevel(int level, double val) => SetStringByLevel(level, val.ToString("0.0##", CultureInfo.InvariantCulture));
        public void SetDecimalByLevel(int level, decimal val) => SetStringByLevel(level, val.ToString("0.0##", CultureInfo.InvariantCulture));
        public void SetEnumByLevel<T>(T val) where T : struct, System.Enum => SetString(Enum.GetName(typeof(T), val));

        #endregion
    }
}