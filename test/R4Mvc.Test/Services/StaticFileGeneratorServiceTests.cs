﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R4Mvc.Test.Locators;
using R4Mvc.Tools.Locators;
using R4Mvc.Tools.Services;
using Xunit;

namespace R4Mvc.Test.Services
{
    public class StaticFileGeneratorServiceTests
    {
        [Theory]
        [InlineData("helloworld", "helloworld")]
        [InlineData("hello-world", "hello_world")]
        [InlineData("hello world", "hello_world")]
        [InlineData("hello%world", "hello_world")]
        [InlineData("hello % world", "hello___world")]
        [InlineData("helloworld^", "helloworld_")]
        [InlineData("helloworld^^", "helloworld__")]
        [InlineData("^helloworld", "_helloworld")]
        [InlineData("^^helloworld", "__helloworld")]
        [InlineData("helloworld0", "helloworld0")]
        [InlineData("hello0world", "hello0world")]
        [InlineData("0helloworld", "_0helloworld")]
        public void SanitiseName(string name, string sanitisedName)
        {
            Assert.Equal(sanitisedName, StaticFileGeneratorService.SanitiseName(name));
        }

        [Fact]
        public void AddStaticFiles()
        {
            var staticFileLocator = new DefaultStaticFileLocator(VirtualFileLocator.Default);
            var staticFiles = staticFileLocator.Find(VirtualFileLocator.ProjectRoot_wwwroot);
            var staticFileGeneratorService = new StaticFileGeneratorService(new[] { staticFileLocator }, new Tools.Settings());

            var c = SyntaxFactory.ClassDeclaration("Test");
            c = staticFileGeneratorService.AddStaticFiles(c, string.Empty, staticFiles);

            Assert.Collection(c.Members,
                m =>
                {
                    var pathClass = m.AssertIsClass("css");
                    Assert.Collection(pathClass.Members, m2 => m2.AssertIsSingleField("site_css"));
                },
                m =>
                {
                    var pathClass = m.AssertIsClass("js");
                    Assert.Collection(pathClass.Members, m2 => m2.AssertIsSingleField("site_js"));
                },
                m =>
                {
                    var pathClass = m.AssertIsClass("lib");
                    Assert.Collection(pathClass.Members, m2 =>
                    {
                        var pathClass2 = m2.AssertIsClass("jslib");
                        Assert.Collection(pathClass2.Members, m3 => m3.AssertIsSingleField("core_js"));
                    });
                },
                m => m.AssertIsSingleField("favicon_ico")
            );
        }
    }
}
