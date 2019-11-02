using System;
using System.Collections.Generic;
using UnityEngine;

namespace FerramAerospaceResearch
{
    // composition makes creating shallow ConfigNode copies trivial though every method needs to be reimplemented
    // using inheritance would save implementing most of the methods but would lose on references
    public class ConfigNodeWrapper : IConfigNode
    {
        private ConfigNode configNode;

        public ConfigNode Node
        {
            get { return configNode; }
            set { configNode = value; }
        }

        public ConfigNodeWrapper()
        {
            configNode = new ConfigNode();
        }

        public ConfigNodeWrapper(string name)
        {
            configNode = new ConfigNode(name);
        }

        public ConfigNodeWrapper(string name, string vcomment)
        {
            configNode = new ConfigNode(name, vcomment);
        }

        public ConfigNodeWrapper(ConfigNode node)
        {
            configNode = node;
        }

        public string Name
        {
            get { return configNode.name; }
            set { configNode.name = value; }
        }

        public string ID
        {
            get { return configNode.id; }
            set { configNode.id = value; }
        }

        public string Comment
        {
            get { return configNode.comment; }
            set { configNode.comment = value; }
        }

        public bool HasData
        {
            get { return configNode.HasData; }
        }

        public int CountValues
        {
            get { return configNode.CountValues; }
        }

        public int CountNodes
        {
            get { return configNode.CountNodes; }
        }

        public void CopyTo(IConfigNode node)
        {
            if (node is ConfigNodeWrapper wrapper)
                Node.CopyTo(wrapper.configNode);
        }

        public void CopyTo(IConfigNode node, bool overwrite)
        {
            if (node is ConfigNodeWrapper wrapper)
                Node.CopyTo(wrapper.configNode, overwrite);
        }

        public void CopyTo(IConfigNode node, string newName)
        {
            if (node is ConfigNodeWrapper wrapper)
                Node.CopyTo(wrapper.configNode, newName);
        }

        public IConfigNode CreateCopy()
        {
            return Wrap(configNode.CreateCopy());
        }

        public bool Save(string fileFullName)
        {
            return configNode.Save(fileFullName);
        }

        public bool Save(string fileFullName, string header)
        {
            return configNode.Save(fileFullName, header);
        }

        public void ClearData()
        {
            configNode.ClearData();
        }

        public void AddData(IConfigNode node)
        {
            if (node is ConfigNodeWrapper wrapper)
                configNode.AddData(wrapper.configNode);
        }

        public bool HasValue(string name)
        {
            return configNode.HasValue(name);
        }

        public bool HasValue()
        {
            return configNode.HasValue();
        }

        public void AddValue(string name, object value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, string value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, object value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, string value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, bool value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, bool value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, byte value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, byte value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, sbyte value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, sbyte value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, char value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, char value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, decimal value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, decimal value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, double value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, double value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, float value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, float value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, int value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, int value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, uint value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, uint value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, long value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, long value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, ulong value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, ulong value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, short value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, short value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, ushort value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, ushort value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Vector2 value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Vector2 value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Vector3 value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Vector3 value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Vector3d value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Vector3d value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Vector4 value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Vector4 value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Quaternion value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Quaternion value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, QuaternionD value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, QuaternionD value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Matrix4x4 value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Matrix4x4 value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Color value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Color value)
        {
            configNode.AddValue(name, value);
        }

        public void AddValue(string name, Color32 value, string vcomment)
        {
            configNode.AddValue(name, value, vcomment);
        }

        public void AddValue(string name, Color32 value)
        {
            configNode.AddValue(name, value);
        }

        public string GetValue(string name)
        {
            return configNode.GetValue(name);
        }

        public string GetValue(string name, int index)
        {
            return configNode.GetValue(name, index);
        }

        public string[] GetValues()
        {
            return configNode.GetValues();
        }

        public string[] GetValues(string name)
        {
            return configNode.GetValues(name);
        }

        public List<string> GetValuesList(string name)
        {
            return configNode.GetValuesList(name);
        }

        public string[] GetValuesStartsWith(string name)
        {
            return configNode.GetValuesStartsWith(name);
        }

        public bool SetValue(string name, string newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, string newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, string newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, string newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, bool newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, bool newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, bool newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, bool newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, byte newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, byte newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, byte newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, byte newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, sbyte newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, sbyte newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, sbyte newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, sbyte newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, char newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, char newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, char newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, char newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, decimal newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, decimal newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, decimal newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, decimal newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, double newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, double newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, double newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, double newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, float newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, float newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, float newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, float newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, int newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, int newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, int newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, int newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, uint newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, uint newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, uint newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, uint newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, long newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, long newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, long newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, long newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, ulong newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, ulong newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, ulong newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, ulong newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, short newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, short newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, short newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, short newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, ushort newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, ushort newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, ushort newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, ushort newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector2 newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Vector2 newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector2 newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Vector2 newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector3 newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Vector3 newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector3 newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Vector3 newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector3d newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Vector3d newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector3d newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Vector3d newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector4 newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Vector4 newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Vector4 newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Vector4 newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Quaternion newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Quaternion newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Quaternion newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(
            string name,
            Quaternion newValue,
            string vcomment,
            int index,
            bool createIfNotFound = false
        )
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, QuaternionD newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, QuaternionD newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, QuaternionD newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(
            string name,
            QuaternionD newValue,
            string vcomment,
            int index,
            bool createIfNotFound = false
        )
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Matrix4x4 newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Matrix4x4 newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Matrix4x4 newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Matrix4x4 newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Color newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Color newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Color newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Color newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public bool SetValue(string name, Color32 newValue, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, createIfNotFound);
        }

