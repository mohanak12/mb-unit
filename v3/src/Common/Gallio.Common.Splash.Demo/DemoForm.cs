﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gallio.Common.Splash.Demo
{
    public partial class DemoForm : Form
    {
        public DemoForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Style defaultStyle = Style.CreateDefaultStyle();
            StyleBuilder styleBuilder;
            
            styleBuilder = new StyleBuilder();
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Text...\nText 2... ");

            styleBuilder = new StyleBuilder()
            {
                Color = Color.Green,
                Font = new Font(FontFamily.GenericSerif, 16)
            };
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Text 3... ");

            styleBuilder = new StyleBuilder()
            {
                Color = Color.Gold
            };
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Text 4...\n");
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "\nMore \tText...\n");

            styleBuilder = new StyleBuilder();
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Why Hello مرحبا العالمي World?  How are you?");
            splashView1.AppendLine(styleBuilder.ToStyle(defaultStyle));
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Tab1\tTab2\tTab3\tTab4\tTab5\tTab6\tTab7\tTab8\n");
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Tab.1\tTab.2\tTab.3\tTab.4\tTab.5\tTab.6\tTab.7\tTab.8\n");

            styleBuilder = new StyleBuilder()
            {
                Font = new Font(defaultStyle.Font, FontStyle.Italic),
                LeftMargin = 30,
                RightMargin = 30,
                FirstLineIndent = 40
            };
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "This paragraph has a first line indent, left margin and right margin.  Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\n");

            styleBuilder = new StyleBuilder()
            {
                WordWrap = false,
                Color = Color.Blue,
                Font = new Font(defaultStyle.Font, FontStyle.Bold)
            };
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.  Word wrap disabled.\n");

            styleBuilder = new StyleBuilder()
            {
                Color = Color.Red,
                Font = new Font(FontFamily.GenericSerif, 16)
            };
            splashView1.AppendLine(styleBuilder.ToStyle(defaultStyle));
            splashView1.AppendText(styleBuilder.ToStyle(defaultStyle), "القرآن تمتلك المظهر الخارجي وعمق خفي ، وهو المعنى الظاهر والمعنى الباطني. هذا المعنى الباطني بدوره يخفي معنى باطني (هذا العمق تمتلك العمق ، بعد صورة من الكرات السماوية التي هي المغلقة داخل بعضها البعض). غني عن ذلك لمدة سبعة المعاني الباطنية (سبعة من عمق أعماق المخفية).");
            splashView1.AppendLine(styleBuilder.ToStyle(defaultStyle));
        }
    }
}
