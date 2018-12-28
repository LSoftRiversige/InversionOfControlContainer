using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace InversionOfControlContainer
{
    public class StringDependencyParser : IStringDependencyParser
    {
        private const string typeDelimiter = "->";
        private const string endDelimiter = ";";
        private const string comment = "//";
        private Assembly _typeLib;

        public Assembly TypeLib { get => _typeLib; set => _typeLib = value; }

        public StringDependencyParser(Assembly lib = null) => TypeLib = lib ?? Assembly.GetExecutingAssembly();

        public (Type, Type) Parse(string s)
        {
            if (Skip(s))
            {
                return (null, null);
            }
            ExtractTypes(s, out string left, out string right);

            return FindTypes(left, right);
        }

        private static void ExtractTypes(string s, out string left, out string right)
        {
            int delimPos = FindPattern(s, typeDelimiter);
            int endPos = FindPattern(s, endDelimiter);

            left = GetLeft(s, delimPos);
            right = GetRight(s, delimPos, endPos);
        }

        private (Type, Type) FindTypes(string left, string right)
        {
            Type[] types = TypeLib.GetTypes();
            Type leftType = types.FirstOrDefault(t => t.Name.Equals(left));
            Type rightType = types.FirstOrDefault(t => t.Name.Equals(right));
            return (leftType, rightType);
        }

        private bool Skip(string s)
        {
            if (SkipCommentedLine(s))
            {
                return true;
            }
            if (SkipEmpty(s))
            {
                return true;
            }
            return false;
        }

        private static bool SkipEmpty(string s)
        {
            return s == string.Empty;
        }

        private bool SkipCommentedLine(string s)
        {
            return s.TrimStart().StartsWith(comment);
        }

        private static string GetRight(string s, int delimPos, int endPos)
        {
            int rightStartPos = delimPos + typeDelimiter.Length;
            return s.Substring(rightStartPos, endPos - rightStartPos).Trim();
        }

        private static string GetLeft(string s, int delimPos)
        {
            return s.Substring(0, delimPos - 1).Trim();
        }

        private static int FindPattern(string s, string pattern)
        {
            int p = s.IndexOf(pattern);
            if (p == -1)
            {
                throw new InvalidParseException($"Pattern '{pattern}' not found in line '{s}'");
            }
            return p;
        }

        public void BindTo(IContainer container, string s)
        {
            (Type, Type) typesPare = Parse(s);
            if (typesPare.Item1 == null || typesPare.Item2 == null)
            {
                return;
            }
            container.Bind(typesPare.Item1, typesPare.Item2);
        }

        public void BindTextTo(IContainer container, string[] text)
        {
            foreach (var line in text)
            {
                BindTo(container, line);
            }
        }
    }
}