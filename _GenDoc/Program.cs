// ======================================================================
// 低代码平台 - 基于实体与查询的成套业务页面自动生成设计方案
// ======================================================================
// 文档风格：ModernCorporate（商务报告风）+ CJK 中文排版
// OpenXML SDK 3.2.0
// ======================================================================

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WpPageSize = DocumentFormat.OpenXml.Wordprocessing.PageSize;

const string OUTPUT_PATH = @"d:\Project\MDD\9.0\nactivity\docs\低代码平台-成套业务页面自动生成设计方案.docx";

// 如果目标目录不存在先创建
var dir = System.IO.Path.GetDirectoryName(OUTPUT_PATH);
if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
    System.IO.Directory.CreateDirectory(dir);

using var doc = WordprocessingDocument.Create(OUTPUT_PATH, WordprocessingDocumentType.Document);
var mainPart = doc.AddMainDocumentPart();
mainPart.Document = new Document(new Body());
var body = mainPart.Document.Body!;

// ======================================================================
// 1. 样式系统（ModernCorporate + 中文适配）
// ======================================================================
var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
stylesPart.Styles = new Styles();
var styles = stylesPart.Styles;

// —— 文档默认值：Aptos / Calibri + SimSun，11pt 正文，1.15 行距，段后8pt
styles.Append(new DocDefaults(
    new RunPropertiesDefault(new RunPropertiesBaseStyle(
        new RunFonts
        {
            Ascii = "Calibri", HighAnsi = "Calibri",
            EastAsia = "SimSun", ComplexScript = "Calibri"
        },
        new FontSize { Val = "22" },
        new FontSizeComplexScript { Val = "22" },
        new Color { Val = "333333" },
        new Languages { Val = "en-US", EastAsia = "zh-CN" }
    )),
    new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(
        new SpacingBetweenLines
        {
            Line = "276",              // 1.15x
            LineRule = LineSpacingRuleValues.Auto,
            After = "160"              // 8pt after
        }
    ))
));

// —— Normal 默认样式
styles.Append(new Style(
    new StyleName { Val = "Normal" },
    new UIPriority { Val = 0 },
    new PrimaryStyle()
)
{
    Type = StyleValues.Paragraph,
    StyleId = "Normal",
    Default = true
});

// —— 标题样式 1-4
(int sz, int before, int after, string color, bool bold)[] headingCfg = new[]
{
    (40, 480, 120, "1F3864", false), // H1 20pt
    (32, 360, 80,  "1F3864", false), // H2 16pt
    (26, 240, 80,  "1F3864", true),  // H3 13pt
    (24, 160, 60,  "2F5496", true)   // H4 12pt
};
for (int i = 0; i < headingCfg.Length; i++)
{
    var c = headingCfg[i];
    var style = new Style { Type = StyleValues.Paragraph, StyleId = $"Heading{i + 1}" };
    style.Append(new StyleName { Val = $"heading {i + 1}" });
    style.Append(new BasedOn { Val = "Normal" });
    style.Append(new NextParagraphStyle { Val = "Normal" });
    style.Append(new UIPriority { Val = 9 });
    style.Append(new PrimaryStyle());
    var pPr = new StyleParagraphProperties(
        new KeepNext(), new KeepLines(),
        new SpacingBetweenLines
        {
            Before = c.before.ToString(),
            After = c.after.ToString()
        },
        new OutlineLevel { Val = i }
    );
    style.Append(pPr);
    var rPr = new StyleRunProperties(
        new RunFonts
        {
            Ascii = "Calibri", HighAnsi = "Calibri",
            EastAsia = "Microsoft YaHei"
        },
        new Color { Val = c.color, ThemeColor = ThemeColorValues.Accent1 },
        new FontSize { Val = c.sz.ToString() },
        new FontSizeComplexScript { Val = c.sz.ToString() }
    );
    if (c.bold) { rPr.Append(new Bold()); rPr.Append(new BoldComplexScript()); }
    style.Append(rPr);
    styles.Append(style);
}

// —— Title / Subtitle 样式
styles.Append(new Style(
    new StyleName { Val = "Title" },
    new BasedOn { Val = "Normal" },
    new UIPriority { Val = 10 },
    new PrimaryStyle(),
    new StyleParagraphProperties(
        new SpacingBetweenLines { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto },
        new Justification { Val = JustificationValues.Center }
    ),
    new StyleRunProperties(
        new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri", EastAsia = "Microsoft YaHei" },
        new FontSize { Val = "56" }, new FontSizeComplexScript { Val = "56" },
        new Color { Val = "1F3864" },
        new Bold(), new BoldComplexScript()
    )
) { Type = StyleValues.Paragraph, StyleId = "Title" });

styles.Append(new Style(
    new StyleName { Val = "Subtitle" },
    new BasedOn { Val = "Normal" },
    new UIPriority { Val = 11 },
    new PrimaryStyle(),
    new StyleParagraphProperties(
        new SpacingBetweenLines { After = "320" },
        new Justification { Val = JustificationValues.Center }
    ),
    new StyleRunProperties(
        new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri", EastAsia = "SimSun" },
        new Color { Val = "5A5A5A" },
        new FontSize { Val = "24" }, new FontSizeComplexScript { Val = "24" }
    )
) { Type = StyleValues.Paragraph, StyleId = "Subtitle" });

// —— 正文-项目符号（ListParagraph）
styles.Append(new Style(
    new StyleName { Val = "List Paragraph" },
    new BasedOn { Val = "Normal" },
    new UIPriority { Val = 20 },
    new StyleParagraphProperties(
        new Indentation { Left = "480", Hanging = "240" },
        new SpacingBetweenLines { After = "80" }
    )
) { Type = StyleValues.Paragraph, StyleId = "ListParagraph" });

styles.Save();

// —— 编号：项目符号
var numPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
numPart.Numbering = new Numbering();
var absNum = new AbstractNum(
    new Level(
        new NumberingFormat { Val = NumberFormatValues.Bullet },
        new LevelText { Val = "\u25CF" },
        new LevelJustification { Val = LevelJustificationValues.Left },
        new ParagraphProperties(new Indentation { Left = "480", Hanging = "240" }),
        new RunProperties(new FontSize { Val = "22" })
    ) { LevelIndex = 0 }
) { AbstractNumberId = 1 };
numPart.Numbering.Append(absNum);
numPart.Numbering.Append(new NumberingInstance(new AbstractNumId { Val = 1 }) { NumberID = 1 });
numPart.Numbering.Save();

// —— 设置、字体表、主题（最小化）
var settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
settingsPart.Settings = new Settings(
    new Compatibility(new CompatibilitySetting
    {
        Name = CompatSettingNameValues.CompatibilityMode,
        Uri = "http://schemas.microsoft.com/office/word",
        Val = "15"
    })
);
settingsPart.Settings.Save();

