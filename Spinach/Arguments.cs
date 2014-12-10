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

        [NamedArgument("key", MetaVar = "Key", Required = true)]
        public string KeyFilePath { get; set; }
    }
}
