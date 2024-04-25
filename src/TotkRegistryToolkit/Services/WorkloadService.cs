namespace TotkRegistryToolkit.Services;

public class WorkloadService
{
    public static async ValueTask DelegateTasks(string[] args, CancellationToken cancellationToken, Func<string, CancellationToken, ValueTask> process)
    {
        switch (args.Length) {
            case 0:
                break;
            case 1: {
                await process(args[0], cancellationToken);
                break;
            }
            case < 13: {
                foreach (var arg in args) {
                    await process(arg, cancellationToken);
                }
                break;
            }
            default: {
                await Parallel.ForEachAsync(args, process);
                break;
            }
        }
    }
}
