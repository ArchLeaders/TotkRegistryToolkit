using BymlLibrary;
using CommunityToolkit.HighPerformance.Buffers;
using Revrs;
using Revrs.Buffers;
using Revrs.Extensions;
using TotkRegistryToolkit.Attributes;
using TotkRegistryToolkit.Extensions;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Features;

[Feature("BYML", "Converts BYML to YAML and vice versa.")]
public class BymlFeature : IFeature
{
    public async static ValueTask Execute(string[] args)
    {
        switch (args.Length) {
            case 0:
                break;
            case 1: {
                ProcessByml(args[0]);
                break;
            }
            case < 13: {
                foreach (var arg in args) {
                    ProcessByml(arg);
                }
                break;
            }
            default: {
                await Parallel.ForEachAsync(args, (arg, cancellationToken) => {
                    ProcessByml(arg);
                    return ValueTask.CompletedTask;
                });
                break;
            }
        }
    }

    private static void ProcessByml(string path)
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