var fontPart = mainPart.AddNewPart<FontTablePart>();
fontPart.Fonts = new Fonts(
    new Font(new FontCharSet { Val = "00" }, new FontFamily { Val = FontFamilyValues.Swiss }) { Name = "Calibri" },
    new Font(new FontCharSet { Val = "00" }, new FontFamily { Val = FontFamilyValues.Swiss }) { Name = "Microsoft YaHei" },
    new Font(new FontCharSet { Val = "00" }, new FontFamily { Val = FontFamilyValues.Auto }) { Name = "SimSun" }
);
fontPart.Fonts.Save();

// ======================================================================
// 2. 页脚 + 页码
// ======================================================================
var footerPart = mainPart.AddNewPart<FooterPart>();
footerPart.Footer = new Footer(
    new Paragraph(
        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
        new Run(new Text("第 ")),
        new Run(
            new FieldChar { FieldCharType = FieldCharValues.Begin }
        ),
        new Run(new FieldCode { Text = "PAGE" }),
        new Run(
            new FieldChar { FieldCharType = FieldCharValues.Separate }
        ),
        new Run(new Text("1")),
        new Run(
            new FieldChar { FieldCharType = FieldCharValues.End }
        ),
        new Run(new Text(" 页 / 共 ")),
        new Run(
            new FieldChar { FieldCharType = FieldCharValues.Begin }
        ),
        new Run(new FieldCode { Text = "NUMPAGES" }),
        new Run(
            new FieldChar { FieldCharType = FieldCharValues.Separate }
        ),
        new Run(new Text("1")),
        new Run(
            new FieldChar { FieldCharType = FieldCharValues.End }
        ),
        new Run(new Text(" 页"))
    )
);
footerPart.Footer.Save();
var footerId = mainPart.GetIdOfPart(footerPart);

// ======================================================================
// 3. 页面设置：A4，1 英寸页边距
// ======================================================================
var sectPr = new SectionProperties(
    new WpPageSize { Width = 11906, Height = 16838 }, // A4
    new PageMargin
    {
        Top = 1440, Right = 1440, Bottom = 1440, Left = 1440,
        Header = 720, Footer = 720, Gutter = 0
    },
    new FooterReference { Type = HeaderFooterValues.Default, Id = footerId }
);

// ======================================================================
// 4. 工具函数：标题、段落、列表、表格
// ======================================================================
Paragraph H(int level, string text)
{
    var p = new Paragraph();
    var pPr = new ParagraphProperties();
    pPr.Append(new ParagraphStyleId { Val = $"Heading{level}" });
    p.Append(pPr);
    p.Append(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    return p;
}

Paragraph P(string text, string? style = null)
{
    var p = new Paragraph();
    if (!string.IsNullOrEmpty(style))
        p.Append(new ParagraphProperties(new ParagraphStyleId { Val = style }));
    p.Append(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    return p;
}

Paragraph Bullet(string text)
{
    var p = new Paragraph();
    var pPr = new ParagraphProperties(
        new ParagraphStyleId { Val = "ListParagraph" },
        new NumberingProperties(
            new NumberingLevelReference { Val = 0 },
            new NumberingInstance { Val = 1 }
        )
    );
    p.Append(pPr);
    p.Append(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    return p;
}

// —— 生成商务风格表格：表头深蓝底白字 + 斑马纹
Table MakeStyledTable(string[] headers, string[][] rows, int?[]? colWidthsDxa = null)
{
    var table = new Table();
    var tblPr = new TableProperties(
        new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
        new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 8, Space = 0, Color = "1F3864" },
            new LeftBorder { Val = BorderValues.Single, Size = 8, Space = 0, Color = "1F3864" },
            new BottomBorder { Val = BorderValues.Single, Size = 8, Space = 0, Color = "1F3864" },
            new RightBorder { Val = BorderValues.Single, Size = 8, Space = 0, Color = "1F3864" },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "1F3864" },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "1F3864" }
        ),
        new TableLook { Val = "04A0" }
    );
    table.Append(tblPr);

    // 列宽：等分或指定
    var grid = new TableGrid();
    int total = 9360;
    if (colWidthsDxa != null && colWidthsDxa.All(x => x.HasValue))
    {
        foreach (var w in colWidthsDxa) grid.Append(new GridColumn { Width = w!.Value.ToString() });
    }
    else
    {
        int col = headers.Length;
        int w0 = total / col;
        for (int i = 0; i < col; i++) grid.Append(new GridColumn { Width = w0.ToString() });
    }
    table.Append(grid);

    // 表头
    var hRow = new TableRow();
    foreach (var h in headers)
    {
        var tcPr = new TableCellProperties(
            new Shading
            {
                Val = ShadingPatternValues.Clear,
                Color = "auto",
                Fill = "1F3864"
            }
        );
        var cell = new TableCell(tcPr,
            new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(new Bold(), new Color { Val = "FFFFFF" }),
                    new Text(h) { Space = SpaceProcessingModeValues.Preserve }
                )
            )
        );
        hRow.Append(cell);
    }
    table.Append(hRow);

    // 数据行 + 斑马
    for (int i = 0; i < rows.Length; i++)
    {
        var r = new TableRow();
        string? shade = i % 2 == 1 ? "D9E2F3" : null;
        foreach (var cellText in rows[i])
        {
            var tcPr = new TableCellProperties();
            if (shade != null)
                tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = shade });
            var cell = new TableCell(tcPr,
                new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { After = "40", Line = "260", LineRule = LineSpacingRuleValues.Auto }),
                    new Run(new Text(cellText) { Space = SpaceProcessingModeValues.Preserve })
                )
            );
            r.Append(cell);
        }
        table.Append(r);
    }

    body!.Append(table);
    body.Append(new Paragraph());
    return table;
}

// ======================================================================
// 5. 封面
// ======================================================================
body.Append(H(0, "").ParagraphProperties is null ? P("", null) : P("", null)); // 空段填充

// 用 Title 样式居中写封面标题
body.Append(new Paragraph(
    new ParagraphProperties(
        new ParagraphStyleId { Val = "Title" },
        new SpacingBeforeLines { Val = 500 }
    ),
    new Run(new Text("低代码平台设计方案"))
));

body.Append(new Paragraph(
    new ParagraphProperties(new ParagraphStyleId { Val = "Subtitle" }),
    new Run(new Text("基于业务实体与查询的成套业务页面自动生成"))
));

body.Append(new Paragraph(
    new ParagraphProperties(
        new Justification { Val = JustificationValues.Center },
        new SpacingBeforeLines { Val = 400 },
        new SpacingBetweenLines { After = "200" }
    ),
    new Run(
        new RunProperties(new FontSize { Val = "24" }, new Color { Val = "5A5A5A" }),
        new Text("文档版本：V1.0"))
));

