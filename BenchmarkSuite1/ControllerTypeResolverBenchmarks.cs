using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Reflection;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.Compilation;

namespace MvcSiteMapProvider.Benchmarks
{
    [MemoryDiagnoser]
    public class ControllerTypeResolverBenchmarks
    {
        private class StubControllerBuilder : IControllerBuilder
        {
            public HashSet<string> DefaultNamespaces { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public IControllerFactory GetControllerFactory() => new DefaultControllerFactory();
            public void SetControllerFactory(Type controllerFactoryType) { }
            public void SetControllerFactory(IControllerFactory controllerFactory) { }
        }

        private class StubBuildManager : IBuildManager
        {
            public ICollection GetReferencedAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
        }

        private class ExposedControllerTypeResolver : ControllerTypeResolver
        {
            public ExposedControllerTypeResolver(IEnumerable<string> areaNamespacesToIgnore, RouteCollection routes, IControllerBuilder controllerBuilder, IBuildManager buildManager)
                : base(areaNamespacesToIgnore, routes, controllerBuilder, buildManager) { }
            public List<Type> InvokeGetListOfControllerTypes() => base.GetListOfControllerTypes();
        }

        private class LegacyControllerTypeResolver : ControllerTypeResolver
        {
            public LegacyControllerTypeResolver(IEnumerable<string> areaNamespacesToIgnore, RouteCollection routes, IControllerBuilder controllerBuilder, IBuildManager buildManager)
                : base(areaNamespacesToIgnore, routes, controllerBuilder, buildManager) { }

            public List<Type> InvokeLegacyGetListOfControllerTypes()
            {
                IEnumerable<Type> typesSoFar = Type.EmptyTypes;
                var assemblies = BuildManager.GetReferencedAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    Type[] typesInAsm;
                    try
                    {
                        typesInAsm = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        typesInAsm = ex.Types;
                    }
                    typesSoFar = typesSoFar.Concat(typesInAsm);
                }
                return typesSoFar.Where(t =>
                    t is { IsClass: true, IsPublic: true, IsAbstract: false } &&
                    t.Name.IndexOf("Controller", StringComparison.OrdinalIgnoreCase) != -1 &&
                    typeof(IController).IsAssignableFrom(t)).ToList();
            }
        }

        private RouteCollection _routes = null!;
        private IControllerBuilder _controllerBuilder = null!;
        private IBuildManager _buildManager = null!;
        private IReadOnlyList<string> _ignoreNamespaces = null!;

        [GlobalSetup]
        public void Setup()
        {
            _routes = new RouteCollection();
            _controllerBuilder = new StubControllerBuilder();
            _buildManager = new StubBuildManager();
            _ignoreNamespaces = Array.Empty<string>();
        }

        [Benchmark(Baseline = true)]
        public int BuildControllerTypeList()
        {
            var resolver = new ExposedControllerTypeResolver(_ignoreNamespaces, _routes, _controllerBuilder, _buildManager);
            var list = resolver.InvokeGetListOfControllerTypes();
            return list.Count;
        }

        [Benchmark]
        public Type? ResolveControllerType_FirstCall()
        {
            var resolver = new ExposedControllerTypeResolver(_ignoreNamespaces, _routes, _controllerBuilder, _buildManager);
            return resolver.ResolveControllerType("", "Home");
        }

        [Benchmark]
        public int BuildControllerTypeList_Legacy()
        {
            var resolver = new LegacyControllerTypeResolver(_ignoreNamespaces, _routes, _controllerBuilder, _buildManager);
            var list = resolver.InvokeLegacyGetListOfControllerTypes();
            return list.Count;
        }
    }
}
