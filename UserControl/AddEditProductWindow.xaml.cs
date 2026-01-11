using Microsoft.Win32;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace login_store
{
    public partial class AddEditProductWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentProductId = 0;

        private ObservableCollection<ProductVariant> variantsCollection = new ObservableCollection<ProductVariant>();
        private ObservableCollection<ProductImage> galleryImagesCollection = new ObservableCollection<ProductImage>();

        private List<int> variantsToDelete = new List<int>();
        private List<int> imagesToDelete = new List<int>();

        public AddEditProductWindow()
        {
            InitializeComponent();
            this.currentProductId = 0;
            txtTitle.Text = "เพิ่มสินค้าใหม่";
            InitializeDataBindings();
        }

        public AddEditProductWindow(int productId)
        {
            InitializeComponent();
            this.currentProductId = productId;
            txtTitle.Text = $"แก้ไขสินค้า ID: {productId}";
            InitializeDataBindings();
        }

        private void InitializeDataBindings()
        {
            VariantsDataGrid.ItemsSource = variantsCollection;
            GalleryImagesListBox.ItemsSource = galleryImagesCollection;
        }

        // --- 1. โหลดข้อมูล ---
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            LoadBrands();

            if (currentProductId > 0)
            {
                LoadProductData();
            }
        }

        // (ตัวช่วยโหลดรูปภาพ)
        private BitmapImage GetBitmapImageFromRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string path = relativePath.TrimStart('/');
                string fullPath = Path.Combine(baseDir, path);

                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception) { /* Handle error */ }
            return null;
        }

        private void LoadCategories()
        {
            List<ProductCategory> categories = new List<ProductCategory>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT category_id, name FROM product_categories";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new ProductCategory
                            {
                                CategoryId = reader.GetInt32("category_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
                cmbCategory.ItemsSource = categories;
            }
            catch (Exception ex) { CustomMessageBoxWindow.Show("Error loading categories: " + ex.Message, "Error"); }
        }

        private void LoadBrands()
        {
            List<ProductBrand> brands = new List<ProductBrand>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT brand_id, name FROM product_brands";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            brands.Add(new ProductBrand
                            {
                                BrandId = reader.GetInt32("brand_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
                cmbBrand.ItemsSource = brands;
            }
            catch (Exception ex) { CustomMessageBoxWindow.Show("Error loading brands: " + ex.Message, "Error"); }
        }

        // (แก้) 👈 (สำคัญ) แก้ไขเมธอดนี้ทั้งหมด
        private void LoadProductData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. โหลดข้อมูลสินค้า
                    string sqlProduct = "SELECT * FROM products WHERE product_id = @id";
                    using (MySqlCommand cmdProduct = new MySqlCommand(sqlProduct, conn))
                    {
                        cmdProduct.Parameters.AddWithValue("@id", this.currentProductId);
                        using (MySqlDataReader reader = cmdProduct.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtName.Text = GetStringSafe(reader, "name");
                                txtDescription.Text = GetStringSafe(reader, "description");
                                txtPrice.Text = reader.GetDecimal("price").ToString();
                                txtImagePath.Text = GetStringSafe(reader, "image_path");
                                cmbCategory.SelectedValue = reader.IsDBNull(reader.GetOrdinal("category_id")) ? (int?)null : reader.GetInt32("category_id");

                                // โหลดแบรนด์และรุ่น
                                int? brandId = reader.IsDBNull(reader.GetOrdinal("brand_id")) ? (int?)null : reader.GetInt32("brand_id");
                                cmbBrand.SelectedValue = brandId;

                                if (brandId.HasValue)
                                {
                                    LoadModelsByBrand(brandId.Value);
                                    // เลือกค่ารุ่น
                                    int? modelId = reader.IsDBNull(reader.GetOrdinal("model_id")) ? (int?)null : reader.GetInt32("model_id");
                                    cmbModel.SelectedValue = modelId;
                                }

                                chkIsActive.IsChecked = reader.GetBoolean("is_active");
                                imgMainPreview.Source = GetBitmapImageFromRelativePath(txtImagePath.Text);
                                txtSizeChartPath.Text = GetStringSafe(reader, "size_chart_path");
                                imgSizeChartPreview.Source = GetBitmapImageFromRelativePath(txtSizeChartPath.Text);
                            }
                        }
                    }

                    // 2. โหลด variants (เรียงตาม sort_order)
                    variantsCollection.Clear();
                    string sqlVariants = "SELECT * FROM product_variants WHERE product_id = @id ORDER BY sort_order ASC";
                    using (MySqlCommand cmdVariants = new MySqlCommand(sqlVariants, conn))
                    {
                        cmdVariants.Parameters.AddWithValue("@id", this.currentProductId);
                        using (MySqlDataReader reader = cmdVariants.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                variantsCollection.Add(new ProductVariant
                                {
                                    VariantId = reader.GetInt32("variant_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    SizeName = GetStringSafe(reader, "size_name"),
                                    StockQuantity = reader.GetInt32("stock_quantity"),
                                    SortOrder = reader.IsDBNull(reader.GetOrdinal("sort_order")) ? 0 : reader.GetInt32("sort_order")
                                });
                            }
                        }
                    }

                    // 3. โหลด images
                    galleryImagesCollection.Clear();
                    string sqlImages = "SELECT * FROM product_images WHERE product_id = @id ORDER BY sort_order";
                    using (MySqlCommand cmdImages = new MySqlCommand(sqlImages, conn))
                    {
                        cmdImages.Parameters.AddWithValue("@id", this.currentProductId);
                        using (MySqlDataReader reader = cmdImages.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string path = GetStringSafe(reader, "image_path");
                                galleryImagesCollection.Add(new ProductImage
                                {
                                    ImageId = reader.GetInt32("image_id"),
                                    ProductId = this.currentProductId,
                                    ImagePath = path,
                                    SortOrder = reader.GetInt32("sort_order"),
                                    ImagePreview = GetBitmapImageFromRelativePath(path)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลสินค้าได้: " + ex.Message, "Error");
                this.Close();
            }
        }

        // --- 2. การจัดการรูปภาพ (Browse) ---
        private (string OriginalPath, string RelativePath) BrowseAndCopyImage(string subFolder)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = openDialog.FileName;
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFilePath);
                    string destinationFolder = Path.Combine(AppContext.BaseDirectory, "Images", subFolder);
                    Directory.CreateDirectory(destinationFolder);
                    string destinationFilePath = Path.Combine(destinationFolder, uniqueFileName);
                    File.Copy(sourceFilePath, destinationFilePath, true);
                    string relativePath = $"/Images/{subFolder}/{uniqueFileName}";
                    return (sourceFilePath, relativePath);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการคัดลอกไฟล์รูปภาพ: " + ex.Message, "File Error");
                    return (null, null);
                }
            }
            return (null, null);
        }

        private void btnBrowseMainImage_Click(object sender, RoutedEventArgs e)
        {
            var (originalPath, relativePath) = BrowseAndCopyImage("Products");
            if (relativePath != null)
            {
                txtImagePath.Text = relativePath;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(originalPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgMainPreview.Source = bitmap;
            }
        }

        private void btnBrowseSizeChart_Click(object sender, RoutedEventArgs e)
        {
            var (originalPath, relativePath) = BrowseAndCopyImage("SizeCharts");
            if (relativePath != null)
            {
                txtSizeChartPath.Text = relativePath;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(originalPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgSizeChartPreview.Source = bitmap;
            }
        }

        private void btnAddGalleryImage_Click(object sender, RoutedEventArgs e)
        {
            var (originalPath, relativePath) = BrowseAndCopyImage("Products");
            if (relativePath != null)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(originalPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ProductImage newImg = new ProductImage
                {
                    ImageId = 0,
                    ProductId = this.currentProductId,
                    ImagePath = relativePath,
                    SortOrder = galleryImagesCollection.Count + 1,
                    ImagePreview = bitmap
                };
                galleryImagesCollection.Add(newImg);
            }
        }

        private void btnDeleteGalleryImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ProductImage imageToRemove)
            {
                if (imageToRemove.ImageId > 0)
                {
                    imagesToDelete.Add(imageToRemove.ImageId);
                }
                galleryImagesCollection.Remove(imageToRemove);
            }
        }


        // --- 3. การจัดการ Variants (Tab 2) ---
        private void VariantsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VariantsDataGrid.SelectedItem is ProductVariant selectedVariant)
            {
                txtVariantSize.Text = selectedVariant.SizeName;
                txtVariantStock.Text = selectedVariant.StockQuantity.ToString();
                txtVariantSort.Text = selectedVariant.SortOrder.ToString();
            }
        }

        private void btnAddVariant_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVariantSize.Text) || string.IsNullOrWhiteSpace(txtVariantStock.Text))
            {
                CustomMessageBoxWindow.Show("กรุณากรอกชื่อไซส์และสต็อก", "ข้อมูลไม่ครบ");
                return;
            }

            // (เพิ่ม) 👈 อ่านค่าลำดับ
            int.TryParse(txtVariantSort.Text, out int sortOrder);

            if (VariantsDataGrid.SelectedItem is ProductVariant selectedVariant)
            {
                selectedVariant.SizeName = txtVariantSize.Text.Trim();
                selectedVariant.StockQuantity = int.Parse(txtVariantStock.Text);
                selectedVariant.SortOrder = sortOrder; // (เพิ่ม) 👈 อัปเดตค่า
                VariantsDataGrid.Items.Refresh();
            }
            else
            {
                if (variantsCollection.Any(v => v.SizeName.Equals(txtVariantSize.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    CustomMessageBoxWindow.Show("มีไซส์นี้อยู่แล้ว กรุณาเลือกจากตารางเพื่ออัปเดต", "ไซส์ซ้ำ");
                    return;
                }
                variantsCollection.Add(new ProductVariant
                {
                    VariantId = 0,
                    ProductId = this.currentProductId,
                    SizeName = txtVariantSize.Text.Trim(),
                    StockQuantity = int.Parse(txtVariantStock.Text),
                    SortOrder = sortOrder // (เพิ่ม) 👈 ใส่ค่าตอนเพิ่ม
                });
            }
            txtVariantSize.Clear();
            txtVariantStock.Clear();
            txtVariantSort.Text = "0"; // (เพิ่ม) 👈 รีเซ็ตช่องกรอก
            VariantsDataGrid.SelectedItem = null;
        }

        private void btnDeleteVariant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ProductVariant variantToRemove)
            {
                if (variantToRemove.VariantId > 0)
                {
                    variantsToDelete.Add(variantToRemove.VariantId);
                }
                variantsCollection.Remove(variantToRemove);
            }
        }

        // --- 4. บันทึกข้อมูล (ปุ่ม Save หลัก) ---
        // --- 4. บันทึกข้อมูล (แก้ไข - เพิ่ม model_id) ---
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text) || cmbCategory.SelectedValue == null || cmbBrand.SelectedValue == null)
            {
                CustomMessageBoxWindow.Show("กรุณากรอกข้อมูลหลักให้ครบ", "ข้อมูลไม่ครบ");
                return;
            }
            if (variantsCollection.Count == 0)
            {
                CustomMessageBoxWindow.Show("คุณต้องเพิ่มไซส์อย่างน้อย 1 รายการ", "ข้อมูลไม่ครบ");
                return;
            }

            // (เพิ่ม) รับค่า Model
            object modelValue = cmbModel.SelectedValue;

            MySqlConnection conn = new MySqlConnection(connectionString);
            MySqlTransaction transaction = null;

            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                int totalStock = variantsCollection.Sum(v => v.StockQuantity);

                string sqlProduct;
                if (currentProductId == 0)
                {
                    // (แก้) เพิ่ม model_id
                    sqlProduct = @"INSERT INTO products (name, description, price, image_path, size_chart_path, stock_quantity, brand_id, category_id, model_id, is_active)
                                   VALUES (@name, @desc, @price, @img, @sizeChart, @stock, @brand, @cat, @model, @active);
                                   SELECT LAST_INSERT_ID();";
                }
                else
                {
                    // (แก้) เพิ่ม model_id
                    sqlProduct = @"UPDATE products SET name = @name, description = @desc, price = @price, image_path = @img, 
                                   size_chart_path = @sizeChart, stock_quantity = @stock, brand_id = @brand, category_id = @cat, model_id = @model, is_active = @active
                                   WHERE product_id = @id;";
                }

                using (MySqlCommand cmdProduct = new MySqlCommand(sqlProduct, conn, transaction))
                {
                    cmdProduct.Parameters.AddWithValue("@id", this.currentProductId);
                    cmdProduct.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    cmdProduct.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmdProduct.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                    cmdProduct.Parameters.AddWithValue("@img", txtImagePath.Text);
                    cmdProduct.Parameters.AddWithValue("@sizeChart", txtSizeChartPath.Text);
                    cmdProduct.Parameters.AddWithValue("@stock", totalStock);
                    cmdProduct.Parameters.AddWithValue("@brand", cmbBrand.SelectedValue);
                    cmdProduct.Parameters.AddWithValue("@cat", cmbCategory.SelectedValue);
                    // (เพิ่ม) Parameter model
                    if (modelValue != null)
                        cmdProduct.Parameters.AddWithValue("@model", modelValue);
                    else
                        cmdProduct.Parameters.AddWithValue("@model", DBNull.Value);

                    cmdProduct.Parameters.AddWithValue("@active", chkIsActive.IsChecked);

                    if (currentProductId == 0)
                    {
                        this.currentProductId = Convert.ToInt32(cmdProduct.ExecuteScalar());
                    }
                    else
                    {
                        cmdProduct.ExecuteNonQuery();
                    }
                }

                // (ลบ)
                foreach (int variantId in variantsToDelete)
                {
                    using (MySqlCommand cmdDel = new MySqlCommand("DELETE FROM product_variants WHERE variant_id = @id", conn, transaction))
                    {
                        cmdDel.Parameters.AddWithValue("@id", variantId);
                        cmdDel.ExecuteNonQuery();
                    }
                }
                foreach (int imageId in imagesToDelete)
                {
                    using (MySqlCommand cmdDel = new MySqlCommand("DELETE FROM product_images WHERE image_id = @id", conn, transaction))
                    {
                        cmdDel.Parameters.AddWithValue("@id", imageId);
                        cmdDel.ExecuteNonQuery();
                    }
                }

                // (บันทึก Variants)
                foreach (ProductVariant variant in variantsCollection)
                {
                    string sqlVariant;
                    if (variant.VariantId == 0)
                    {
                        sqlVariant = "INSERT INTO product_variants (product_id, size_name, stock_quantity, sort_order) VALUES (@pid, @size, @stock, @sort)";
                    }
                    else
                    {
                        sqlVariant = "UPDATE product_variants SET size_name = @size, stock_quantity = @stock, sort_order = @sort WHERE variant_id = @vid";
                    }
                    using (MySqlCommand cmdVar = new MySqlCommand(sqlVariant, conn, transaction))
                    {
                        cmdVar.Parameters.AddWithValue("@vid", variant.VariantId);
                        cmdVar.Parameters.AddWithValue("@pid", this.currentProductId);
                        cmdVar.Parameters.AddWithValue("@size", variant.SizeName);
                        cmdVar.Parameters.AddWithValue("@stock", variant.StockQuantity);
                        cmdVar.Parameters.AddWithValue("@sort", variant.SortOrder);
                        cmdVar.ExecuteNonQuery();
                    }
                }

                // (บันทึก Gallery)
                int sortOrder = 1;
                foreach (ProductImage image in galleryImagesCollection)
                {
                    if (image.ImageId == 0)
                    {
                        string sqlImg = "INSERT INTO product_images (product_id, image_path, sort_order) VALUES (@pid, @path, @sort)";
                        using (MySqlCommand cmdImg = new MySqlCommand(sqlImg, conn, transaction))
                        {
                            cmdImg.Parameters.AddWithValue("@pid", this.currentProductId);
                            cmdImg.Parameters.AddWithValue("@path", image.ImagePath);
                            cmdImg.Parameters.AddWithValue("@sort", sortOrder);
                            cmdImg.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string sqlImgUpdate = "UPDATE product_images SET sort_order = @sort WHERE image_id = @imgId";
                        using (MySqlCommand cmdImg = new MySqlCommand(sqlImgUpdate, conn, transaction))
                        {
                            cmdImg.Parameters.AddWithValue("@sort", sortOrder);
                            cmdImg.Parameters.AddWithValue("@imgId", image.ImageId);
                            cmdImg.ExecuteNonQuery();
                        }
                    }
                    sortOrder++;
                }

                transaction.Commit();
                CustomMessageBoxWindow.Show("บันทึกข้อมูลสินค้าสำเร็จ", "Success");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึก: " + ex.Message, "Database Error");
            }
            finally
            {
                conn?.Close();
            }
        }

        // (เพิ่ม) เมธอดโหลดรุ่นตามแบรนด์
        private void LoadModelsByBrand(int brandId)
        {
            List<ProductModel> models = new List<ProductModel>(); // (หรือ ProductModelItem)
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT model_id, name, brand_id FROM product_models WHERE brand_id = @brandId ORDER BY name";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@brandId", brandId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                models.Add(new ProductModel
                                {
                                    ModelId = reader.GetInt32("model_id"),
                                    Name = reader.GetString("name"),
                                    BrandId = reader.GetInt32("brand_id")
                                });
                            }
                        }
                    }
                }
                cmbModel.ItemsSource = models;
                cmbModel.IsEnabled = models.Count > 0; // ถ้ามีรุ่นให้เลือกค่อยเปิดใช้งาน
            }
            catch (Exception ex) { CustomMessageBoxWindow.Show("Error loading models: " + ex.Message, "Error"); }
        }

        // (เพิ่ม) Event เมื่อเลือกแบรนด์เปลี่ยน
        private void cmbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBrand.SelectedValue != null && int.TryParse(cmbBrand.SelectedValue.ToString(), out int brandId))
            {
                LoadModelsByBrand(brandId);
            }
            else
            {
                cmbModel.ItemsSource = null;
                cmbModel.IsEnabled = false;
            }
        }

        // --- 5. อื่นๆ ---
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Border) || e.OriginalSource.GetType() == typeof(Grid))
            {
                if (e.ButtonState == MouseButtonState.Pressed) { this.DragMove(); }
            }
        }
        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex)) return string.Empty;
            return reader.GetString(colIndex);
        }
    }
}