body.Append(new Paragraph(
    new ParagraphProperties(
        new Justification { Val = JustificationValues.Center },
        new SpacingBetweenLines { After = "200" }
    ),
    new Run(
        new RunProperties(new FontSize { Val = "24" }, new Color { Val = "5A5A5A" }),
        new Text("文档日期：2026 年 9 月 20 日"))
));

body.Append(new Paragraph(
    new ParagraphProperties(
        new Justification { Val = JustificationValues.Center },
        new SpacingBetweenLines { After = "200" }
    ),
    new Run(
        new RunProperties(new FontSize { Val = "24" }, new Color { Val = "5A5A5A" }),
        new Text("适用范围：nactivity 工作流引擎 + src 后端 + coding 前端编辑器"))
));

// 目录（Word 打开后更新）
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

body.Append(H(1, "目录"));
body.Append(new Paragraph(
    new ParagraphProperties(new ParagraphStyleId { Val = "TOCHeading" }),
    new Run(new Text("说明：在 Word 中按 F9 / 右键“更新域”以生成完整目录。"))
));
body.Append(new Paragraph(
    new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }),
    new Run(new FieldCode { Text = @" TOC \o ""1-3"" \h \z \u ", Space = SpaceProcessingModeValues.Preserve }),
    new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }),
    new Run(new Text("[目录 - 打开文档后右键更新域]")),
    new Run(new FieldChar { FieldCharType = FieldCharValues.End })
));
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

// ======================================================================
// 正文
// ======================================================================

// —— 1. 背景与目标 ——
body.Append(H(1, "1  背景与目标"));
body.Append(P("本项目是一个面向复杂企业业务的模型驱动低代码平台，包含工作流引擎（nactivity）、后端服务（src）、前端编辑器（coding）三大部分。当前搭建一个业务页面（例如采购订单），实施人员通常需要手工完成以下步骤："));
body.Append(Bullet("先建立业务实体、定义字段与关系、点击部署完成物理建表；"));
body.Append(Bullet("编写列表查询脚本与主记录查找脚本，再执行“分析”以得到字段元数据；"));
body.Append(Bullet("打开空白画布，手工拖入控件、逐一配置字段绑定、显隐只读规则、按钮与动作；"));
body.Append(Bullet("再单独配置权限、流程、打印模板等。"));
body.Append(P("上述链路虽然灵活，但操作成本高、专业门槛高、容易遗漏配置且一致性差。尤其在客户希望“以最快速度得到一整套可跑通的业务功能”时，空白画布方式明显拖慢交付节奏。"));

body.Append(H(2, "1.1  本次设计目标"));
MakeStyledTable(
    ["目标", "说明", "验收指标"],
    [
        ["降低起步成本", "从实体或查询一键生成“列表+编辑+详情+筛选”的成套页面包，而不是从空白画布开始。", "采购订单从实体到可运行页面 < 3 分钟"],
        ["保持专业模式", "自动生成只是起点，不取代原有设计器，保留手工接管与专业模式。", "生成后可切换专业编辑且不丢失配置"],
        ["保证一致性", "字段标题、格式、选择器、校验在页面、查询、权限中保持同一来源。", "改一次字段标题，三个页面同时生效"],
        ["可持续演进", "新增字段后可选择同步已有页面，不覆盖人工改动。", "新增字段后差异可视化，可逐项采纳"],
        ["支持行业模块", "与行业模块打包、安装、升级机制无缝衔接。", "安装采购模块时默认生成全套页面"]
    ]
);

body.Append(H(2, "1.2  非目标（不在本期范围）"));
body.Append(Bullet("不取代现有表单生成器与 XML 存储格式；"));
body.Append(Bullet("不实现完全可视化的拖拽重排（已有设计器承担）；"));
body.Append(Bullet("不承诺生成 100% 完美的复杂跨表编辑页，复杂场景仍需专业模式；"));
body.Append(Bullet("不在本次引入新的前端框架或换肤。"));

// —— 2. 设计原则 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "2  设计原则"));
body.Append(P("设计遵循“深度模块（Deep Module）”思想：调用方只需一个很小的触发入口（实体右键/查询右键“生成成套页面”），背后封装复杂的推断、绑定、策略选择逻辑。"));
MakeStyledTable(
    ["原则", "说明", "在本设计中的体现"],
    [
        ["最小接口", "暴露给用户的入口越少越好，复杂度藏在模块内部。", "只暴露一个“生成成套页面”入口，策略参数折叠为预设场景"],
        ["单一事实源", "同一字段的标题、格式、选择器只定义一次。", "页面绑定优先读取实体字段语义（Semantic），不再副本拷贝"],
        ["只生成不覆盖", "生成系统只能生成新资产或建议变更，不能无提示覆盖人工接管内容。", "所有二次同步都走差异-采纳模式"],
        ["向前兼容", "不破坏现有实体、查询、表单 XML 结构。", "新增元数据放在扩展节点，老版本忽略即可"],
        ["深度优于广度", "先把“实体/查询→页面”这条链路做深做稳，再扩展到报表、打印等外围。", "首版只覆盖列表、编辑、详情、筛选四个核心页面"]
    ]
);

// —— 3. 核心概念与领域模型 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "3  核心概念与领域模型"));

body.Append(H(2, "3.1  术语表"));
MakeStyledTable(
    ["术语", "英文", "定义"],
    [
        ["业务实体", "Business Entity", "对应数据库表的领域对象，包含字段、关系、语义配置。"],
        ["查询/脚本", "Query / Script", "平台内的 SQL 或 SmartSql 查询定义，可返回列表或单条记录。"],
        ["页面包", "Page Bundle", "一次性生成的一组相关页面（列表+编辑+详情+筛选）+ 动作绑定。"],
        ["页面草稿", "Page Draft", "生成后处于草稿状态、尚未部署/发布的页面定义。"],
        ["字段语义", "Field Semantic", "字段在实体上的业务语义（单据编号、金额、币别、人员等）。"],
        ["生成策略", "Generation Strategy", "针对不同场景选择字段分组、控件类型、默认列的策略集。"],
        ["接管标记", "TakenOver", "生成的元素被用户手工修改后，标记为不再被重新生成覆盖。"],
        ["引用选择器", "Lookup Selector", "针对引用类型（供应商、职员等）的共享选择器资产。"],
        ["生成清单", "Generation Manifest", "一次生成产生的资产目录与前后映射关系。"]
    ]
);

