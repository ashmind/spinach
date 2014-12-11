using System.Collections.Generic;
using Mono.Cecil;

namespace Spinach.Processing {
    public class AssemblyDetails {
        public AssemblyDetails() {
            ReferencedBy = new HashSet<AssemblyDetails>();
        }
        
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public AssemblyDefinition Assembly { get; set; }
        public AssemblyNameDefinition Name {
            get { return Assembly.Name; }
        }
        public ISet<AssemblyDetails> ReferencedBy { get; private set; }
        public bool NeedsToBeSigned { get; set; }
    }
}