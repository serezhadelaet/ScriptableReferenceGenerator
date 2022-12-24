using Scriptables.References;
using UnityEngine;

namespace DefaultNamespace
{
    public class UsageExample : MonoBehaviour
    {
        [SerializeField] private ReferenceInstanceUsageExampleRefSO _exampleRef;

        private void Start()
        {
            Debug.Log(_exampleRef.Instance.GetType());
        }
    }
}