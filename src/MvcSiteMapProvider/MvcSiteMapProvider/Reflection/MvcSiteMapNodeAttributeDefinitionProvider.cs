using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MvcSiteMapProvider.Builder;

namespace MvcSiteMapProvider.Reflection
{
    public class MvcSiteMapNodeAttributeDefinitionProvider
        : IMvcSiteMapNodeAttributeDefinitionProvider
    {
        public IEnumerable<IMvcSiteMapNodeAttributeDefinition> GetMvcSiteMapNodeAttributeDefinitions(
            IEnumerable<Assembly> assemblies)
        {
            var result = new List<IMvcSiteMapNodeAttributeDefinition>();
            var types = GetTypesFromAssemblies(assemblies);

            foreach (var type in types)
            {
                result.AddRange(GetAttributeDefinitionsForControllers(type));
                result.AddRange(GetAttributeDefinitionsForActions(type));
            }

            return result;
        }

        protected virtual IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        protected virtual IEnumerable<Type> GetTypesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            var result = new List<Type>();
            foreach (var assembly in assemblies)
            {
                result.AddRange(GetTypesFromAssembly(assembly));
            }

            return result;
        }

        protected virtual IEnumerable<IMvcSiteMapNodeAttributeDefinition>
            GetAttributeDefinitionsForControllers(Type type)
        {
            var result = new List<IMvcSiteMapNodeAttributeDefinition>();
            if (type.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), true) is not IMvcSiteMapNodeAttribute[]
                attributes)
            {
                return result;
            }

            foreach (var attribute in attributes)
            {
                result.Add(new MvcSiteMapNodeAttributeDefinitionForController
                {
                    SiteMapNodeAttribute = attribute, ControllerType = type
                });
            }

            return result;
        }

        protected virtual IEnumerable<IMvcSiteMapNodeAttributeDefinition> GetAttributeDefinitionsForActions(Type type)
        {
            var result = new List<IMvcSiteMapNodeAttributeDefinition>();
            // Add their methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), true).Any());

            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), false) is not IMvcSiteMapNodeAttribute
                    [] attributes)
                {
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    result.Add(new MvcSiteMapNodeAttributeDefinitionForAction
                    {
                        SiteMapNodeAttribute = attribute, ControllerType = type, ActionMethodInfo = method
                    });
                }
            }

            return result;
        }
    }
}
