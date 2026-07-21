// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//          - GitHub:   https://github.com/RimuruDev
//
// **************************************************************** //

using UnityEngine;

namespace MothDIed.DI
{
    public class MonoSingleton<TComponent> 
        where TComponent : Component
    {
        protected static TComponent instance;

        public static bool HasInstance => instance != null;

        public static TComponent Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = GameObject.FindFirstObjectByType<TComponent>();

                if (instance != null) return instance;

                var obj = new GameObject
                {
                    name = $"[ === {typeof(TComponent).Name} === ]"
                };

                instance = obj.AddComponent<TComponent>();

                return instance;
            }
        }
    }
}