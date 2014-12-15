using System;

namespace Simple.NH.Modeling
{
    [Serializable]
    public struct ReferenceTo<T> : IReferenceTo where T : IHasId, IHasName
    {
        public ReferenceTo(T reference) : this(reference.Id, reference.Name) { }

        public ReferenceTo(long id) : this(id, null) { }

        public ReferenceTo(long id, string name)
            : this()
        {
            Id = id;
            Name = name;
        }

        public long Id { get; set; }

        public string Name { get; set; }
    }

}
