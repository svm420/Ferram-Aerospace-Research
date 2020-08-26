namespace FerramAerospaceResearch.Resources.Loading
{
    public interface IAssetRequest
    {
        Progress State { get; set; }
        string Url { get; }
        void OnError();
    }

    public interface IAssetRequest<in T> : IAssetRequest
    {
        void OnLoad(T resource);
    }
}
