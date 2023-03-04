using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mesawer.InfrastructureLayer.EntityFrameworkCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.InfrastructureLayer.EntityFrameworkCore.Extensions;

[PublicAPI]
public static class ApplicationDbContextExtensions
{
    public static void ApplySeedsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
    {
        var @interface = typeof(IEntityTypeSeeder<>);
        var seeders    = assembly.GetTypes().Where(t => t.IsAssignableFrom(@interface)).ToList();

        if (!seeders.Any()) return;

        var getSeedDataMethod = @interface.GetMethods().Single(e => e.Name == "GetSeedData");
        var getDataTypeMethod = @interface.GetMethods().Single(e => e.Name == "GetData");

        foreach (var type in seeders)
        {
            var instance = Activator.CreateInstance(type);
            var data     = (getSeedDataMethod.Invoke(instance, null) as IEnumerable<object>)?.ToList();

            if (data is null || !data.Any() ||
                getDataTypeMethod.Invoke(instance, null) is not Type dataType)
                continue;

            modelBuilder.Entity(dataType).HasData(data);
        }
    }

    public static string GetExceptionMessage(this DbUpdateException exception)
        => exception.InnerException?.Message ?? exception.Message;
}
