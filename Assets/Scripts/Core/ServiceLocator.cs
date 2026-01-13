using System;
using System.Collections.Generic;
using UnityEngine;

namespace MusicOM.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static bool _isInitialized;

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Replacing.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"[ServiceLocator] Registered {type.Name}");
            }
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] Service {type.Name} not found. Did you forget to register it?");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var found))
            {
                service = found as T;
                return true;
            }

            service = null;
            return false;
        }

        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered {type.Name}");
            }
        }

        public static void Clear()
        {
            _services.Clear();
            _isInitialized = false;
            Debug.Log("[ServiceLocator] All services cleared");
        }

        public static void MarkInitialized()
        {
            _isInitialized = true;
        }

        public static bool IsInitialized => _isInitialized;
    }
}
