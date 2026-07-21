using System;

namespace MothDIed.ServiceLocators
{
    public interface IServiceLocator
    {
        int Count { get; }
        bool Contains(Type serviceType);

        object GetBlind(Type serviceType);
    }
}