using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NLog;
using Sys.Rules.DmnEngine.Parser.Dto;
using Sys.Rules.DmnEngine.Parser.Dto.Diagram;
using Sys.Rules.DmnEngine.Utils;

namespace Sys.Rules.DmnEngine.Parser
{
    /// <summary>
    /// DMN Model XML parser
    /// </summary>
    public class DmnParser
    {
        /// <summary>
        /// XML namespace for DMN 1.1 documents
        /// </summary>
        public const string XmlNamespaceDmn11 = "http://www.omg.org/spec/DMN/20151101/dmn.xsd";
        /// <summary>
        /// XML namespace for DMN 1.3 documents
        /// </summary>
        public const string XmlNamespaceDmn13 = "https://www.omg.org/spec/DMN/20191111/MODEL/";
        /// <summary>
        /// XML namespace for DMN 1.4 documents
        /// </summary>
        public const string XmlNamespaceDmn14 = "https://www.omg.org/spec/DMN/20211108/MODEL/";
        /// <summary>
        /// XML namespace for DMN 1.5 documents
        /// </summary>
        public const string XmlNamespaceDmn15 = "https://www.omg.org/spec/DMN/20230324/MODEL/";

        /// <summary>
        /// DMNDI namespace for DMN 1.3 documents
        /// </summary>
        internal const string XmlNamespaceDmndi13 = "https://www.omg.org/spec/DMN/20191111/DMNDI/";
        /// <summary>
        /// DMNDI namespace for DMN 1.4 documents
        /// </summary>
        internal const string XmlNamespaceDmndi14 = "https://www.omg.org/spec/DMN/20211108/DMNDI/";
        /// <summary>
        /// DMNDI namespace for DMN 1.5 documents
        /// </summary>
        internal const string XmlNamespaceDmndi15 = "https://www.omg.org/spec/DMN/20230324/DMNDI/";

        /// <summary>
        /// DMN standard version to be used by <see cref="DmnParser"/>
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum DmnVersionEnum
        {
            /// <summary>
            /// DMN version 1.1
            /// </summary>
            V1_1,
            /// <summary>
            /// DMN version 1.3
            /// </summary>
            V1_3,
            /// <summary>
            /// DMN version 1.3 with extensions
            /// </summary>
            V1_3ext,
            /// <summary>
            /// DMN version 1.4
            /// </summary>
            V1_4,
            /// <summary>
            /// DMN version 1.5
            /// </summary>
            V1_5
        }

        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// DMN Model XML serializer for DMN v1.1
        /// </summary>
        protected static XmlSerializer DmnDefinitionsSerializer = new XmlSerializer(
            typeof(DmnModel), null, new Type[] { },
            defaultNamespace: XmlNamespaceDmn11, root: new XmlRootAttribute("definitions") { Namespace = XmlNamespaceDmn11 }
            );

        /// <summary>
        /// DMN Model XML serializer for DMN v1.3
        /// </summary>
        protected static XmlSerializer DmnDefinitionsSerializer13 = new XmlSerializer(
            typeof(DmnModel), null, new Type[] { },
            defaultNamespace: XmlNamespaceDmn13, root: new XmlRootAttribute("definitions") { Namespace = XmlNamespaceDmn13 }
            );

        /// <summary>
        /// Creates a DMN Model XML serializer with version-specific DMNDI namespace overrides
        /// </summary>
        private static XmlSerializer CreateDmnSerializer(string modelNamespace, string dmndiNamespace)
        {
            var overrides = new XmlAttributeOverrides();

            var diExtAttrs = new XmlAttributes();
            diExtAttrs.XmlElements.Add(new XmlElementAttribute("DMNDI") { Namespace = dmndiNamespace });
            overrides.Add(typeof(DmnModel), nameof(DmnModel.DiagramExtension), diExtAttrs);

            var diagramsAttrs = new XmlAttributes();
            diagramsAttrs.XmlElements.Add(new XmlElementAttribute("DMNDiagram") { Namespace = dmndiNamespace });
            overrides.Add(typeof(DmnDi), nameof(DmnDi.Diagrams), diagramsAttrs);

            var shapesAttrs = new XmlAttributes();
            shapesAttrs.XmlElements.Add(new XmlElementAttribute("DMNShape") { Namespace = dmndiNamespace });
            overrides.Add(typeof(Diagram), nameof(Diagram.Shapes), shapesAttrs);

            var edgesAttrs = new XmlAttributes();
            edgesAttrs.XmlElements.Add(new XmlElementAttribute("DMNEdge") { Namespace = dmndiNamespace });
            overrides.Add(typeof(Diagram), nameof(Diagram.Edges), edgesAttrs);

            return new XmlSerializer(
                typeof(DmnModel), overrides, new Type[] { },
                defaultNamespace: modelNamespace,
                root: new XmlRootAttribute("definitions") { Namespace = modelNamespace });
        }

