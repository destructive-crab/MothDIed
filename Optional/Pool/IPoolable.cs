namespace MothDIed.Pool
{
    public interface IPoolable
    {
        void OnPicked();
        void OnReleased();

        void ReleaseThis();
    }
}