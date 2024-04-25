using CommunityToolkit.HighPerformance.Buffers;
using Totk.Common.Extensions;
using TotkRegistryToolkit.Attributes;
using TotkRegistryToolkit.Extensions;
using TotkRegistryToolkit.Models;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Features;

[Feature("ZSTD", "ZSTD De/Compress", "*", "zstd.ico")]
public class ZsFeature : IFeature
{
    public static async ValueTask Execute(string[] args)
    {
        ZstdExtension.LoadDictionaries(TotkConfig.Shared.ZsDicPath);

        await WorkloadService.DelegateTasks(args, Process, CancellationToken.None);
    }

    public static ValueTask Process(string path, CancellationToken _)
    {
        using FileStream fs = File.OpenRead(path);
        int size = Convert.ToInt32(fs.Length);
        using SpanOwner<byte> buffer = SpanOwner<byte>.Allocate(size);
        Span<byte> data = buffer.Span;
        fs.Read(data);

        return data.IsZsCompressed() switch {
            true => Decompress(data, path),
            false => Compress(data, path),
        };
    }

    private static ValueTask Decompress(Span<byte> data, string path)
    {
        using SpanOwner<byte> decompressed = SpanOwner<byte>.Allocate(data.GetZsDecompressedSize());
        data.ZsDecompress(decompressed.Span, out int id);

        using FileStream fs = File.Create(path.TrimExtension());
        fs.Write(decompressed.Span);

        return ValueTask.CompletedTask;
    }

    private static ValueTask Compress(ReadOnlySpan<byte> data, string path)
    {
        ReadOnlySpan<char> ext = Path.GetExtension(path.AsSpan());
        ReadOnlySpan<char> subExt = Path.GetExtension(Path.GetFileNameWithoutExtension(path.AsSpan()));

        using SpanOwner<byte> compressed = SpanOwner<byte>.Allocate(data.Length);
        int size = data.ZsCompress(compressed.Span, ext switch {
            ".pack" => 3,
            ".byml" => subExt switch {
                ".bcett" => 2,
                _ => 1,
            },
            _ => 1
        });

        using FileStream fs = File.Create(path + ".zs");
        fs.Write(compressed.Span[..size]);

        return ValueTask.CompletedTask;
    }
}
