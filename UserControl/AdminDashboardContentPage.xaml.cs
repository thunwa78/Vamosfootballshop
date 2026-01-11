//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading; // 👈 (เพิ่ม) 1. สำหรับ Timer

namespace login_store
{
    public partial class AdminDashboardContentPage : Page, INotifyPropertyChanged
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        // (เพิ่ม) 2. ตัวแปรสำหรับ Real-time Timer
        private DispatcherTimer _timer;

        // (LiveCharts Properties - เหมือนเดิม)
        public SeriesCollection RevenueSeriesCollection { get; set; }
        public SeriesCollection TopSellingSeriesCollection { get; set; }
        public List<string> RevenueLabels { get; set; }
        public List<string> TopSellingLabels { get; set; }
        public Func<double, string> RevenueFormatter { get; set; }

        public AdminDashboardContentPage()
        {
            InitializeComponent();

            // (โค้ด LiveCharts - เหมือนเดิม)
            RevenueSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "รายได้",
                    Values = new ChartValues<decimal>(),
                    PointGeometry = null,
                    StrokeThickness = 3,
                    Stroke = (Brush)new BrushConverter().ConvertFrom("#2ECC71"),
                    Fill = Brushes.Transparent
                }
            };
            RevenueLabels = new List<string>();
            RevenueFormatter = value => value.ToString("N0");

            TopSellingSeriesCollection = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "จำนวน",
                    Values = new ChartValues<int>(),
                    Fill = (Brush)new BrushConverter().ConvertFrom("#F39C12")
                }
            };
            TopSellingLabels = new List<string>();

            this.DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // (แก้) 3. ตั้งค่าเริ่มต้นให้ปฏิทิน
            dpStartDate.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpEndDate.SelectedDate = DateTime.Now;

            // (แก้) 4. โหลดข้อมูลครั้งแรก
            LoadDashboardData();

            // (เพิ่ม) 5. เริ่ม Timer ให้โหลดซ้ำทุก 1 นาที
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // (เพิ่ม) 6. จัดการ Event เมื่อปิดหน้านี้
            this.Unloaded += Page_Unloaded;
        }

        // (เพิ่ม) 7. หยุด Timer เมื่อออกจากหน้า
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }

        // (เพิ่ม) 8. เมื่อ Timer ทำงาน ให้โหลดข้อมูลใหม่
        private void Timer_Tick(object sender, EventArgs e)
        {
            LoadDashboardData();
        }

        // (เพิ่ม) 9. ปุ่ม "รีเฟรช"
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        // (ลบ) 10. ลบ CmbRevenueFilter_SelectionChanged ออก

        // (ลบ) 11. ลบ LoadSelectedRevenue และ CalculateRevenue ออก

        // (แก้) 12. (สำคัญ) เมธอดหลักในการดึงข้อมูลทั้งหมด
        private void LoadDashboardData()
        {
            DateTime startDate = dpStartDate.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = (dpEndDate.SelectedDate ?? DateTime.MaxValue).AddDays(1).AddSeconds(-1);

            if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue) return;

            // (แก้) เรียกเมธอดที่แก้ไขแล้ว
            LoadDashboardStats(startDate, endDate);
            LoadRevenueChartData(startDate, endDate);
            LoadTopSellingChartData(startDate, endDate);
            LoadLowStockItems(); // (อันนี้ไม่ต้องแก้วันที่)

            txtLastUpdated.Text = $"(อัปเดตล่าสุด: {DateTime.Now:HH:mm:ss})";
        }


        // (แก้) 13. แก้ไขเมธอดนี้ ให้รับวันที่ และโหลด KPI ทั้งหมด
        // (แก้) 13. แก้ไขเมธอดนี้ ให้รับวันที่ และโหลด KPI ทั้งหมด
        private void LoadDashboardStats(DateTime start, DateTime end)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. ยอดขายรวม (ตามโค้ดเดิมของคุณ: 'total_amount', 'Approved', 'Shipped')
                    string sqlSales = "SELECT SUM(total_amount) FROM orders WHERE (status = 'Approved' OR status = 'Shipped') AND order_date BETWEEN @start AND @end";
                    using (MySqlCommand cmd = new MySqlCommand(sqlSales, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                            txtTotalSales.Text = $"{Convert.ToDecimal(result):N2} บาท";
                        else
                            txtTotalSales.Text = "0.00 บาท";
                    }

                    // 2. คำสั่งซื้อใหม่ (ตามโค้ดเดิม: 'Pending Payment')
                    string sqlPending = "SELECT COUNT(*) FROM orders WHERE status = 'Pending Payment' AND order_date BETWEEN @start AND @end";
                    using (MySqlCommand cmd = new MySqlCommand(sqlPending, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);
                        txtPendingOrders.Text = ((long)cmd.ExecuteScalar()).ToString();
                    }

                    // (แก้) 3. ลูกค้าทั้งหมด (ลบ 'created_at' และ Parameters ออก)
                    string sqlUsers = "SELECT COUNT(*) FROM users WHERE role = 'user'";
                    using (MySqlCommand cmd = new MySqlCommand(sqlUsers, conn))
                    {
                        // (ไม่มี Parameters)
                        txtNewCustomers.Text = ((long)cmd.ExecuteScalar()).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดสถิติการ์ด KPI: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) 14. แก้ไขเมธอดนี้ ให้รับวันที่
        private void LoadRevenueChartData(DateTime start, DateTime end)
        {
            var tempRevenues = new List<decimal>();
            var tempLabels = new List<string>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (แก้) 15. เปลี่ยน WHERE clause ให้ใช้ @start และ @end
                    string sql = @"
                        SELECT 
                            DATE(order_date) AS Day, 
                            SUM(total_amount) AS DailyRevenue
                        FROM orders
                        WHERE 
                            (status = 'Approved' OR status = 'Shipped')
                            AND order_date BETWEEN @start AND @end
                        GROUP BY DATE(order_date)
                        ORDER BY Day ASC;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        // (เพิ่ม) 16. ส่งค่า Parameters
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tempRevenues.Add(reader.GetDecimal("DailyRevenue"));
                                tempLabels.Add(reader.GetDateTime("Day").ToString("dd MMM"));
                            }
                        }
                    }
                }

                RevenueSeriesCollection[0].Values = new ChartValues<decimal>(tempRevenues);
                RevenueLabels = tempLabels;
                OnPropertyChanged(nameof(RevenueLabels));
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดกราฟรายได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) 17. แก้ไขเมธอดนี้ ให้รับวันที่
        private void LoadTopSellingChartData(DateTime start, DateTime end)
        {
            var tempValues = new List<int>();
            var tempLabels = new List<string>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // (แก้) SQL นี้รองรับทั้งออเดอร์เก่า (ผ่าน variant) และใหม่ (ผ่าน product_id)
                    string sql = @"
                        SELECT 
                            p.name, 
                            SUM(oi.quantity) AS total_sold 
                        FROM order_items oi 
                        
                        -- 1. จอยกับ variants ก่อน เพื่อหา product_id จากไซส์ (เผื่อใน order_items เป็น NULL)
                        LEFT JOIN product_variants pv ON oi.product_variant_id = pv.variant_id
                        
                        -- 2. จอยกับ products โดยใช้ COALESCE (ถ้า oi.product_id ไม่มี ให้ใช้ pv.product_id แทน)
                        JOIN products p ON p.product_id = COALESCE(pv.product_id, oi.product_id)
                        
                        JOIN orders o ON oi.order_id = o.order_id 
                        
                        WHERE (o.status = 'Approved' OR o.status = 'Shipped')
                          AND o.order_date BETWEEN @start AND @end
                        
                        GROUP BY p.product_id, p.name 
                        ORDER BY total_sold DESC 
                        LIMIT 10";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // (แก้) ใช้ Convert.ToInt32 เพื่อความปลอดภัย (บางที MySQL ส่งค่ามาเป็น decimal)
                                tempValues.Add(Convert.ToInt32(reader["total_sold"]));
                                tempLabels.Add(reader.GetString("name"));
                            }
                        }
                    }
                }

                // กลับด้านข้อมูลเพื่อให้กราฟแท่งแสดงจาก มาก -> น้อย (บนลงล่าง)
                tempValues.Reverse();
                tempLabels.Reverse();

                TopSellingSeriesCollection[0].Values = new ChartValues<int>(tempValues);
                TopSellingLabels = tempLabels;
                OnPropertyChanged(nameof(TopSellingLabels));
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดกราฟสินค้าขายดี: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // 20. เมธอดนี้ถูกต้องแล้ว (สต็อกไม่เกี่ยวกับวันที่)
        private void LoadLowStockItems()
        {
            List<Product> lowStockList = new List<Product>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (แก้) 21. ใช้ 'stock_quantity' จาก 'products' (ซึ่งคือสต็อกรวม)
                    string sql = "SELECT product_id, name, stock_quantity " +
                                 "FROM products WHERE is_active = 1 AND stock_quantity < 10 " +
                                 "ORDER BY stock_quantity ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lowStockList.Add(new Product
                            {
                                ProductId = reader.GetInt32("product_id"),
                                Name = reader.GetString("name"),
                                // Price = reader.GetDecimal("price"), // (เราไม่ได้ดึง Price มา)
                                StockQuantity = reader.GetInt32("stock_quantity")
                            });
                        }
                    }
                }
                LowStockDataGrid.ItemsSource = lowStockList;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลสินค้าใกล้หมดได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- (โค้ดส่วนที่เหลือเหมือนเดิม) ---

        private void PendingCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (this.Parent is Frame parentFrame)
            {
                var adminDashboardPage = FindParent<AdminDashboardPage>(this);

                if (adminDashboardPage != null)
                {
                    // (แก้) 22. ควรส่งค่า Filter ไปให้หน้า Orders
                    parentFrame.Navigate(new AdminManageOrdersPage("Pending Payment"));
                    adminDashboardPage.SetActiveButton(adminDashboardPage.btnManageOrders);
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // (แก้) เติมโค้ดในเมธอดนี้
        private void TopSellingChart_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. ดึงวันที่เริ่มต้น-สิ้นสุด (ถ้าไม่มีให้ใช้ค่า Default)
            DateTime startDate = dpStartDate.SelectedDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = (dpEndDate.SelectedDate ?? DateTime.Now).AddDays(1).AddSeconds(-1);

            // 2. เรียกเมธอดโหลดข้อมูลกราฟสินค้าขายดี
            LoadTopSellingChartData(startDate, endDate);
        }
    }
}