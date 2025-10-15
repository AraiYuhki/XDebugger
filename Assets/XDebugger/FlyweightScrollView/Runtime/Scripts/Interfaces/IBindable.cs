namespace Xeon.Common
{
    public interface IBindable<TData>
    {
        void Bind(TData data);
    }
}
