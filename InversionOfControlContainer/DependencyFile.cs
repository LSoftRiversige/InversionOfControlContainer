using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InversionOfControlContainer
{
    public class DependencyFile
    {
        public IContainer Container { get; }
        public IStringDependencyParser Parser { get; }

        public DependencyFile(IContainer container, IStringDependencyParser parser)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public void LoadFromFile(string fileName)
        {
            CheckFileExist(fileName);
            string[] text = File.ReadAllLines(fileName);
            Parser.BindTextTo(Container, text);
        }

        private static void CheckFileExist(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found", fileName);
            }
        }
    }
}
