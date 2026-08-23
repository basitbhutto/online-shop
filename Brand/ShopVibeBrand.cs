namespace Presentation.Brand;

public static class ShopVibeBrand
{
    public const string Name = "ShopVibe";
    public const string AdminName = "ShopVibe Admin";
    public const string Tagline = "Curated fashion and premium accessories for the modern individual.";
    public const string SupportEmail = "support@shopvibe.com";
    public const string LogoPath = "~/images/logo.png";
    public const string LogoOnDarkPath = "~/images/logo-light.svg";
    public const string LogoAlt = "ShopVibe";

    public static string LogoFor(string? variant) =>
        string.Equals(variant, "dark", StringComparison.OrdinalIgnoreCase) ? LogoOnDarkPath : LogoPath;
}
