using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Mesawer.InfrastructureLayer.EntityFrameworkCore.Interfaces;

/// <summary>
/// Use it to seed tables for some data that less likely to change
/// </summary>
[PublicAPI]
public interface IEntityTypeSeeder<out T>
{
    public IEnumerable<T> GetSeedData();
    public Type GetDataType();
}
