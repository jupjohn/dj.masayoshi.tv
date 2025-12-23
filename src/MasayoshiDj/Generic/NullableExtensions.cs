namespace MasayoshiDj.Generic;

public static class NullableExtensions
{
    public static TValue OrDefaultTo<TValue>(this TValue? value, TValue @default) where TValue : notnull
    {
        return value ?? @default;
    }
}
