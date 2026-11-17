using System.Text.Json;
using System.Text.Json.Nodes;

namespace IgroCehApp.Tests.Infrastructure
{
    // the answers are checked by the names the front reads, e.g. "eventRecords[0].game.name"
    public static class JsonPaths
    {
        public static JsonNode? At(this JsonNode? node, string path)
        {
            foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var name = part;
                var indexes = new List<int>();
                var bracket = name.IndexOf('[');
                if (bracket >= 0)
                {
                    indexes.AddRange(name[bracket..].Trim('[', ']').Split("][").Select(int.Parse));
                    name = name[..bracket];
                }

                if (name.Length > 0)
                {
                    node = node?[name];
                }

                foreach (var index in indexes)
                {
                    node = node?[index];
                }
            }

            return node;
        }

        public static string? Str(this JsonNode? node, string path) => node.At(path)?.GetValue<string>();

        public static int Int(this JsonNode? node, string path) => node.At(path)!.GetValue<int>();

        public static int? IntOrNull(this JsonNode? node, string path) => node.At(path)?.GetValue<int>();

        public static double? Num(this JsonNode? node, string path) => node.At(path)?.GetValue<double>();

        public static bool Bool(this JsonNode? node, string path) => node.At(path)!.GetValue<bool>();

        public static bool? BoolOrNull(this JsonNode? node, string path) => node.At(path)?.GetValue<bool>();

        public static DateTimeOffset? Date(this JsonNode? node, string path) => node.At(path) is { } value ? DateTimeOffset.Parse(value.GetValue<string>()) : null;

        public static JsonArray Arr(this JsonNode? node, string path) => node.At(path)?.AsArray() ?? throw new InvalidOperationException($"No array at {path}");

        public static List<string?> Strs(this JsonNode? node, string path, string itemPath = "") =>
            node.Arr(path).Select(item => itemPath == "" ? item?.GetValue<string>() : item.Str(itemPath)).ToList();

        public static List<int> Ints(this JsonNode? node, string path, string itemPath = "") =>
            node.Arr(path).Select(item => itemPath == "" ? item!.GetValue<int>() : item.Int(itemPath)).ToList();

        public static bool Has(this JsonNode? node, string path) => node.At(path) != null;

        public static string Compact(this JsonNode? node) => node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false }) ?? "null";
    }
}
