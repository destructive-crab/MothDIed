using UnityEngine;

namespace MothDIed.Pool
{
    [DisallowMultipleComponent]
    public sealed class PoolInstanceRoot : MonoBehaviour
    {
        private string poolName;

        public bool Setup(string poolName)
        {
            this.poolName = poolName;
            gameObject.name = poolName;
            
            return true;
        }
    }
}