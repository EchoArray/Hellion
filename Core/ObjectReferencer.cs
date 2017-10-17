using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
namespace Echo
{
    sealed public class ValueReference : System.Attribute
    {
        /// <summary>
        /// Identifies the name of the value
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public ValueReference(string identifier)
        {
            _name = identifier;
        }
    }

    sealed public class MethodReference : System.Attribute
    {
        /// <summary>
        /// Identifies the name of the value
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public MethodReference(string identifier)
        {
            _name = identifier;
        }
    }

    sealed internal class ObjectReferencer
    {
        private static Dictionary<Type, IEnumerable<MemberInfo>> _typeMemberInfos = new Dictionary<Type, IEnumerable<MemberInfo>>();
        private static Dictionary<string, PropertyInfo> _propertyInfos = new Dictionary<string, PropertyInfo>();
        private static Dictionary<string, FieldInfo> _fieldInfos = new Dictionary<string, FieldInfo>();

        private static Dictionary<Type, IEnumerable<MemberInfo>> _methodMemberInfos = new Dictionary<Type, IEnumerable<MemberInfo>>();
        private static Dictionary<string, MethodInfo> _methodInfos = new Dictionary<string, MethodInfo>();


        internal static System.Object GetValue(System.Object rootObject, string path)
        {
            if (rootObject == null)
                return null;


            // Split path by separator
            string[] pathDirectories = path.Split('/');
            string storedKey = rootObject.ToString() + path;
            string location = string.Empty;

            System.Object currentObject = rootObject;
            for (int i = 0; i < pathDirectories.Length - 1; i++)
            {
                // Obtain parent
                if (i == 0)
                    location += pathDirectories[i];
                else
                    location += "/" + pathDirectories[i];

                storedKey = rootObject.ToString() + location;

                currentObject = FindGetValue(currentObject, pathDirectories[i], storedKey);

                // If we couldn't obtain it, abort
                if (currentObject == null)
                    return null;
            }
            storedKey = rootObject.ToString() + path;
            return FindGetValue(currentObject, pathDirectories[pathDirectories.Length - 1], storedKey);
        }
        internal static System.Object FindGetValue(System.Object obj, string name, string storedKey)
        {

            // Of the object has already been located and stored, return it
            if (_propertyInfos.ContainsKey(storedKey))
                return _propertyInfos[storedKey].GetValue(obj, null);
            else if (_fieldInfos.ContainsKey(storedKey))
                return _fieldInfos[storedKey].GetValue(obj);
            else
            {
                // Otherwise find the object store it and return

                // Define object type
                Type objectType = obj.GetType();

                // Initialize queue
                IEnumerable<MemberInfo> filteredMemberInfos = null;

                // If the type has already been mapped, define queue
                if (_typeMemberInfos.ContainsKey(objectType))
                    filteredMemberInfos = _typeMemberInfos[objectType];
                else
                {
                    // Otherwise map type, define queue, store
                    PropertyInfo[] propertyInfos = objectType.GetProperties();

                    FieldInfo[] fieldInfos = objectType.GetFields();
                    IEnumerable<MemberInfo> filteredPropertyInfos = propertyInfos.Where(propertyInfo => propertyInfo.GetCustomAttributes(typeof(ValueReference), false).Length > 0).Cast<MemberInfo>();
                    IEnumerable<MemberInfo> filteredFieldInfos = fieldInfos.Where(fieldInfo => fieldInfo.GetCustomAttributes(typeof(ValueReference), false).Length > 0).Cast<MemberInfo>();
                    filteredMemberInfos = filteredPropertyInfos.Union(filteredFieldInfos);
                    _typeMemberInfos.Add(objectType, filteredMemberInfos);
                }

                foreach (MemberInfo memberInfo in filteredMemberInfos)
                {
                    // Define property attributes
                    object[] attributes = memberInfo.GetCustomAttributes(typeof(ValueReference), false);

                    // Find target attribute
                    foreach (object attribute in attributes)
                    {
                        if (((ValueReference)attribute).Name == name)
                        {
                            if (memberInfo.MemberType == MemberTypes.Property)
                            {
                                PropertyInfo propertyInfo = ((PropertyInfo)memberInfo);
                                _propertyInfos.Add(storedKey, propertyInfo);
                                return propertyInfo.GetValue(obj, null);
                            }
                            else if (memberInfo.MemberType == MemberTypes.Field)
                            {
                                FieldInfo fieldInfo = ((FieldInfo)memberInfo);
                                _fieldInfos.Add(storedKey, fieldInfo);
                                return fieldInfo.GetValue(obj);
                            }
                            else
                                return null;
                        }
                    }
                }
                return null;
            }
        }

