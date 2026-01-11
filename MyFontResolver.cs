using PdfSharp.Fonts;
using System;
using System.IO;

namespace login_store
{
    // นี่คือตัวจัดการฟอนต์แบบกำหนดเองของเรา
    public class MyFontResolver : IFontResolver
    {
        public string DefaultFontName => "Arial";

        public byte[] GetFont(string faceName)
        {
            // faceName คือชื่อไฟล์ .ttf ที่เราจะส่งคืน
            // เราจะหาไฟล์ฟอนต์จากโฟลเดอร์ "Fonts" ที่อยู่ในตำแหน่งเดียวกับ .exe
            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", faceName);

            if (File.Exists(fontPath))
            {
                return File.ReadAllBytes(fontPath);
            }
            else
            {
                // ถ้าไม่เจอ ให้ลองหาตัวปกติแทน
                fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "ARIAL.TTF");
                if (File.Exists(fontPath))
                {
                    return File.ReadAllBytes(fontPath);
                }
            }
            // ถ้าไม่เจออะไรเลยจริงๆ ก็ต้องโยน Error
            throw new FileNotFoundException($"ไม่พบฟอนต์: {faceName} ในโฟลเดอร์ Fonts");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // ส่วนนี้จะบอก PDFsharp ว่า "Arial" แบบ "Bold" คือไฟล์ไหน

            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    // ชื่อไฟล์ฟอนต์ที่เรามีในโฟลเดอร์ Fonts
                    return new FontResolverInfo("ARIALBI.TTF");
                }
                if (isBold)
                {
                    return new FontResolverInfo("ARIALBD.TTF");
                }
                if (isItalic)
                {
                    return new FontResolverInfo("ARIALI.TTF");
                }
                // ปกติ
                return new FontResolverInfo("ARIAL.TTF");
            }

            // ถ้าไม่ใช่ Arial ก็ใช้ตัวปกติไปก่อน
            return new FontResolverInfo("ARIAL.TTF");
        }
    }
}