body.Append(H(2, "3.2  核心对象关系"));
body.Append(P("一个业务实体（或查询）通过生成器（PageBundleGenerator）产出一个页面包，页面包内部由多页定义组成；每一页定义与实体/查询形成绑定，并通过生成清单记录来源与变更轨迹。"));
MakeStyledTable(
    ["领域对象", "关键属性", "持久化"],
    [
        ["PageBundle（页面包）", "BundleId, Name, SourceKind(Entity|Query), SourceKey, Status, Version", "新增 PageBundles 表 + 页面定义 XML"],
        ["PageDefinition（页定义）", "PageId, BundleId, PageKind, Layout, DataSource", "沿用现有表单定义格式，扩展节点标记 <GeneratedFrom/>"],
        ["FieldBinding（字段绑定）", "FieldKey, ControlKind, ReadOnly, Required, LookupId, Semantic", "沿用现有控件定义，添加 semantic 与 takenOver 标记"],
        ["GenerationStrategy（生成策略）", "StrategyId, Scene, Preset, OverrideRules", "JSON/XML 配置文件，支持项目级覆盖"],
        ["GenerationManifest（生成清单）", "ManifestId, BundleId, CreatedAt, SourceSnapshot, AssetList", "新增 GenerationManifests 表 + 清单 JSON"]
    ]
);

// —— 4. 两种生成模式 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "4  两种生成模式：从实体生成 vs 从查询生成"));

body.Append(H(2, "4.1  模式 A：从业务实体直接生成"));
body.Append(P("适用场景：用户已经通过“实体→部署”完成物理建表，需要快速得到一套标准的 CRUD 页面。生成器会基于实体字段+关系自动推断查询、动作、选择器与页面布局。"));
body.Append(H(3, "4.1.1  入口与操作"));
body.Append(Bullet("用户在“实体列表”或“实体设计器”的实体上右键：“生成成套页面”；"));
body.Append(Bullet("弹出向导：选择场景（基础档案/主子表单据/流程申请/查询分析）→ 选择默认页面包 → 勾选是否同时生成列表脚本 + 主键查找脚本 + 默认动作权限模板；"));
body.Append(Bullet("确认后展示“将要生成资产清单”，用户可删除不需要的项；"));
body.Append(Bullet("一键确认，生成草稿包并打开统一预览。"));

body.Append(H(3, "4.1.2  自动创建的资产"));
MakeStyledTable(
    ["资产类型", "命名约定", "说明"],
    [
        ["列表查询脚本", "{实体代码}_List", "默认取非大文本字段，含分页与常见可筛选列；若为引用列，默认显示“名称”并保留外键。"],
        ["主键查找脚本", "{实体代码}_GetById", "按 ID 查单条，返回全部可写字段 + 引用显示列。"],
        ["列表页", "{实体名称}列表", "顶部筛选区（引用列→选择器，日期→范围，枚举→下拉）+ 数据表格 + 动作栏（新增/编辑/删除/导入/导出/刷新）。"],
        ["编辑页", "{实体名称}编辑", "分组 Tab（基础信息/明细/附加信息）+ 保存/保存并新增/取消 + 必填校验。"],
        ["详情页", "{实体名称}详情", "只读卡片布局 + 打印入口 + 流程进度面板占位。"],
        ["动作权限模板", "{实体代码}_动作模板", "模板化角色操作权限（创建者可改、审批人可看等），需用户确认后应用。"],
        ["生成清单", "{时间戳}_{BundleId}", "记录以上资产及其来源快照，用于后续增量同步。"]
    ]
);

body.Append(H(2, "4.2  模式 B：从查询/脚本直接生成"));
body.Append(P("适用场景：用户已经有成熟 SQL（如采购合同执行明细），希望快速得到 BI 风格的自助查询页/报表页。由于查询中可能包含计算字段、聚合列、跨表连接，本模式默认以“只读展示 + 多维筛选”为主，不直接生成可编辑页。"));
body.Append(H(3, "4.2.1  入口与操作"));
body.Append(Bullet("用户在“业务脚本”列表的脚本上右键：“生成成套页面”；"));
body.Append(Bullet("先执行一次分析得到字段元数据，识别：实体字段 / 关联显示字段 / 计算字段(VIRTUAL_TABLE) / 聚合字段；"));
body.Append(Bullet("弹出向导：选择页面类型（自助查询 / 主从报表 / 搜索详情跳转）→ 选择主实体（用于跳回编辑的链接锚点，可选）→ 勾选保存视图、收藏、导出；"));
body.Append(Bullet("展示字段清单，允许用户拖拽调整默认列与分组。"));

body.Append(H(3, "4.2.2  自动创建的资产"));
MakeStyledTable(
    ["资产类型", "命名约定", "说明"],
    [
        ["查询页", "{脚本名称}查询", "顶部筛选器（Faceted Search 风格）+ 数据表格 + 个人/团队视图保存。"],
        ["详情面板（可选）", "{脚本名称}详情", "单击行后弹出侧栏详情，只显示非表格主列的补充信息。"],
        ["跳转链接（可选）", "{主实体}编辑", "如果用户指定主实体主键列，自动在该列生成跳转编辑页的链接并校验权限。"],
        ["导出配置", "默认导出模板", "按分析字段生成 Excel 导出字段映射，可后续覆盖。"]
    ]
);

body.Append(H(2, "4.3  模式对比与选择策略"));
MakeStyledTable(
    ["维度", "模式A：从实体生成", "模式B：从查询生成", "推荐选择"],
    [
        ["主要目标", "标准 CRUD 业务单据/档案", "BI 风格查询/报表/分析", "业务录入用 A，查询分析用 B"],
        ["可编辑", "默认完整编辑能力", "默认只读，跳转链接可选", "写操作用 A，只读用 B"],
        ["对 SQL 依赖", "自动生成默认脚本（可覆盖）", "完全基于已有 SQL", "有复杂 SQL 的场景优先 B"],
        ["权限建议", "绑定“创建者/审批人/分配者”动作", "只绑定查询范围和导出权限", "权限模型在第 8 章融合"],
        ["AI 辅助建议", "适合让 AI 建议字段分组与校验", "适合让 AI 建议字段解释与筛选维度", "第 12 章详述"]
    ]
);

// —— 5. 页面包组成 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "5  生成的页面包（Page Bundle）组成"));
body.Append(P("首版统一把页面包拆为“核心四页 + 附加能力”，避免一次性过宽范围导致不可维护。核心四页覆盖 80% 常见企业场景。"));

MakeStyledTable(
    ["页面", "页类型", "主要区域", "默认动作"],
    [
        ["列表页", "List", "筛选区、快捷操作栏、数据表格、分页", "新增、编辑、查看、删除、批量删除、导入、导出、刷新、保存视图"],
        ["编辑页", "Edit", "面包屑、表单主体（分组/Tab）、明细表格、操作面板", "保存、保存并新增、保存并提交、取消（带未保存确认）"],
        ["详情页", "Detail", "基础信息卡片、关联面板（明细、日志、附件、审批轨迹）", "编辑、打印、通过流程动作进入任务处理"],
        ["筛选器资产（共享）", "Filter Asset", "维度卡片 + 时间范围 + 搜索框 + 已选条件胶囊", "重置、应用、保存为公共/个人视图"],
        ["打印模板（附加）", "PrintTemplate", "由平台现有打印模板生成器生成", "打印、导出为 PDF"],
        ["流程挂点（附加）", "BpmnHook", "状态变化触发 BPMN 启动/任务跳转动按钮", "启动流程、处理任务、查看轨迹"]
    ]
);

