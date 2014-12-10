using System;
using System.IO;
using System.Reflection;
using clipr;
using Mono.Cecil;

namespace Spinach {
    public class Program {
        public static int Main(string[] args) {
            try {
                FluentConsole.Green.Line("Spinach: Making your assemblies stronger.")
                             .NewLine();

                // all command line parsers for .NET are terrible.
                // this one does not really shoow any help message 
                // when something fails.
                //
                // but it would do for now.
                var arguments = CliParser.Parse<Arguments>(args);
                Main(arguments);
            }
            catch (Exception ex) {
                FluentConsole.Red.Line(ex);
                return ex.HResult;
            }

            return 0;
        }

        private static void Main(Arguments arguments) {
            foreach (var path in arguments.AssemblyPaths) {
                FluentConsole.Text("Processing ").White.Text(path).NewLine();
                var assembly = AssemblyDefinition.ReadAssembly(path);
                var name = assembly.Name;
                if (name.HasPublicKey) {
                    FluentConsole.Yellow.Text("Assembly {0} is already signed.", path);
                    continue;
                }

                File.Copy(path, path + ".unsigned.bak");
                using (var keyStream = new FileStream(arguments.KeyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    assembly.Write(path, new WriterParameters {
                        StrongNameKeyPair = new StrongNameKeyPair(keyStream)
                    });
                }
            }
        }
    }
}
