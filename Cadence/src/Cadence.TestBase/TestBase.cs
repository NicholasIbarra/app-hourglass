using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachma.Base.Tests;

public class TestBase<T> : TestBase where T : class
{
    protected T Instance() => GetInstance<T>();
}

public class TestBase
{
    protected AutoMocker Mocker { get; } = new();

    protected T GetInstance<T>() where T : class
    {
        return Mocker.CreateInstance<T>(true);
    }
}
