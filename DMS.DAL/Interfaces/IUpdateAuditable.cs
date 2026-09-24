namespace DMS.DAL.Interfaces;

public interface IUpdateAuditable : ICreateAuditable
{
    DateTime? UpdatedOn { get; set; }
}