using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extant.Unity.Caching
{
    public static class ResourceBank
    {
        private static long _maxSize = 20;
        static Dictionary<string, ObjectLifePair> _bank = new Dictionary<string, ObjectLifePair>();

        public static long MaxSize
        {
            get { return _maxSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("MaxSize must be greater than zero.");
                _maxSize = value;
                TrimExcess();
            }
        }

        public static UnityEngine.GameObject Instantiate(string path)
        {
            return (UnityEngine.GameObject)UnityEngine.Object.Instantiate(_bank.ContainsKey(path) ?
                _bank[path].Object :
                AddToBank(path));
        }

        private static UnityEngine.Object AddToBank(string path)
        {
            UnityEngine.Object obj;
            try
            {
                obj = UnityEngine.Resources.Load(path);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Unable to load resource: " + path + " \n" + e.ToString());
            }

            IncreaceGeneration();
            _bank.Add(path, new ObjectLifePair(obj));

            TrimExcess();
            return obj;
        }

        private static void IncreaceGeneration()
        {
            foreach (var ofp in _bank)
            {
                ofp.Value.IncreaseGeneration();
            }
        }

        private static void TrimExcess()
        {
            if (_bank.Count > MaxSize)
            {
                long toRemoveCount = MaxSize - _bank.Count;
                KeyValuePair<string, ObjectLifePair> oldest;
                while (toRemoveCount > 0 && _bank.Count > 0)
                {
                    oldest = _bank.First();
                    foreach (var kvp in _bank)
                    {
                        if (kvp.Value.Generation >= oldest.Value.Generation)
                        {
                            oldest = kvp;
                            toRemoveCount--;
                        }
                    }
                    UnityEngine.Resources.UnloadAsset(_bank[oldest.Key].Object);
                    _bank.Remove(oldest.Key);
                }
            }
        }
    }
}
