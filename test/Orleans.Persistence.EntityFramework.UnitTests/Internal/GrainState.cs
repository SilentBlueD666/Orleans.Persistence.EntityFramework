﻿using System;

namespace Orleans.Persistence.EntityFramework.UnitTests.Internal
{
    public class TestGrainState<T> : IGrainState
        where T : class, new()
    {
        public T State;
        object IGrainState.State
        {
            get => State;
            set => State = value as T;
        }

        public Type Type => typeof(TestGrainState<T>);

        public string ETag { get; set; }
    }
}