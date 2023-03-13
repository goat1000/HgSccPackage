//=========================================================================
// Copyright 2015 Sergey Antonov <sergant_@mail.ru>
//
// This software may be used and distributed according to the terms of the
// GNU General Public License version 2 as published by the Free Software
// Foundation.
//
// See the file COPYING.TXT for the full text of the license, or see
// http://www.gnu.org/licenses/gpl-2.0.txt
//
//=========================================================================

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

//-----------------------------------------------------------------------------
namespace HgSccHelper.UI
{
    //-----------------------------------------------------------------------------
    public sealed class ThemeManager
    {
        List<Theme> themes;
        ResourceDictionary base_dict;

        Theme current;
        HashSet<FrameworkElement> controls;

        static readonly ThemeManager instance = new ThemeManager();

        //-----------------------------------------------------------------------------
        static ThemeManager()
        {
        }

        //-----------------------------------------------------------------------------
        private ThemeManager()
        {
            // force load of assembly
            GC.KeepAlive(typeof(FirstFloor.ModernUI.Windows.Controls.ModernButton));

            base_dict = new ResourceDictionary { Source = new Uri("/HgSccHelper;component/UI/BaseTheme.xaml", UriKind.Relative) };

            controls = new HashSet<FrameworkElement>();

            themes = new List<Theme>();
            themes.Add(new Theme
            {
                Name = "Light",
                ResourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("/HgSccHelper;component/UI/LightTheme.xaml", UriKind.Relative)
                },
                AccentColor = System.Windows.Media.Color.FromRgb(27, 191, 235),
                ErrorColor = Colors.Red,
                RevLogLineColor = Colors.Black,
                RevLogNodeColor = Colors.Blue,
                AnnotateColor = Colors.Blue,
            });

            themes.Add(new Theme
            {
                Name = "Dark",
                ResourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("/HgSccHelper;component/UI/DarkTheme.xaml", UriKind.Relative)
                },
                AccentColor = System.Windows.Media.Color.FromRgb(27, 121, 175),
                ErrorColor = Colors.LightSalmon,
                RevLogLineColor = Colors.White,
                RevLogNodeColor = Colors.LightBlue,
                AnnotateColor = Colors.LightBlue
            });

            themes.Add(new Theme
            {
                Name = "Visual Studio",
                ResourceDictionary = new ResourceDictionary { },
            });

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
            LoadThemeColors();

