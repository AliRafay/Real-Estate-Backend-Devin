using Demo.WebApi.Application.Common.Messaging;
using Demo.WebApi.Domain.Identity;
using Demo.WebApi.Host.Controllers;
using Demo.WebApi.Infrastructure.Auth;
using NetArchTest.Rules;
using System.Reflection;

namespace Demo.WebApi.Tests;

public class ArchitectureTests
{
    public ArchitectureTests() => this.InitializeAssemblies();

    private void InitializeAssemblies()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

        var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
        var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

        toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
    }

    [Fact]
    public void DomainShouldNotDependOnAnyOtherLayerTest()
    {
        var domainsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "Demo.WebApi.Domain");

        var result = Types.InAssembly(domainsAssembly)
            .Should()
            .NotHaveDependencyOn("Demo.WebApi.Application")
            .And()
            .NotHaveDependencyOn("Demo.WebApi.Infrastructure")
            .And()
            .NotHaveDependencyOn("Demo.WebApi.Host")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void ApplicationShouldNotDependOnInfrastructureOrPresentationTest()
    {
        var applicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "Demo.WebApi.Application");

        var result = Types.InAssembly(applicationAssembly)
            .Should()
            .NotHaveDependencyOn("Demo.WebApi.Infrastructure")
            .And()
            .NotHaveDependencyOn("Demo.WebApi.Host")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void InfrastructureShouldNotDependOnPresentationTest()
    {
        var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "Demo.WebApi.Infrastructure");

        var result = Types.InAssembly(infrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Demo.WebApi.Host")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
