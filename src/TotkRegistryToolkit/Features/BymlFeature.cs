using BymlLibrary;
using CommunityToolkit.HighPerformance.Buffers;
using Revrs;
using Revrs.Buffers;
using Revrs.Extensions;
using TotkRegistryToolkit.Attributes;
using TotkRegistryToolkit.Extensions;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Features;

[Feature("BYML", "Convert BYML/YAML", "*", "byml.ico")]
public class BymlFeature : IFeature
{
    public async static ValueTask Execute(string[] args)
    {
        await WorkloadService.DelegateTasks(args, ProcessByml, CancellationToken.None);
    }

    private static ValueTask ProcessByml(string path, CancellationToken _)
    {
        using FileStream fs = File.OpenRead(path);
        int size = Convert.ToInt32(fs.Length);
        using ArraySegmentOwner<byte> buffer = ArraySegmentOwner<byte>.Allocate(size);
        ArraySegment<byte> data = buffer.Segment;

        if (data.AsSpan().Read<short>() is 0x4259 or 0x5942) {
            ToYaml(data, path);
        }

        if (Path.GetExtension(path) is ".yml" or ".yaml") {
            ToByml(data, path);
        }

        return ValueTask.CompletedTask;
    }

    private static void ToYaml(ArraySegment<byte> data, string path)
    {
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);
        using ArrayPoolBufferWriter<byte> bufferWriter = new();
        byml.WriteYaml(bufferWriter);

        using FileStream fs = File.Create(path + ".yml");
        fs.Write(bufferWriter.WrittenSpan);
    }

    private static void ToByml(ArraySegment<byte> data, string path)
    {
        Byml byml = Byml.FromText(data);
        byml.WriteBinary(path.TrimExtension(), Endianness.Little, version: 7);
    }
}
