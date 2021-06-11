namespace DanishAddressSeed.Location
{
    internal record EntityChange<T>
    {
        public string Operation { get; init; }
        public T Data { get; init; }
    }
}
