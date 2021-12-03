#nullable enable

using Garyon.Reflection;
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
        var cookiesClasses = AppDomainCache.Current.AllNonAbstractClasses.Where(TypeExtensions.Inherits<Cookies>);

        return cookiesClasses.FirstOrDefault(MemberInfoExtensions.HasCustomAttribute<SecretsContainerAttribute>)
                            ?.InitializeInstance<Cookies>();
    }
}
