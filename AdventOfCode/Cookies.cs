#nullable enable

using Garyon.Reflection;
using System;
using System.Linq;

namespace AdventOfCode;

public static class SecretsStorage
{
    private static bool searchedCookies = false;
    private static Cookies? cookies;

    public static Cookies? Cookies
    {
        get
        {
            if (searchedCookies)
                return cookies;

            searchedCookies = true;
            return cookies = GetCookies();
        }
    }

    private static Cookies? GetCookies()
    {
        // Do NOT you worry my friend, Garyon 0.2.6 is on its way:tm:
        var cookiesClasses = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.Inherits<Cookies>());

        return cookiesClasses
                  .FirstOrDefault(Utilities.MemberInfoExtensions.HasCustomAttribute<SecretsContainerAttribute>)
                 ?.InitializeInstance<Cookies>();
    }
}

/// <summary>Decorates a class to denote that it acts as a container for secrets.</summary>
public interface ISecretsContainer { }

/// <summary>Represents an uninitialized cookie container for performing input requests.</summary>
/// <remarks>WARNING: Do not eat! Santa will be sad!</remarks>
public abstract class Cookies
{
    public abstract string GA { get; }
    public abstract string Session { get; }

    public override string ToString()
    {
        return $"_ga={GA}; session={Session}";
    }
}

/// <summary>Marks a class as a secrets container from which information should be retrieved.</summary>
/// <remarks>It is important that the marked class implements the <seealso cref="ISecretsContainer"/> interface.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SecretsContainerAttribute : Attribute
{
}
