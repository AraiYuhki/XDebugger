namespace Xeon.Common
{
    public interface ISetupable<TData>
    {
        void Setup(TData data);
    }
}
