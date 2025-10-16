using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Mvc;
using System.Web.Routing;
using MvcSiteMapProvider.Web.Compilation;
using MvcSiteMapProvider.Web.Mvc;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Web
{
    // Top-level test controller types (must be top-level so reflection sees IsPublic)
    public class ValidPublicController : Controller { }
    public class Specialcontroller : Controller { } // case-insensitive suffix match
    public abstract class AbstractSampleController : Controller { }
    internal class InternalVisibilityController : Controller { }
    public class NameWithoutSuffix : Controller { }
    public class EndsWithControllerButNotIController { } // ends with Controller but not implementing IController
    public class XController : Controller { } // should be included (length > 10, ends with Controller)

    /// <summary>
    /// Unit tests for the GetListOfControllerTypes method of ControllerTypeResolver. 
    /// </summary>
    [TestFixture]
    public class ControllerTypeResolverGetListOfControllerTypesTest
    {
        /// <summary>
        /// A stub implementation of IControllerBuilder with minimal functionality for testing. 
        /// </summary>
        private class StubControllerBuilder : IControllerBuilder
        {
            public HashSet<string> DefaultNamespaces { get; } = new(StringComparer.OrdinalIgnoreCase);
            public IControllerFactory GetControllerFactory() => throw new NotImplementedException();
            public void SetControllerFactory(Type controllerFactoryType) => throw new NotImplementedException();
            public void SetControllerFactory(IControllerFactory controllerFactory) => throw new NotImplementedException();
        }

        /// <summary>
        /// A fake build manager that returns a predefined set of assemblies. 
        /// </summary>
        private class FakeBuildManager : IBuildManager
        {
            private readonly ICollection _assemblies;
            public FakeBuildManager(ICollection assemblies) { _assemblies = assemblies; }
            public ICollection GetReferencedAssemblies() => _assemblies;
        }

        /// <summary>
        /// A testable subclass of ControllerTypeResolver that exposes the protected GetListOfControllerTypes method. 
        /// </summary>
        private class TestableControllerTypeResolver : ControllerTypeResolver
        {
            public TestableControllerTypeResolver(
                IEnumerable<string> areaNamespacesToIgnore,
                RouteCollection routes,
                IControllerBuilder controllerBuilder,
                IBuildManager buildManager)
                : base(areaNamespacesToIgnore, routes, controllerBuilder, buildManager) { }

            public List<Type> InvokeGetListOfControllerTypes() => base.GetListOfControllerTypes();
        }

        /// <summary>
        /// Tests that GetListOfControllerTypes returns only valid controllers:
        /// - public
        /// - non-abstract
        /// - implements IController
        /// - name ends with "Controller" (case-insensitive)
        /// - not in system assemblies
        /// - not in ignored namespaces (not tested here, as none are defined)
        /// Also tests that a dynamically created assembly with a "system-like" name is filtered out.
        /// </summary>
        [Test]
        public void GetListOfControllerTypes_ReturnsExpectedControllers_AndFiltersInvalidOnes()
        {
            // Arrange
            var testAssembly = typeof(ControllerTypeResolverGetListOfControllerTypesTest).Assembly;

            var sysAsmName = new AssemblyName("System.SyntheticTestAssembly");
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(sysAsmName, AssemblyBuilderAccess.Run);
            var moduleBuilder = asmBuilder.DefineDynamicModule("Main");
            var typeBuilder = moduleBuilder.DefineType("FakeController",
                TypeAttributes.Public | TypeAttributes.Class, typeof(Controller));
            typeBuilder.CreateType();

            var assemblies = new ArrayList
            {
                testAssembly,
                asmBuilder // should be skipped by IsSystemAssembly
            };

            var buildManager = new FakeBuildManager(assemblies);
            var resolver = new TestableControllerTypeResolver(
                Enumerable.Empty<string>(),
                new RouteCollection
                {
                    AppendTrailingSlash = false,
                    LowercaseUrls = false,
                    RouteExistingFiles = false
                },
                new StubControllerBuilder(),
                buildManager);

            // Act
            var result = resolver.InvokeGetListOfControllerTypes();

            var resultNames = result
                .Where(t => t.Assembly == testAssembly)
                .Select(t => t.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Assert inclusions
            Assert.That(resultNames.Contains(nameof(ValidPublicController)), "Valid public controller missing.");
            Assert.That(resultNames.Contains(nameof(Specialcontroller)), "Case-insensitive suffix controller missing.");
            Assert.That(resultNames.Contains(nameof(XController)), "XController should be included.");

            // Assert exclusions
            Assert.That(resultNames.Contains(nameof(AbstractSampleController)), Is.False, "Abstract controller should be excluded.");
            Assert.That(resultNames.Contains(nameof(InternalVisibilityController)), Is.False, "Internal controller should be excluded.");
            Assert.That(resultNames.Contains(nameof(NameWithoutSuffix)), Is.False, "Type without Controller suffix should be excluded.");
            Assert.That(resultNames.Contains(nameof(EndsWithControllerButNotIController)), Is.False, "Type ending with Controller but not implementing IController should be excluded.");

            // Ensure dynamic system-named assembly type not included
            Assert.That(result.Any(t => t.Assembly == asmBuilder && t.Name == "FakeController"), Is.False,
                "Controllers from system-like assemblies should be filtered out.");
        }
    }
}
