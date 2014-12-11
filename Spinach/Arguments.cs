using System;
using System.Collections.Generic;
using System.Linq;
using clipr;

namespace Spinach {
    public class Arguments {
        public Arguments() {
            AssemblyPaths = new List<string>();
        }

        [PositionalArgument(0, MetaVar = "Assemblies", NumArgs = 1, Constraint = NumArgsConstraint.AtLeast)]
        public IList<string> AssemblyPaths { get; private set; }

        [NamedArgument('k', "key", Required = true)]
        public string KeyFilePath { get; set; }

        [NamedArgument('o', "out", Required = true)]
        public string OutputDirectoryPath { get; set; }
        
        [NamedArgument(
            "sign-all", Required = false, Action = ParseAction.StoreTrue, 
            Description = "Forces all assemblies to be signed with a new key (even if the assembly is already signed and does not need reference patching)."
        )]
        public bool SignAll { get; set; }

        [NamedArgument("skip", Required = false)]
        public SkipMode? Skip { get; set; }
    }
}