        internal static void SetValue(System.Object rootObject, System.Object value, string path)
        {
            if (rootObject == null)
                return;

            // Split path by separator
            string[] pathDirectories = path.Split('/');
            string storedKey = rootObject.ToString() + path;
            string location = string.Empty;

            System.Object currentObject = rootObject;
            for (int i = 0; i < pathDirectories.Length - 1; i++)
            {
                // Obtain parent
                if (i == 0)
                    location += pathDirectories[i];
                else
                    location += "/" + pathDirectories[i];

                storedKey = rootObject.ToString() + location;

                currentObject = FindGetValue(currentObject, pathDirectories[i], storedKey);

                // If we couldn't obtain it, abort
                if (currentObject == null)
                    return;
            }
            storedKey = rootObject.ToString() + path;
            FindSetValue(currentObject, value, pathDirectories[pathDirectories.Length - 1], storedKey);
        }
        internal static void FindSetValue(object obj, object value, string name, string storedKey)
        {
            // Of the object has already been located and stored, return it
            if (_propertyInfos.ContainsKey(storedKey))
            {
                _propertyInfos[storedKey].SetValue(obj, value, null);
                return;
            }
            else if (_fieldInfos.ContainsKey(storedKey))
            {
                _fieldInfos[storedKey].SetValue(obj, value);
                return;
            }
            else
            {
                // Otherwise find the object store it and return

                // Define object type
                Type objectType = obj.GetType();

                // Initialize queue
                IEnumerable<MemberInfo> filteredMemberInfos = null;

                // If the type has already been mapped, define queue
                if (_typeMemberInfos.ContainsKey(objectType))
                    filteredMemberInfos = _typeMemberInfos[objectType];
                else
                {
                    // Otherwise map type, define queue, store
                    PropertyInfo[] propertyInfos = objectType.GetProperties();

                    FieldInfo[] fieldInfos = objectType.GetFields();
                    IEnumerable<MemberInfo> filteredPropertyInfos = propertyInfos.Where(propertyInfo => propertyInfo.GetCustomAttributes(typeof(ValueReference), false).Length > 0).Cast<MemberInfo>();
                    IEnumerable<MemberInfo> filteredFieldInfos = fieldInfos.Where(fieldInfo => fieldInfo.GetCustomAttributes(typeof(ValueReference), false).Length > 0).Cast<MemberInfo>();
                    filteredMemberInfos = filteredPropertyInfos.Union(filteredFieldInfos);
                    _typeMemberInfos.Add(objectType, filteredMemberInfos);
                }

                foreach (MemberInfo memberInfo in filteredMemberInfos)
                {
                    // Define property attributes
                    object[] attributes = memberInfo.GetCustomAttributes(typeof(ValueReference), false);

                    // Find target attribute
                    foreach (object attribute in attributes)
                    {
                        if (((ValueReference)attribute).Name == name)
                        {
                            if (memberInfo.MemberType == MemberTypes.Property)
                            {
                                PropertyInfo propertyInfo = ((PropertyInfo)memberInfo);
                                _propertyInfos.Add(storedKey, propertyInfo);
                                propertyInfo.SetValue(obj, value, null);
                                return;
                            }
                            else if (memberInfo.MemberType == MemberTypes.Field)
                            {
                                FieldInfo fieldInfo = ((FieldInfo)memberInfo);
                                _fieldInfos.Add(storedKey, fieldInfo);
                                fieldInfo.SetValue(obj, value);
                                return;
                            }
                            else
                                return;
                        }
                    }
                }
            }
        }

        internal static void InvokeMethod(System.Object rootObject, string path, object[] paramaters = null)
        {
            System.Object currentObject = rootObject;

            // Split path by separator
            string storedInfoKey = string.Empty;
            string name = string.Empty;
            string[] pathDirectories = path.Split('/');
            for (int i = 0; i < pathDirectories.Length - 1; i++)
            {
                name = pathDirectories[i];
                storedInfoKey = rootObject.ToString() + " - " + path + name;
                currentObject = FindGetValue(currentObject, pathDirectories[i], storedInfoKey);
            }

            name = pathDirectories[pathDirectories.Length - 1];
            storedInfoKey = rootObject.ToString() + " - " + path + " - " + name;
            if (currentObject != null)
                FindMethod(currentObject, name, storedInfoKey).Invoke(currentObject, paramaters);
        }
        private static MethodInfo FindMethod(System.Object obj, string name, string storedKey)
        {
            // Of the method has already been located and stored, return it
            if (_methodInfos.ContainsKey(storedKey))
                return _methodInfos[storedKey];
            else
            {
                // Otherwise find the method store it and return

                // Define object type
                Type objectType = obj.GetType();

                // Initialize queue
                IEnumerable<MemberInfo> filteredMemberInfos = null;

                // If the type has already been mapped, define queue
                if (_methodMemberInfos.ContainsKey(objectType))
                    filteredMemberInfos = _methodMemberInfos[objectType];
                else
                {
                    // Otherwise map type, define queue, store
                    MethodInfo[] methodInfos = objectType.GetMethods();
                    filteredMemberInfos = methodInfos.Where(methodInfo => methodInfo.GetCustomAttributes(typeof(MethodReference), false).Length > 0).Cast<MemberInfo>();
                    _methodMemberInfos.Add(objectType, filteredMemberInfos);
                }

                foreach (MemberInfo memberInfo in filteredMemberInfos)
                {
                    // Define property attributes
                    object[] attributes = memberInfo.GetCustomAttributes(typeof(MethodReference), false);

                    // Find target attribute
                    foreach (object attribute in attributes)
                    {
                        if (((MethodReference)attribute).Name == name)
                        {
                            MethodInfo methodInfo = ((MethodInfo)memberInfo);
                            _methodInfos.Add(storedKey, methodInfo);
                            return methodInfo;
                        }
                    }
                }
                return null;
            }
        }
    }
}