            if (!Load())
                Current = themes[0];
        }

        private void LoadThemeColors()
        {
            Theme t = themes[2];
            t.AccentColor = VSThemeColor(EnvironmentColors.AccentMediumColorKey);
            t.ErrorColor = VSThemeColor(EnvironmentColors.ToolWindowValidationErrorTextColorKey);
            t.RevLogLineColor = VSThemeColor(EnvironmentColors.ToolWindowTextColorKey);
            t.RevLogNodeColor = VSThemeColor(EnvironmentColors.ControlLinkTextColorKey);
            t.AnnotateColor = VSThemeColor(EnvironmentColors.ControlLinkTextColorKey);
            t.ResourceDictionary["WindowText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["WindowTextReadOnly"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowInactiveCaptionTextColorKey));
            t.ResourceDictionary["WindowBackgroundColor"] = VSThemeColor(EnvironmentColors.MediumColorKey);
            t.ResourceDictionary["ButtonBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MediumColorKey));
            t.ResourceDictionary["ButtonBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.CommandBarMouseOverBackgroundBeginColorKey));
            t.ResourceDictionary["ButtonBackgroundPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonDownColorKey));
            t.ResourceDictionary["ButtonBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MediumColorKey));
            t.ResourceDictionary["ButtonBorderHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.CommandBarMouseDownBorderColorKey));
            t.ResourceDictionary["ButtonBorderPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.CommandBarMouseDownBorderColorKey));
            t.ResourceDictionary["ButtonBorderDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MediumColorKey));
            t.ResourceDictionary["ButtonText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ButtonTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ButtonTextPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ButtonTextDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));

            t.ResourceDictionary["DataGridBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MediumColorKey));
            t.ResourceDictionary["DataGridForeground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["DataGridCellBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MediumColorKey));
            t.ResourceDictionary["DataGridCellBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.CommandBarMouseOverBackgroundBeginColorKey));
            t.ResourceDictionary["DataGridCellBackgroundSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["DataGridCellForeground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["DataGridCellForegroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["DataGridCellForegroundSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["DataGridHeaderBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingBackgroundColorKey));
            t.ResourceDictionary["DataGridHeaderBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingBackgroundColorKey));
            t.ResourceDictionary["DataGridHeaderBackgroundPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingBackgroundColorKey));
            t.ResourceDictionary["DataGridHeaderBackgroundSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingBackgroundColorKey));
            t.ResourceDictionary["DataGridHeaderForeground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingTextColorKey));
            t.ResourceDictionary["DataGridHeaderForegroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingTextColorKey));
            t.ResourceDictionary["DataGridHeaderForegroundPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingTextColorKey));
            t.ResourceDictionary["DataGridHeaderForegroundSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridHeadingTextColorKey));
            t.ResourceDictionary["DataGridGridLines"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridLineColorKey));
            t.ResourceDictionary["DataGridDropSeparator"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.GridLineColorKey));
            t.ResourceDictionary["Hyperlink"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.PanelHyperlinkColorKey));
            t.ResourceDictionary["HyperlinkHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.PanelHyperlinkHoverColorKey));
            t.ResourceDictionary["HyperlinkDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.PanelHyperlinkDisabledColorKey));

            t.ResourceDictionary["InputBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ComboBoxBackgroundColorKey));
            t.ResourceDictionary["InputBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ComboBoxMouseOverBackgroundBeginColorKey));
            t.ResourceDictionary["InputBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ComboBoxBorderColorKey));
            t.ResourceDictionary["InputBorderHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ComboBoxMouseOverBorderColorKey));
            t.ResourceDictionary["InputText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["InputTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["InputTextDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowInactiveCaptionTextColorKey));
            t.ResourceDictionary["ItemBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ItemBackgroundSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ItemBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ComboBoxBorderColorKey));
            t.ResourceDictionary["ItemText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ItemTextSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ItemTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ItemTextDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));

            t.ResourceDictionary["LinkButtonText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["LinkButtonTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["LinkButtonTextPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["LinkButtonTextDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));

            t.ResourceDictionary["MenuText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemMenuTextColorKey));
            t.ResourceDictionary["MenuTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemMenuTextColorKey));
            t.ResourceDictionary["MenuTextSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemMenuTextColorKey));

            t.ResourceDictionary["ModernButtonBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonActiveBorderColorKey));
            t.ResourceDictionary["ModernButtonBorderHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonHoverActiveBorderColorKey));
            t.ResourceDictionary["ModernButtonBorderPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonHoverActiveBorderColorKey));
            t.ResourceDictionary["ModernButtonBorderDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonHoverActiveBorderColorKey));
            t.ResourceDictionary["ModernButtonIconBackgroundPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonHoverActiveBorderColorKey));
            t.ResourceDictionary["ModernButtonIconForegroundPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.MainWindowButtonHoverActiveBorderColorKey));
            t.ResourceDictionary["ModernButtonText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ModernButtonTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ModernButtonTextPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));
            t.ResourceDictionary["ModernButtonTextDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.ButtonTextColorKey));

            t.ResourceDictionary["PopupBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ProgressBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbBackgroundDragging"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbForeground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbForegroundDragging"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["ScrollBarThumbForegroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SeparatorBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderSelectionBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderSelectionBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBackgroundDragging"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBackgroundHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBackgroundDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBorderDragging"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBorderHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderThumbBorderDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderTrackBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderTrackBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderTick"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SliderTickDisabled"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SubMenuText"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SubMenuTextHover"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SubMenuTextSelected"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["WindowBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["WindowBorder"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["WindowBorderActive"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            /*
            <LinearGradientBrush x:Key="WindowHeaderGradient" StartPoint="0, 0" EndPoint="0, 1" Opacity=".1">
                <GradientStop Offset="0" Color="{DynamicResource AccentColor}" />
                <GradientStop Offset=".3" Color="{DynamicResource AccentColor}" />
                <GradientStop Offset="1" Color="Transparent" />
            </LinearGradientBrush>
            */
            t.ResourceDictionary["CloseButtonBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["CloseButtonBackgroundOnMoseOver"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["CloseButtonForegroundOnMoseOver"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["CloseButtonBackgroundIsPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["CloseButtonForegroundIsPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.DesignerBackgroundColorKey));
            t.ResourceDictionary["SystemButtonBackground"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemButtonFaceColorKey));
            t.ResourceDictionary["SystemButtonBackgroundOnMoseOver"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemButtonFaceColorKey));
            t.ResourceDictionary["SystemButtonForegroundOnMoseOver"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemButtonTextColorKey));
            t.ResourceDictionary["SystemButtonBackgroundIsPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemButtonFaceColorKey));
            t.ResourceDictionary["SystemButtonForegroundIsPressed"] = new SolidColorBrush(VSThemeColor(EnvironmentColors.SystemButtonTextColorKey));
            // <Rectangle x:Key="WindowBackgroundContent" x:Shared="false" Height="96" Fill="{StaticResource WindowHeaderGradient}" VerticalAlignment="Top"/>
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            LoadThemeColors();
        }

        //-----------------------------------------------------------------------------
        private bool Load()
        {
            string theme_name;
            if (!Cfg.Get("Themes", "Current", out theme_name, ""))
                return false;

            var theme = themes.Where(t => t.Name == theme_name).FirstOrDefault();
            if (theme == null)
                return false;

            Current = theme;
            return true;
        }

        //-----------------------------------------------------------------------------
        public static void Save()
        {
            Cfg.Set("Themes", "Current", Instance.Current.Name);
        }

        //-----------------------------------------------------------------------------
        public static ThemeManager Instance
        {
            get
            {
                return instance;
            }
        }

        //-----------------------------------------------------------------------------
        public ResourceDictionary BaseDictionary { get { return base_dict; } }

        //-----------------------------------------------------------------------------
        public Theme Current
        {
            get { return current; }
            set
            {
                if (current == value)
                    return;

                current = value;

                foreach (var c in controls)
                    ApplyTheme(c, current);
            }
        }

        //---------------------------------------------------------------------
        public IEnumerable<Theme> Themes
        {
            get { return themes; }
        }

        //---------------------------------------------------------------------
        private bool HaveModernUIBase(FrameworkElement control)
        {
            var r = GetAccentDictionary(control);
            return r != null;
        }

        //---------------------------------------------------------------------
        private ResourceDictionary GetAccentDictionary(FrameworkElement control)
        {
            var r = (from dict in control.Resources.MergedDictionaries
                     where dict.Contains("Accent")
                     select dict).FirstOrDefault<ResourceDictionary>();
            return r;
        }

        //---------------------------------------------------------------------
        private ResourceDictionary GetCurrentThemeDictionary(FrameworkElement control)
        {
            var r = (from dict in control.Resources.MergedDictionaries
                     where dict.Contains("WindowBackground")
                     select dict).FirstOrDefault<ResourceDictionary>();
            return r;
        }

        //-----------------------------------------------------------------------------
        private void ApplyAccentColor(FrameworkElement control, Color c)
        {
            var r = GetAccentDictionary(control);
            if (r == null)
                return;

            r["AccentColor"] = c;
        }

        //-----------------------------------------------------------------------------
        private void ApplyTheme(FrameworkElement control, Theme theme)
        {
            var r = GetCurrentThemeDictionary(control);

            if (r != null)
                control.Resources.MergedDictionaries.Remove(r);

            ApplyAccentColor(control, theme.AccentColor);
            control.Resources.MergedDictionaries.Add(theme.ResourceDictionary);

            // Set background for windows
            var wnd = control as Window;
            if (wnd != null)
            {
                wnd.SetResourceReference(Window.BackgroundProperty, "WindowBackground");
                wnd.SetResourceReference(Window.ForegroundProperty, "WindowText");

                return;
            }
        }

        //------------------------------------------------------------------
        private void wnd_Closing(object sender, EventArgs e)
        {
            var wnd = sender as Window;
            if (wnd == null)
                return;

            wnd.Closing -= wnd_Closing;
            Unsubscribe(wnd);
        }

        //-----------------------------------------------------------------------------
        public void Subscribe(FrameworkElement control)
        {
            if (!HaveModernUIBase(control))
                control.Resources.MergedDictionaries.Add(BaseDictionary);

            ApplyTheme(control, Current);

            controls.Add(control);

            var wnd = control as Window;
            if (wnd != null)
            {
                wnd.Closing += wnd_Closing;
            }

            Logger.WriteLine("Subscribe: Count = {0}", controls.Count);
        }

        //-----------------------------------------------------------------------------
        public void Unsubscribe(FrameworkElement control)
        {
            controls.Remove(control);

            Logger.WriteLine("Unsubscribe: Count = {0}", controls.Count);
        }

        public static System.Windows.Media.Color VSThemeColor(ThemeResourceKey k)
        {
            //IVsUIShell5 shell5 = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell5;
            //return VsColors.GetThemedWPFColor(shell5, k);

            return DrawingColorToMediaColor(VSColorTheme.GetThemedColor(k));
        }

        public static System.Windows.Media.Color DrawingColorToMediaColor(System.Drawing.Color c)
        {
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static System.Drawing.Color MediaColorToDrawingColor(System.Windows.Media.Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.G);
        }
    }

    //-----------------------------------------------------------------------------
    public class Theme
    {
        public string Name { get; set; }
        public ResourceDictionary ResourceDictionary { get; set; }
        public Color AccentColor { get; set; }
        public Color ErrorColor { get; set; }
        public Color AnnotateColor { get; set; }

        private Color rev_log_line_color;
        private Color rev_log_node_color;

        //-----------------------------------------------------------------------------
        public Color RevLogLineColor
        {
            get
            {
                return rev_log_line_color;
            }
            set
            {
                if (rev_log_line_color != value)
                {
                    rev_log_line_color = value;
                    var pen = new Pen(new SolidColorBrush(value), 1);
                    pen.Freeze();

                    RevLogLinePen = pen;
                }
            }
        }

        //-----------------------------------------------------------------------------
        public Color RevLogNodeColor
        {
            get
            {
                return rev_log_node_color;
            }
            set
            {
                if (rev_log_node_color != value)
                {
                    rev_log_node_color = value;
                    var pen = new Pen(new SolidColorBrush(value), 1);
                    pen.Freeze();

                    RevLogNodePen = pen;
                }
            }
        }

        public Pen RevLogLinePen { get; private set; }
        public Pen RevLogNodePen { get; private set; }
    }
}
