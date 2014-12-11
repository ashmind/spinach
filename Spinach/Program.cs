using System;
using System.Collections.Generic;
using System.Linq;
using clipr;
using Spinach.Processing;

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
                new Processor().Process(arguments);
            }
            catch (Exception ex) {
                FluentConsole.Red.Line(ex);
                return ex.HResult;
            }

            return 0;
        }
    }
}
