using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AshMind.Extensions;
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

        private class AssemblyInProgress {
            public string Path { get; set; }
            public AssemblyDefinition Assembly { get; set; }
            public AssemblyNameDefinition Name {
                get { return Assembly.Name; }
            }
        }

        private static void Main(Arguments arguments) {
            FluentConsole.White.Line("Loading");
            var assemblies = arguments.AssemblyPaths.Select(path => {
                FluentConsole.Line("  " + path);
                return new AssemblyInProgress {
                    Path = path,
                    Assembly = AssemblyDefinition.ReadAssembly(path)
                };
            }).ToArray();

            FluentConsole.NewLine().White.Line("Signing");
            var signed = new Dictionary<string, AssemblyDefinition>();
            foreach (var x in assemblies) {
                var name = x.Name;
                FluentConsole.Text("  {0}: ", name.Name);
                if (name.HasPublicKey)
                    FluentConsole.Text("rewriting public key, ");

                File.Copy(x.Path, x.Path + ".backup", true);
                WriteSigned(x.Path, x.Assembly, arguments.KeyFilePath);
                x.Assembly = AssemblyDefinition.ReadAssembly(x.Path);
                signed.Add(name.Name, x.Assembly);
                FluentConsole.Line("OK");
            }

            FluentConsole.NewLine().White.Line("References");
            foreach (var x in assemblies) {
                FluentConsole.Line("  {0}", x.Name.Name);
                foreach (var reference in x.Assembly.MainModule.AssemblyReferences) {
                    var @new = signed.GetValueOrDefault(reference.Name);
                    if (@new == null)
                        continue;

                    FluentConsole.Line("    {0} => {1}", reference.FullName, @new.FullName);
                    reference.HasPublicKey = true;
                    reference.PublicKey = @new.Name.PublicKey;
                    reference.PublicKeyToken = @new.Name.PublicKeyToken;
                }

                WriteSigned(x.Path, x.Assembly, arguments.KeyFilePath);
            }
        }

        private static void WriteSigned(string path, AssemblyDefinition assembly, string keyFilePath) {
            using (var keyStream = new FileStream(keyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                assembly.Write(path, new WriterParameters {
                    StrongNameKeyPair = new StrongNameKeyPair(keyStream)
                });
            }
        }
    }
}
