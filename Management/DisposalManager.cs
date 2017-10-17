using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Echo.Management
{
    sealed public class DisposalManager : MonoBehaviour
    {
        private const float DISPOSAL_DELAY = 120f;
        private static List<GameObject> _objects = new List<GameObject>();

        public static void Add(GameObject gameObject)
        {
            _objects.Add(gameObject);
            Destroy(gameObject, DISPOSAL_DELAY);
        }

        public static void DisposeAll()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                Destroy(_objects[i]);
                _objects.RemoveAt(i);

                if (i > 0)
                    i--;
            }
        }
        public static void DisposeAllImmediate()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                DestroyImmediate(_objects[i]);
                _objects.RemoveAt(i);

                if (i > 0)
                    i--;
            }
        }
    }
}
