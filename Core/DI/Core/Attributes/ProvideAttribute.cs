using System;

namespace MothDIed.DI
{
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