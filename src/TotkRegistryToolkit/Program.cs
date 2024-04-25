using Cocona;
using System.Reflection;
using System.Runtime.Versioning;
using TotkRegistryToolkit.Components;

[assembly: SupportedOSPlatform("windows")]

Console.WriteLine($"""
    TotK Registry Toolkit [Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "???"}]
    (c) 2024 ArchLeaders. MIT.

    """);

CoconaLiteApp app = CoconaLiteApp.Create(args);
app.AddCommands<ActionCommands>();
app.AddCommands<EditCommands>();
await app.RunAsync();