body.Append(H(2, "5.1  列表页的智能默认"));
body.Append(Bullet("只取前 N 个字段（默认 10 个）作为列表默认显示列，按字段语义排序：编号 > 名称/标题 > 关键引用(供应商/客户) > 日期 > 金额/数量 > 状态；"));
body.Append(Bullet("长文本、附件、图片、富文本默认不进列表列，而是进详情/侧栏；"));
body.Append(Bullet("引用类型列默认显示名称（NameColumn）而不是 ID，ID 隐藏用于跳转；"));
body.Append(Bullet("每个列表都自带“列选择器”，允许用户在运行时显示更多列；管理员可改默认配置。"));

body.Append(H(2, "5.2  编辑页的智能分组"));
body.Append(Bullet("字段数 ≤ 12：单列滚动布局，无 Tab；"));
body.Append(Bullet("13–24：两组 Tab（基础信息 / 其他信息），按语义分组；"));
body.Append(Bullet("> 24 或存在多个子表：主 Tab（基础信息） + 子表 Tab（按子实体名称命名） + 附加 Tab（附件/备注）；"));
body.Append(Bullet("所有 Tab 顺序可在“生成预览”中拖拽调整；一旦用户手工调整，该分组标记为 TakenOver，后续重新生成不再覆盖。"));

body.Append(H(2, "5.3  筛选器：APEX Faceted Search 思想"));
body.Append(P("借鉴 Oracle APEX 的 Faceted Search / Smart Filters：筛选器不再是零散查询条件，而是“维度卡片 + 已选胶囊 + 命中计数”。每一个筛选维度有默认类型（下拉、范围、树、搜索框），按字段语义推断。"));
MakeStyledTable(
    ["字段语义", "默认筛选器类型", "交互说明"],
    [
        ["状态/枚举", "多选下拉 + 计数", "展开显示每个状态的条数；支持多选"],
        ["日期（下单日、创建日）", "范围选择器 + 快捷区间", "快捷：今日/本周/本月/本季/本年/自定义"],
        ["引用（供应商、部门）", "搜索下拉（LookUp）", "复用共享选择器；支持多选"],
        ["数值（金额、数量）", "最小值–最大值范围框", "两侧输入框；支持空值"],
        ["文本（单号、名称、备注）", "顶部统一搜索框", "模糊搜索，可限定字段范围"],
        ["人员（创建人、业务员）", "员工选择器", "支持按部门树筛选 + 搜索"]
    ]
);

// —— 6. 生成流程与智能选择算法 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "6  生成流程与智能选择算法"));

body.Append(H(2, "6.1  总体流程（7 步）"));
MakeStyledTable(
    ["步骤", "名称", "输入", "输出", "主要逻辑"],
    [
        ["S1", "选择触发源", "Entity or Query", "SourceKind + SourceKey", "区分实体/查询模式"],
        ["S2", "推断场景", "用户选择场景 Preset + 元数据", "策略集 Strategy", "场景×元数据→策略；可覆盖"],
        ["S3", "字段分类", "字段列表 + 语义 + 关系", "字段分组与默认页面归属", "哪些进列表/编辑/详情/筛选"],
        ["S4", "控件映射", "字段分类结果", "控件类型 + 默认配置", "数据类型×语义→控件选择器"],
        ["S5", "默认动作 + 权限模板", "场景 + 关系 + 状态", "默认动作列表 + 角色模板", "单据=审核动作，档案=停用启用动作"],
        ["S6", "生成资产 + 清单", "以上结果", "PageBundle + Manifest", "生成器写入 XML 扩展节点"],
        ["S7", "预览 + 用户采纳", "清单 + 差异（二次生成时）", "用户确认后的草稿包", "用户可取消/勾选具体项后再保存"]
    ]
);

body.Append(H(2, "6.2  字段分类算法（伪代码说明）"));
body.Append(P("对于实体模式，字段首先按以下优先级归类，优先级从高到低："));
body.Append(Bullet("规则1：主键（ID）— 不在编辑页显示，列表页默认隐藏，详情页显示；"));
body.Append(Bullet("规则2：系统字段（CREATEDATE/CREATOR/UPDATEDATE/UPDATOR）— 全部进详情页的“系统信息”折叠组，列表/编辑页不显示；"));
body.Append(Bullet("规则3：语义为“单据编号、名称、标题、状态、关键引用”的字段 — 默认进列表首屏列 + 编辑页基础信息组 + 筛选区；"));
body.Append(Bullet("规则4：语义为“币别、金额、数量”的字段 — 默认进列表且右对齐、带千分位，编辑页独立分组“财务信息”；"));
body.Append(Bullet("规则5：日期类字段 — 列表、编辑、筛选默认全部进入；"));
body.Append(Bullet("规则6：布尔、枚举、下拉 — 默认进筛选器；"));
body.Append(Bullet("规则7：长文本、富文本、图片、附件数组 — 默认进详情/侧栏；"));
body.Append(Bullet("规则8：子表（主从关系）— 编辑页单独 Tab，详情页折叠面板；"));
body.Append(Bullet("规则9：查询模式下的 VIRTUAL_TABLE/聚合字段 — 默认进列表与详情，不可编辑，筛选器按类型推断。"));

body.Append(H(2, "6.3  控件选择映射（按数据类型 × 语义）"));
MakeStyledTable(
    ["数据类型", "常见语义", "默认控件", "附加默认配置"],
    [
        ["String(≤50)", "单据编号/名称", "Input + 标签", "必填、长度校验；编号型添加复制按钮"],
        ["String(≤50) + 引用", "供应商/客户/职员", "LookUp 选择器", "复用共享选择器；自动回填名称/编码"],
        ["String(>200)", "说明/备注", "TextArea", "可折叠；默认允许为空"],
        ["Int/Decimal", "金额/税率", "InputNumber", "金额千分位+币种符号，税率%，精度按语义"],
        ["Int/Decimal", "数量", "InputNumber", "数量支持小数位配置；关联单位"],
        ["Date/DateTime", "下单日/交货日", "DatePicker/Range", "默认允许空；交货日≥下单日校验"],
        ["Bool", "启用/停用", "Switch", "编辑页可操作；列表进 Badge"],
        ["Enum", "状态/类型", "Select", "列表进 Tag/Badge；筛选器进多选下拉"],
        ["UUID(引用)", "外键", "LookUp", "复用共享选择器；保持名称/编码的显示列"],
        ["File/Image", "附件/图片", "Uploader", "多文件；大小与类型限制可配置"]
    ]
);

