namespace TaskTower.Domain.JobStatuses.Mappings;

using System.Data;
using Dapper;

public class JobStatusTypeHandler : SqlMapper.ITypeHandler
{
    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.Value = value.ToString();
    }

    public object Parse(Type destinationType, object value)
    {
        return JobStatus.Of(value.ToString());
    }
}
