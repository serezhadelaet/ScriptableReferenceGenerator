using UnityEngine;

namespace Scriptables.References
{
    public abstract class BaseRefSO<TComponent> : ScriptableObject
    {
        public TComponent Instance { get; private set; }

        public void SetComponent(TComponent c)
        {
            Instance = c;
        }
    }
}