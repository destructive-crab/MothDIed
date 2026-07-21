using UnityEngine;

namespace MothDIed.Optional.Extensions
{
    public static class UnityExtensions
    {
        public static void SetPosition(this Transform transform, Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        
        public static void SetPosition(this GameObject gameObject, Vector2 position)
        {
            gameObject.transform.position = new Vector3(position.x, position.y, gameObject.transform.position.z);
        }

        public static void AddPosition(this Transform transform, Vector2 vector)
        {
            transform.position = new Vector3(transform.position.x + vector.x, transform.position.y + vector.y, transform.position.z);
        }
    }
}