        /// <summary>
        /// DMN Model XML serializer for DMN v1.4
        /// </summary>
        protected static XmlSerializer DmnDefinitionsSerializer14 = CreateDmnSerializer(XmlNamespaceDmn14, XmlNamespaceDmndi14);

        /// <summary>
        /// DMN Model XML serializer for DMN v1.5
        /// </summary>
        protected static XmlSerializer DmnDefinitionsSerializer15 = CreateDmnSerializer(XmlNamespaceDmn15, XmlNamespaceDmndi15);

        /// <summary>
        /// Parse the <paramref name="filePath">file</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_3">DMN standard version 1.3</see>.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <returns> Parsed DMN Model</returns>
        /// <exception cref="DmnParserException">Missing file path (<paramref name="filePath"/> is null or empty)</exception>
        /// <exception cref="DmnParserException">File doesn't exist</exception>
        /// <exception cref="DmnParserException">Can't parse file</exception>
        public static DmnModel Parse13(string filePath)
        {
            return Parse(filePath, DmnVersionEnum.V1_3);
        }

        /// <summary>
        /// Parse the <paramref name="filePath">file</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_3">DMN standard version 1.3 with extensions</see>.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <returns> Parsed DMN Model</returns>
        /// <exception cref="DmnParserException">Missing file path (<paramref name="filePath"/> is null or empty)</exception>
        /// <exception cref="DmnParserException">File doesn't exist</exception>
        /// <exception cref="DmnParserException">Can't parse file</exception>
        // ReSharper disable once InconsistentNaming
        public static DmnModel Parse13ext(string filePath)
        {
            return Parse(filePath, DmnVersionEnum.V1_3ext);
        }

        /// <summary>
        /// Parse the <paramref name="filePath">file</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_4">DMN standard version 1.4</see>.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <returns>Parsed DMN Model</returns>
        public static DmnModel Parse14(string filePath)
        {
            return Parse(filePath, DmnVersionEnum.V1_4);
        }

        /// <summary>
        /// Parse the <paramref name="filePath">file</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_5">DMN standard version 1.5</see>.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <returns>Parsed DMN Model</returns>
        public static DmnModel Parse15(string filePath)
        {
            return Parse(filePath, DmnVersionEnum.V1_5);
        }

