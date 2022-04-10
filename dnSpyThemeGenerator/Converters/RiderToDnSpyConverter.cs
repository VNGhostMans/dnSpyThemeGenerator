using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnSpyThemeGenerator.Themes;

namespace dnSpyThemeGenerator.Converters
{
    internal class RiderToDnSpyConverter : IThemeConverter<RiderTheme, DnSpyTheme>
    {
        private static readonly Dictionary<string, string> AttributeMap = new()
        {
            {"defaulttext", "TEXT"},
            {"operator", "DEFAULT_OPERATION_SIGN"},
            {"punctuation", "DEFAULT_DOT"},
            {"number", "DEFAULT_NUMBER"},
            {"comment", "DEFAULT_LINE_COMMENT"},
            {"keyword", "DEFAULT_KEYWORD"},
            {"string", "DEFAULT_STRING"},
            {"verbatimstring", "DEFAULT_STRING"},
            {"char", "DEFAULT_STRING"},
            {"namespace", "ReSharper.NAMESPACE_IDENTIFIER"},
            {"type", "DEFAULT_CLASS_NAME"},
            // TODO: sealedtype, statictype
            {"statictype", "ReSharper.STATIC_CLASS_IDENTIFIER"},
            {"delegate", "ReSharper.DELEGATE_IDENTIFIER"},
            {"enum", "ReSharper.ENUM_IDENTIFIER"},
            {"interface", "DEFAULT_INTERFACE_NAME"},
            {"valuetype", "ReSharper.STRUCT_IDENTIFIER"},
            // TODO: module
            {"typegenericparameter", "ReSharper.TYPE_PARAMETER_IDENTIFIER"},
            {"methodgenericparameter", "ReSharper.TYPE_PARAMETER_IDENTIFIER"},
            {"instancemethod", "DEFAULT_INSTANCE_METHOD"},
            {"staticmethod", "DEFAULT_STATIC_METHOD"},
            {"extensionmethod", "ReSharper.EXTENSION_METHOD_IDENTIFIER"},
            {"instancefield", "DEFAULT_INSTANCE_FIELD"},
            {"instanceevent", "DEFAULT_INSTANCE_FIELD"},
            {"instanceproperty", "DEFAULT_INSTANCE_FIELD"},
            {"enumfield", "DEFAULT_INSTANCE_FIELD"},
            {"literalfield", "DEFAULT_INSTANCE_FIELD"},
            {"staticfield", "DEFAULT_STATIC_FIELD"},
            {"staticevent", "DEFAULT_STATIC_FIELD"},
            {"staticproperty", "DEFAULT_STATIC_FIELD"},
            {"local", "DEFAULT_LOCAL_VARIABLE"},
            {"parameter", "DEFAULT_PARAMETER"},
            {"preprocessorkeyword", "DEFAULT_KEYWORD"},
            // TODO: preprocessortext
            {"label", "DEFAULT_LABEL"},
            {"opcode", "DEFAULT_KEYWORD"},
            // TODO: ...
        };

        private static readonly Dictionary<(string key, string attribute), string> ColorMap = new()
        {
            {("linenumber", "fg"), "LINE_NUMBERS_COLOR"},
            {("selectedtext", "bg"), "SELECTION_BACKGROUND"},
            {("inactiveselectedtext", "bg"), "SELECTION_BACKGROUND"},
            {("environmentscrollbarthumbbackground", "bg"), "ScrollBar.thumbColor"},
            {("environmentscrollbarthumbmouseoverbackground", "bg"), "ScrollBar.hoverThumbColor"},
            {("environmentscrollbarbackground", "bg"), "ScrollBar.trackColor"},
            {("environmentscrollbararrowbackground", "bg"), "ScrollBar.trackColor"},
            {("environmentscrollbararrowdisabledbackground", "bg"), "ScrollBar.trackColor"},
            {("treeview", "bg"), "PROMOTION_PANE"},
            {("glyphmargin", "bg"), "GUTTER_BACKGROUND"},
            // Main background
            {("environmentbackground", "fg"), "PROMOTION_PANE"},
            {("environmentbackground", "bg"), "PROMOTION_PANE"},
            {("environmentbackground", "color3"), "PROMOTION_PANE"},
            {("environmentbackground", "color4"), "PROMOTION_PANE"},
            // Top toolbar
            {("toolbarhorizontalbackground", "fg"), "PROMOTION_PANE"},
            {("toolbarhorizontalbackground", "bg"), "PROMOTION_PANE"},
            {("toolbarhorizontalbackground", "color3"), "PROMOTION_PANE"},
            // Top header
            {("environmentmainwindowactivecaption", "bg"), "PROMOTION_PANE"},
            {("environmentmainwindowinactivecaption", "bg"), "PROMOTION_PANE"},
            // Tool window headers
            {("environmenttitlebaractive", "bg"), "PROMOTION_PANE"},
            {("environmenttitlebaractivegradient", "bg"), "PROMOTION_PANE"},
            {("environmenttitlebaractivegradient", "fg"), "PROMOTION_PANE"},
            {("environmenttitlebaractivegradient", "color3"), "PROMOTION_PANE"},
            {("environmenttitlebaractivegradient", "color4"), "PROMOTION_PANE"},
            {("environmenttitlebarinactive", "bg"), "PROMOTION_PANE"},
            {("environmenttitlebarinactivegradient", "bg"), "PROMOTION_PANE"},
            {("environmenttitlebarinactivegradient", "fg"), "PROMOTION_PANE"},
            {("environmenttitlebarinactivegradient", "color3"), "PROMOTION_PANE"},
            {("environmenttitlebarinactivegradient", "color4"), "PROMOTION_PANE"},
            // Editor tabs
            {("environmentfiletabbackground", "bg"), "PROMOTION_PANE"},
            {("environmentfiletabinactivegradient", "bg"), "GUTTER_BACKGROUND"},
            {("environmentfiletabinactivegradient", "fg"), "GUTTER_BACKGROUND"},
            {("environmentfiletabinactiveborder", "bg"), "GUTTER_BACKGROUND"},
            // Header for dialogs
            {("dialogwindowactivecaption", "bg"), "PROMOTION_PANE"},
            {("dialogwindowinactivecaption", "bg"), "PROMOTION_PANE"},
            // Could also style environmentfiletabselectedgradient, but which color?
            // Dialogs
            {("dialogwindow", "bg"), "PROMOTION_PANE"},
            // Settings list background
            {("appsettingstreeview", "bg"), "GUTTER_BACKGROUND"},
            // Context menu
            // {("contextmenubackground", "bg"), "PROMOTION_PANE"},
            // {("contextmenurectanglefill", "bg"), "PROMOTION_PANE"},
            // Some controls
            // {("commoncontrolsbutton", "bg"), "GUTTER_BACKGROUND"},
            // {("commoncontrolshover", "bg"), "GUTTER_BACKGROUND"},
        };

