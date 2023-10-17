﻿using Kiota.Builder.OrderComparers;
using Kiota.Builder.Configuration;

namespace Kiota.Builder.CodeRenderers;
public class PythonCodeRenderer : CodeRenderer
{
    public PythonCodeRenderer(GenerationConfiguration configuration) : base(configuration, new CodeElementOrderComparerPython()) { }
}
