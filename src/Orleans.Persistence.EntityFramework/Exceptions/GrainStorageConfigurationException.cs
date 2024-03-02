using System;

namespace Orleans.Persistence.EntityFramework.Exceptions;

public class GrainStorageConfigurationException : Exception
{
    public GrainStorageConfigurationException()
    {
    }

    public GrainStorageConfigurationException(string message) : base(message)
    {
    }

    public GrainStorageConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}