body.Append(H(2, "6.4  策略覆盖机制（项目级与实体级）"));
body.Append(P("平台级提供默认策略（Preset：档案、单据、申请、分析），每个项目可在 {项目}/config/page-generator-strategies.json 中覆盖，也可在单个实体上用“生成策略”属性绑定。覆盖层级为："));
body.Append(Bullet("平台内建默认 < 项目级策略 < 实体级单独覆盖 < 用户在生成向导中的临时勾选。"));

// —— 7. 增量修改机制 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "7  生成后增量修改机制（可持续演进）"));
body.Append(P("这是本设计与“一次性生成模板”最大的差异点：必须解决“生成后用户手工改了，后续实体新增字段如何安全同步”的问题。"));

body.Append(H(2, "7.1  核心机制：TakenOver 接管标记"));
body.Append(P("生成器写出的每个控件、每个分组、每段动作绑定都会写入扩展节点："));
body.Append(P("  <GeneratedFrom bundleId=\"xxx\" manifestId=\"xxx\" takenOver=\"false\" />", "Normal"));
body.Append(P("一旦用户在设计器中手工修改该元素（包括改位置、改属性、改绑定），takenOver 置为 true。后续重新生成或同步："));
MakeStyledTable(
    ["元素状态", "takenOver=false", "takenOver=true"],
    [
        ["字段标题/格式改动", "自动跟随实体语义更新", "保留用户覆盖值，提示用户是否统一"],
        ["新增字段", "按策略插入合理位置", "新增字段会出现在差异清单中，用户单独勾选是否采纳"],
        ["删除字段", "移除绑定并标记废弃控件", "保留控件，但以“红色警示”提示字段已失效，用户确认后删除"],
        ["移动位置", "按策略重新排布", "保持用户位置，不移动"],
        ["控件类型变更", "按策略更新", "不更新"]
    ]
);

body.Append(H(2, "7.2  差异-采纳交互（受 Git 影响）"));
body.Append(Bullet("用户在实体/查询上点击“同步页面包”；"));
body.Append(Bullet("系统展示三类差异：新增（绿色+）、变更（黄色Δ）、删除（红色−）；"));
body.Append(Bullet("每个差异点击可预览“新布局 vs 当前布局”；"));
body.Append(Bullet("支持“全部采纳新增、全部跳过手工接管项、仅采纳标题/格式统一”三种快捷模式；"));
body.Append(Bullet("采纳后写入新 ManifestId，保留历史回滚能力（回滚到上一版 Manifest 对应的 XML 快照）。"));

// —— 8. 与现有能力集成 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "8  与现有平台能力的集成"));

body.Append(H(2, "8.1  与表单生成器的集成"));
body.Append(P("不重写表单生成器。新增“生成成套页面”命令，在现有 createformcommand/createformdialog 的基础上做批量封装，复用现有的控件描述符、字段绑定辅助与 XML 持久化。"));
body.Append(Bullet("复用点：控件描述符（Tools/DescriptorGenerator）、字段绑定 helper、保存/发布流程；"));
body.Append(Bullet("扩展点：页定义 XML 中新增 <GeneratedFrom>、<Semantic>、<TakenOver> 三个可选扩展节点（放在扩展区，老逻辑忽略）。"));

body.Append(H(2, "8.2  与部署/发布的集成"));
body.Append(Bullet("页面包生成后处于“草稿”，不立即生效；用户需显式点击“部署页面包”；"));
body.Append(Bullet("部署前执行“一致性检查”（Oracle APEX 的 Advisor 思想）：查找字段引用失效、权限冲突、必填缺失、脚本未部署等；"));
body.Append(Bullet("通过检查后，整体加入一次发布批次，形成 Manifest，与现有发布包衔接。"));

body.Append(H(2, "8.3  与权限系统的集成（重点）"));
body.Append(P("生成阶段只建议权限模板，不强制授予任何权限，避免越权。集成要点如下："));
MakeStyledTable(
    ["权限维度", "生成器行为", "用户后续在哪配置"],
    [
        ["动作级权限（新建/编辑/删除/审核）", "生成“动作模板草案”，按场景给出默认推荐（如档案=新增+编辑+停用，单据=新增+编辑+提交+反审核）", "权限工作台应用模板"],
        ["数据范围（本人/本部门/全公司）", "不生成具体范围，仅给出建议“适用角色”占位", "权限工作台的“操作级范围”"],
        ["字段级可见/可改", "按语义推断：草稿状态字段可改，已审批金额字段默认仅可见", "权限工作台/流程节点权限"],
        ["页面可见性（菜单）", "默认加入“业务对象”导航节点，但不授权给角色", "菜单授权工作台"],
        ["流程任务权限", "不越权设置，仅在编辑页动作中预留“启动流程/处理任务”的挂点", "流程模型 + 流程任务授权"]
    ]
);

body.Append(H(2, "8.4  与 nactivity（BPMN/DMN）集成"));
body.Append(Bullet("单据场景：编辑页自动预留“启动流程”挂点按钮，用户在向导中勾选后，按模板生成 ProcStart 动作；"));
body.Append(Bullet("DMN 规则场景：查询页中对“计算指标”自动提示来源规则，支持点击查看解释；"));
body.Append(Bullet("详情页自动嵌入：当前记录流程轨迹（已有/可复用）、任务处理面板（如果当前用户有待办）。"));

body.Append(H(2, "8.5  与行业模块的集成"));
body.Append(Bullet("行业模块安装时自动运行“实体 → 生成页面包”的 S1–S6，形成草稿包；"));
body.Append(Bullet("安装向导最后一步显示草稿包，允许客户勾选不生成某些页面（如不需要打印模板）；"));
body.Append(Bullet("行业模块升级时，先对客户当前页面包做差异比较，再走 7.2 差异-采纳流程，避免升级直接覆盖客户定制。"));

// —— 9. 数据模型 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "9  数据模型设计"));

body.Append(H(2, "9.1  新增数据表"));
MakeStyledTable(
    ["表名（小写）", "主要字段", "说明"],
    [
        ["page_bundle", "bundle_id(pk), name, source_kind, source_key, status(草稿/已部署/停用), current_manifest_id, created_at, updated_at", "页面包头表，一个实体/查询对应一个或多个版本的页面包"],
        ["page_bundle_manifest", "manifest_id(pk), bundle_id(fk), version_no, asset_list(json), source_snapshot(json), created_by, created_at, notes", "每次生成/同步产生一条清单，用于回滚和差异比较"],
        ["page_bundle_page", "page_id(pk), bundle_id(fk), page_kind, page_def_id, route_path, display_name, default_enabled, create_from_manifest_id", "页面包内的单页清单，连接到实际表单定义"],
        ["page_gen_strategy_override", "override_id(pk), scope(project|entity), scope_key, strategy_preset, override_rules(json), updated_at", "项目级/实体级生成策略覆盖"]
    ]
);
body.Append(P("说明：PostgreSQL 下全部表名、列名采用小写；字符串列 >1 字符统一使用 citext。沿用项目规范。"));

