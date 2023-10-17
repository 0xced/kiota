﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kiota.Builder.CodeDOM;
using Kiota.Builder.Configuration;
using Kiota.Builder.Extensions;

namespace Kiota.Builder.Refiners;
public class PythonRefiner : CommonLanguageRefiner, ILanguageRefiner
{
    public PythonRefiner(GenerationConfiguration configuration) : base(configuration) { }
    public override Task Refine(CodeNamespace generatedCode, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            RemoveMethodByKind(generatedCode, CodeMethodKind.RawUrlConstructor);
            AddDefaultImports(generatedCode, defaultUsingEvaluators);
            DisableActionOf(generatedCode,
            CodeParameterKind.RequestConfiguration);
            MoveRequestBuilderPropertiesToBaseType(generatedCode,
                new CodeUsing
                {
                    Name = "BaseRequestBuilder",
                    Declaration = new CodeType
                    {
                        Name = $"{AbstractionsPackageName}.base_request_builder",
                        IsExternal = true
                    }
                }, AccessModifier.Public);
            cancellationToken.ThrowIfCancellationRequested();
            ReplaceIndexersByMethodsWithParameter(generatedCode,
                false,
                static x => $"by_{x.ToSnakeCase()}",
                static x => x.ToSnakeCase(),
                GenerationLanguage.Python);
            RemoveCancellationParameter(generatedCode);
            CorrectCoreType(generatedCode, CorrectMethodType, CorrectPropertyType, CorrectImplements);
            cancellationToken.ThrowIfCancellationRequested();
            CorrectCoreTypesForBackingStore(generatedCode, "field(default_factory=BackingStoreFactorySingleton(backing_store_factory=None).backing_store_factory.create_backing_store, repr=False)");
            AddPropertiesAndMethodTypesImports(generatedCode, true, true, true, codeTypeFilter);
            AddParsableImplementsForModelClasses(generatedCode, "Parsable");
            cancellationToken.ThrowIfCancellationRequested();
            ReplaceBinaryByNativeType(generatedCode, "bytes", string.Empty);
            ReplaceReservedNames(
                generatedCode,
                new PythonReservedNamesProvider(),
                static x => $"{x}_"
            );
            ReplaceReservedExceptionPropertyNames(
                generatedCode,
                new PythonExceptionsReservedNamesProvider(),
                static x => $"{x}_"
            );
            RemoveRequestConfigurationClassesCommonProperties(generatedCode,
                new CodeUsing
                {
                    Name = "BaseRequestConfiguration",
                    Declaration = new CodeType
                    {
                        Name = $"{AbstractionsPackageName}.base_request_configuration",
                        IsExternal = true
                    }
                });
            cancellationToken.ThrowIfCancellationRequested();
            MoveClassesWithNamespaceNamesUnderNamespace(generatedCode);
            ReplacePropertyNames(generatedCode,
                new() {
                    CodePropertyKind.Custom,
                    CodePropertyKind.QueryParameter,
                },
                static s => s.ToSnakeCase());
            AddParentClassToErrorClasses(
                generatedCode,
                "APIError",
                $"{AbstractionsPackageName}.api_error"
            );
            AddGetterAndSetterMethods(generatedCode,
                new() {
                    CodePropertyKind.Custom,
                    CodePropertyKind.AdditionalData,
                },
                static (_, s) => s.ToSnakeCase(),
                false,
                false,
                string.Empty,
                string.Empty);
            AddConstructorsForDefaultValues(generatedCode, true);
            var defaultConfiguration = new GenerationConfiguration();
            cancellationToken.ThrowIfCancellationRequested();
            ReplaceDefaultSerializationModules(
                generatedCode,
                defaultConfiguration.Serializers,
                new(StringComparer.OrdinalIgnoreCase) {
                    "kiota_serialization_json.json_serialization_writer_factory.JsonSerializationWriterFactory",
                    "kiota_serialization_text.text_serialization_writer_factory.TextSerializationWriterFactory"
                }
            );
            ReplaceDefaultDeserializationModules(
                generatedCode,
                defaultConfiguration.Deserializers,
                new(StringComparer.OrdinalIgnoreCase) {
                    "kiota_serialization_json.json_parse_node_factory.JsonParseNodeFactory",
                    "kiota_serialization_text.text_parse_node_factory.TextParseNodeFactory"
                }
            );
            AddSerializationModulesImport(generatedCode,
            new[] { $"{AbstractionsPackageName}.api_client_builder.register_default_serializer",
                    $"{AbstractionsPackageName}.api_client_builder.enable_backing_store_for_serialization_writer_factory",
                    $"{AbstractionsPackageName}.serialization.SerializationWriterFactoryRegistry"},
            new[] { $"{AbstractionsPackageName}.api_client_builder.register_default_deserializer",
                    $"{AbstractionsPackageName}.serialization.ParseNodeFactoryRegistry" });
            cancellationToken.ThrowIfCancellationRequested();
            AddQueryParameterMapperMethod(
                generatedCode
            );
            AddDiscriminatorMappingsUsingsToParentClasses(
                generatedCode,
                "ParseNode",
                addUsings: true,
                includeParentNamespace: true
            );
            AddPrimaryErrorMessage(generatedCode,
                "primary_message",
                () => new CodeType { Name = "str", IsNullable = false, IsExternal = true },
                true
            );
        }, cancellationToken);
    }

    private const string AbstractionsPackageName = "kiota_abstractions";
    private static readonly AdditionalUsingEvaluator[] defaultUsingEvaluators = {
        new (static x => x is CodeClass, "__future__", "annotations"),
        new (static x => x is CodeClass, "typing", "Any, Callable, Dict, List, Optional, TYPE_CHECKING, Union"),
        new (static x => x is CodeProperty prop && prop.IsOfKind(CodePropertyKind.RequestAdapter),
            $"{AbstractionsPackageName}.request_adapter", "RequestAdapter"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.RequestGenerator),
            $"{AbstractionsPackageName}.method", "Method"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.RequestGenerator),
            $"{AbstractionsPackageName}.request_information", "RequestInformation"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.RequestGenerator),
            $"{AbstractionsPackageName}.request_option", "RequestOption"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.Serializer),
            $"{AbstractionsPackageName}.serialization", "SerializationWriter"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.Deserializer),
            $"{AbstractionsPackageName}.serialization", "ParseNode"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.Constructor, CodeMethodKind.ClientConstructor, CodeMethodKind.IndexerBackwardCompatibility),
            $"{AbstractionsPackageName}.get_path_parameters", "get_path_parameters"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.RequestExecutor),
            $"{AbstractionsPackageName}.serialization", "Parsable", "ParsableFactory"),
        new (static x => x is CodeClass @class && @class.IsOfKind(CodeClassKind.Model),
            $"{AbstractionsPackageName}.serialization", "Parsable"),
        new (static x => x is CodeClass @class && @class.IsOfKind(CodeClassKind.Model) && @class.Properties.Any(static x => x.IsOfKind(CodePropertyKind.AdditionalData)),
            $"{AbstractionsPackageName}.serialization", "AdditionalDataHolder"),
        new (static x => x is CodeMethod method && method.IsOfKind(CodeMethodKind.ClientConstructor) &&
                    method.Parameters.Any(y => y.IsOfKind(CodeParameterKind.BackingStore)),
            $"{AbstractionsPackageName}.store", "BackingStoreFactory", "BackingStoreFactorySingleton"),
        new (static x => x is CodeProperty prop && prop.IsOfKind(CodePropertyKind.BackingStore),
            $"{AbstractionsPackageName}.store", "BackedModel", "BackingStore", "BackingStoreFactorySingleton" ),
        new (static x => x is CodeClass @class && (@class.IsOfKind(CodeClassKind.Model) || x.Parent is CodeClass), "dataclasses", "dataclass, field"),
        new (static x => x is CodeClass { OriginalComposedType: CodeIntersectionType intersectionType } && intersectionType.Types.Any(static y => !y.IsExternal) && intersectionType.DiscriminatorInformation.HasBasicDiscriminatorInformation,
            $"{AbstractionsPackageName}.serialization", "ParseNodeHelper"),
    };
    private static void CorrectCommonNames(CodeElement currentElement)
    {
        if (currentElement is CodeMethod m &&
            currentElement.Parent is CodeClass parentClassM)
        {
            parentClassM.RenameChildElement(m.Name, m.Name.ToFirstCharacterLowerCase().ToSnakeCase());

            foreach (var param in m.Parameters)
            {
                param.Name = param.Name.ToFirstCharacterLowerCase().ToSnakeCase();
            }
        }
        else if (currentElement is CodeClass c)
        {
            c.Name = c.Name.ToFirstCharacterUpperCase();
        }
        else if (currentElement is CodeProperty p &&
            (p.IsOfKind(CodePropertyKind.RequestAdapter, CodePropertyKind.BackingStore,
                 CodePropertyKind.Custom) ||
            p.IsOfKind(CodePropertyKind.PathParameters) ||
            p.IsOfKind(CodePropertyKind.QueryParameters) ||
            p.IsOfKind(CodePropertyKind.UrlTemplate)) &&
            currentElement.Parent is CodeClass parentClassP)
        {
            if (p.SerializationName != null)
                p.SerializationName = p.Name;

            parentClassP.RenameChildElement(p.Name, p.Name.ToFirstCharacterLowerCase().ToSnakeCase());
        }
        else if (currentElement is CodeIndexer i)
        {
            i.IndexParameter.Name = i.IndexParameter.Name.ToFirstCharacterLowerCase().ToSnakeCase();
        }
        else if (currentElement is CodeEnum e)
        {
            foreach (var option in e.Options)
            {
                if (!string.IsNullOrEmpty(option.Name) && Char.IsLower(option.Name[0]))
                {
                    if (string.IsNullOrEmpty(option.SerializationName))
                    {
                        option.SerializationName = option.Name;
                    }
                    option.Name = option.Name.ToCamelCase().ToFirstCharacterUpperCase();
                }
            }
        }

        CrawlTree(currentElement, element => CorrectCommonNames(element));
    }
    
    private static void CorrectImplements(ProprietableBlockDeclaration block)
    {
        block.Implements.Where(x => "IAdditionalDataHolder".Equals(x.Name, StringComparison.OrdinalIgnoreCase)).ToList().ForEach(x => x.Name = x.Name[1..]); // skipping the I
    }
    private static void CorrectPropertyType(CodeProperty currentProperty)
    {
        if (currentProperty.IsOfKind(CodePropertyKind.RequestAdapter))
            currentProperty.Type.Name = "RequestAdapter";
        else if (currentProperty.IsOfKind(CodePropertyKind.BackingStore))
            currentProperty.Type.Name = currentProperty.Type.Name[1..]; // removing the "I"
        else if (currentProperty.IsOfKind(CodePropertyKind.Options))
            currentProperty.Type.Name = "List[RequestOption]";
        else if (currentProperty.IsOfKind(CodePropertyKind.Headers))
            currentProperty.Type.Name = "Dict[str, Union[str, List[str]]]";
        else if (currentProperty.IsOfKind(CodePropertyKind.AdditionalData))
        {
            currentProperty.Type.Name = "Dict[str, Any]";
            currentProperty.DefaultValue = "field(default_factory=dict)";
        }
        else if (currentProperty.IsOfKind(CodePropertyKind.PathParameters))
        {
            currentProperty.Type.IsNullable = false;
            currentProperty.Type.Name = "Dict[str, Any]";
            if (!string.IsNullOrEmpty(currentProperty.DefaultValue))
                currentProperty.DefaultValue = "{}";
        }
        CorrectCoreTypes(currentProperty.Parent as CodeClass, DateTypesReplacements, currentProperty.Type);
    }
    private static void CorrectMethodType(CodeMethod currentMethod)
    {
        if (currentMethod.IsOfKind(CodeMethodKind.Serializer))
            currentMethod.Parameters.Where(x => x.IsOfKind(CodeParameterKind.Serializer) && x.Type.Name.StartsWith("i", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(x => x.Type.Name = x.Type.Name[1..]);
        else if (currentMethod.IsOfKind(CodeMethodKind.Deserializer))
            currentMethod.ReturnType.Name = "Dict[str, Callable[[ParseNode], None]]";
        else if (currentMethod.IsOfKind(CodeMethodKind.ClientConstructor, CodeMethodKind.Constructor, CodeMethodKind.Factory))
        {
            currentMethod.Parameters.Where(x => x.IsOfKind(CodeParameterKind.RequestAdapter, CodeParameterKind.BackingStore, CodeParameterKind.ParseNode))
                .Where(static x => x.Type.Name.StartsWith("I", StringComparison.InvariantCultureIgnoreCase))
                .ToList()
                .ForEach(static x => x.Type.Name = x.Type.Name[1..]); // removing the "I"
            var urlTplParams = currentMethod.Parameters.FirstOrDefault(x => x.IsOfKind(CodeParameterKind.PathParameters));
            if (urlTplParams != null &&
                urlTplParams.Type is CodeType originalType)
            {
                originalType.Name = "Dict[str, Any]";
                urlTplParams.Documentation.Description = "The raw url or the Url template parameters for the request.";
                var unionType = new CodeUnionType
                {
                    Name = "raw_url_or_template_parameters",
                    IsNullable = true,
                };
                unionType.AddType(originalType, new()
                {
                    Name = "str",
                    IsNullable = true,
                    IsExternal = true,
                });
                urlTplParams.Type = unionType;
            }
        }
        CorrectCoreTypes(currentMethod.Parent as CodeClass, DateTypesReplacements, currentMethod.Parameters
                                            .Select(x => x.Type)
                                            .Union(new[] { currentMethod.ReturnType })
                                            .ToArray());
    }

    // Caters for QueryParameters and RequestConfiguration which are implemented as nested classes.
    // No imports required for nested classes in Python.
    public static IEnumerable<CodeTypeBase> codeTypeFilter(IEnumerable<CodeTypeBase> usingsToAdd)
    {
        var nestedTypes = usingsToAdd.OfType<CodeType>().Where(
            static codeType => codeType.TypeDefinition is CodeClass codeClass
            && codeClass.IsOfKind(CodeClassKind.RequestConfiguration, CodeClassKind.QueryParameters));

        return usingsToAdd.Except(nestedTypes);
    }

    private const string DateTimePackageName = "datetime";
    private const string UUIDPackageName = "uuid";
    private static readonly Dictionary<string, (string, CodeUsing?)> DateTypesReplacements = new(StringComparer.OrdinalIgnoreCase) {
    {"DateTimeOffset", ("datetime.datetime", new CodeUsing {
                                    Name = DateTimePackageName,
                                    Declaration = new CodeType {
                                        Name = "-",
                                        IsExternal = true,
                                    },
                                })},
    {"TimeSpan", ("datetime.timedelta", new CodeUsing {
                                    Name = DateTimePackageName,
                                    Declaration = new CodeType {
                                        Name = "-",
                                        IsExternal = true,
                                    },
                                })},
    {"DateOnly", ("datetime.date", new CodeUsing {
                            Name = DateTimePackageName,
                            Declaration = new CodeType {
                                Name = "-",
                                IsExternal = true,
                            },
                        })},
    {"TimeOnly", ("datetime.time", new CodeUsing {
                            Name = DateTimePackageName,
                            Declaration = new CodeType {
                                Name = "-",
                                IsExternal = true,
                            },
                        })},
    {"Guid", ("uuid", new CodeUsing {
                        Name = "UUID",
                        Declaration = new CodeType {
                            Name = UUIDPackageName,
                            IsExternal = true,
                        },
                    })},
    };
}
