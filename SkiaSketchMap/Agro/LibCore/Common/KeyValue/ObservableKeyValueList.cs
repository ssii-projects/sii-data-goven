using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Agro.LibCore
{
    [Serializable()]
    public class ObservableKeyValueList<TKey, TValue> : ObservableCollection<KeyValue<TKey, TValue>>
    {
        #region Properties

        public virtual TValue this[TKey key]
        {
            get { return GetValue(key); }
            set { SetValue(key, value); }
        }

        #endregion

        #region Ctor

        public ObservableKeyValueList()
        {
        }

        #endregion

        #region Methods

        #region Methods - Public

        //public void Add(TKey key, TValue value)
        //{
        //    Add(new KeyValue<TKey, TValue>(key, value));
        //}

        public bool ContainsKey(TKey key)
        {
            return this.Any(c => c.Key.Equals(key));
        }

        public void Remove(TKey key)
        {
            for (int i = 0; i < this.Count; i++)
            {
                var item = this[i];
                if (item.Key.Equals(key))
                {
                    this.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region Methods - Private

        private TValue GetValue(TKey key)
        {
            var pair = this.Where(c => c.Key.Equals(key)).FirstOrDefault();
            if (pair == null)
            {
                pair = new KeyValue<TKey, TValue>() { Key = key };
                Add(pair);
            }

            return pair.Value;
        }

        private void SetValue(TKey key, TValue value)
        {
            var pair = this.Where(c => c.Key.Equals(key)).FirstOrDefault();
            if (pair == null)
            {
                pair = new KeyValue<TKey, TValue>() { Key = key };
                Add(pair);
            }

            pair.Value = value;
        }

        #endregion

        #endregion
    }
}
