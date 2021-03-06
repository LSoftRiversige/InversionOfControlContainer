﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using InversionOfControl.Tests.ClassesForTest;
using InversionOfControlContainer;
using System.Reflection;
using System.IO;

namespace InversionOfControl.Tests
{
    public class DependencyFileTests
    {
        private const string FileName = "Dependencies.txt";
        private const string UnknownFileName = "FileNameThatNotExists.txt";

        public DependencyFileTests()
        {
            File.Delete(FileName);
            File.AppendAllLines(FileName, new string[] { "IBar -> Bar;", "IFoo -> Foo;" });
        }

        [Fact]
        public void LoadFromFile_FileNotExists_Throw()
        {
            var f = new DependencyFile(
                new Container(),
                new StringDependencyParser(
                    Assembly.GetExecutingAssembly()));

            var e = Assert.Throws<FileNotFoundException>(() => f.LoadFromFile(UnknownFileName));
            Assert.Equal("File not found", e.Message);
        }

        [Fact]
        public void LoadFromFile_LoadOK_NewObject()
        {
            var f = new DependencyFile(
                new Container(),
                new StringDependencyParser(
                    Assembly.GetExecutingAssembly()));

            f.LoadFromFile(FileName);

            IBar bar = f.Container.Get<IBar>();

            Assert.IsAssignableFrom<IBar>(bar);
        }
    }
}
