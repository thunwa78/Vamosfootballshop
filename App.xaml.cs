using PdfSharp.Fonts;   // <--- using ที่จำเป็น
using PdfSharp.WPFonts; // <--- using ที่จำเป็น
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace login_store
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // ❗️❗️ เปลี่ยนมาใช้บรรทัดนี้แทน ❗️❗️
            PdfSharp.Fonts.GlobalFontSettings.FontResolver = new login_store.MyFontResolver();

            // (โค้ดเดิมของคุณ)
            base.OnStartup(e);
            
        }
    }
}