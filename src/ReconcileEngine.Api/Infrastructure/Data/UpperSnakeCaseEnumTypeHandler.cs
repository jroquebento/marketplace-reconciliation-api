using System.Data;
using System.Text;
using Dapper;

namespace ReconcileEngine.Api.Infrastructure.Data;

public sealed class UpperSnakeCaseEnumTypeHandler<TEnum> : SqlMapper.TypeHandler<TEnum>
    where TEnum : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, TEnum value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = ToUpperSnakeCase(value.ToString());
    }

    public override TEnum Parse(object value)
    {
        var raw = value.ToString()
            ?? throw new InvalidOperationException($"Cannot convert null value to {typeof(TEnum).Name}.");

        var normalized = raw.Replace("_", string.Empty);

        return Enum.Parse<TEnum>(normalized, ignoreCase: true);
    }

    private static string ToUpperSnakeCase(string input)
    {
        var builder = new StringBuilder(input.Length + 4);

        for (var i = 0; i < input.Length; i++)
        {
            var current = input[i];
            if (i > 0 && char.IsUpper(current))
                builder.Append('_');

            builder.Append(char.ToUpperInvariant(current));
        }

        return builder.ToString();
    }
}