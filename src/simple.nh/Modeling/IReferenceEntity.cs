namespace Simple.NH.Modeling
{
    public interface IReferenceEntity : IHasId, IHasName
    {
        bool IsActive { get; set; }
    }
}