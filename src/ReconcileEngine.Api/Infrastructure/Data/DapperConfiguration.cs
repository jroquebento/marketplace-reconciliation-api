using Dapper;
using ReconcileEngine.Api.Domain.Enums;

namespace ReconcileEngine.Api.Infrastructure.Data;

public static class DapperConfiguration
{
    public static void Configure()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new UpperSnakeCaseEnumTypeHandler<BatchStatus>());
        SqlMapper.AddTypeHandler(new UpperSnakeCaseEnumTypeHandler<StagingStatus>());
        SqlMapper.AddTypeHandler(new UpperSnakeCaseEnumTypeHandler<DiscrepancyType>());
    }
}