        /// <summary>
        /// Detects the DMN version from the root element namespace of the given XML content.
        /// </summary>
        /// <param name="xmlContent">DMN XML content</param>
        /// <returns>Detected DMN version</returns>
        /// <exception cref="DmnParserException">Unknown namespace or missing definitions element</exception>
        public static DmnVersionEnum DetectVersion(string xmlContent)
        {
            using var reader = XmlReader.Create(new StringReader(xmlContent));
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "definitions")
                {
                    return reader.NamespaceURI switch
                    {
                        XmlNamespaceDmn11 => DmnVersionEnum.V1_1,
                        XmlNamespaceDmn13 => DmnVersionEnum.V1_3,
                        XmlNamespaceDmn14 => DmnVersionEnum.V1_4,
                        XmlNamespaceDmn15 => DmnVersionEnum.V1_5,
                        _ => throw Logger.Fatal<DmnParserException>($"Unknown DMN namespace: {reader.NamespaceURI}")
                    };
                }
            }
            throw Logger.Fatal<DmnParserException>("Could not detect DMN version - no definitions element found");
        }

        /// <summary>
        /// Parse the <paramref name="filePath">file</paramref> with DMN Model XML definition, automatically detecting the DMN version from the root element namespace.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <returns>Parsed DMN Model</returns>
        /// <summary>
        /// When true (default), DMN 1.3 files exported by Camunda Modeler are automatically
        /// parsed using <see cref="DmnVersionEnum.V1_3ext"/> rules, which use the output Name
        /// attribute (instead of Label) for variable naming — matching Camunda's conventions.
        /// </summary>
        public static bool AutoUpgradeCamundaExports { get; set; } = true;

        public static DmnModel ParseAutoDetect(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw Logger.Fatal<DmnParserException>($"{nameof(filePath)} is null or empty");
            if (!File.Exists(filePath)) throw Logger.Fatal<DmnParserException>($"File {filePath} doesn't exist");

            var content = File.ReadAllText(filePath);
            var version = DetectVersion(content);

            // Auto-upgrade V1_3 → V1_3ext for Camunda exports
            if (AutoUpgradeCamundaExports && version == DmnVersionEnum.V1_3 && IsCamundaExport(content))
            {
                version = DmnVersionEnum.V1_3ext;
                Logger.Info($"Auto-upgraded DMN version to V1_3ext (Camunda export detected) for file {filePath}");
            }
            else
            {
                Logger.Info($"Auto-detected DMN version {version} for file {filePath}");
            }

            return ParseString(content, version);
        }

        /// <summary>
        /// Checks whether the DMN XML content was exported by Camunda Modeler
        /// by inspecting the exporter attribute on the root definitions element.
        /// </summary>
        private static bool IsCamundaExport(string xmlContent)
        {
            try
            {
                using var reader = XmlReader.Create(new StringReader(xmlContent));
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "definitions")
                    {
                        var exporter = reader.GetAttribute("exporter");
                        return exporter != null &&
                               exporter.Contains("amunda", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch { /* best-effort detection */ }
            return false;
        }

        /// <summary>
        /// Parse the <paramref name="filePath">file</paramref> with DMN Model XML definition based on <paramref name="dmnVersion">DMN standard version</paramref>.
        /// </summary>
        /// <param name="filePath">Path to the file to be parsed</param>
        /// <param name="dmnVersion">DMN standard version to be used for parsing. Version 1.1 (<see cref="DmnVersionEnum.V1_1"/> is used as default if the version is not provided.</param>
        /// <returns> Parsed DMN Model</returns>
        /// <exception cref="DmnParserException">Missing file path (<paramref name="filePath"/> is null or empty)</exception>
        /// <exception cref="DmnParserException">File doesn't exist</exception>
        /// <exception cref="DmnParserException">Can't parse file</exception>
        public static DmnModel Parse(string filePath, DmnVersionEnum dmnVersion = DmnVersionEnum.V1_1)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw Logger.Fatal<DmnParserException>($"{nameof(filePath)} is null or empty");
            if (!File.Exists(filePath)) throw Logger.Fatal<DmnParserException>($"File {filePath} doesn't exist");

            DmnModel def;
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var rdr = new StreamReader(filePath))
            {
                try
                {
                    Logger.Info($"Parsing DMN file {filePath}...");

                    switch (dmnVersion)
                    {
                        case DmnVersionEnum.V1_1:
                            def = (DmnModel)DmnDefinitionsSerializer.Deserialize(rdr);
                            break;
                        case DmnVersionEnum.V1_3:
                        case DmnVersionEnum.V1_3ext:
                            def = (DmnModel)DmnDefinitionsSerializer13.Deserialize(rdr);
                            break;
                        case DmnVersionEnum.V1_4:
                            def = (DmnModel)DmnDefinitionsSerializer14.Deserialize(rdr);
                            break;
                        case DmnVersionEnum.V1_5:
                            def = (DmnModel)DmnDefinitionsSerializer15.Deserialize(rdr);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dmnVersion), dmnVersion, null);
                    }

                    Logger.Info($"Parsed DMN file {filePath}");
                }
                catch (Exception ex)
                {
                    throw Logger.Fatal<DmnParserException>($"Can't parse file {filePath}: {ex.Message}", ex);
                }
            }

            def.DmnVersion=dmnVersion;
            return def;
        }

        /// <summary>
        /// Parse the <paramref name="dmnDefinition">string</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_3">DMN standard version 1.3</see>.
        /// </summary>
        /// <param name="dmnDefinition">DMN Model XML definition</param>
        /// <returns>Parsed DMN Model</returns>
        /// <exception cref="DmnParserException">Missing DMN Model definition (<paramref name="dmnDefinition"/> is null or empty)</exception>
        /// <exception cref="DmnParserException">Can't parse DMN definition</exception>
        public static DmnModel ParseString13(string dmnDefinition)
        {
            return ParseString(dmnDefinition, DmnVersionEnum.V1_3);
        }

        /// <summary>
        /// Parse the <paramref name="dmnDefinition">string</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_3">DMN standard version 1.3 with extensions</see>.
        /// </summary>
        /// <param name="dmnDefinition">DMN Model XML definition</param>
        /// <returns>Parsed DMN Model</returns>
        /// <exception cref="DmnParserException">Missing DMN Model definition (<paramref name="dmnDefinition"/> is null or empty)</exception>
        /// <exception cref="DmnParserException">Can't parse DMN definition</exception>
        // ReSharper disable once InconsistentNaming
        public static DmnModel ParseString13ext(string dmnDefinition)
        {
            return ParseString(dmnDefinition, DmnVersionEnum.V1_3ext);
        }

        /// <summary>
        /// Parse the <paramref name="dmnDefinition">string</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_4">DMN standard version 1.4</see>.
        /// </summary>
        /// <param name="dmnDefinition">DMN Model XML definition</param>
        /// <returns>Parsed DMN Model</returns>
        public static DmnModel ParseString14(string dmnDefinition)
        {
            return ParseString(dmnDefinition, DmnVersionEnum.V1_4);
        }

        /// <summary>
        /// Parse the <paramref name="dmnDefinition">string</paramref> with DMN Model XML definition based on <see cref="DmnVersionEnum.V1_5">DMN standard version 1.5</see>.
        /// </summary>
        /// <param name="dmnDefinition">DMN Model XML definition</param>
        /// <returns>Parsed DMN Model</returns>
        public static DmnModel ParseString15(string dmnDefinition)
        {
            return ParseString(dmnDefinition, DmnVersionEnum.V1_5);
        }

        /// <summary>
        /// Parse the <paramref name="dmnDefinition">string</paramref> with DMN Model XML definition based on <paramref name="dmnVersion">DMN standard version</paramref>.
        /// </summary>
        /// <param name="dmnDefinition">DMN Model XML definition</param>
        /// <param name="dmnVersion">DMN standard version to be used for parsing. Version 1.1 (<see cref="DmnVersionEnum.V1_1"/> is used as default if the version is not provided.</param>
        /// <returns>Parsed DMN Model</returns>
        /// <exception cref="DmnParserException">Missing DMN Model definition (<paramref name="dmnDefinition"/> is null or empty)</exception>
        /// <exception cref="DmnParserException">Can't parse DMN definition</exception>
        public static DmnModel ParseString(string dmnDefinition, DmnVersionEnum dmnVersion = DmnVersionEnum.V1_1)
        {
            dmnDefinition = dmnDefinition?.Trim();
            if (string.IsNullOrWhiteSpace(dmnDefinition)) throw Logger.Fatal<DmnParserException>("Missing DMN Model definition");

            DmnModel def;
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var rdr = new StringReader(dmnDefinition))
            {
                try
                {
                    Logger.Info($"Parsing DMN definition from given string...");
                    if (Logger.IsTraceEnabled)
                        Logger.Trace(dmnDefinition);

                    switch (dmnVersion)
                    {
                        case DmnVersionEnum.V1_1:
                            def = (DmnModel)DmnDefinitionsSerializer.Deserialize(rdr);
                            break;
                        case DmnVersionEnum.V1_3:
                        case DmnVersionEnum.V1_3ext:
                            def = (DmnModel)DmnDefinitionsSerializer13.Deserialize(rdr);
                            break;
                        case DmnVersionEnum.V1_4:
                            def = (DmnModel)DmnDefinitionsSerializer14.Deserialize(rdr);
                            break;
                        case DmnVersionEnum.V1_5:
                            def = (DmnModel)DmnDefinitionsSerializer15.Deserialize(rdr);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dmnVersion), dmnVersion, null);
                    }

                    Logger.Info($"Parsed DMN definition from given string");
                }
                catch (Exception ex)
                {
                    throw Logger.Fatal<DmnParserException>($"Can't parse definition from given string: {ex.Message}", ex);
                }
            }
            
            def.DmnVersion=dmnVersion;
            return def;
        }
    }
}

