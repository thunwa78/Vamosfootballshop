using System;
using System.Collections.Generic;
using System.IO;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace login_store
{
    public class InvoiceDocument : IDocument
    {
        private OrderDetails Order;
        private List<CartItemDetails> Items;
        private CompanyInfo ShopInfo; // ข้อมูลร้านค้า
        private string FontPathRegular;
        private string FontPathBold;
        private string LogoPath;

        public InvoiceDocument(OrderDetails order, List<CartItemDetails> items, CompanyInfo shopInfo)
        {
            this.Order = order;
            this.Items = items;
            this.ShopInfo = shopInfo;

            // Path ของฟอนต์ (ต้องตั้งค่า Copy if newer ที่ไฟล์ฟอนต์ด้วย)
            FontPathRegular = Path.Combine(AppContext.BaseDirectory, "Fonts", "Sarabun-Regular.ttf");
            FontPathBold = Path.Combine(AppContext.BaseDirectory, "Fonts", "Sarabun-Bold.ttf");

            // Path ของโลโก้ (ต้องตั้งค่า Copy if newer ที่ไฟล์รูปด้วย)
            LogoPath = Path.Combine(AppContext.BaseDirectory, "Images", "Vamoslogo1.png");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(style => style.FontFamily(FontPathRegular).FontSize(10));

                // ---------------------------------------------------------
                // 1. ส่วนหัว (Header)
                // ---------------------------------------------------------
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        // A. โลโก้ร้าน (ซ้ายสุด)
                        if (File.Exists(LogoPath))
                        {
                            row.ConstantItem(60).Image(LogoPath).FitArea();
                        }

                        // B. ข้อมูลร้าน (ดึงจาก DB)
                        row.RelativeItem().PaddingLeft(15).Column(c =>
                        {
                            c.Item().Text("VAMOS SHOP").FontFamily(FontPathBold).FontSize(24).FontColor(Colors.Green.Darken2);
                            c.Item().Text(ShopInfo.Address ?? "ที่อยู่ร้านค้า");
                            c.Item().Text($"เลขประจำตัวผู้เสียภาษี: {ShopInfo.TaxId ?? "-"}");
                            c.Item().Text($"โทร: {ShopInfo.Phone ?? "-"}");
                            if (!string.IsNullOrEmpty(ShopInfo.Email))
                            {
                                c.Item().Text($"อีเมล: {ShopInfo.Email}");
                            }
                        });

                        // C. ข้อมูลใบเสร็จ (ขวาสุด)
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("ใบเสร็จรับเงิน / RECEIPT").FontFamily(FontPathBold).FontSize(16);
                            c.Item().Text($"เลขที่ (No): INV-{Order.OrderId:D5}");
                            c.Item().Text($"วันที่ (Date): {DateTime.Now:dd/MM/yyyy}");
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // ข้อมูลลูกค้า
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ลูกค้า (Customer):").FontFamily(FontPathBold);
                            c.Item().Text(UserSession.Username);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("อ้างอิงคำสั่งซื้อ (Order Ref):").FontFamily(FontPathBold);
                            c.Item().Text($"#{Order.OrderId} ({Order.OrderDate:dd/MM/yyyy})");
                        });
                    });
                });

                // ---------------------------------------------------------
                // 2. ส่วนเนื้อหา (Content) - ตารางสินค้า
                // ---------------------------------------------------------
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // ตารางสินค้า
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // ลำดับ
                            columns.RelativeColumn(4);  // รายการ
                            columns.RelativeColumn(2);  // ราคาต่อหน่วย
                            columns.RelativeColumn(1);  // จำนวน
                            columns.RelativeColumn(2);  // รวม
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("รายการสินค้า (Description)");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("ราคา (Price)");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("จำนวน (Qty)");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("รวม (Total)");
                        });

                        // Item Rows
                        int index = 1;
                        foreach (var item in Items)
                        {
                            table.Cell().Element(CellStyle).Text($"{index}");
                            table.Cell().Element(CellStyle).Column(c =>
                            {
                                c.Item().Text(item.Name).FontFamily(FontPathBold);
                                if (!string.IsNullOrEmpty(item.SizeName))
                                {
                                    c.Item().Text($"Size: {item.SizeName}").FontSize(9).FontColor(Colors.Grey.Darken1);
                                }
                            });
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{(item.Price * item.Quantity):N2}");
                            index++;
                        }
                    });

                    // ---------------------------------------------------------
                    // 3. ส่วนสรุปยอด (Calculation - VAT Added)
                    // ---------------------------------------------------------
                    col.Item().PaddingTop(10).AlignRight().Column(c =>
                    {
                        // ดึงข้อมูลจาก DB (Order.TotalAmount คือยอดสุทธิที่รวม VAT แล้วตามที่เราคำนวณใน Checkout)
                        decimal grandTotal = Order.TotalAmount;       // (เช่น 12,650.61)
                        decimal discount = Order.DiscountAmount;      // (เช่น 5,067)

                        // คำนวณย้อนกลับหาฐานภาษี
                        // ยอดหลังหักส่วนลด + VAT = GrandTotal
                        // ยอดหลังหักส่วนลด * 1.07 = GrandTotal
                        // ยอดหลังหักส่วนลด = GrandTotal / 1.07
                        decimal amountBeforeVat = grandTotal / 1.07m; // (เช่น 11,823)
                        decimal vatAmount = grandTotal - amountBeforeVat; // (เช่น 827.61)

                        // ราคาสินค้ารวม (Subtotal) = ยอดหลังหักส่วนลด + ส่วนลด
                        decimal subTotal = amountBeforeVat + discount; // (เช่น 16,890)

                        // ตารางสรุปตัวเลข
                        c.Item().Width(280).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            // 1. ราคาสินค้า (Subtotal)
                            table.Cell().Text("ราคาสินค้า (Subtotal):").AlignRight();
                            table.Cell().Text($"{subTotal:N2}").AlignRight();

                            // 2. หักส่วนลด
                            if (discount > 0)
                            {
                                string discountLabel = "หักส่วนลด (Discount):";
                                if (!string.IsNullOrEmpty(Order.VoucherCode))
                                {
                                    discountLabel = $"หักส่วนลด ({Order.VoucherCode}):";
                                }

                                table.Cell().Text(discountLabel).AlignRight().FontColor(Colors.Red.Medium);
                                table.Cell().Text($"-{discount:N2}").AlignRight().FontColor(Colors.Red.Medium);
                            }

                            // 3. ยอดก่อนภาษี
                            table.Cell().PaddingTop(5).Text("มูลค่าสินค้า (Price after disc.):").AlignRight();
                            table.Cell().PaddingTop(5).Text($"{amountBeforeVat:N2}").AlignRight();

                            // 4. ภาษี
                            table.Cell().Text("ภาษีมูลค่าเพิ่ม 7% (VAT):").AlignRight().FontSize(9).FontColor(Colors.Grey.Darken1);
                            table.Cell().Text($"{vatAmount:N2}").AlignRight().FontSize(9).FontColor(Colors.Grey.Darken1);

                            // 5. ยอดสุทธิ
                            table.Cell().ColumnSpan(2).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);

                            table.Cell().PaddingTop(2).Text("จำนวนเงินทั้งสิ้น (Net Total):").AlignRight().FontFamily(FontPathBold).FontSize(12);
                            table.Cell().PaddingTop(2).Text($"{grandTotal:N2} บาท").AlignRight().FontFamily(FontPathBold).FontSize(12);
                        });
                    });
                });

                // ---------------------------------------------------------
                // 4. ส่วนท้าย (Footer)
                // ---------------------------------------------------------
                page.Footer().Column(col =>
                {
                    col.Item().AlignCenter().Text("ขอบคุณที่อุดหนุน VAMOS SHOP").FontSize(12).FontFamily(FontPathBold);
                    col.Item().AlignCenter().Text("สินค้าซื้อแล้วไม่รับเปลี่ยนหรือคืน (ยกเว้นความผิดพลาดจากการผลิต)").FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).AlignCenter().Text(x =>
                    {
                        x.Span("หน้า ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });
        }

        // --- เมธอดช่วยตกแต่ง (Style) ---
        private IContainer HeaderStyle(IContainer container)
        {
            return container.Background(Colors.Grey.Lighten3).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        }

        private IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5);
        }
    }
}