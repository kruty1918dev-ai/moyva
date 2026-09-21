using UnityEngine;

namespace Zenject.Tests.AutoInjecter
{
    public class Qux : MonoBehaviour
    {
        [Inject]
        [System.NonSerialized]
        public DiContainer Container;
    }
}

