// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//          - GitHub:   https://github.com/RimuruDev
//
// **************************************************************** //

using System;
using UnityEngine;

namespace MothDIed.DI
{
    [HelpURL("https://github.com/RimuruDev/IndieInject")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ProvideAttribute : Attribute
    {
        public bool IsSingleton;

        public ProvideAttribute(bool isSingleton)
        {
            IsSingleton = isSingleton;
        }
    }
}