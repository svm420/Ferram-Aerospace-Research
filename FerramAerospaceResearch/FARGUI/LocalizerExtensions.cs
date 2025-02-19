using KSP.Localization;

namespace FerramAerospaceResearch.FARGUI
{
    public static class LocalizerExtensions
    {
        public static string Get(string tag)
        {
            return Localizer.TryGetStringByTag(tag, out string r) ? r : tag;
        }
    }
}
