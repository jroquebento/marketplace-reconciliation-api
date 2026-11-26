using System.Data;

namespace ReconcileEngine.Api.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
