using System;
using System.Collections.Generic;

namespace Mesawer.ApplicationLayer.Interfaces
{
    /// <summary>
    /// Use it to seed tables for some data that less likely to change
    /// </summary>
    public interface IEntityTypeSeeder<out T>
    {
        public IEnumerable<T> GetSeedData();
        public Type GetDataType();
    }
}
