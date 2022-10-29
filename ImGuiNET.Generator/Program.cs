﻿using System.Text;
using CppSharp;
using ImGuiNET.Generator.Logging;

// ReSharper disable StringLiteralTypo

namespace ImGuiNET.Generator;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length is 0)
        {
            Console.WriteLine("No arguments given, available arguments:");
            Console.WriteLine("\t-old: generate old version");
            Console.WriteLine("\t-new: generate new version");
            Console.WriteLine("\t-cln: cleaning new version");
            return;
        }

        Console.WriteLine("Code generation started.");

        if (args.Contains("-old", StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine("Generating old version.");

            Directory.CreateDirectory("OLD");

            using (var writer = new StreamWriter(File.Create(@"OLD\output.txt")))
            using (new AggregateConsoleOut(writer))
            {
                ConsoleDriver.Run(new MyLibrary { Enhanced = false });
            }
        }

        if (args.Contains("-new", StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine("Generating new version.");

            Directory.CreateDirectory("NEW");

            using (var writer = new StreamWriter(File.Create(@"NEW\output.txt")))
            using (new AggregateConsoleOut(writer))
            {
                ConsoleDriver.Run(new MyLibrary { Enhanced = true });
            }
        }

        if (args.Contains("-cln", StringComparer.OrdinalIgnoreCase))
        {
            var path = Path.Combine(Environment.CurrentDirectory, "NEW", "imgui.cs");

            if (!File.Exists(path))
                return;

            // GeneratorOutputPass misses some structs for whatever reason so let's do it all here

            var text = File.ReadAllText(path);

            var cleanup = Cleanup(text);

            File.WriteAllText(path, cleanup);
        }

        Console.WriteLine("Code generation finished.");
    }

    private static string Cleanup(string text)
    {
        var builder = new StringBuilder(text);

        builder.Replace(
            "imgui",
            "ImGui"
        );

        builder.Replace(
            "\"ImGui\"",
            "\"imgui\""
        );

        // hide pointers that should have been internal

        builder.Replace(
            "public __IntPtr __Instance { get; protected set; }",
            "internal __IntPtr __Instance { get; set; }"
        );

        // hide structs that should have been internal

        builder.Replace(
            "public partial struct __Internal",
            "internal partial struct __Internal"
        );

        builder.Replace(
            "public unsafe partial struct __Internal",
            "internal unsafe partial struct __Internal"
        );

        // hide ImVector namespace as internal class as it cannot be moved onto ImVector<T> because of CS7042

        builder.Replace(
            "namespace ImVector",
            "internal static partial class ImVector"
        );

        // merge symbols with class to remove __Symbols namespace

        builder.Replace(
            "}\r\nnamespace ImGuiNET.__Symbols\r\n{\r\n    internal class ImGui",
            "    public unsafe partial class ImGui"
        );

        builder.Replace(
            "public static IntPtr _EmptyString_ImGuiTextBuffer__2PADA",
            "internal static IntPtr _EmptyString_ImGuiTextBuffer__2PADA"
        );

        builder.Replace(
            ".__Symbols",
            string.Empty
        );

        return builder.ToString();
    }
}