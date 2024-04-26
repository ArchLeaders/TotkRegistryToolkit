using CommunityToolkit.HighPerformance.Buffers;
using TotkCommon;
using TotkRegistryToolkit.Attributes;
using TotkRegistryToolkit.Extensions;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Features;

[Feature("ZSTD", "ZSTD De/Compress", "*", "zstd.ico")]
public class ZsFeature : IFeature
{
    public static async ValueTask Execute(string[] args)
    {
        await WorkloadService.DelegateTasks(args, Process, CancellationToken.None);
    }

    public static ValueTask Process(string path, CancellationToken _)
    {
        using FileStream fs = File.OpenRead(path);
        int size = Convert.ToInt32(fs.Length);
        using SpanOwner<byte> buffer = SpanOwner<byte>.Allocate(size);
        Span<byte> data = buffer.Span;
        fs.Read(data);

        return Zstd.IsCompressed(data) switch {
            true => Decompress(data, path),
            false => Compress(data, path),
        };
    }

    private static ValueTask Decompress(Span<byte> data, string path)
    {
        using SpanOwner<byte> decompressed = SpanOwner<byte>.Allocate(Zstd.GetDecompressedSize(data));
        Totk.Zstd.Decompress(data, decompressed.Span, out int id);

        using FileStream fs = File.Create(path.TrimExtension());
        fs.Write(decompressed.Span);

        return ValueTask.CompletedTask;
    }

    private static ValueTask Compress(ReadOnlySpan<byte> data, string path)
    {
        ReadOnlySpan<char> ext = Path.GetExtension(path.AsSpan());
        ReadOnlySpan<char> subExt = Path.GetExtension(Path.GetFileNameWithoutExtension(path.AsSpan()));

        using SpanOwner<byte> compressed = SpanOwner<byte>.Allocate(data.Length);
        int size = Totk.Zstd.Compress(data, compressed.Span, ext switch {
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
