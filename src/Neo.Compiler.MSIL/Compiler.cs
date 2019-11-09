using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Compiler
{
    public class Compiler
    {
        public class Assembly
        {
            public readonly byte[] Dll;
            public readonly byte[] Pdb;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="dll">Library</param>
            /// <param name="pdb">PDB</param>
            public Assembly(byte[] dll, byte[] pdb)
            {
                Dll = dll;
                Pdb = pdb;
            }

            /// <summary>
            /// Create Assembly
            /// </summary>
            /// <param name="comp">Compilation</param>
            /// <returns>Assembly</returns>
            internal static Assembly Create(Compilation comp)
            {
                using (var streamDll = new MemoryStream())
                using (var streamPdb = new MemoryStream())
                {
                    var result = comp.Emit(streamDll, streamPdb);

                    if (!result.Success)
                    {
                        throw new ArgumentException();
                    }

                    streamDll.Position = 0;
                    streamPdb.Position = 0;

                    return new Assembly(streamDll.ToArray(), streamPdb.ToArray());
                }
            }
        }

        /// <summary>
        /// Build script
        /// </summary>
        /// <param name="filenames">File names</param>
        /// <param name="references">References</param>
        /// <returns>Assembly</returns>
        public static Assembly BuildVBScript(string[] filenames, string[] references)
        {
            var tree = filenames.Select(u => VisualBasicSyntaxTree.ParseText(File.ReadAllText(u))).ToArray();
            var op = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release);
            return Assembly.Create(VisualBasicCompilation.Create("SmartContract", tree, CreateReferences(references), op));
        }

        /// <summary>
        /// Build script
        /// </summary>
        /// <param name="filenames">File names</param>
        /// <param name="references">References</param>
        /// <returns>Assembly</returns>
        public static Assembly BuildCSharpScript(string[] filenames, string[] references)
        {
            var tree = filenames.Select(u => CSharpSyntaxTree.ParseText(File.ReadAllText(u))).ToArray();
            var op = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release);
            return Assembly.Create(CSharpCompilation.Create("SmartContract", tree, CreateReferences(references), op));
        }

        /// <summary>
        /// Create references
        /// </summary>
        /// <param name="references">References</param>
        /// <returns>MetadataReferences</returns>
        private static MetadataReference[] CreateReferences(params string[] references)
        {
            var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var refs = new List<MetadataReference>(new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.Numerics.dll")),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DisplayNameAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            });
            refs.AddRange(references.Select(u => MetadataReference.CreateFromFile(u)));
            return refs.ToArray();
        }
    }
}