body.Append(H(2, "9.2  现有表的扩展"));
body.Append(Bullet("业务实体定义 XML：新增 <PageGenStrategy preset=\"...\" /> 可选节点；"));
body.Append(Bullet("表单/页面定义 XML：每个控件/分组/动作扩展 <GeneratedFrom bundleId manifestId takenOver=\"false\" />；"));
body.Append(Bullet("发布批次：关联 ManifestId（可空），用于追溯“这一次发布包含哪些页面包变更”。"));

// —— 10. API ——
body.Append(H(1, "10  API 接口设计"));
MakeStyledTable(
    ["接口", "方法 / 路径", "入参", "返回", "说明"],
    [
        ["预览生成", "POST Api/v1/page-bundles/preview", "{sourceKind,sourceKey,scene,strategyOverride,includes[]}", "PreviewDTO（资产清单、默认控件列表、警告）", "不写库，只返回预览供用户确认"],
        ["确认生成", "POST Api/v1/page-bundles", "{previewToken, includes[], notes, applyPermissionTemplate}", "{bundleId, manifestId, pages[]}", "写库，生成草稿包；幂等 token 防重复提交"],
        ["同步草稿", "POST Api/v1/page-bundles/{bundleId}/sync", "{acceptPlan}", "SyncResult（变更统计、冲突列表）", "acceptPlan 是用户勾选的差异采纳计划"],
        ["回滚到清单", "POST Api/v1/page-bundles/{bundleId}/rollback", "{manifestId}", "RollbackResult", "整体还原 XML 到指定 Manifest 快照"],
        ["部署检查", "GET Api/v1/page-bundles/{bundleId}/advisor", "-", "AdvisorResult（问题列表、严重级别、修复建议）", "Oracle APEX Advisor 思路"],
        ["查询页面包", "GET Api/v1/page-bundles/{bundleId}", "-", "BundleDetail", "详情+ Manifest 历史"],
        ["策略覆盖", "PUT Api/v1/page-gen-strategies/{scope}/{key}", "{preset, rules}", "{saved}", "项目/实体级策略覆盖"]
    ]
);

// —— 11. 前端交互流程 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "11  前端交互流程"));

body.Append(H(2, "11.1  实体→生成（三步极简向导）"));
MakeStyledTable(
    ["步骤", "屏幕内容", "关键操作"],
    [
        ["Step1 触发", "实体列表/实体详情右键菜单：生成成套页面", "选择场景 Preset（默认=自动推断）"],
        ["Step2 配置", "左右两栏：左侧资产清单（勾选/取消），右侧预览（列表/编辑/详情切换）", "点击字段标签可拖拽改分组；切换“普通模式/专业模式”"],
        ["Step3 确认", "将生成的资产清单、可能的风险、后续可编辑路径汇总显示", "“一键生成草稿” + “立即打开专业设计器” 双按钮；另含“跳过，保留下次”"]
    ]
);

body.Append(H(2, "11.2  查询→生成（两步）"));
body.Append(Bullet("Step1：先执行“分析”，得到字段分类结果；用户可编辑 VIRTUAL_TABLE 显示名、聚合字段格式；"));
body.Append(Bullet("Step2：选择查询页类型（列表报表/主从/搜索详情跳转），预览后生成。"));

body.Append(H(2, "11.3  同步/回滚页面包"));
body.Append(P("在“业务对象工作台”页面包详情页，提供“同步、差异、回滚”三个面板；差异面板以三态（+Δ−）彩色列表呈现，每一项支持点击切换旧/新预览。"));

body.Append(H(2, "11.4  一致性检查（Advisor）面板"));
body.Append(P("部署前强制过一次 Advisor，按严重级别分组：致命（无法保存）/警告（确认后可继续）/提示。"));
MakeStyledTable(
    ["检查项", "严重级别", "说明"],
    [
        ["引用字段关联的选择器未定义", "致命", "列表/编辑将无法展示名称"],
        ["生成页引用的脚本未部署", "致命", "运行时脚本找不到"],
        ["实体未部署", "致命", "连基础表都没有，页面包无法运行"],
        ["建议添加某字段到筛选", "提示", "例如日期/状态字段未进入筛选器，可一键采纳"],
        ["字段标题与显示名不一致", "警告", "可能造成用户误解，可选择统一以实体语义为准"]
    ]
);

// —— 12. 与受控 AI 结合 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "12  与受控 AI 的结合点"));
body.Append(P("AI 仅用于“生成草稿和辅助理解”，绝不直接写库或执行部署。所有 AI 输出都必须进入上述 S1–S7 的生成管道，经过 Advisor + 人工确认再落库。"));

body.Append(H(2, "12.1  建议的 AI 介入点"));
MakeStyledTable(
    ["场景", "AI 做什么", "谁最终生效", "安全约束"],
    [
        ["实体建模时", "推荐“适合的页面场景”（档案/单据/申请）与字段分组", "用户在向导中确认", "只读访问实体元数据，不写库"],
        ["生成预览中", "推荐字段是否进列表/筛选/编辑", "差异采纳面板逐项确认", "只输出 JSON 建议，不直接改 XML"],
        ["查询分析后", "解释计算字段口径、建议筛选维度", "用户采纳后写回策略覆盖", "禁止解释中含真实敏感数据样例"],
        ["差异同步中", "推荐差异合并规则（新增全采纳/不触碰接管项等）", "用户点击合并计划", "推荐可回溯，计划写入 Manifest"],
        ["权限建议", "给出“建议动作模板”而非具体授权", "管理员在权限工作台确认", "不写权限存储，不改权限评估逻辑"]
    ]
);

body.Append(H(2, "12.2  AI 调用边界（Hard Rule）"));
body.Append(Bullet("Rule A：AI 输出必须是结构化建议（JSON 或明确字段），不能是自由文本直接注入 XML；"));
body.Append(Bullet("Rule B：AI 输出的控件类型、字段位置、分组，必须通过“生成器内部的策略校验”后才能进入预览；"));
body.Append(Bullet("Rule C：任何涉及部署、权限授予、DDL 的操作，AI 只能提供“待确认的草稿”，执行必须走已有的人工确认+后台校验链。"));

