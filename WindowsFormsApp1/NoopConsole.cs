using System;

namespace WindowsFormsApp1
{
    internal static class NoopConsole
    {
        public static void WriteLine() { }
        public static void WriteLine(string? value) { }
        public static void WriteLine(object? value) { }
        public static void WriteLine(string? format, params object?[] args) { }

        public static void Write(string? value) { }
        public static void Write(string? format, params object?[] args) { }

        // Preservar comportamento de Beep caso seja utilizado em alguma parte da UI
        public static void Beep(int frequency, int duration)
        {
            try { Console.Beep(frequency, duration); } catch { }
        }
    }
}