        public bool SetValue(string name, Color32 newValue, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, index, createIfNotFound);
        }

        public bool SetValue(string name, Color32 newValue, string vcomment, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, createIfNotFound);
        }

        public bool SetValue(string name, Color32 newValue, string vcomment, int index, bool createIfNotFound = false)
        {
            return configNode.SetValue(name, newValue, vcomment, index, createIfNotFound);
        }

        public void RemoveValue(string name)
        {
            configNode.RemoveValue(name);
        }

        public void RemoveValues(params string[] names)
        {
            configNode.RemoveValues(names);
        }

        public void RemoveValues(string startsWith)
        {
            configNode.RemoveValues(startsWith);
        }

        public void RemoveValuesStartWith(string startsWith)
        {
            configNode.RemoveValuesStartWith(startsWith);
        }

        public void ClearValues()
        {
            configNode.ClearValues();
        }

        public bool HasNodeID(string id)
        {
            return configNode.HasNodeID(id);
        }

        public bool HasNode(string name)
        {
            return configNode.HasNode(name);
        }

        public bool HasNode()
        {
            return configNode.HasNode();
        }

        public IConfigNode AddNode(string name)
        {
            return Wrap(configNode.AddNode(name));
        }

        public IConfigNode AddNode(string name, string vcomment)
        {
            return Wrap(configNode.AddNode(name, vcomment));
        }

        public IConfigNode AddNode(IConfigNode node)
        {
            if (node is ConfigNodeWrapper wrapper)
                configNode.AddNode(wrapper.configNode);
            return node;
        }

        public IConfigNode AddNode(string name, IConfigNode node)
        {
            if (!(node is ConfigNodeWrapper wrapper))
                return null;
            ConfigNode result = configNode.AddNode(name, wrapper.configNode);
            return result == null ? null : wrapper;
        }

        public IConfigNode GetNodeID(string id)
        {
            return Wrap(configNode.GetNodeID(id));
        }

        public IConfigNode GetNode(string name)
        {
            return Wrap(configNode.GetNode(name));
        }

        public IConfigNode GetNode(string name, string valueName, string value)
        {
            return Wrap(configNode.GetNode(name, valueName, value));
        }

        public IConfigNode GetNode(string name, int index)
        {
            return Wrap(configNode.GetNode(name, index));
        }

        public IConfigNode[] GetNodes(string name)
        {
            return Wrap(configNode.GetNodes(name));
        }

        public IConfigNode[] GetNodes(string name, string valueName, string value)
        {
            return Wrap(configNode.GetNodes(name, valueName, value));
        }

        public IConfigNode[] GetNodes()
        {
            return Wrap(configNode.GetNodes());
        }

        public bool SetNode(string name, IConfigNode newNode, bool createIfNotFound = false)
        {
            if (newNode is ConfigNodeWrapper wrapper)
                return configNode.SetNode(name, wrapper.configNode, createIfNotFound);
            return false;
        }

        public bool SetNode(string name, IConfigNode newNode, int index, bool createIfNotFound = false)
        {
            if (newNode is ConfigNodeWrapper wrapper)
                return configNode.SetNode(name, wrapper.configNode, index, createIfNotFound);
            return false;
        }

        public void RemoveNode(string name)
        {
            configNode.RemoveNode(name);
        }

        public void RemoveNode(IConfigNode node)
        {
            if (node is ConfigNodeWrapper wrapper)
                configNode.RemoveNode(wrapper.configNode);
        }

        public void RemoveNodesStartWith(string startsWith)
        {
            configNode.RemoveNodesStartWith(startsWith);
        }

        public void RemoveNodes(string name)
        {
            configNode.RemoveNodes(name);
        }

        public void ClearNodes()
        {
            configNode.ClearNodes();
        }

        public bool TryGetNode(string name, ref IConfigNode node)
        {
            if (node is ConfigNodeWrapper wrapper)
                return configNode.TryGetNode(name, ref wrapper.configNode);
            return false;
        }

        public bool TryGetValue(string name, ref string value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref string[] value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref float value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref double value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref int value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref uint value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref long value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref ulong value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref bool value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Vector3 value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Vector3d value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Vector2 value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Vector2d value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Vector4 value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Vector4d value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Quaternion value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref QuaternionD value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Rect value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Color value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetValue(string name, ref Color32 value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool HasValues(params string[] values)
        {
            return configNode.HasValues(values);
        }

        public bool TryGetValue(string name, ref Guid value)
        {
            return configNode.TryGetValue(name, ref value);
        }

        public bool TryGetEnum<T>(string name, ref T value, T defaultValue)
            where T : IComparable, IFormattable, IConvertible
        {
            return configNode.TryGetEnum(name, ref value, defaultValue);
        }

        public bool TryGetEnum(string name, Type enumType, ref Enum value)
        {
            return configNode.TryGetEnum(name, enumType, ref value);
        }

        public static IConfigNode Wrap(ConfigNode node)
        {
            return new ConfigNodeWrapper(node);
        }

        public static IConfigNode[] Wrap(ConfigNode[] nodes)
        {
            var wrappers = new IConfigNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                wrappers[i] = Wrap(nodes[i]);
            return wrappers;
        }
    }
}
