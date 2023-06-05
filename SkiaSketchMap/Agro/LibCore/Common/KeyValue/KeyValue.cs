using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Agro.LibCore
{
    [Serializable()]
    [DataContract]
    public class KeyValue<TKey, TValue>// : CDObject
    {
        #region Properties

        [DataMember]
        public TKey Key { get; set; }
        [DataMember]
        public TValue Value { get; set; }

        #endregion

        #region Ctor

        public KeyValue()
        {
        }

        public KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        #endregion

        #region Methods

        #region Methods - Override

        public override string ToString()
        {
            return string.Format("{0} : {1}", Key, Value);
        }

        #endregion

        #endregion
    }

    [Serializable()]
    public class StringStringPair : KeyValue<string, string>
    {
        #region Ctor

        public StringStringPair()
        {
        }

        public StringStringPair(string key, string value)
            : base(key, value)
        {
        }

        #endregion
    }

    [Serializable()]
    public class StringObjectPair : KeyValue<string, object>
    {
        #region Ctor

        public StringObjectPair()
        {
        }

        public StringObjectPair(string key, object value)
            : base(key, value)
        {
        }

        #endregion
    }

}