// —— 13. 实施路线 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "13  分阶段实施路线（推荐）"));
MakeStyledTable(
    ["阶段", "目标", "范围", "交付标志", "预计投入（参考）"],
    [
        ["P1 打底（2 周）", "跑通实体→列表+编辑的一键生成", "仅模式 A；基础策略；TakenOver 初步；无差异面板（全量）", "采购订单实体可一键出列表+编辑；字段标题/格式一致", "后端 1.5 人 + 前端 1.5 人"],
        ["P2 闭环（2 周）", "加详情页 + 筛选器资产 + Manifest + 回滚 + Advisor", "仍模式 A 为主；加入项目级策略覆盖", "实体→四页完整可用；可回滚；部署前可检查", "同 P1 团队"],
        ["P3 查询模式（1.5 周）", "跑通查询→分析→生成查询页 + 跳转链接", "模式 B + VIRTUAL_TABLE 识别优化", "采购合同执行明细可秒出查询页并可跳回编辑", "后端 1 + 前端 1.5"],
        ["P4 增量进化（2 周）", "差异-采纳面板 + 字段同步不覆盖人工改动", "TakenOver 全量；三态差异；行业模块挂点", "新增字段后可安全同步到旧页面包", "后端 1.5 + 前端 2"],
        ["P5 行业+AI（2–3 周）", "行业模块生成挂点 + AI 辅助推荐", "12.1 全部介入点 + 安装向导最后一步", "安装采购模块可生成全套页面；AI 建议可被采纳", "后端 1.5 + 前端 1.5 + AI 接口 0.5"]
    ]
);

// —— 14. 风险与取舍 ——
body.Append(H(1, "14  风险与取舍"));
MakeStyledTable(
    ["风险", "可能性", "影响", "缓解策略"],
    [
        ["生成页面与现有空白画布风格不一致，造成两档页面并存", "中", "高", "统一使用现有控件体系；生成器只调用 createformcommand 的现有能力"],
        ["TakenOver 机制复杂，用户忘记哪些是人工改动", "中", "中", "设计器中增加“接管标记”徽标，悬浮显示来源与最后修改人"],
        ["差异同步过于细，用户操作成本上升", "中", "中", "提供“新增全采纳、接管跳过、标题格式统一”三个快捷模式"],
        ["一次性生成的查询脚本性能不佳（未加索引/未拆分页）", "高", "中", "Advisor 检查：列表页默认加分页 + 建议必要索引；复杂场景建议走查询模式"],
        ["AI 建议漂移导致同一实体多次建议不一致", "中", "低", "AI 建议走“策略缓存”，同一 Manifest 内同一场景只出一次可回溯建议"],
        ["与行业模块升级冲突，覆盖客户定制", "高", "高", "强制 P4 完成后再启用行业模块升级；升级前强制做差异备份"]
    ]
);

// —— 15. 验收标准 ——
body.Append(H(1, "15  验收标准"));
body.Append(H(2, "15.1  功能验收"));
MakeStyledTable(
    ["编号", "验收点", "通过标准"],
    [
        ["F1", "从实体生成成套页面", "采购订单实体在 ≤3 分钟内得到可运行的“列表+编辑+详情+筛选器”四页草稿"],
        ["F2", "从查询生成查询页", "“采购合同执行明细”脚本一键生成筛选+表格+个人视图的只读页"],
        ["F3", "TakenOver 生效", "用户改动一个分组顺序后，下次同步不重置此顺序"],
        ["F4", "差异-采纳", "实体新增“交期”字段后，差异面板显示 +Δ，并可逐项采纳到三个页面"],
        ["F5", "Advisor 拦截", "未部署实体时尝试部署页面包，被 Advisor 阻止并给出准确提示"],
        ["F6", "权限不越权", "生成页面包时只产生权限模板草案，不自动授予任何角色权限"]
    ]
);

body.Append(H(2, "15.2  非功能验收"));
MakeStyledTable(
    ["维度", "指标"],
    [
        ["性能", "预览生成 ≤ 3 秒（普通实体，≤ 40 字段，2 个子表）"],
        ["一致性", "同一实体字段标题在列表/编辑/详情/筛选器中一致率 100%（用户接管除外）"],
        ["兼容性", "所有生成的 XML 可被原有设计器打开，不报错、不丢失内容"],
        ["可维护性", "生成器主逻辑 ≤ 3 个核心类 + 策略配置文件；策略覆盖不涉及代码改"]
    ]
);

// —— 附：模型分工使用情况 ——
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
body.Append(H(1, "附  本次设计过程中的模型分工说明"));
body.Append(P("为了保证“需求不跑偏、设计有深度、实现有抓手、文档可交付”，本方案在思考过程中采用了四种模型分工协作，对应 Trae 提示词规则中定义的角色如下："));

MakeStyledTable(
    ["分工", "对应模型", "在本次设计中承担的工作", "关键产出"],
    [
        ["需求分析", "GPT-5.6-Terra", "业务场景拆解、角色视角切换（实施人员/业务用户/管理员）、痛点识别、范围与非范围、验收标准初稿。", "第 1 章背景与目标、非目标；第 15 章的验收标准业务条目 F1–F6"],
        ["方案设计", "GPT-6-Astra", "架构取舍、模块边界（生成器 vs 现有设计器 vs 部署链）、核心概念与数据模型、双生成模式、TakenOver + 差异-采纳机制、分阶段实施与风险控制；深度模块思想贯穿全文。", "第 3–10、13、14 章；第 11 章交互流程的结构与关键面板"],
        ["编码/落盘（本次未写业务代码，但在文档工程中承担）", "GPT-5.6-Sol", "按 OpenXML SDK 3.2 规范把设计内容落盘为可打开的 Word 文档：样式体系、表格布局、目录与页码、中文排版规范、结构一致性检查；严格复用设计而非自行发挥。", "本文档（DOCX）的技术实现：Styles/Numbering/Tables/封面/目录/页脚"],
        ["文档润色与组织（本次在文档工程中辅助）", "GPT-5.6-Luna", "统一术语、检查章节节奏与重复、补充非目标与边界说明、确认交付给业务/管理层也能阅读。", "第 1 章非目标、第 2 章原则表格化、第 15 章验收标准的拆分表达"]
    ]
);
body.Append(P("一句话总结：Terra 负责把“要解决什么问题”讲清楚；Astra 负责把“怎么解、为何这么解”搭成架构骨架；Sol 负责把骨架钉成可审阅的正式 Word；Luna 负责让文档读起来顺、不啰嗦。这四者在一个设计任务里是串行协作关系，而不是互相替代。"));

// ======================================================================
// 收尾：页面设置 + 保存
// ======================================================================
body.Append(sectPr);

mainPart.Document.Save();
doc.Save();
doc.Dispose();

System.Console.WriteLine($"文档已生成：{OUTPUT_PATH}");
System.Console.WriteLine($"文件大小：{new System.IO.FileInfo(OUTPUT_PATH).Length} 字节");
