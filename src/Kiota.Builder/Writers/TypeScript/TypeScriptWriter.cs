﻿using Kiota.Builder.PathSegmenters;

namespace Kiota.Builder.Writers.TypeScript;

public class TypeScriptWriter : LanguageWriter
{
    public TypeScriptWriter(string rootPath, string clientNamespaceName, bool usesBackingStore = false)
    {
        PathSegmenter = new TypeScriptPathSegmenter(rootPath, clientNamespaceName);
        var conventionService = new TypeScriptConventionService();
        AddOrReplaceCodeElementWriter(new CodeClassDeclarationWriter(conventionService, clientNamespaceName));
        AddOrReplaceCodeElementWriter(new CodeBlockEndWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeEnumWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeMethodWriter(conventionService, usesBackingStore));
        AddOrReplaceCodeElementWriter(new CodeFunctionWriter(conventionService, clientNamespaceName));
        AddOrReplaceCodeElementWriter(new CodePropertyWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeTypeWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeInterfaceDeclarationWriter(conventionService, clientNamespaceName));
        AddOrReplaceCodeElementWriter(new CodeFileBlockEndWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeFileDeclarationWriter(conventionService, clientNamespaceName));
        AddOrReplaceCodeElementWriter(new CodeConstantWriter(conventionService));
    }
}
