using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AshMind.Extensions;
using Mono.Cecil;

namespace Spinach.Processing {
    public class Processor {
        public void Process(Arguments arguments) {
            FluentConsole.White.Line("Loading");
            var assemblies = LoadAssemblies(arguments);
            var assembliesByName = assemblies.ToDictionary(d => d.Name.Name);
            DetectReferences(assembliesByName);
            MarkAssembliesToSign(assemblies, arguments);

            FluentConsole.NewLine().White.Line("Processing");
            SignOrCopy(assemblies, arguments);

            FluentConsole.NewLine().White.Line("References");
            UpdateReferences(assemblies, arguments);
        }

        private static void MarkAssembliesToSign(IEnumerable<AssemblyDetails> assemblies, Arguments arguments) {
            // ReSharper disable once PossibleMultipleEnumeration
            var assembliesToSign = assemblies;
            if (!arguments.SignAll) {
                // by default, it would sign only the assemblies that do not have public keys, 
                // or assemblies referencing those (recursively)

                // ReSharper disable once PossibleMultipleEnumeration
                assembliesToSign = assemblies
                    .Where(a => !a.Name.HasPublicKey)
                    .SelectRecursive(a => a.ReferencedBy)
                    .AsSet();
            }

            foreach (var assembly in assembliesToSign) {
                assembly.NeedsToBeSigned = true;
            }
        }

        private static void DetectReferences(IDictionary<string, AssemblyDetails> assemblies) {
            foreach (var details in assemblies.Values) {
                foreach (var reference in details.Assembly.MainModule.AssemblyReferences) {
                    var referenced = assemblies.GetValueOrDefault(reference.Name);
                    if (referenced == null)
                        continue;

                    referenced.ReferencedBy.Add(details);
                }
            }
        }

        private AssemblyDetails[] LoadAssemblies(Arguments arguments) {
            var assemblies = arguments.AssemblyPaths.Select(path => {
                FluentConsole.Line("  " + path);
                return new AssemblyDetails {
                    SourcePath = path,
                    TargetPath = Path.Combine(arguments.OutputDirectoryPath, Path.GetFileName(path)),
                    Assembly = AssemblyDefinition.ReadAssembly(path)
                };
            }).ToArray();
            return assemblies;
        }

        private void SignOrCopy(IReadOnlyList<AssemblyDetails> assemblies, Arguments arguments) {
            foreach (var details in assemblies) {
                var name = details.Name;
                FluentConsole.Line("  {0}", name.Name)
                             .Line("    => {0}", details.TargetPath);

                if (arguments.Skip == SkipMode.Existing && File.Exists(details.TargetPath)) {
                    FluentConsole.Line("    Already exists, skipping.");
                    details.NeedsToBeSigned = false;
                    details.Assembly = AssemblyDefinition.ReadAssembly(details.TargetPath);
                    continue;
                }

                if (!details.NeedsToBeSigned) {
                    FluentConsole.Line("    No need to re-sign (copying as is).");
                    File.Copy(details.SourcePath, details.TargetPath);
                    continue;
                }

                if (name.HasPublicKey)
                    FluentConsole.Line("    Rewriting existing public key.");

                WriteSigned(details.TargetPath, details.Assembly, arguments.KeyFilePath);
                details.Assembly = AssemblyDefinition.ReadAssembly(details.TargetPath);
            }
        }
        
        private void WriteSigned(string path, AssemblyDefinition assembly, string keyFilePath) {
            using (var keyStream = new FileStream(keyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                assembly.Write(path, new WriterParameters {
                    StrongNameKeyPair = new StrongNameKeyPair(keyStream)
                });
            }
        }

        private void UpdateReferences(IReadOnlyCollection<AssemblyDetails> assemblies, Arguments arguments) {
            var assembliesToUpdate = assemblies.Where(a => a.NeedsToBeSigned);
            var byName = assembliesToUpdate.ToDictionary(a => a.Name.Name);
            foreach (var details in byName.Values) {
                FluentConsole.Line("  {0}", details.Name.Name);
                foreach (var reference in details.Assembly.MainModule.AssemblyReferences) {
                    var target = byName.GetValueOrDefault(reference.Name);
                    if (target == null)
                        continue;

                    FluentConsole.Line("    {0} => {1}", reference.FullName, target.Name.FullName);
                    reference.HasPublicKey = true;
                    reference.PublicKey = target.Name.PublicKey;
                    reference.PublicKeyToken = target.Name.PublicKeyToken;
                }

                WriteSigned(details.TargetPath, details.Assembly, arguments.KeyFilePath);
            }
        }
    }
}