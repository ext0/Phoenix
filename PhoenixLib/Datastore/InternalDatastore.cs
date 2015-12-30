using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixLib.Datastore
{
    [Serializable()]
    public class InternalDatastore<K, V>
    {
        private Dictionary<K, V> data;
        private Dictionary<K, List<Action<object, DatastoreUpdateEventArgs<K, V>>>> listeners;
        public InternalDatastore(Dictionary<K, V> existing = null)
        {
            if (existing != null)
            {
                data = existing;
            }
            else
            {
                data = new Dictionary<K, V>();
            }
            listeners = new Dictionary<K, List<Action<object, DatastoreUpdateEventArgs<K,V>>>>();
        }
        public void addEntry(K key, V value, Client client)
        {
            data.Add(key, value);
            triggerListeners(key, value, client);
        }
        public bool hasKey(K key)
        {
            return data.ContainsKey(key);
        }
        public bool hasValue(V value)
        {
            return data.ContainsValue(value);
        }
        public void modifyEntry(K key, V value, Client client)
        {
            if (data.ContainsKey(key))
            {
                data[key] = value;
                triggerListeners(key, value, client);
            }
            else
            {
                addEntry(key, value, client);
            }
        }
        public bool removeKey(K key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                return true;
            }
            return false;
        }
        public V getValue(K key)
        {
            return data[key];
        }
        public bool containsValue(V value)
        {
            return data.ContainsValue(value);
        }
        public void triggerListeners(K key, V oldValue, Client client)
        {
            if (listeners.ContainsKey(key))
            {
                foreach (Action<Object, DatastoreUpdateEventArgs<K,V>> action in listeners[key])
                {
                    action(this, new DatastoreUpdateEventArgs<K, V>(key, oldValue, data[key], client));
                }
            }
        }
        public void removeListeners(K key)
        {
            if (listeners.ContainsKey(key))
            {
                listeners.Remove(key);
            }
        }
        public void addListener(K key, Action<object, DatastoreUpdateEventArgs<K, V>> callback)
        {
            if (!listeners.ContainsKey(key))
            {
                listeners.Add(key, new List<Action<Object, DatastoreUpdateEventArgs<K, V>>>{ callback });
            }
            else
            {
                foreach (Action<Object, DatastoreUpdateEventArgs<K, V>> action in listeners[key])
                {
                    if (action.Method.Equals(callback.Method))
                    {
                        return;
                    }
                }
                listeners[key].Add(callback);
            }
        }
    }

    [Serializable()]
    public class DatastoreUpdateEventArgs<K, V> : EventArgs
    {
        public K key { get; set; }
        public V oldValue { get; set; }
        public V newValue { get; set; }
        public Client client { get; set; }
        public DatastoreUpdateEventArgs(K key, V oldValue, V newValue, Client client)
        {
            this.key = key;
            this.oldValue = oldValue;
            this.newValue = newValue;
            this.client = client;
        }
    }
}
