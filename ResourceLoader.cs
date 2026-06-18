using System.Reflection;

namespace MiniPlayer
{
    static class ResourceLoader
    {
        public static string ReadText(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new FileNotFoundException($"Embedded resource not found: {name}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
