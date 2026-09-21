using UnityEngine;

namespace Zenject.Tests.AutoInjecter
{
    public class Gorp : MonoBehaviour
    {
        [Inject]
        [System.NonSerialized]
        public DiContainer Container;
    }
}

