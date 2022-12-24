using UnityEngine;

namespace Scriptables.References
{
    public abstract class BaseRefSetter<TComponent, TScriptable> : MonoBehaviour where TScriptable : BaseRefSO<TComponent> 
    {
        [SerializeField] private TScriptable _scriptable;
        
        private void Awake()
        {
            _scriptable.SetComponent(GetComponent<TComponent>());
        } 

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (_scriptable == null)
            {
                Debug.LogError(GetType().Name + $" has no {typeof(TScriptable).Name} attached on " + name);
            }
#endif
        }
    }
}