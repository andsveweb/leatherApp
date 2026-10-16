namespace leatherApp.Data;

public static class PriceFormatter
{
    public static string Format(decimal amount)
    {
        return $"{amount:#,0} kr".Replace(",", " ");
    }
}