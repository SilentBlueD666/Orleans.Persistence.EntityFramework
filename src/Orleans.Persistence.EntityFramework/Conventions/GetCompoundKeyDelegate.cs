using System;
using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework.Conventions
{
    public delegate ValueType GetCompoundKeyDelegate(IAddressable addressable, out string keyExt);
}