        private static readonly Dictionary<(string key, string attribute), string> HardcodedColors = new()
        {
            {("treeviewitemselected", "bg"), "#1FFFFFFF"},
            {("treeviewitemmouseover", "bg"), "#3FFFFFFF"},
            {("environmentfiletabborder", "bg"), "transparent"},
        };

        public void CopyTo(RiderTheme source, DnSpyTheme donor)
        {
            donor.Name = source.Name.ToLower().Replace(" ", "_");
            donor.MenuName = source.Name;
            var bytes = new byte[16];
            new Random(source.Name.GetHashCode()).NextBytes(bytes);
            donor.Guid = new Guid(bytes);
            donor.Order = 9001;
            foreach ((string dnSpyColorName, var dnSpyAttributes) in donor.Colors)
            {
                foreach ((string dnSpyAttributeName, _) in dnSpyAttributes)
                {
                    if (HardcodedColors.TryGetValue((dnSpyColorName, dnSpyAttributeName), out var result))
                    {
                        dnSpyAttributes[dnSpyAttributeName] = result;
                    }
                    else if (AttributeMap.TryGetValue(dnSpyColorName, out string riderAttributeKey))
                    {
                        var riderAttributeName = MapAttributeName(dnSpyAttributeName);
                        if (riderAttributeName is null)
                        {
                            if (dnSpyAttributeName != "name")
                                Console.ForegroundColor = ConsoleColor.Red;
                                Debug.WriteLine("Skipping unknown attribute " + dnSpyAttributeName);
                        }
                        else if (!source.Attributes.TryGetValue(riderAttributeKey, out var riderAttributes))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Debug.WriteLine("Couldn't resolve rider attribute " + dnSpyAttributeName);
                        }
                        else if (!riderAttributes.TryGetValue(riderAttributeName, out var riderValue))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Debug.WriteLine("Couldn't find attribute in rider attributes: " + riderAttributeName);
                        }
                        else
                        {
                            dnSpyAttributes[dnSpyAttributeName] = ConvertColor(riderValue);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Mapping attribute {dnSpyColorName}.{dnSpyAttributeName}");
                        }
                    }
                    else if (ColorMap.TryGetValue((dnSpyColorName, dnSpyAttributeName), out var riderColorName))
                    {
                        if (!source.Colors.TryGetValue(riderColorName, out var riderColor))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Debug.WriteLine($"Couldn't find color {riderColor} in rider theme");
                        }
                        else
                        {
                            dnSpyAttributes[dnSpyAttributeName] = ConvertColor(riderColor);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Mapping color {dnSpyColorName}.{dnSpyAttributeName}");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Debug.WriteLine("Skipping unknown key " + dnSpyColorName);
                    }
                }
            }
        }

        private static string ConvertColor(string color)
        {
            return "#" + color.PadLeft(6, '0');
        }

        private static string MapAttributeName(string dnSpyName)
        {
            return dnSpyName switch
            {
                "fg" => "FOREGROUND",
                "bg" => "BACKGROUND",
                _ => null,
            };
        }
    }
}