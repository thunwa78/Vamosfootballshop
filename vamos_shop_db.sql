-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Jan 11, 2026 at 04:55 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `vamos_shop_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `ad_slides`
--

CREATE TABLE `ad_slides` (
  `ad_id` int(11) NOT NULL,
  `image_path` varchar(255) NOT NULL,
  `target_url` varchar(255) DEFAULT NULL,
  `sort_order` int(11) NOT NULL DEFAULT 0,
  `is_active` tinyint(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `ad_slides`
--

INSERT INTO `ad_slides` (`ad_id`, `image_path`, `target_url`, `sort_order`, `is_active`) VALUES
(11, '/Images/Ads/arsenal.png', 'product_id:2', 1, 1),
(13, '/Images/Ads/bootsads.png', 'brand:NIKE', 2, 1),
(14, '/Images/Ads/c6a5c102-36fc-4aa5-b4e9-9e4df6562cbe.png', 'brand:ADIDAS', 3, 1),
(15, '/Images/Ads/8886d5b5-cc25-4017-be72-a4b1fa450dfa.png', 'product_id:25', 4, 1);

-- --------------------------------------------------------

--
-- Table structure for table `cart_items`
--

CREATE TABLE `cart_items` (
  `cart_item_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `product_variant_id` int(11) NOT NULL,
  `quantity` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `cart_items`
--

INSERT INTO `cart_items` (`cart_item_id`, `user_id`, `product_variant_id`, `quantity`) VALUES
(44, 2, 95, 1),
(45, 2, 5, 1);

-- --------------------------------------------------------

--
-- Table structure for table `company_info`
--

CREATE TABLE `company_info` (
  `id` int(11) NOT NULL DEFAULT 1,
  `description` text DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `tax_id` varchar(50) DEFAULT NULL,
  `address` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `company_info`
--

INSERT INTO `company_info` (`id`, `description`, `email`, `phone`, `tax_id`, `address`) VALUES
(1, 'VAMOS Shop ก่อตั้งขึ้นในปี 2025 โดยกลุ่มคนที่หลงใหลในกีฬาฟุตบอล\nภารกิจของเราคือการนำเสนออุปกรณ์ฟุตบอลที่ดีที่สุด ไม่ว่าจะเป็นรองเท้าสตั๊ด, เสื้อแข่งทีมโปรด, ลูกฟุตบอลมาตรฐาน, และอุปกรณ์เสริมต่างๆ ที่มีคุณภาพสูงสุดส่งตรงถึงมือคุณ\n\nเราเชื่อว่าอุปกรณ์ที่ดีคือส่วนสำคัญในการดึงศักยภาพสูงสุดของนักกีฬา ทีมงานของเราพร้อมให้คำแนะนำและบริการที่เป็นเลิศ เพื่อให้คุณได้รับประสบการณ์การช้อปปิ้งที่ดีที่สุด', 'Vamosfootballshop@gmail.com', '098-632-3560', '0-1234-56789-00-0', '123 ถ.มิตรภาพ, ต.ในเมือง อ.เมือง, จ.ขอนแก่น 40000');

-- --------------------------------------------------------

--
-- Table structure for table `notifications`
--

CREATE TABLE `notifications` (
  `notification_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `message` text NOT NULL,
  `is_read` tinyint(1) NOT NULL DEFAULT 0,
  `created_at` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `notifications`
--

INSERT INTO `notifications` (`notification_id`, `user_id`, `message`, `is_read`, `created_at`) VALUES
(1, 2, 'การชำระเงินสำหรับ Order #3 ได้รับการอนุมัติแล้ว', 0, '2025-11-03 12:47:02'),
(2, 2, 'การชำระเงินสำหรับ Order #4 ได้รับการอนุมัติแล้ว', 0, '2025-11-03 22:30:50'),
(3, 2, 'การชำระเงินสำหรับ Order #5 ได้รับการอนุมัติแล้ว', 0, '2025-11-03 22:36:58'),
(4, 5, 'การชำระเงินสำหรับ Order #8 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 1, '2025-11-04 00:24:22'),
(5, 2, 'การชำระเงินสำหรับ Order #6 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-04 00:24:24'),
(6, 2, 'การชำระเงินสำหรับ Order #9 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-05 01:03:18'),
(7, 5, 'การชำระเงินสำหรับ Order #22 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-14 10:07:26'),
(8, 5, 'การชำระเงินสำหรับ Order #24 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-14 10:20:16'),
(9, 5, 'Order #27 ถูกปฏิเสธ เนื่องจากหลักฐานการโอนเงินไม่ถูกต้อง', 0, '2025-11-14 15:46:05'),
(10, 5, 'การชำระเงินสำหรับ Order #28 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-17 21:37:54'),
(11, 5, 'การชำระเงินสำหรับ Order #29 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:06'),
(12, 5, 'การชำระเงินสำหรับ Order #26 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:09'),
(13, 5, 'การชำระเงินสำหรับ Order #25 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:11'),
(14, 5, 'การชำระเงินสำหรับ Order #19 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:12'),
(15, 5, 'การชำระเงินสำหรับ Order #14 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:14'),
(16, 5, 'การชำระเงินสำหรับ Order #13 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:15'),
(17, 5, 'การชำระเงินสำหรับ Order #12 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:31:16'),
(18, 5, 'การชำระเงินสำหรับ Order #30 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:32:35'),
(19, 5, 'การชำระเงินสำหรับ Order #31 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 21:42:43'),
(20, 7, 'การชำระเงินสำหรับ Order #32 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-19 22:42:37'),
(21, 8, 'การชำระเงินสำหรับ Order #41 ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง', 0, '2025-11-20 20:08:35');

-- --------------------------------------------------------

--
-- Table structure for table `orders`
--

CREATE TABLE `orders` (
  `order_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `order_date` datetime NOT NULL DEFAULT current_timestamp(),
  `total_amount` decimal(10,2) NOT NULL,
  `status` varchar(50) NOT NULL DEFAULT 'Pending Payment',
  `slip_path` varchar(255) DEFAULT NULL,
  `discount_amount` decimal(10,2) DEFAULT 0.00,
  `used_voucher_id` int(11) DEFAULT NULL,
  `vat_percentage` decimal(5,2) DEFAULT 7.00,
  `final_amount` decimal(10,2) NOT NULL,
  `address_line` varchar(255) DEFAULT NULL,
  `zip_code` varchar(10) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `orders`
--

INSERT INTO `orders` (`order_id`, `user_id`, `order_date`, `total_amount`, `status`, `slip_path`, `discount_amount`, `used_voucher_id`, `vat_percentage`, `final_amount`, `address_line`, `zip_code`) VALUES
(3, 2, '2025-11-03 00:48:42', 7500.00, 'Approved', NULL, 0.00, NULL, 7.00, 0.00, NULL, NULL),
(4, 2, '2025-11-03 22:30:27', 90000.00, 'Approved', NULL, 0.00, NULL, 7.00, 0.00, NULL, NULL),
(5, 2, '2025-11-03 22:36:33', 90000.00, 'Approved', NULL, 0.00, NULL, 7.00, 0.00, NULL, NULL),
(6, 2, '2025-11-03 23:16:08', 9000.00, 'Approved', '/Images/Slips/slip.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(8, 5, '2025-11-04 00:20:27', 124200.00, 'Approved', '/Images/Slips/slip.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(9, 2, '2025-11-05 01:02:13', 96300.00, 'Approved', '/Images/Slips/slip.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(12, 5, '2025-11-10 14:27:10', 4601.00, 'Approved', '/Images/Slips/slip_5_d0024b07-aa2a-4c36-b6eb-d785ee2272e8.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(13, 5, '2025-11-10 15:52:56', 4708.00, 'Approved', '/Images/Slips/slip_5_2de397ff-e188-4241-8d2d-7ee13278e6c8.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(14, 5, '2025-11-12 14:23:22', 9202.00, 'Approved', '/Images/Slips/slip_5_84bf8246-b3d7-44a8-b987-b585f653ba6e.png', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(19, 5, '2025-11-12 14:39:59', 9202.00, 'Approved', '/Images/Slips/slip_5_ea070158-0c05-4e91-9107-dcbf6e8b1fee.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(22, 5, '2025-11-12 15:28:40', 9202.00, 'Approved', '/Images/Slips/slip_5_4b628d34-a94e-4048-9436-465ef504e765.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(24, 5, '2025-11-14 10:19:38', 4601.00, 'Approved', '/Images/Slips/slip_5_bc529b2d-35b7-48dc-8df4-45dfdfa72e7f.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(25, 5, '2025-11-14 10:24:54', 4601.00, 'Approved', '/Images/Slips/slip_5_fa1428af-46c2-49d5-b569-b2eb7bb65059.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(26, 5, '2025-11-14 10:45:46', 4601.00, 'Approved', '/Images/Slips/slip_5_7a9bec74-b114-4e66-83f7-66ba858cf144.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(27, 5, '2025-11-14 15:18:11', 8667.00, 'Cancelled', '/Images/Slips/slip_5_16af511c-e246-4233-8729-0aa33c6b04da.jpg', 500.00, 2, 7.00, 0.00, NULL, NULL),
(28, 5, '2025-11-17 21:37:09', 8560.00, 'Approved', '/Images/Slips/slip_5_8d326af7-17d9-4c81-9113-45d49e6ddb2e.jpg', 1000.00, 3, 7.00, 0.00, NULL, NULL),
(29, 5, '2025-11-19 10:38:24', 8025.00, 'Approved', '/Images/Slips/slip_5_8f7d8e10-2d10-4c93-8a63-d7ce872242f8.jpg', 3000.00, 4, 7.00, 0.00, NULL, NULL),
(30, 5, '2025-11-19 21:32:11', 28248.00, 'Approved', '/Images/Slips/slip_5_3dfef508-9ed1-4c0a-9408-a48947b79b59.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(31, 5, '2025-11-19 21:42:24', 203407.00, 'Approved', '/Images/Slips/slip_5_e4105f37-213c-4dbf-a5ed-c58a8faa4f59.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(32, 7, '2025-11-19 22:36:43', 35122.75, 'Approved', '/Images/Slips/slip_7_871a98b1-a56f-4d6a-9f3c-18e6bc87158c.jpg', 3000.00, 4, 7.00, 0.00, NULL, NULL),
(33, 5, '2025-11-19 23:42:39', 8988.00, 'Pending Approval', '/Images/Slips/slip_5_0da67f4f-8c5a-43a7-a853-68e59623b30d.jpg', 2100.00, 7, 7.00, 0.00, NULL, NULL),
(34, 7, '2025-11-20 01:32:42', 12650.61, 'Pending Approval', '/Images/Slips/slip_7_02e79aac-157d-4ae8-81c0-a17ddc0b0e41.jpg', 5067.00, 9, 7.00, 0.00, NULL, NULL),
(35, 5, '2025-11-20 10:34:26', 4868.50, 'Pending Approval', '/Images/Slips/slip_5_f4370310-fa44-41a2-8f37-ecf5b696b222.jpg', 1950.00, 9, 7.00, 0.00, NULL, NULL),
(36, 2, '2025-11-20 10:39:12', 12650.61, 'Pending Approval', '/Images/Slips/slip_2_034e6b12-182b-4e91-8bb1-f877c0439947.jpg', 5067.00, 9, 7.00, 0.00, NULL, NULL),
(37, 7, '2025-11-20 10:57:22', 11823.00, 'Pending Approval', '/Images/Slips/slip_7_4b8afdd3-ccdb-4669-87d7-a87dbe24cde4.jpg', 5067.00, 10, 7.00, 0.00, NULL, NULL),
(38, 5, '2025-11-20 11:49:48', 18072.30, 'Pending Approval', '/Images/Slips/slip_5_8ada76fb-ddcd-40a3-a384-1d9292a823cc.jpg', 0.00, NULL, 7.00, 0.00, NULL, NULL),
(39, 2, '2025-11-20 13:46:32', 12650.61, 'Pending Approval', '/Images/Slips/slip_2_13f5eb14-c753-4d61-8388-c3451ff5eeef.jpg', 5067.00, 10, 7.00, 0.00, NULL, NULL),
(40, 5, '2025-11-20 13:49:26', 12650.61, 'Pending Approval', '/Images/Slips/slip_5_85178f95-aad1-47cc-a1ab-c9e557b19b12.jpg', 5067.00, 10, 7.00, 0.00, NULL, NULL),
(41, 8, '2025-11-20 20:06:52', 4964.80, 'Approved', '/Images/Slips/slip_8_026fb1db-c14e-4090-a4a1-ce87f121ff30.jpg', 1160.00, 7, 7.00, 0.00, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `order_items`
--

CREATE TABLE `order_items` (
  `item_id` int(11) NOT NULL,
  `order_id` int(11) NOT NULL,
  `product_variant_id` int(11) NOT NULL,
  `product_id` int(11) DEFAULT NULL,
  `quantity` int(11) NOT NULL,
  `price_at_purchase` decimal(10,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `order_items`
--

INSERT INTO `order_items` (`item_id`, `order_id`, `product_variant_id`, `product_id`, `quantity`, `price_at_purchase`) VALUES
(1, 9, 0, NULL, 20, 4500.00),
(2, 12, 3, NULL, 1, 4300.00),
(3, 13, 4, NULL, 1, 4400.00),
(4, 14, 3, NULL, 2, 4300.00),
(9, 19, 3, NULL, 2, 4300.00),
(12, 22, 3, NULL, 2, 4300.00),
(14, 24, 3, NULL, 1, 4300.00),
(15, 25, 3, NULL, 1, 4300.00),
(16, 26, 3, NULL, 1, 4300.00),
(17, 27, 3, NULL, 2, 4300.00),
(18, 28, 17, NULL, 1, 9000.00),
(19, 29, 22, NULL, 1, 10500.00),
(20, 30, 84, NULL, 10, 2640.00),
(21, 31, 105, NULL, 15, 6500.00),
(22, 31, 71, NULL, 10, 6960.00),
(23, 31, 114, NULL, 5, 4600.00),
(24, 32, 54, NULL, 1, 8500.00),
(25, 32, 101, NULL, 1, 7600.00),
(26, 32, 6, NULL, 1, 5225.00),
(27, 32, 61, NULL, 1, 2900.00),
(28, 32, 42, NULL, 1, 4600.00),
(29, 32, 32, NULL, 1, 4300.00),
(30, 32, 90, NULL, 1, 1200.00),
(31, 32, 47, NULL, 1, 1500.00),
(32, 33, 26, NULL, 1, 10500.00),
(33, 34, 65, NULL, 1, 8500.00),
(34, 34, 95, NULL, 1, 8390.00),
(35, 35, 105, NULL, 1, 6500.00),
(36, 36, 65, NULL, 1, 8500.00),
(37, 36, 95, NULL, 1, 8390.00),
(38, 37, 65, NULL, 1, 8500.00),
(39, 37, 95, NULL, 1, 8390.00),
(40, 38, 65, NULL, 1, 8500.00),
(41, 38, 95, NULL, 1, 8390.00),
(42, 39, 67, NULL, 1, 8500.00),
(43, 39, 97, NULL, 1, 8390.00),
(44, 40, 67, NULL, 1, 8500.00),
(45, 40, 98, NULL, 1, 8390.00),
(46, 41, 50, NULL, 2, 2900.00);

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `product_id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` text DEFAULT NULL,
  `price` decimal(10,2) NOT NULL,
  `image_path` varchar(255) DEFAULT NULL,
  `size_chart_path` varchar(255) DEFAULT NULL,
  `stock_quantity` int(11) DEFAULT 0,
  `brand_id` int(11) DEFAULT NULL,
  `model_id` int(11) DEFAULT NULL,
  `category_id` int(11) DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`product_id`, `name`, `description`, `price`, `image_path`, `size_chart_path`, `stock_quantity`, `brand_id`, `model_id`, `category_id`, `is_active`) VALUES
(2, 'ADIDAS ARSENAL 1992/1994 HOME RETRO JERSEY - TEAM COLLEG RED', 'สรรค์สร้างมาเพื่อคว้าชัย เสื้อฟุตบอลตัวนี้เป็นการทำใหม่แบบหนึ่งต่อหนึ่งจากเสื้อที่ อาร์เซนอลคว้าแชมป์บอลถ้วยในประเทศครั้งแรกของสโมสรและถ้วยยุโรปครั้งที่สอง โดยมีโลโก้ adidas Equipment ป้ายวินเทจตรงกลาง แถบ 3-Stripes ขนาดใหญ่พิเศษ และสปอนเซอร์ที่เหมาะสมกับยุคสมัย ผ้าเนื้อนุ่มจะช่วยให้คุณรู้สึกสบายขณะยกย่องให้กับประวัติศาสตร์\r\n\r\nรำลึกด้วยเสื้อฟุตบอลอาร์เซนอลที่สื่อถึงการคว้าแชมป์\r\n\r\nเสื้อฟุตบอลชุดเหย้า Arsenal 92-94\r\n\r\nทรงเรกูลาร์\r\nคอวี\r\nโพลีเอสเตอร์ (รีไซเคิล) 100%\r\nแต่งตราสโมสรอาร์เซนอลแบบวินเทจ', 4300.00, '/Images/Products/2df73d38-8347-4420-aa58-5692f4866957.png', '/Images/SizeCharts/7992de23-3751-4c24-9f88-30a34931af50.png', 119, 5, NULL, 2, 1),
(4, 'NIKE PHANTOM 6 LOW ELITE NU2 FG - PISTACHIO FROST/HYPER ORANGE', 'NIKE PHANTOM 6 LOW ELITE NU2 FG\r\n\r\nNike United Phantom 6 Low Elite\r\nรองเท้าฟุตบอลหุ้มต่ำ พื้นสนามหญ้า (Firm-Ground Soccer Cleats)\r\nสร้างความหวาดกลัวในสนามด้วย Nike United Phantom 6 Elite รองเท้าฟุตบอลที่ออกแบบมาสำหรับนักเตะที่เล่นเกมรุกด้วยความแม่นยำราวกับจับวาง อย่าง Sophia Wilson และ Deyna Castellanos มาพร้อมเทคโนโลยี Gripknit รุ่นปฏิวัติใหม่ เพื่อการควบคุมลูกบอลอย่างแม่นยำสูงสุด และพื้นรองเท้า Cyclone 360 ที่ออกแบบเพื่อการเปลี่ยนทิศทางที่เฉียบคมและดุดัน\r\n\r\nแรงบันดาลใจจากดีไซน์: Nike United\r\nคอลเลกชัน Nike United เฉลิมฉลองนักกีฬาผู้มีความแม่นยำราวกับเครื่องจักร อย่าง Sophia Wilson และ Deyna Castellanos โดยใช้โทนสีที่สะท้อนถึงความทะเยอทะยานและพลังอันเฉียบแหลมที่พวกเธอนำมาสู่เกม\r\n\r\nสัมผัสที่ยอดเยี่ยม (Exceptional Touch)\r\nเทคโนโลยี Nike Gripknit มอบพื้นผิวสัมผัสที่มีความเหนียว ช่วยเพิ่มการยึดเกาะเฉพาะจุดเพื่อการควบคุมลูกบอลที่แม่นยำสูงสุด ให้ประสิทธิภาพการจับบอลที่ดีเยี่ยมทั้งในสภาพเปียกและแห้ง\r\n\r\nแรงยึดเกาะในสนาม (Traction for the Field)\r\nลวดลายพื้นปุ่มแบบวงกลม Cyclone 360 ที่ออกแบบและวางตำแหน่งอย่างเหมาะสมบริเวณปลายเท้า ช่วยให้คุณหมุนตัวและเปลี่ยนทิศทางได้รวดเร็วและมั่นคง\r\n\r\nกระชับเป็นธรรมชาติ (Natural Fit)\r\nโครงรองเท้ารุ่นใหม่ให้ความรู้สึกกระชับเป็นธรรมชาติ โดยเฉพาะบริเวณปลายเท้า ช่วยให้รองเท้าปรับเข้ารูปกับเท้าได้อย่างพอดี และเพิ่มการสัมผัสลูกบอลได้อย่างเป็นธรรมชาติยิ่งขึ้น\r\n\r\nรายละเอียดเพิ่มเติม\r\n- เหมาะสำหรับพื้นสนามหญ้าแห้ง (Firm Ground)\r\n- พื้นรองเท้านุ่มสบาย (Cushioned Sockliner)', 9500.00, '/Images/Products/cf69d7ac-1fa4-4cc3-a0dd-040627a915a3.png', '/Images/SizeCharts/d78b1f7e-5985-4651-985d-af7e3606f680.png', 100, 1, 2, 1, 1),
(5, 'MIZUNO MORELIA NEO IV JAPAN FG - WHITE/NEON GREEN/COOL GRAY 3C', 'MIZUNO MORELIA NEO IV JAPAN\r\n\r\nBF KNIT & LEATHER NEO\r\nอัพเกรดลายถักกับ BF KNIT NEO น้ำหนักเบาพิเศษ พร้อมทั้งเสริมการยึดเกาะด้วยโครงแบบใหม่ และยังคงความพรีเมี่ยมของ K-LEATHER ด้วย BF LEATHER NEO ที่มีน้ำหนักลดลงถึง 5 กรัม เพื่อประสบการณ์การสวมใส่ที่เปรียบเสมือนเท้าเปล่าได้ดียิ่งขึ้น\r\n \r\nWAVE FIT SHOE LACING SYSTEM NEO\r\nการจัดเรียงรูร้อยเชือกแบบใหม่ พร้อมทั้งเพิ่มจำนวนรูร้อยเชือกให้ถี่ขึ้น เพื่อความกระชับของรูปเท้า รวมถึงการช่วยลดรอยพับและรอยย่น\r\n \r\nSUEDE SOCKLINER\r\nแผ่นรองพื้นผิวหนังกลับ ที่ช่วยเหลือในการยึดเกาะให้ดียิ่งขึ้น แถมยังดูดซับเหงื่อได้ดีกว่าวัสดุทั่วไป\r\n \r\nY SHANK OUTSOLE\r\nปรับปรุงจากตัว NEO III ในรูปแบบ \"I SHANK\" เป็น SHAPE ใหม่ ที่ช่วยเพิ่มความมั่นคงมากขึ้นเมื่อใส่ลงสนาม ในรูปแบบของ \"Y SHANK\"', 5225.00, '/Images/Products/d68b5086-14ea-43dd-9d44-2a0651d2276f.png', '/Images/SizeCharts/46a34392-c8c4-436e-b4cb-3bf0cc529390.png', 99, 3, 6, 1, 1),
(6, 'NIKE TIEMPO LEGEND 10 ELITE NU2 FG - VAST GREY/RACER BLUE', 'NIKE TIEMPO LEGEND 10 ELITE NU2 FG\r\n\r\nNike United Tiempo Legend 10 Elite\r\nรองเท้าฟุตบอลหุ้มต่ำพื้นสนามหญ้า (Firm-Ground Low-Top Soccer Cleats)\r\nเพียงแค่หนึ่งสัมผัสจาก Naomi Girma หรือ Patricia Guijarro ก็สามารถทำให้คู่แข่งหมดหนทางรับมือได้ รองเท้ารุ่น Nike United Legend 10 ถูกออกแบบมาเพื่อการควบคุมลูกบอลอย่างเฉียบคม ด้วยวัสดุ FlyTouch Plus engineered leather หนังเทียมคุณภาพสูงที่ปรับเข้ากับรูปเท้าอย่างเป็นธรรมชาติ ให้คุณเล่นได้อย่างมั่นใจทุกตำแหน่งในสนาม\r\n\r\nแรงบันดาลใจจากดีไซน์: Nike United\r\nคอลเลกชัน Nike United เฉลิมฉลองความสามารถในการควบคุมเกมอย่างเหนือชั้นของนักเตะอย่าง Naomi Girma และ Patricia Guijarro โดยใช้โทนสีที่สะท้อนพลัง ความทะเยอทะยาน และความมั่นใจที่แผ่รัศมีออกมาราวกับเวทมนตร์\r\n\r\nสัมผัสที่แม่นยำยิ่งขึ้น (Amplified Touch)\r\nจุดไมโครดอต (microdots) บนส่วนบนของรองเท้าช่วยขยายโซนสัมผัสสำหรับการยิง การเลี้ยง และการจ่ายบอล โฟมภายในถูกปรับให้บางลงจากรุ่น Tiempo 9 เพื่อให้เท้าอยู่ใกล้กับลูกบอลมากขึ้น พร้อมเทคโนโลยี All Conditions Control (ACC) ที่ช่วยเพิ่มการยึดเกาะทั้งในสภาพสนามเปียกและแห้ง\r\n\r\nกระชับสบายตามธรรมชาติ (Natural, Conforming Fit)\r\nหนังเทียม FlyTouch Plus นุ่มเป็นพิเศษ ปรับเข้ารูปเท้าได้ดีโดยไม่ยืดเสียรูป ขอบข้อเท้าแบบ Flyknit ให้ความกระชับและมั่นคง ช่วยให้คุณเคลื่อนไหวได้อย่างมั่นใจตลอดเกม\r\n\r\nรายละเอียดเพิ่มเติม\r\n- เหมาะสำหรับพื้นสนามหญ้าแห้ง (Firm Ground)\r\n- พื้นรองเท้านุ่มสบาย (Cushioned Sockliner)', 8300.00, '/Images/Products/dcf3094c-dc87-46c6-a80c-4bd543f8cd06.png', '/Images/SizeCharts/79211edd-8829-44c4-b621-f27c4e62a0cc.png', 100, 1, 3, 1, 1),
(7, 'NIKE MERCURIAL VAPOR 16 ELITE NU2 FG - SILT RED/RACER BLUE', 'NIKE MERCURIAL VAPOR 16 ELITE NU2 FG\r\n\r\nNike United Mercurial Vapor 16 Elite\r\nรองเท้าฟุตบอลหุ้มต่ำพื้นสนามหญ้า (Firm-Ground Low-Top Soccer Cleats)\r\nความเร็วอาจน่ากลัว แต่กับ Nike United Vapor 16 นี่คือฝันร้ายของคู่แข่งโดยแท้ รองเท้ารุ่นนี้ถูกออกแบบมาเพื่อมอบพลังเร่งความเร็วให้กับคุณและผู้เล่นอย่าง Lauren James และ Salma Paralluelo ด้วยเทคโนโลยี Air Zoom แบบ 3/4 ความยาวเต็มฝ่าเท้า และพื้นปุ่มลวดลายคลื่นที่ให้แรงยึดเกาะเฉียบคม\r\n\r\nแรงบันดาลใจจากดีไซน์: Nike United\r\nคอลเลกชัน Nike United เฉลิมฉลองให้กับนักเตะที่มีความเร็วเหนือชั้นอย่าง Lauren James และ Salma Paralluelo โดยใช้เฉดสีพิเศษที่สะท้อนถึงพลัง ความทะเยอทะยาน และความเฉียบคมที่พวกเขาแสดงออกในสนาม\r\n\r\nแรงยึดเกาะที่ตอบสนองเร็ว (Fast Traction)\r\nเทคโนโลยี Air Zoom แบบ 3/4 ความยาว ถูกฝังไว้ในแผ่นพื้นรองเท้า ผสานกับปุ่มลวดลายคลื่นแบบ Chevron ที่ช่วยให้ยึดพื้นได้อย่างมั่นใจในทุกจังหวะการเร่งสปีด\r\n\r\nสัมผัสแบบเท้าเปล่า (Barefoot Touch)\r\nวัสดุ Nike Gripknit เป็นเนื้อผ้าชนิดพิเศษที่มอบการสัมผัสลูกบอลได้อย่างยอดเยี่ยม พื้นผิวแบบไมโครโมลด์ช่วยให้รองเท้าปรับเข้ากับรูปเท้าได้พอดี และให้แรงยึดเกาะเท่ากันทั้งในสภาพเปียกและแห้ง เพื่อความกระชับแบบรองเท้าสปรินต์\r\n\r\nฟิตกระชับและมั่นใจ (Snug Fit)\r\nส่วนบนของรองเท้าใช้เทคโนโลยี Flyknit ที่ออกแบบมาสำหรับผู้เล่นที่ต้องการความเร็วสูง มอบความกระชับแบบถุงเท้า และผสานกับ Gripknit เพื่อสร้างส่วนบนของ Mercurial ที่บางที่สุดเท่าที่เคยมีมา ทำให้ผู้เล่นสัมผัสบอลได้ใกล้ชิดยิ่งขึ้นและใช้เวลาปรับตัวน้อยลง\r\n\r\nรายละเอียดเพิ่มเติม\r\n\r\n- เหมาะสำหรับพื้นสนามหญ้าแห้งธรรมชาติ\r\n- พื้นรองเท้าด้านในนุ่มสบาย (Cushioned Sockliner)', 9000.00, '/Images/Products/c03bd6bd-6caf-4e89-b1e1-d12a5db3fa27.png', '/Images/SizeCharts/b89b59a9-8b68-47d7-b9a1-5509b51dc055.png', 0, 1, NULL, 1, 1),
(8, 'ADIDAS PREDATOR ELITE FT FG - SIGNAL CORAL/FTWR WHITE/BEAM ORANG', 'ADIDAS PREDATOR ELITE FT FG\r\n\r\nค้นพบความแตกต่างระหว่างการมุ่งทำประตูและการรู้ว่าจะทำประตูได้ด้วยรองเท้าฟุตบอล adidas Predator ที่ได้รับการออกแบบมาเพื่อการทำประตูโดยเฉพาะ รองเท้าฟุตบอล Elite คู่นี้มีอัปเปอร์ HybridTouch พร้อมลิ้นรองเท้าพับได้เสริมด้วยครีบยาง Strikeskin ที่ช่วยให้สัมผัสกับบอลได้อย่างสมบูรณ์แบบ ชุดพื้นชั้นล่าง Controlframe 2.0 สำหรับพื้นสนามหญ้าและคอรองเท้า adidas PRIMEKNIT ออกแบบมาให้คุณยืนได้อย่างมั่นคงในจังหวะยิงประตู\r\n\r\nรองเท้าฟุตบอลสำหรับยิงประตูบนพื้นสนามหญ้าพร้อมลิ้นรองเท้าพับได้\r\n\r\nรองเท้าฟุตบอล Predator Elite สำหรับพื้นสนามหญ้าพร้อมลิ้นรองเท้าพับได้\r\n\r\n- ทรงเรกูลาร์\r\n- มีเชือกผูกรองเท้า\r\n- อัปเปอร์ HybridTouch เสริมครีบ Strikeskin\r\n- คอรองเท้า adidas PRIMEKNIT\r\n- ลิ้นรองเท้าพับได้\r\n- ชุดพื้นชั้นล่าง Controlframe 2.0 สำหรับพื้นสนามหญ้า', 10500.00, '/Images/Products/407fd7d9-d3fd-4a17-9544-967859e5fd9e.png', '/Images/SizeCharts/cae4f739-d716-4852-9ec9-677f49d66810.png', 98, 5, 4, 1, 1),
(9, 'ADIDAS F50 ELITE LACELESS FG - BEAM ORANGE/LUCID BLUE/FTWR WHITE', 'ADIDAS F50 ELITE LACELESS FG\r\n\r\nค้นหาความเร็วเพื่อแสดงตัวตนของคุณอย่างแท้จริงในสนาม สัมผัสความเร้าใจในรองเท้าฟุตบอล adidas F50 ที่ออกแบบมาเพื่อความเร็ว รองเท้าฟุตบอล Elite คู่นี้มีอัปเปอร์ Fibertouch ที่บาง พร้อมขอบล็อคข้อเท้าผ้า adidas Primeknit เพื่อการล็อคพอดีเท้า และมีผิวสัมผัสเทคโนโลยี Sprintweb 3D เพื่อช่วยให้ลูกฟุตบอลหนึบเท้า ออกแบบมาสำหรับสนามหญ้าแห้ง พื้นชั้นล่างเทคโนโลยี Sprintframe 360 ที่มีความยืดหยุ่นช่วยรองรับความเร็วที่ต่อเนื่อง\r\n\r\nรองเท้าฟุตบอลแบบไร้เชือกสำหรับพื้นสนามหญ้าเพื่อการเร่งความเร็วหลายครั้งต่อเนื่อง\r\n\r\nรองเท้าฟุตบอล F50 Elite แบบไร้เชือกสำหรับพื้นสนามหญ้า\r\n\r\n- ทรงเรกูลาร์\r\n- โครงสร้างไร้เชือก\r\n- อัปเปอร์ Fibertouch ผิวสัมผัสเทคโนโลยี Sprintweb 3D\r\n- คอรองเท้า adidas PRIMEKNIT\r\n- พื้นชั้นล่างเทคโนโลยี Sprintframe 360 สําหรับพื้นสนามหญ้า', 9500.00, '/Images/Products/fc171890-1252-46c1-b80d-9ef5607eb462.png', '/Images/SizeCharts/07bd94c4-e76a-417e-8a99-7d00b47652da.png', 100, 5, 5, 1, 1),
(10, 'ADIDAS LIVERPOOL 2025/2026 HOME PLAYER JERSEY - STRAWBERRY RED', 'ADIDAS LIVERPOOL 2025/2026 HOME PLAYER JERSEY\r\n\r\nยินดีต้อนรับกลับบ้าน หงส์แดง Liverpool FC และ adidas เป็นทีมที่สมบูรณ์แบบเสมอมา เสื้อแข่งเหย้าของแท้ตัวนี้เป็นการเริ่มต้นช่วงเวลาแห่งการร่วมงานกันครั้งที่สาม มอบบรรยากาศฟุตบอลยุคกลางปี 2000 อย่างแท้จริง ด้วยรูปลักษณ์ที่สะอาดตา คมชัด ตกแต่งด้วยเส้นสีขาว ออกแบบมาเพื่อสร้างความประทับใจให้กับแฟนๆ ที่สนามแอนฟิลด์ นี่คือเสื้อที่นักเตะสวมใส่ในวันแข่งขัน ตราสัญลักษณ์ Liver Bird ประดับอย่างสง่างามบนหน้าอกเสื้อ นำพาความหวังมาสู่หัวใจของแฟนบอลทุกคน\r\n\r\nเสื้อเจอร์ซีย์ประสิทธิภาพสูงที่สืบทอดประวัติศาสตร์อันยาวนานของสโมสรฟุตบอลลิเวอร์พูลกับอาดิดาส\r\n\r\nเสื้อเจอร์ซีย์ทีมเหย้าแท้ 25/26 ของสโมสรลิเวอร์พูล\r\n\r\n- ทรงเข้ารูป\r\n- คอกลม\r\n- โพลีเอสเตอร์ 100% (รีไซเคิล)\r\n- ตราสัญลักษณ์สโมสรลิเวอร์พูลติดด้วยความร้อน', 4600.00, '/Images/Products/e905365c-abda-46f0-83c9-c2af4c55c369.png', '/Images/SizeCharts/9694093a-c755-4aae-8f01-23f5d97ea5f5.png', 119, 5, NULL, 2, 1),
(11, 'ADIDAS TRIONDA LEAGUE STREET BALL - WHITE BLUE/POWER RED/AMAZON GREEN', '', 1500.00, '/Images/Products/afd76515-0dd6-41bd-a185-2ccd44855dc0.png', '/Images/SizeCharts/ca685f3b-9516-4cca-adf9-4fb46aabc78c.png', 19, 5, NULL, 3, 1),
(12, 'ADIDAS LIVERPOOL 2025/2026 THIRD REPLICA JERSEY - SEA GREEN/WHITE', 'ADIDAS LIVERPOOL 2025/2026 THIRD REPLICA JERSEY\r\n\r\nเสื้อฟุตบอลคลาสสิกแห่งยุคใหม่ที่ได้รับแรงบันดาลใจจากประวัติศาสตร์แถบ 3-Stripes ของสโมสรฟุตบอลลิเวอร์พูล แถบแนวตั้งโทนสีเดียวกันบนเสื้อฟุตบอลชุดที่สามที่สง่างามตัวนี้มาในสีเขียว adidas Equipment อันเป็นเอกลักษณ์ และยังเป็นครั้งแรกนับตั้งแต่ปี 1991 ที่เราประดับตราสโมสรดีไซน์ย้อนยุคบนหน้าอกร่วมกับโลโก้ Trefoil อย่างโดดเด่นไม่แพ้กัน เสื้อตัวนี้ออกแบบมาเพื่อการเดินทางสำหรับเด็กหงส์ โดยมาพร้อมเทคโนโลยีจัดการกับความชื้น AEROREADY จึงให้ความรู้สึกดีไม่แพ้ลุคที่เห็นจากภายนอก\r\n\r\nเสื้อฟุตบอลที่ผสมผสาน DNA ของสโมสรฟุตบอลลิเวอร์พูลและอาดิดาสเข้าด้วยกัน รังสรรค์มาเพื่อความสบายของแฟนบอล\r\n\r\nเสื้อฟุตบอลชุดที่สาม Liverpool FC 25/26\r\n\r\n- ทรงสลิม\r\n- คอกลม\r\n- โพลีเอสเตอร์ (รีไซเคิล) 100%\r\n- เทคโนโลยี AEROREADY\r\n- แต่งตราแบบทอ Liverpool FC', 2900.00, '/Images/Products/1f6ace00-5d3e-4d74-9f33-845ebf409b78.png', '/Images/SizeCharts/1403c1d0-91ae-4714-9c68-a15eb9e805b2.png', 118, 5, NULL, 2, 1),
(13, 'ADIDAS F50 ELITE FG - BEAM ORANGE/LUCID BLUE/FTWR WHITE', 'ADIDAS F50 ELITE FG\r\n\r\nค้นหาความเร็วเพื่อแสดงตัวตนของคุณอย่างแท้จริงในสนาม สัมผัสความเร้าใจในรองเท้าฟุตบอล adidas F50 ที่ออกแบบมาเพื่อความเร็ว รองเท้าฟุตบอล Elite คู่นี้มีอัปเปอร์ Fibertouch ที่บาง พร้อมลิ้นรองเท้าทรงรัดกระชับเพื่อการล็อคพอดีเท้า และมีพื้นผิวเทคโนโลยี Sprintweb 3D เพื่อช่วยให้ลูกฟุตบอลอยู่ใกล้เท้าขณะที่คุณกำลังวิ่งเร็ว ออกแบบมาสำหรับสนามหญ้าแห้ง พื้นชั้นล่างเทคโนโลยี Sprintframe 360 ที่มีความยืดหยุ่นช่วยรองรับความเร็วที่ต่อเนื่อง\r\n\r\nรองเท้าฟุตบอลน้ำหนักเบาสำหรับพื้นสนามหญ้า เพื่อการเร่งความเร็วได้อย่างรวดเร็ว\r\n\r\nรองเท้าฟุตบอล F50 Elite สำหรับพื้นสนามหญ้า\r\n\r\n- ทรงเรกูลาร์\r\n- มีเชือกผูกรองเท้า\r\n- อัปเปอร์ Fibertouch ผิวสัมผัสเทคโนโลยี Sprintweb 3D\r\n- ด้านในรองเท้าบุด้วยวัสดุสังเคราะห์\r\n- พื้นชั้นล่างเทคโนโลยี Sprintframe 360 สําหรับพื้นสนามหญ้า', 8500.00, '/Images/Products/36a22b21-f1bb-4706-8b56-27e6aceeac38.png', '/Images/SizeCharts/6fe3dde2-0645-415f-b055-bfb300ac618b.png', 99, 5, 5, 1, 1),
(14, 'ADIDAS MAN UTD 2025/2026 HOME REPLICA JERSEY - MUFC RED', 'ADIDAS MANCHESTER UNITED 2025/2026 HOME REPLICA JERSEY\r\n\r\nมีเวทีกีฬาเพียงไม่กี่แห่งที่สามารถเทียบได้กับความตื่นเต้นที่สนามเหย้าสุดไอคอนิกของแมนเชสเตอร์ยูไนเต็ดเคยสร้างขึ้นมาตลอดหลายปีที่ผ่านมา เสื้อฟุตบอลอาดิดาสตัวนี้จึงเป็นการเชิดชูสนามกีฬาที่ทีมจะลงเล่นในฤดูกาล 25/26 โดยมีข้อความว่า \"Theatre of Dreams\" และกราฟิกแอบสแตรกที่ได้แรงบันดาลใจจากสนามโอลด์แทรฟฟอร์ดบนแขนเสื้อ ออกแบบมาเพื่อแฟนฟุตบอลโดยเฉพาะ นอกจากนี้ยังมีเทคโนโลยี AEROREADY ที่จัดการความชื้นและตราสโมสรแบบทอ\r\n\r\nเสื้อฟุตบอลชุดเหย้าแมนฯ ยูไนเต็ดสุดคลาสสิกตัวนี้ สร้างขึ้นเพื่อความสบาย และยังมีกลิ่นอายของสนามโอลด์แทรฟฟอร์ดเล็กน้อยด้วย\r\n\r\nเสื้อฟุตบอลชุดเหย้า Manchester United 25/26\r\n\r\n- ทรงสลิม\r\n- คอวี\r\n- โพลีเอสเตอร์ (รีไซเคิล) 100%\r\n- เทคโนโลยี AEROREADY\r\n- ทอตราสโมสรแมนเชสเตอร์ยูไนเต็ด', 2900.00, '/Images/Products/7941c176-8079-4c24-bae7-d6e7f311b115.png', '/Images/SizeCharts/e51f1dc7-7f49-451b-87b1-15352a24b860.png', 119, 5, NULL, 2, 1),
(15, 'ADIDAS PREDATOR ELITE FG - SIGNAL CORAL/FTWR WHITE/BEAM ORANGE', 'ค้นพบความแตกต่างระหว่างการมุ่งทำประตูและการรู้ว่าจะทำประตูได้ด้วยรองเท้าฟุตบอล adidas Predator ที่ได้รับการออกแบบมาเพื่อการทำประตูโดยเฉพาะ รองเท้าฟุตบอล Elite คู่นี้มีอัปเปอร์เทคโนโลยี Hybridtouch พร้อมฟินยาง Strikeskin ที่จัดวางเฉพาะจุดเพื่อให้สัมผัสกับลูกฟุตบอลได้อย่างสมบูรณ์แบบ ชุดพื้นชั้นล่าง Controlframe 2.0 สำหรับพื้นสนามหญ้าและคอรองเท้า adidas PRIMEKNIT ออกแบบมาให้คุณยืนได้อย่างมั่นคงในจังหวะยิงประตู\r\n\r\nรองเท้าฟุตบอลที่รองรับได้ดีเพื่อการทำประตูที่แม่นยำบนพื้นสนามหญ้า\r\n\r\nรองเท้าฟุตบอล Predator Elite สำหรับพื้นสนามหญ้า\r\n\r\n- ทรงเรกูลาร์\r\n- มีเชือกผูกรองเท้า\r\n- อัปเปอร์ HybridTouch เสริมครีบ Strikeskin\r\n- คอรองเท้า adidas PRIMEKNIT\r\n- พื้นชั้นล่าง Controlframe 2.0 สำหรับพื้นสนามหญ้า', 8500.00, '/Images/Products/95a62007-c98a-4d46-8066-f1bb5e496d23.png', '/Images/SizeCharts/36fa433d-aa1a-443d-ac25-add9a8ebfd63.png', 94, 5, 4, 1, 1),
(16, 'NIKE MERCURIAL VAPOR 16 ELITE FG - MAGIC FLAMINGO/BLACK/TOTAL CRIMSON', 'NIKE MERCURIAL VAPOR 16 ELITE FG\r\nNike Mercurial Vapor 16 Elite\r\nรองเท้าฟุตบอลพื้น FG ทรง Low-Top หลงใหลในความเร็วใช่ไหม? นักเตะระดับโลกก็เช่นกัน\r\nนั่นคือเหตุผลที่เราสร้างสตั๊ดรุ่น Elite คู่นี้พร้อม Air Zoom ความยาว 3/4 รุ่นปรับปรุงใหม่\r\nช่วยส่งแรงกระแทกกลับให้คุณพุ่งทะลุแนวรับได้อย่างทรงพลัง\r\nนี่คือ Mercurial ที่ตอบสนองเร็วที่สุดเท่าที่เราเคยสร้างมา เพราะคุณต้องการความสุดยอดทั้งจากตัวเองและรองเท้าที่สวมใส่\r\n\r\nยึดเกาะเพื่อความเร็วสูงสุด\r\nลวดลายดอกยางแบบคลื่นทำงานร่วมกับหน่วย Air Zoom เพื่อปลดปล่อยความเร็ว\r\nกินพื้นที่มากกว่าในรุ่น Vapor 15 พร้อมมอบแรงยึดเกาะที่เหมาะสม\r\nผสานกับปุ่มแบบเชฟรอนและใบมีด เพื่อให้คุณหยุดได้ทันเมื่อเปลี่ยนทิศทางอย่างรวดเร็ว\r\n\r\nสัมผัสบอลยอดเยี่ยม\r\nNike Gripknit วัสดุแบบเหนียวช่วยเพิ่มการควบคุมบอลแม้ขณะเลี้ยงด้วยความเร็วสูง\r\nให้การยึดเกาะเท่ากันทั้งในสภาพเปียกและแห้ง\r\n\r\nกระชับพอดีราวกับถุงเท้า\r\nอัปเปอร์ Flyknit ที่ออกแบบมาสำหรับผู้เล่นที่เร็วที่สุด มอบความกระชับแบบถุงเท้า\r\nทำงานร่วมกับ Gripknit เพื่อให้ได้อัปเปอร์ Mercurial ที่บางที่สุดเท่าที่เคยมีมา\r\nช่วยให้คุณสัมผัสบอลได้ใกล้ชิดขึ้น และลดระยะเวลาการเบรกอิน\r\n\r\nรายละเอียดเพิ่มเติม\r\n- เหมาะสำหรับพื้นสนามหญ้าจริงแบบแห้ง\r\n- พื้นรองเท้าด้านในบุรองนุ่มสบาย', 6960.00, '/Images/Products/bf9bb3ca-6a21-4721-8e4d-a3bd61c7c999.png', '/Images/SizeCharts/ac92a103-96dc-4547-ab51-c6ba63824dbe.png', 90, 1, NULL, 1, 1),
(17, 'NIKE MERCURIAL VAPOR 16 PRO FG - MAGIC FLAMINGO/BLACK/TOTAL CRIMSON', 'NIKE MERCURIAL VAPOR 16 PRO FG\r\n\r\nรองเท้าฟุตบอลพื้น Firm Ground ทรงโลว์คัท\r\nจริงจังกับความเร็วของคุณใช่ไหม? รองเท้าระดับ Pro คู่นี้ออกแบบมาสำหรับผู้เล่นที่ต้องการเร่งสปีดตลอดทั้งเกม เราได้ติดตั้งหน่วย Air Zoom ที่ได้รับการปรับปรุงใหม่ เพื่อมอบแรงส่งที่ช่วยให้คุณทะลวงแนวรับได้อย่างมั่นใจ ยกระดับทักษะของคุณด้วยนวัตกรรมชั้นนำของ Nike อย่าง Flyknit บนส่วนบนที่ช่วยลดน้ำหนักรองเท้า ทำให้คุณเคลื่อนไหวได้เร็วขึ้น\r\n\r\nสัมผัสแม่นยำยิ่งขึ้น\r\nผิวสัมผัสแบบหนึบ (tacky skin finish) ถูกออกแบบมาเพื่อการยิงประตูและควบคุมบอลได้ดี แม้ขณะเลี้ยงด้วยความเร็วสูง\r\n\r\nยึดเกาะไวทันใจ\r\nลายปุ่มแบบคลื่น (wave-like traction) คือชุดของปุ่มเรียงซ้อนที่ช่วยให้หน่วย Air Zoom สัมผัสพื้นผิวมากขึ้น พร้อมให้การยึดเกาะอย่างพอดี ปุ่มหลักมีความสูงเท่ากับปุ่มตรงกลางแบบดั้งเดิม เพื่อรักษาประสิทธิภาพการยึดเกาะ และเสริมด้วยปุ่มบั้งและปุ่มใบมีดรุ่นใหม่ เพื่อช่วยให้คุณหยุดหรือเปลี่ยนทิศได้อย่างรวดเร็ว\r\n\r\nกระชับ พร้อมสำหรับความเร็ว\r\nครั้งแรกในตระกูล Mercurial ที่พัฒนา Flyknit แบบเต็มชิ้นทั่วทั้งรองเท้า ออกแบบเฉพาะสำหรับความต้องการของเกมความเร็วสูง Flyknit บริเวณด้านข้างมีน้ำหนักเบาแต่แข็งแรง ช่วยรองรับการเคลื่อนไหวและทำให้คุณรู้สึกใกล้ชิดกับลูกบอลมากขึ้น\r\n\r\nรายละเอียดเพิ่มเติม\r\n- เหมาะสำหรับสนามหญ้าจริงที่สั้นและเปียกเล็กน้อย\r\n- แผ่นรองเท้าด้านในเสริมเบาะเพื่อความนุ่มสบาย', 4320.00, '/Images/Products/8fd373a7-9e09-442c-a479-56599bed24d2.png', '/Images/SizeCharts/29813f9a-10af-41e3-9e53-31f6cebed7d2.png', 100, 1, 1, 1, 1),
(18, 'NIKE MERCURIAL VAPOR 16 ACADEMY FG/MG - MAGIC FLAMINGO/BLACK/TOTAL CRIMSON', 'NIKE MERCURIAL VAPOR 16 ACADEMY FG/MG\r\n\r\nNike Mercurial Vapor 16 Academy\r\nรองเท้าฟุตบอลพื้น MG ทรงโลว์คัท\r\n\r\nอยากเพิ่มระดับความเร็วของคุณใช่ไหม? รองเท้ารุ่น Academy คู่นี้มาพร้อมหน่วย Air Zoom ที่ส้นเท้าแบบใหม่ ช่วยเพิ่มแรงส่งให้คุณเร่งทะลุแนวรับได้อย่างมั่นใจ ผลลัพธ์คือ Mercurial ที่ตอบสนองได้ดีที่สุดเท่าที่เคยมีมา เพื่อให้คุณควบคุมจังหวะและความเร็วของเกมได้ตลอดทั้งแมตช์\r\n\r\nสัมผัสแม่นยำยิ่งขึ้น\r\nส่วนบนของรองเท้าทำจากวัสดุ NikeSkin พร้อมลวดลายบั้ง (chevron) ฝังในเนื้อวัสดุ ช่วยให้ควบคุมบอลได้ดีขึ้น และให้ความรู้สึกเหมือนเล่นฟุตบอลเท้าเปล่า\r\n\r\nยึดเกาะเร็วทันใจ\r\nลายปุ่มแบบคลื่น (wave-like traction) ประกอบด้วยปุ่มเรียงซ้อนหลายชั้น ช่วยเพิ่มพื้นที่สัมผัสกับหน่วย Air Zoom และให้การยึดเกาะที่เหมาะสม โดยปุ่มหลักยังคงความสูงเท่ากับปุ่มมาตรฐานเพื่อไม่ให้สูญเสียประสิทธิภาพ พร้อมผสมผสานกับปุ่มทรงบั้งและปุ่มใบมีดดีไซน์ใหม่ เพื่อช่วยให้คุณหยุดหรือเปลี่ยนทิศได้อย่างฉับไว\r\n\r\nกระชับพอดีเท้า\r\nเราได้อัปเกรดตาข่ายยืดจากรุ่นก่อน เป็นวัสดุถักที่ยืดหยุ่น ปรับตัวได้ดี รองรับการเคลื่อนไหว และช่วยให้คุณรู้สึกใกล้ชิดกับลูกบอลมากขึ้น\r\n\r\nรายละเอียดเพิ่มเติม\r\n- เหมาะสำหรับสนามหญ้าจริงและหญ้าเทียม\r\n- แผ่นรองเท้าด้านในเสริมเบาะเพื่อความนุ่มสบาย', 2640.00, '/Images/Products/731c33b4-a804-45dd-bcdc-53bf5f23494f.png', '/Images/SizeCharts/b4c8e706-5d93-4838-8785-868f3df17753.png', 90, 1, 1, 1, 1),
(19, 'NIKE PHANTOM 6 LOW ACADEMY NU2 FG/MG - PISTACHIO FROST/HYPER ORANGE', 'NIKE PHANTOM 6 LOW ACADEMY NU2 FG/MG\r\n\r\nNike United Phantom 6 Low Academy\r\nรองเท้าฟุตบอลหุ้มต่ำ พื้นสนามหญ้าและสนามสังเคราะห์ (Multi-Ground Soccer Cleats)\r\nสร้างความหวาดกลัวในสนามด้วย Nike United Phantom 6 รองเท้าฟุตบอลที่ออกแบบมาสำหรับนักเตะผู้จู่โจมด้วยความแม่นยำสูงอย่าง Sophia Wilson และ Deyna Castellanos รุ่นนี้ช่วยเพิ่มความเฉียบคมในการควบคุมลูกบอลด้วยพื้นที่สัมผัส NikeSkin ที่ได้รับการออกแบบใหม่เพื่อการยิงที่แม่นยำ และพื้นรองเท้า Cyclone 360 ที่ให้การเคลื่อนไหวรวดเร็วและมั่นคงในทุกจังหวะ\r\n\r\nแรงบันดาลใจจากดีไซน์: Nike United\r\nคอลเลกชัน Nike United เฉลิมฉลองนักกีฬาผู้มีความแม่นยำระดับสูงอย่าง Sophia Wilson และ Deyna Castellanos โดยใช้โทนสีที่สื่อถึงความมุ่งมั่นและพลังแห่งความเฉียบคมที่พวกเธอนำมาสู่เกม\r\n\r\nสัมผัสที่ดียิ่งขึ้น (Amplified Touch)\r\nพื้นที่สัมผัส NikeSkin ที่ขยายกว้างขึ้นและเสริมด้วยผ้าตาข่ายวิศวกรรมพิเศษ (Engineered Mesh) ช่วยให้เท้าเข้าใกล้ลูกบอลมากขึ้น มอบการควบคุมที่ดียิ่งขึ้นในการเลี้ยง ส่ง และยิง ไม่ว่าสภาพสนามจะเปียกหรือแห้ง\r\n\r\nแรงยึดเกาะในสนาม (Traction for the Field)\r\nลวดลายพื้นปุ่มแบบวงกลม Cyclone 360 ที่วางตำแหน่งเฉพาะในบริเวณปลายเท้า ช่วยให้คุณหมุนตัวและเปลี่ยนทิศทางได้อย่างรวดเร็วและมั่นคง\r\n\r\nกระชับตามธรรมชาติ (Natural Fit)\r\nโครงรองเท้ารุ่นใหม่ให้ความกระชับและพอดีมากขึ้น โดยเฉพาะบริเวณปลายเท้า ช่วยให้รองเท้าปรับเข้ารูปกับเท้าได้อย่างเป็นธรรมชาติ และเพิ่มความใกล้ชิดระหว่างเท้ากับลูกบอล เพื่อการควบคุมที่แม่นยำกว่าเดิม\r\n\r\nรายละเอียดเพิ่มเติม\r\n- เหมาะสำหรับพื้นสนามหญ้าธรรมชาติและพื้นสังเคราะห์ (Multi-Ground)\r\n- พื้นรองเท้านุ่มสบาย (Cushioned Sockliner)', 3600.00, '/Images/Products/44d787ff-928a-476a-a9fa-b182e01d7a5c.png', '/Images/SizeCharts/58f35f01-cdc3-4d22-80a3-8b01e166cf04.png', 100, 1, 2, 1, 1),
(20, 'NIKE GOALKEEPER MATCH GLOVES - BLACK/WHITE/WHITE', 'NIKE GOALKEEPER MATCH GLOVES\r\n\r\nNike Match\r\nGoalkeeper Soccer Gloves\r\nFearlessly make every save with foam-padded palms to help absorb impact of the hardest shots. Its smooth surface helps you grip the ball better and your hands can stay cool thanks to mesh panels that help with airflow.\r\n \r\n More Details \r\n \r\nAdjustable wrist strap\r\n40% polyester/33% latex/21% EVA/6% nylon\r\nHand wash', 1200.00, '/Images/Products/764be513-6569-4caa-bf89-751443ccc599.png', '/Images/SizeCharts/680e712d-3902-47b1-b623-97e5f931bcf3.png', 19, 1, NULL, 3, 1),
(21, 'MIZUNO ALPHA II JAPAN LTD FG - DNA BLACK', 'MIZUNO ALPHA II JAPAN LTD FG\r\n\r\nAn innovative model that combines the fitting of MIZUNO ALPHA II with the barefoot outsole of the MORELIA NEO IV β.\r\n\r\nMizuno Launches the Innovation Pack – 10th Edition of the Rebuild Project\r\n\r\nThe Japanese football brand Mizuno proudly announces the release of the Innovation Pack, a special collection of five football shoes that marks the 10th edition of Mizuno’s ongoing Rebuild Project.\r\nEach model in the Innovation Pack is based on an existing inline silhouette, but reimagined with unique specifications and material updates. From experimenting with premium leathers like cow leather to exploring new combinations of fit and feel, Mizuno continues to push the limits of what’s possible in football footwear.\r\nThe Rebuild Project was born from Mizuno’s desire to fuse its time-honored craftsmanship—refined over more than a century—with the brand’s latest technological innovations. It represents the spirit of constant evolution and fearless experimentation that drives Mizuno’s football design philosophy.\r\nThe Innovation Pack is not only a tribute to the legacy of Mizuno’s craftsmanship, but also a bold step into the future—challenging conventions and redefining performance on the pitch.', 8390.00, '/Images/Products/81b29db0-a2ac-4cb2-8f8d-ad354f1eaf4b.png', '/Images/SizeCharts/242748e9-40af-438a-b75c-0e436d7c868c.png', 94, 3, 7, 1, 1),
(22, 'MIZUNO ALPHA II JAPAN FG - MORELIA 40TH RED/BLACK/GOLD', 'MIZUNO ALPHA II JAPAN FG\r\n\r\nสัมผัสความเร็วที่ดีที่สุดด้วยรองเท้าฟุตบอลที่ได้รับการออกแบบและผลิตในญี่ปุ่นอย่างพิถีพิถันเพื่อเพิ่มความเร็วด้วยความพอดีที่สบายเท้าและน้ำหนักเบา รองเท้าฟุตบอลคู่นี้เหมาะอย่างยิ่งสำหรับผู้เล่นที่ชอบรองเท้าฟุตบอลที่ทำจากวัสดุที่ไม่ใช่หนังจิงโจ้เพื่อเพิ่มความเร็ว อีกทั้งยังมีน้ำหนักเบา ยืดหยุ่น และสวมใส่สบายเท้า รองเท้าฟุตบอลคู่นี้มีน้ำหนักเพียงประมาณ 185 กรัม นอกจากนี้ยังมี Engineered Fit Last Neo ที่ให้ความสบายอย่างเหลือเชื่อ ส่วนบนที่ออกแบบใหม่ให้พอดีเท้ามากขึ้น พื้นรองเท้าทำจากวัสดุ KaRVO และ MIZUNO ENERZY ที่ช่วยผลักดันให้คุณก้าวไปข้างหน้า และพื้นรองเท้าที่เสริมการยึดเกาะด้วย ZEROGLIDE α MESH\r\n \r\nประโยชน์:\r\n1) พื้นรองเท้า\r\nใช้ Engineered Fit Last Neo ซึ่งได้รับการพัฒนาใหม่สำหรับผู้เล่นฟุตบอลสมัยใหม่ พื้นรองเท้าที่ได้รับการออกแบบใหม่ด้วยความพิถีพิถันในทุกรายละเอียด ทำให้รองเท้าฟุตบอลคู่นี้มีความพอดีเท้าที่ Mizuno เท่านั้นที่ทำได้\r\n \r\n2) น้ำหนักรองเท้า\r\nรองเท้าเพิ่มความเร็วที่มีน้ำหนักประมาณ 185 กรัม (ขนาด 27.0 ซม.) ซึ่งเบากว่ารุ่นก่อนหน้าด้วยซ้ำ\r\n \r\n3) ส่วนบน\r\nวัสดุที่เลือกทั้งหมดได้รับการตรวจสอบใหม่ทั้งหมดเพื่อให้สวมใส่ได้สบายยิ่งขึ้น รูปแบบของส่วนบนยังได้รับการออกแบบใหม่เพื่อให้สวมใส่ได้พอดียิ่งขึ้น\r\n \r\n4) พื้นรองเท้า\r\nรองเท้ายังคงใช้พื้นรองเท้าชั้นนอกที่เน้นการเพิ่มความเร็วในทิศทางไปข้างหน้า ความหนาของวัสดุ KaRVO ที่ใช้ที่ปลายเท้าได้รับการปรับให้เหมาะสมเพื่อส่งคืนพลังงานและความยืดหยุ่น นอกจากนี้ MIZUNO ENERZY ที่ส้นเท้ายังช่วยเพิ่มการรองรับแรงกระแทกและการส่งคืนพลังงาน\r\n \r\n5) พื้นรองเท้า\r\nการยึดเกาะของ ZEROGLIDE α MESH ช่วยลดการเคลื่อนตัวด้านข้างและรองรับการเคลื่อนไหวที่แม่นยำ\r\n \r\nคุณสมบัติ:\r\nออกแบบมาเพื่อความเร็วโดยเฉพาะ\r\nMIZUNO α II ช่วยให้ผู้เล่นบรรลุความเร็วสูงสุดด้วยความพอดีที่สบายยิ่งขึ้นและน้ำหนักที่เบากว่า\r\nรุ่นนี้ได้รับการออกแบบและผลิตในญี่ปุ่น', 7600.00, '/Images/Products/dcc57a5a-f227-4fbb-bfd5-4954db0a3a0b.png', '/Images/SizeCharts/dd430df3-e738-4c2d-be25-4218040fca6f.png', 129, 3, 7, 1, 1),
(23, 'MIZUNO ALPHA II JAPAN FG - GALAXY SILVER/8605 C/GOLD', 'MIZUNO ALPHA II JAPAN FG\r\n\r\nAchieve your best speed ever with this boot meticulously designed and made in Japan to boost velocity through its comfortable fit and lighter weight. This is the perfect boot for players who prefer a non-kangaroo leather material for an additional speed boost, plus a lightweight, flexible, barefoot feel. Weighing in at just about 185 grams, this featherweight boot also features an Engineered Fit Last Neo for unbelievable comfort, an upper redesigned for a better fit, a sole with KaRVO and MIZUNO ENERZY material that push you forward, and an insole with grip-boosting ZEROGLIDE α MESH.\r\n\r\nBenefit:\r\n1) Shoe last\r\nUses the Engineered Fit Last Neo, which was redeveloped for modern football players. The last, which has been meticulously redesigned down to the smallest detail, achieves a fit that only Mizuno could deliver.\r\n\r\n2) Shoe weight\r\nSpeed-boosting boot with a weight of about 185 g (27.0 cm size), which is even lighter than the previous model.\r\n\r\n3) Upper\r\nAll material choices were reviewed from scratch to achieve a more comfortable fit. The upper pattern was also redesigned to improve the fit.\r\n\r\n4) Sole\r\nThe boot continues to use an outsole focused on increasing forward-direction speed. The thickness of the KaRVO material used at the forefoot was optimized in pursuit of energy return and flexibility. Plus, the MIZUNO ENERZY at the heel enhances cushioning and energy return.\r\n\r\n5) Insole\r\nThe grip of the ZEROGLIDE α MESH reduces lateral shifts and supports precise movements.\r\n\r\nFeatures:\r\nEntirely designed for speed.\r\nThe MIZUNO α II helps players achieve maximum speed through its more comfortable fit and lighter weight.\r\nThis model is designed and made in Japan.', 6500.00, '/Images/Products/53a7abb8-39c0-43d4-b944-14351998c6e1.png', '/Images/SizeCharts/721b4a54-14c0-423d-aafe-e528171e2f7d.png', 104, 3, 7, 1, 1),
(24, 'ADIDAS MANCHESTER UNITED LIFESTYLER JERSEY - MULTICOLOR', 'ADIDAS MANCHESTER UNITED LIFESTYLER JERSEY\r\n\r\nเสื้อฟุตบอล Manchester United LFSTLR ตัวนี้หยิบเอาสิ่งที่สืบทอดจากโลกฟุตบอลมารวมกับโลกแฟชันที่ทันสมัย ผสานวัฒนธรรมของสโมสรเข้ากับความมั่นใจในการใช้ชีวิตเพื่อให้คุณได้สวมใส่สีสันที่สะท้อนความเป็นตัวเองอย่างภาคภูมิ\r\n\r\nตัดเย็บจากผ้าอินเตอร์ล็อคสัมผัสเรียบ ตัวเสื้อฟุตบอลมาพร้อมคอวีและดีเทลของสโมสรที่เป็นต้นฉบับขนานแท้ รวมสองไอคอนไว้ในหนึ่งเดียวด้วยตราปักสโมสร Manchester United เคียงข้างโลโก้ Trefoil ของอาดิดาส\r\n\r\nไม่ว่าคุณจะมุ่งหน้าไปชมการแข่งขันหรือออกไปเที่ยวในเมือง เสื้อฟุตบอลตัวนี้จะทำให้เกมของคุณเฉียบคมและมีสไตล์ที่เท่ยิ่งกว่าด้วยลุคอันยอดเยี่ยมและโดดเด่นบนท้องถนน\r\n\r\nเสื้อฟุตบอลแต่งตราสโมสรที่เป็นไอคอนและมาในทรงหลวมสวมสบาย\r\n\r\nเสื้อฟุตบอล Manchester United LFSTLR\r\n\r\nทรงหลวม\r\nโพลีเอสเตอร์ 100% (รีไซเคิล 100%)\r\nคอวี\r\nปักโลโก้ Trefoil และตราสโมสร', 4600.00, '/Images/Products/493bdb21-71a7-416d-99af-3f92a2e17702.png', '/Images/SizeCharts/2a695f3e-df14-4cc5-a189-624287a904c0.png', 101, 5, NULL, 2, 1),
(25, 'PUMA ULTRA 6 ULTIMATE RELENTLESS FG - PUMA BLACK/GLOWING RED', 'รายละเอียด : PUMA ULTRA 6 ULTIMATE RELENTLESS FG\r\n\r\nULTRA 6 กลับมาอีกครั้ง พร้อมอัปเปอร์ผ้าตาข่ายดีไซน์ใหม่ที่ผ่านการออกแบบทางวิศวกรรม เพื่อการจบสกอร์ที่เฉียบคม – รองเท้าฟุตบอลที่ให้ความรู้สึกและประสิทธิภาพราวกับเครื่องจักรที่ปรับจูนมาอย่างสมบูรณ์แบบใต้ฝ่าเท้าของคุณ กรอบรองรับ PWRTAPE ช่วยประคองเท้าให้นิ่งภายในรองเท้า โดยไม่ลดทอนความคล่องตัวและอิสระในการเคลื่อนไหว ส่วนพื้นรองเท้า SPEEDSYSTEM และการจัดวางปุ่มแบบ FastTrax ถูกออกแบบมาอย่างแม่นยำ เพื่อพาคุณทะยานจากเสียงนกหวีดเริ่มเกมสู่ตาข่ายประตูเร็วกว่าใคร\r\n\r\nคุณสมบัติและประโยชน์\r\n\r\n- การเร่งสปีด (ACCELERATION) : พื้นรองเท้า SPEEDSYSTEM ของ PUMA มาพร้อมแผ่นรองสปริงตัว และการจัดวางปุ่มแบบใหม่เพื่อการเร่งความเร็วที่ฉับไว\r\n- การยึดเกาะ (TRACTION) : ปุ่ม FastTrax ออกแบบเพื่อการเคลื่อนที่หลายทิศทาง ช่วยให้คุณวิ่ง ตัด และเบรกได้อย่างมั่นใจยิ่งขึ้น\r\n- การจบสกอร์ (FINISHING) : อัปเปอร์ผ้าตาข่ายดีไซน์ใหม่ พร้อมพื้นผิว 3D ในจุดสัมผัสสำคัญ ช่วยเพิ่มการยึดเกาะสำหรับการยิงที่เฉียบคมในจังหวะสปีดสูง\r\n- ผลิตจากวัสดุรีไซเคิลอย่างน้อย 50%\r\n\r\nรายละเอียดผลิตภัณฑ์\r\n\r\nความกว้าง : ปกติ\r\nระบบผูกเชือก : เชือกผูก\r\nแผ่นรองพื้น : แผ่นรอง Ortholite® O-Therm™ ผสมแอโรเจล ช่วยป้องกันความร้อน ให้ความเย็นสบายตลอดการเล่น\r\nส้นรองเท้า : แบน\r\nวัสดุซับใน : สิ่งทอ\r\nGripControl Pro : ผิวสัมผัสพิเศษเพื่อการควบคุมบอลอย่างเฉียบคม\r\nพื้นสนามที่เหมาะสม : สนามหญ้าเทียม (Artificial Ground)\r\nข้อมูลวัสดุ\r\n\r\nแผ่นรองเท้า : สิ่งทอ 100%\r\nพื้นรองเท้าชั้นนอก : สังเคราะห์ 100%\r\nอัปเปอร์: สังเคราะห์ 86.82%, สิ่งทอ 13.18%\r\nซับใน : สิ่งทอ 76.16%, สังเคราะห์ 23.84%', 8000.00, '/Images/Products/2c284117-ab9a-4e5c-a922-d4a26bc4f035.png', '/Images/SizeCharts/af06ffe4-885c-49ad-a749-cc7f407ae7d0.png', 20, 6, NULL, 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `product_brands`
--

CREATE TABLE `product_brands` (
  `brand_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `product_brands`
--

INSERT INTO `product_brands` (`brand_id`, `name`) VALUES
(5, 'ADIDAS'),
(3, 'MIZUNO'),
(1, 'NIKE'),
(6, 'PUMA');

-- --------------------------------------------------------

--
-- Table structure for table `product_categories`
--

CREATE TABLE `product_categories` (
  `category_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `product_categories`
--

INSERT INTO `product_categories` (`category_id`, `name`) VALUES
(5, 'กระเป๋า'),
(6, 'ถุงเท้า'),
(1, 'รองเท้าฟุตบอล'),
(3, 'อุปกรณ์เสริม'),
(2, 'เสื้อผ้า');

-- --------------------------------------------------------

--
-- Table structure for table `product_images`
--

CREATE TABLE `product_images` (
  `image_id` int(11) NOT NULL,
  `product_id` int(11) NOT NULL,
  `image_path` varchar(255) NOT NULL,
  `sort_order` int(11) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `product_images`
--

INSERT INTO `product_images` (`image_id`, `product_id`, `image_path`, `sort_order`) VALUES
(7, 2, '/Images/Products/97415b08-c5e0-4b4f-94bb-2e89c636d2bf.png', 1),
(8, 2, '/Images/Products/64ab1947-ce4e-4c33-bd8c-1072a773ed86.png', 2),
(9, 2, '/Images/Products/c57386b4-879e-4fc9-8786-a0d6c4b70bf2.png', 3),
(10, 2, '/Images/Products/49fa0956-ee3b-423d-b5da-5fb46fc828cb.png', 4),
(11, 2, '/Images/Products/98f4bcc3-1f8f-4979-b122-d233080b060e.png', 5),
(13, 4, '/Images/Products/57c49271-a29c-47ad-949e-4faaac47dcb2.png', 1),
(14, 4, '/Images/Products/842bbf12-ec9b-48ed-8a9a-61bcc44f2349.png', 2),
(15, 4, '/Images/Products/a4ee1682-9e66-47b9-a28c-01368110563e.png', 3),
(16, 4, '/Images/Products/66150294-ba63-4193-a498-bf80a7f7cfb9.png', 4),
(17, 4, '/Images/Products/98b4eed8-0bda-46a0-8a0b-6c1b3f39d52d.png', 5),
(19, 6, '/Images/Products/56cd2061-e306-4102-aa6f-3604a5754c2e.png', 1),
(20, 6, '/Images/Products/b672f8b8-5e6b-48b9-b907-66e09ba50ec8.png', 2),
(21, 6, '/Images/Products/f3852c15-be87-4a59-82c8-4c830ec5a83a.png', 3),
(22, 6, '/Images/Products/7e819819-3217-4846-8cf6-4a40307f57cf.png', 4),
(23, 6, '/Images/Products/f3bf306c-61f0-4e74-b1d0-f5ff59860bc8.png', 5),
(24, 7, '/Images/Products/aee7a5ab-50e1-45c4-ba3e-e8414d1f4cfc.png', 1),
(25, 7, '/Images/Products/507b720e-ac92-4ca7-bb7f-e59731ab94a1.png', 2),
(26, 7, '/Images/Products/cd3be72a-e33a-4c55-b341-ec485d6f7a31.png', 3),
(27, 7, '/Images/Products/38859868-cad9-48fa-a69d-0e11ed8bb044.png', 4),
(29, 8, '/Images/Products/f6ca8089-8b11-4343-819c-f4712868a6cf.png', 1),
(30, 8, '/Images/Products/eb123d67-1d3d-4fb1-b4fe-ae293e684702.png', 2),
(31, 8, '/Images/Products/c74605ef-55c6-4444-ba67-c432f38c7e19.png', 3),
(34, 9, '/Images/Products/80b8fddb-2e73-4a58-bbbe-11d1c1c9ee93.png', 1),
(35, 9, '/Images/Products/9b51be36-3e62-4aa2-9cf6-5b6da59b2925.png', 2),
(36, 9, '/Images/Products/f0fce75c-de59-42f3-b100-c4a7999749df.png', 3),
(54, 13, '/Images/Products/32a05537-27c6-41a1-bd66-bfb2674f09ae.png', 1),
(55, 13, '/Images/Products/62cdbf64-9764-4112-aecd-985a474287bc.png', 2),
(56, 13, '/Images/Products/68d7f066-2f70-4f09-8d0c-d036a6665580.png', 3),
(64, 14, '/Images/Products/cd511d78-2610-4247-b7ec-d2b98472d733.png', 1),
(65, 14, '/Images/Products/6b6b5310-e18f-423e-a5f6-14b03b1d0e41.png', 2),
(66, 14, '/Images/Products/143ccddf-a0a6-43e2-bdcb-62cc9d148abc.png', 3),
(67, 14, '/Images/Products/ab7fbb1d-cef7-45b9-bdf6-2250f9e4af26.png', 4),
(68, 14, '/Images/Products/6d5e6798-db2c-41bf-a7a5-19a396b88eaf.png', 5),
(69, 12, '/Images/Products/5fda88fc-24b7-4e56-9597-dfab15aa5dcb.png', 1),
(70, 12, '/Images/Products/1896b2b7-948a-4d5a-85a7-4711bc063011.png', 2),
(71, 12, '/Images/Products/d3591cd5-fecf-4955-8baf-a883f611ffd3.png', 3),
(72, 12, '/Images/Products/dd1ba06f-249b-43a6-8eb3-bb0d7bbcdde4.png', 4),
(73, 12, '/Images/Products/fc3f2262-b92c-4784-9bcd-5e12c095c1fa.png', 5),
(74, 12, '/Images/Products/0f1491ae-b84c-4d30-b377-19c5bc8edc3a.png', 6),
(75, 12, '/Images/Products/e9147523-ae0b-4da1-a4ed-b92323cafc77.png', 7),
(76, 10, '/Images/Products/4d766cba-e3b8-4527-974f-956762a5bdb5.png', 1),
(77, 10, '/Images/Products/cd6d8b9b-2d3a-412a-9eb0-bf4c7fe391f1.png', 2),
(78, 10, '/Images/Products/bb57e38e-f910-4c4b-b172-28b595cc1d37.png', 3),
(79, 10, '/Images/Products/ed3bd9a9-a91a-420e-9db0-162ff312613c.png', 4),
(80, 10, '/Images/Products/93104047-3ff6-475a-bee5-8c2f697efd05.png', 5),
(81, 11, '/Images/Products/a5458a53-2936-40a7-a677-ea1d52a8d841.png', 1),
(82, 11, '/Images/Products/b4abea3f-a63b-42fe-a3ef-88c35336ecfd.png', 2),
(83, 11, '/Images/Products/615d5dde-577b-4c6b-b3f1-0c722093d9b1.png', 3),
(86, 8, '/Images/Products/459922a0-ae96-4d41-a0dd-1924913dff1f.png', 4),
(87, 8, '/Images/Products/8c99f673-bd37-477e-9b3a-720be33e3db8.png', 5),
(88, 13, '/Images/Products/a4fdf7bc-3a07-4913-8cbc-55145d6ac5c3.png', 4),
(89, 13, '/Images/Products/eace76dc-010b-43ac-a910-1192490a7b28.png', 5),
(90, 9, '/Images/Products/905c1a22-4662-4d62-a0c1-e995d72e0351.png', 4),
(91, 9, '/Images/Products/7982b7f9-4e6a-46da-a661-f8f6dad7361c.png', 5),
(92, 15, '/Images/Products/5cc28054-4a69-4d17-9931-efb621be54c8.png', 1),
(93, 15, '/Images/Products/aaa55aba-917a-457d-8e1b-965f83e639db.png', 2),
(94, 15, '/Images/Products/17b9801c-43b3-4a5d-9742-70bcdd540f16.png', 3),
(95, 15, '/Images/Products/36672885-595f-4c09-9622-d75e7c6bde68.png', 4),
(96, 15, '/Images/Products/efa2d72d-fe6a-4cd6-858a-04b5fd0518d0.png', 5),
(97, 7, '/Images/Products/27a434b9-f433-4ea1-b290-2e7c9669b742.png', 5),
(98, 16, '/Images/Products/c36daaf0-3671-4cc4-a254-5acfd1e9261d.png', 1),
(99, 16, '/Images/Products/68671b26-c91c-468b-ae44-9a5f61891489.png', 2),
(100, 16, '/Images/Products/45377bef-bd7c-400f-8755-c59e5480783e.png', 3),
(101, 16, '/Images/Products/d6a20bd3-db69-4d4f-b7fd-52cc47468c5e.png', 4),
(102, 16, '/Images/Products/14168a0a-58fa-4d24-8ee5-353f265f054f.png', 5),
(103, 16, '/Images/Products/30ebde19-dfba-4eaa-8f3e-b3a4ba956cc9.png', 6),
(104, 16, '/Images/Products/92befffc-134d-4dc2-abae-f10b53322c09.png', 7),
(105, 16, '/Images/Products/d7c5a017-4335-465c-b747-b726fdd218b1.png', 8),
(106, 17, '/Images/Products/5a0f798b-1b9c-4be8-87dd-a9a4192e026c.png', 1),
(107, 17, '/Images/Products/cbd35e4d-5bcf-4728-8c7d-aa3de5b89ff8.png', 2),
(108, 17, '/Images/Products/67a765ad-30f2-4978-a4df-a8f4dfe4c91a.png', 3),
(109, 17, '/Images/Products/c5b5be03-25f7-4310-b9ba-4edfa392c4dd.png', 4),
(110, 17, '/Images/Products/cbdd3644-f2d5-4bb0-ad66-78879f407a2d.png', 5),
(111, 17, '/Images/Products/0aa0af62-6906-47ab-99ae-9a4685934910.png', 6),
(112, 17, '/Images/Products/5dfe9205-2521-491c-81d4-31c013bf9497.png', 7),
(113, 18, '/Images/Products/17c7dfc1-e676-416c-bb47-d8ccef3fac9c.png', 1),
(114, 18, '/Images/Products/6d7eb64e-0159-4e14-97f2-458388f29bd0.png', 2),
(115, 18, '/Images/Products/6058d484-e307-48bd-89b6-84bbb654eb0b.png', 3),
(116, 18, '/Images/Products/d5d30ba6-41ee-46dc-94af-e2aa9b9caa1a.png', 4),
(117, 18, '/Images/Products/67fa1a08-a707-44fe-bd5f-a4bee0711b8e.png', 5),
(118, 18, '/Images/Products/4a7d7a10-7114-4932-91b3-dee57f7c2808.png', 6),
(119, 18, '/Images/Products/38d049ee-8ed0-411f-9680-c8154758d6f9.png', 7),
(120, 19, '/Images/Products/644790ac-7ade-441e-9002-28a1ab2f6006.png', 1),
(121, 19, '/Images/Products/1d7a7ecd-5e57-435d-bc2c-87a2c6dd6cb6.png', 2),
(122, 19, '/Images/Products/6e87daae-fcb0-4886-8326-b56882466b45.png', 3),
(123, 19, '/Images/Products/c2d2d639-7170-4287-8628-be7ca7618eff.png', 4),
(124, 19, '/Images/Products/1d2908e1-58ea-4e7d-92e3-9b0a4c594e5e.png', 5),
(125, 19, '/Images/Products/4e799ecb-7937-4def-9ef0-3d7e993f7405.png', 6),
(126, 19, '/Images/Products/7092c93e-700b-4ccc-930e-43ca5fa9733b.png', 7),
(127, 19, '/Images/Products/eab342a9-f2e2-4c3c-8bcc-af0e54bffc19.png', 8),
(128, 20, '/Images/Products/09d4c213-de3e-4e0c-a173-e42c8ba3b458.png', 1),
(129, 20, '/Images/Products/15b67f34-0841-4e93-aa4a-fa7feb46806d.png', 2),
(130, 5, '/Images/Products/5122dd7c-00cb-4d35-9506-5d3db4e79f1b.png', 1),
(131, 5, '/Images/Products/42416262-eb9b-4783-9d41-da9bfc63fef3.png', 2),
(132, 5, '/Images/Products/969fd474-2c02-4a43-8fae-d04d5eb19bfb.png', 3),
(133, 5, '/Images/Products/51761d86-5d04-44c1-94ab-1663e0c32478.png', 4),
(134, 5, '/Images/Products/7307acab-42f8-41a7-af4b-7b35a4b6c662.png', 5),
(135, 5, '/Images/Products/298aeeec-0a1f-4c62-bf27-94e3eadf6318.png', 6),
(136, 21, '/Images/Products/6acf7305-e8b2-4d50-87ab-14b5f0870168.png', 1),
(137, 21, '/Images/Products/26284c41-31fc-4d6c-acb3-6004179c7b06.png', 2),
(138, 21, '/Images/Products/5fd7bedb-2c62-4367-91b2-477e0205ca6f.png', 3),
(139, 21, '/Images/Products/4fa82501-1af6-4b30-a466-871446dfd37e.png', 4),
(140, 21, '/Images/Products/de8a34a4-1877-4141-b775-25b9e7a0cbdb.png', 5),
(141, 21, '/Images/Products/e476d13b-59eb-41c3-9ad7-92ccf0b549cc.png', 6),
(142, 22, '/Images/Products/250e786e-951c-43ef-b4f0-6b7ddfe3518f.png', 1),
(143, 22, '/Images/Products/87247526-7f91-4876-9050-938a68f22ced.png', 2),
(144, 22, '/Images/Products/5a7798e6-9bb8-4f5e-a628-76ee53e536df.png', 3),
(145, 23, '/Images/Products/8a5fc952-50f8-4d09-a338-5c7e7df2a9aa.png', 1),
(146, 23, '/Images/Products/979987aa-1ff8-4a2e-8c81-4d9e3eb3b51e.png', 2),
(147, 23, '/Images/Products/b9ce64fc-037d-4362-a601-1a7946d31238.png', 3),
(148, 23, '/Images/Products/e669b0b7-fb25-4e8e-acbc-508e3e3d11bb.png', 4),
(149, 23, '/Images/Products/cb4357a8-7bbb-41bf-a6df-e893f449f841.png', 5),
(150, 24, '/Images/Products/4f5f0f50-f9c3-4c9d-8f45-234b3e984bda.png', 1),
(151, 24, '/Images/Products/6cdd8490-d879-49ec-b913-28e609010158.png', 2),
(152, 24, '/Images/Products/86b777e5-6dcf-408d-8413-a8a633b55a55.png', 3),
(153, 24, '/Images/Products/7dd77d14-c7db-43b4-9378-38943a415517.png', 4),
(154, 25, '/Images/Products/ef848a3a-39e0-4d08-b7a9-1c46ece6e271.png', 1),
(155, 25, '/Images/Products/c2ad66f2-ddfa-4224-8c57-67186f5ea8fe.png', 2),
(156, 25, '/Images/Products/72dd6587-5420-44e9-a73d-6855d3f43d99.png', 3),
(157, 25, '/Images/Products/24f4ae72-6e74-405c-90c7-cf6a73dbbdb5.png', 4);

-- --------------------------------------------------------

--
-- Table structure for table `product_models`
--

CREATE TABLE `product_models` (
  `model_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `brand_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `product_models`
--

INSERT INTO `product_models` (`model_id`, `name`, `brand_id`) VALUES
(1, 'MERCURAIL', 1),
(2, 'PHANTOM', 1),
(3, 'TIEMPO', 1),
(4, 'PREDATOR', 5),
(5, 'F50', 5),
(6, 'MORELIA NEO', 3),
(7, 'ALPHA', 3);

-- --------------------------------------------------------

--
-- Table structure for table `product_variants`
--

CREATE TABLE `product_variants` (
  `variant_id` int(11) NOT NULL,
  `product_id` int(11) NOT NULL,
  `size_name` varchar(50) NOT NULL,
  `stock_quantity` int(11) NOT NULL DEFAULT 0,
  `sort_order` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `product_variants`
--

INSERT INTO `product_variants` (`variant_id`, `product_id`, `size_name`, `stock_quantity`, `sort_order`) VALUES
(3, 2, 'S', 20, 1),
(5, 4, '40 EU', 20, 1),
(6, 5, '40 EU', 19, 1),
(7, 6, '40 EU', 20, 1),
(9, 6, '41 EU', 20, 2),
(11, 6, '42 EU', 20, 3),
(13, 6, '43 EU', 20, 4),
(15, 6, '44 EU', 20, 5),
(17, 7, '40', 0, 0),
(22, 8, '40 EU', 19, 1),
(23, 8, '41 EU', 20, 2),
(24, 8, '42 EU', 20, 3),
(25, 8, '43 EU', 20, 4),
(26, 8, '44 EU', 19, 5),
(27, 4, '41 EU', 20, 2),
(28, 4, '42 EU', 20, 3),
(29, 4, '43 EU', 20, 4),
(30, 4, '44 EU', 20, 5),
(31, 2, 'M', 20, 2),
(32, 2, 'L', 19, 3),
(33, 2, 'XL', 20, 4),
(34, 2, '2XL', 20, 5),
(35, 2, '3XL', 20, 6),
(36, 9, '40 EU', 20, 1),
(37, 9, '41 EU', 20, 2),
(38, 9, '42 EU', 20, 3),
(39, 9, '43 EU', 20, 4),
(40, 9, '44 EU', 20, 5),
(41, 10, 'S', 20, 1),
(42, 10, 'M', 19, 2),
(43, 10, 'L', 20, 3),
(44, 10, 'XL', 20, 4),
(45, 10, '2XL', 20, 5),
(46, 10, '3XL', 20, 6),
(47, 11, '5', 19, 1),
(48, 12, 'S', 20, 1),
(49, 12, 'M', 20, 2),
(50, 12, 'L', 18, 3),
(51, 12, 'XL', 20, 4),
(52, 12, '2XL', 20, 5),
(53, 12, '3XL', 20, 6),
(54, 13, '40 EU', 19, 1),
(55, 13, '41 EU', 20, 2),
(56, 13, '42 EU', 20, 3),
(57, 13, '43 EU', 20, 4),
(58, 13, '44 EU', 20, 5),
(59, 14, 'XS', 20, 1),
(60, 14, 'S', 20, 2),
(61, 14, 'M', 19, 3),
(62, 14, 'XL', 20, 4),
(63, 14, '2XL', 20, 20),
(64, 14, '3XL', 20, 6),
(65, 15, '40 EU', 16, 1),
(66, 15, '41 EU', 20, 2),
(67, 15, '42 EU', 18, 3),
(68, 15, '43 EU', 20, 4),
(69, 15, '44 EU', 20, 5),
(70, 16, '40 EU', 20, 1),
(71, 16, '41 EU', 10, 2),
(72, 16, '42 EU', 20, 3),
(73, 16, '43 EU', 20, 4),
(74, 16, '44 EU', 20, 5),
(75, 17, '40 EU', 20, 1),
(76, 17, '41 EU', 20, 2),
(77, 17, '42 EU', 20, 3),
(78, 17, '43 EU', 20, 43),
(79, 17, '44 EU', 20, 44),
(80, 18, '40 EU', 20, 1),
(81, 18, '41 EU', 20, 2),
(82, 18, '42 EU', 20, 3),
(83, 18, '43 EU', 20, 4),
(84, 18, '44 EU', 10, 5),
(85, 19, '40 EU', 20, 1),
(86, 19, '41 EU', 20, 2),
(87, 19, '42 EU', 20, 3),
(88, 19, '43 EU', 20, 4),
(89, 19, '44 EU', 20, 5),
(90, 20, '10', 19, 1),
(91, 5, '41 EU', 20, 2),
(92, 5, '42 EU', 20, 3),
(93, 5, '43 EU', 20, 4),
(94, 5, '44 EU', 20, 5),
(95, 21, '40 EU', 16, 1),
(96, 21, '41 EU', 20, 2),
(97, 21, '42 EU', 19, 3),
(98, 21, '43 EU', 19, 4),
(99, 21, '44 EU', 20, 5),
(100, 22, '40 EU', 20, 1),
(101, 22, '41 EU', 19, 2),
(102, 22, '42 EU', 50, 3),
(103, 22, '43 EU', 20, 4),
(104, 22, '44 EU', 20, 5),
(105, 23, '39 EU', 4, 1),
(106, 23, '40 EU', 20, 2),
(107, 23, '41 EU', 20, 3),
(108, 23, '42 EU', 20, 4),
(109, 23, '43 EU', 20, 5),
(110, 23, '44 EU', 20, 6),
(111, 24, 'S', 20, 1),
(112, 24, 'M', 20, 2),
(113, 24, 'L', 20, 3),
(114, 24, 'XL', 15, 4),
(115, 24, '2XL', 6, 5),
(116, 24, '3XL', 20, 6),
(117, 25, '40 EU', 0, 1),
(118, 25, '41 EU', 0, 1),
(119, 25, '42 EU', 20, 1);

-- --------------------------------------------------------

--
-- Table structure for table `sizes`
--

CREATE TABLE `sizes` (
  `size_id` int(11) NOT NULL,
  `category_id` int(11) NOT NULL,
  `size_name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `team_members`
--

CREATE TABLE `team_members` (
  `member_id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `role` varchar(100) DEFAULT NULL COMMENT 'เช่น "Developer", "Designer"',
  `image_path` varchar(255) DEFAULT NULL,
  `sort_order` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `team_members`
--

INSERT INTO `team_members` (`member_id`, `name`, `role`, `image_path`, `sort_order`) VALUES
(1, 'ธนกร เนื่องภักดี', 'ผู้พัฒนาโปรแกรม', '/Images/Team/572fb6c1-4c9b-4d00-8902-6d3630e39d84.png', 0);

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `email` varchar(255) NOT NULL,
  `username` varchar(100) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `role` varchar(50) NOT NULL DEFAULT 'user',
  `otp_code` varchar(10) DEFAULT NULL,
  `otp_expiry` datetime DEFAULT NULL,
  `first_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) DEFAULT NULL,
  `phone_number` varchar(20) DEFAULT NULL,
  `shipping_address_line1` varchar(255) DEFAULT NULL,
  `shipping_address_city` varchar(100) DEFAULT NULL,
  `shipping_address_zipcode` varchar(10) DEFAULT NULL,
  `profile_image_path` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `email`, `username`, `password_hash`, `role`, `otp_code`, `otp_expiry`, `first_name`, `last_name`, `phone_number`, `shipping_address_line1`, `shipping_address_city`, `shipping_address_zipcode`, `profile_image_path`) VALUES
(1, 'jongsanamb@gmail.com', 'admin', '60fe74406e7f353ed979f350f2fbb6a2e8690a5fa7d1b0c32983d1d8b3f95f67', 'admin', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(2, 'thanakon.tanwa@gmail.com', 'thunwa888', '8d7f59b274a57db109208c588d9112b39d93a50d8f79fc6a5b7e4fa5a0716684', 'user', '557415', '2026-01-11 19:28:06', 'ธนกร', 'เนื่องภักดี', '0986323560', '346 หมู่ 15 บ้านห้วยยางศรีวิไล ต.ดงเมืองแอม อ.เขาสวนกวาง จ.ขอนแก่น', 'เขาสวนกวาง', '40280', 'C:\\Users\\ACER\\source\\repos\\login_store\\login_store\\bin\\Debug\\UserImages\\user_2_1d97ab85-0de1-49d6-90c2-0acc5dd5a9e8.jpg'),
(5, 'thxnwa.wara@gmail.com', 'thunwa', 'e2b3692016e8357724beb35738b3dd94ca7444bf0424c25db30031b4f651cb0a', 'user', NULL, NULL, 'ธนกร', 'เนื่อภักดี', '0986323560', 'มหาวิทยาลัยขอนแก่น', 'เมือง', '40002', 'C:\\Users\\ACER\\source\\repos\\login_store\\login_store\\bin\\Debug\\UserImages\\user_5_1d97ab85-0de1-49d6-90c2-0acc5dd5a9e8.jpg'),
(6, 'tanakon.tanva@gmail.com', 'thunwa55', 'e2b3692016e8357724beb35738b3dd94ca7444bf0424c25db30031b4f651cb0a', 'user', '386996', '2025-11-19 22:30:56', NULL, NULL, NULL, NULL, NULL, NULL, NULL),
(7, 'thanakonthunwaz@gmail.com', 'thunwaZA', '8d7f59b274a57db109208c588d9112b39d93a50d8f79fc6a5b7e4fa5a0716684', 'user', NULL, NULL, 'ธนกร', 'เนื่องภักดี', '0986323560', NULL, NULL, NULL, 'C:\\Users\\ACER\\source\\repos\\login_store\\login_store\\bin\\Debug\\UserImages\\user_7_about us.png'),
(8, 'dongsoyer@gmail.com', 'phrommin47', '818c65096614f36f3584616fdcfa23b940a076738fff7b623bcdd4cf934fbbe2', 'user', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `user_addresses`
--

CREATE TABLE `user_addresses` (
  `address_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `address_label` varchar(50) NOT NULL COMMENT 'เช่น "บ้าน", "ที่ทำงาน"',
  `full_name` varchar(100) NOT NULL,
  `phone_number` varchar(20) NOT NULL,
  `address_line1` varchar(255) NOT NULL COMMENT 'บ้านเลขที่, ถนน',
  `sub_district` varchar(100) NOT NULL COMMENT 'ตำบล',
  `district` varchar(100) NOT NULL COMMENT 'อำเภอ',
  `province` varchar(100) NOT NULL COMMENT 'จังหวัด',
  `postal_code` varchar(10) NOT NULL,
  `is_default` tinyint(1) NOT NULL DEFAULT 0 COMMENT '0=No, 1=Yes'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `user_addresses`
--

INSERT INTO `user_addresses` (`address_id`, `user_id`, `address_label`, `full_name`, `phone_number`, `address_line1`, `sub_district`, `district`, `province`, `postal_code`, `is_default`) VALUES
(1, 5, 'บ้านห้วยยางศรีวิไล', 'นายธนกร เนื่องภักดี', '0986323560', '346 หมู่ 15', 'ดงเมืองแอม', 'เขาสวนกวาง', 'ขอนแก่น', '40280', 1),
(2, 5, 'หออินเตอร์', 'ธันวา', '0986323560', 'มหาวิทยาลัยขอนแก่น', 'บ้านศิลา', 'ในเมือง', 'ขอนแก่น', '40000', 0),
(3, 7, 'บ้านห้วยยาง', 'ธันวา', '0986323560', '346', 'ดงเมืองแอม', 'เขาสวนกวาง', 'ขอนแก่น', '40280', 0),
(4, 7, 'มหาวิทยาลัยขอนแก่น', 'ธันวา', '0986323560', '123', 'ในเมือง', 'เมือง', 'ขอนแก่น', '40002', 1),
(5, 2, 'มหาวิทยาลัยขอนแก่น', 'ธันวา', '0986323560', '346', 'ดงเมืองแอม', 'เขาสวนกวาง', 'ขอนแก่น', '40280', 1),
(6, 8, 'จอมพล', 'พรหมมินทร์ ว๊ฒนกามิน์', '0986283863', '22', 'คำม่วง', 'เขาสวนกวาง', 'ขอนแก่น', '40280', 1);

-- --------------------------------------------------------

--
-- Table structure for table `user_vouchers`
--

CREATE TABLE `user_vouchers` (
  `user_voucher_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `voucher_id` int(11) NOT NULL,
  `collected_date` datetime NOT NULL DEFAULT current_timestamp(),
  `is_used` tinyint(1) NOT NULL DEFAULT 0 COMMENT '0=ยังไม่ใช้, 1=ใช้แล้ว'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `user_vouchers`
--

INSERT INTO `user_vouchers` (`user_voucher_id`, `user_id`, `voucher_id`, `collected_date`, `is_used`) VALUES
(4, 5, 4, '2025-11-19 10:37:37', 1),
(5, 7, 4, '2025-11-19 22:36:00', 1),
(6, 7, 5, '2025-11-19 22:36:02', 0),
(7, 5, 7, '2025-11-19 23:32:26', 1),
(8, 7, 9, '2025-11-20 01:31:59', 1),
(9, 5, 9, '2025-11-20 10:33:20', 1),
(10, 2, 9, '2025-11-20 10:37:08', 1),
(11, 7, 10, '2025-11-20 10:56:17', 1),
(12, 5, 10, '2025-11-20 11:19:04', 1),
(13, 2, 10, '2025-11-20 13:43:16', 1),
(14, 8, 9, '2025-11-20 20:02:22', 0),
(15, 8, 10, '2025-11-20 20:02:27', 0),
(16, 8, 7, '2025-11-20 20:02:30', 1);

-- --------------------------------------------------------

--
-- Table structure for table `vouchers`
--

CREATE TABLE `vouchers` (
  `voucher_id` int(11) NOT NULL,
  `code` varchar(50) NOT NULL,
  `description` varchar(255) NOT NULL,
  `discount_type` enum('Fixed','Percentage') NOT NULL DEFAULT 'Fixed',
  `discount_amount` decimal(10,2) NOT NULL,
  `min_purchase` decimal(10,2) NOT NULL DEFAULT 0.00 COMMENT 'ยอดซื้อขั้นต่ำ',
  `valid_from` datetime NOT NULL,
  `valid_to` datetime NOT NULL COMMENT 'วันหมดอายุ',
  `is_active` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Admin เปิด/ปิด'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `vouchers`
--

INSERT INTO `vouchers` (`voucher_id`, `code`, `description`, `discount_type`, `discount_amount`, `min_purchase`, `valid_from`, `valid_to`, `is_active`) VALUES
(4, 'VAMOS3000', 'ซื้อขั้นต่ำ 10,000 บาท รับส่วนลด 3,000 บาท ทันที', 'Fixed', 3000.00, 10000.00, '2025-11-19 00:00:00', '2025-12-19 23:59:59', 1),
(5, 'VAMOS1000', 'ซื้อขั้นต่ำ 10,000 บาท รับส่วนลด 1,000', 'Fixed', 1000.00, 10000.00, '2025-11-19 00:00:00', '2025-12-19 23:59:59', 1),
(7, 'VAMOS20%', 'โค้ดลด 20% เมื่อซื้อครบ 5,000 บาท', 'Percentage', 20.00, 1000.00, '2025-11-19 00:00:00', '2025-12-19 23:59:59', 1),
(9, 'VAMOS-30%', 'เมื่อซื้อสินค้าครบ 6,000 รับส่วนลด 30%', 'Percentage', 30.00, 6000.00, '2025-11-20 00:00:00', '2025-12-20 23:59:59', 1),
(10, '12DEC-30%', 'ลด 30%', 'Percentage', 30.00, 6000.00, '2025-11-20 00:00:00', '2025-12-20 23:59:59', 1),
(11, 'THUNWA100', 'VAMOS30%', 'Percentage', 30.00, 1000.00, '2026-01-11 00:00:00', '2026-02-10 23:59:59', 1);

-- --------------------------------------------------------

--
-- Table structure for table `wishlist_items`
--

CREATE TABLE `wishlist_items` (
  `wishlist_item_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `product_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `wishlist_items`
--

INSERT INTO `wishlist_items` (`wishlist_item_id`, `user_id`, `product_id`) VALUES
(16, 2, 2),
(14, 2, 5),
(15, 2, 21),
(12, 7, 5),
(11, 7, 15),
(13, 7, 18);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `ad_slides`
--
ALTER TABLE `ad_slides`
  ADD PRIMARY KEY (`ad_id`);

--
-- Indexes for table `cart_items`
--
ALTER TABLE `cart_items`
  ADD PRIMARY KEY (`cart_item_id`),
  ADD UNIQUE KEY `user_id` (`user_id`,`product_variant_id`),
  ADD KEY `product_variant_id` (`product_variant_id`);

--
-- Indexes for table `company_info`
--
ALTER TABLE `company_info`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `notifications`
--
ALTER TABLE `notifications`
  ADD PRIMARY KEY (`notification_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indexes for table `orders`
--
ALTER TABLE `orders`
  ADD PRIMARY KEY (`order_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indexes for table `order_items`
--
ALTER TABLE `order_items`
  ADD PRIMARY KEY (`item_id`),
  ADD KEY `order_id` (`order_id`),
  ADD KEY `product_id` (`product_id`);

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`product_id`),
  ADD KEY `brand_id` (`brand_id`),
  ADD KEY `category_id` (`category_id`);

--
-- Indexes for table `product_brands`
--
ALTER TABLE `product_brands`
  ADD PRIMARY KEY (`brand_id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Indexes for table `product_categories`
--
ALTER TABLE `product_categories`
  ADD PRIMARY KEY (`category_id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Indexes for table `product_images`
--
ALTER TABLE `product_images`
  ADD PRIMARY KEY (`image_id`),
  ADD KEY `product_id` (`product_id`);

--
-- Indexes for table `product_models`
--
ALTER TABLE `product_models`
  ADD PRIMARY KEY (`model_id`);

--
-- Indexes for table `product_variants`
--
ALTER TABLE `product_variants`
  ADD PRIMARY KEY (`variant_id`),
  ADD KEY `product_id` (`product_id`);

--
-- Indexes for table `sizes`
--
ALTER TABLE `sizes`
  ADD PRIMARY KEY (`size_id`),
  ADD KEY `category_id` (`category_id`);

--
-- Indexes for table `team_members`
--
ALTER TABLE `team_members`
  ADD PRIMARY KEY (`member_id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`),
  ADD UNIQUE KEY `email` (`email`);

--
-- Indexes for table `user_addresses`
--
ALTER TABLE `user_addresses`
  ADD PRIMARY KEY (`address_id`),
  ADD KEY `user_id_fk` (`user_id`);

--
-- Indexes for table `user_vouchers`
--
ALTER TABLE `user_vouchers`
  ADD PRIMARY KEY (`user_voucher_id`),
  ADD UNIQUE KEY `user_voucher_unique` (`user_id`,`voucher_id`) COMMENT 'กัน User คนเดียวเก็บโค้ดเดียวกันซ้ำ',
  ADD KEY `voucher_id` (`voucher_id`);

--
-- Indexes for table `vouchers`
--
ALTER TABLE `vouchers`
  ADD PRIMARY KEY (`voucher_id`),
  ADD UNIQUE KEY `code` (`code`);

--
-- Indexes for table `wishlist_items`
--
ALTER TABLE `wishlist_items`
  ADD PRIMARY KEY (`wishlist_item_id`),
  ADD UNIQUE KEY `user_id` (`user_id`,`product_id`),
  ADD KEY `product_id` (`product_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `ad_slides`
--
ALTER TABLE `ad_slides`
  MODIFY `ad_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;

--
-- AUTO_INCREMENT for table `cart_items`
--
ALTER TABLE `cart_items`
  MODIFY `cart_item_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=46;

--
-- AUTO_INCREMENT for table `notifications`
--
ALTER TABLE `notifications`
  MODIFY `notification_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=22;

--
-- AUTO_INCREMENT for table `orders`
--
ALTER TABLE `orders`
  MODIFY `order_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=42;

--
-- AUTO_INCREMENT for table `order_items`
--
ALTER TABLE `order_items`
  MODIFY `item_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=47;

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `product_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

--
-- AUTO_INCREMENT for table `product_brands`
--
ALTER TABLE `product_brands`
  MODIFY `brand_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `product_categories`
--
ALTER TABLE `product_categories`
  MODIFY `category_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `product_images`
--
ALTER TABLE `product_images`
  MODIFY `image_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=158;

--
-- AUTO_INCREMENT for table `product_models`
--
ALTER TABLE `product_models`
  MODIFY `model_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT for table `product_variants`
--
ALTER TABLE `product_variants`
  MODIFY `variant_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=120;

--
-- AUTO_INCREMENT for table `sizes`
--
ALTER TABLE `sizes`
  MODIFY `size_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `team_members`
--
ALTER TABLE `team_members`
  MODIFY `member_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT for table `user_addresses`
--
ALTER TABLE `user_addresses`
  MODIFY `address_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `user_vouchers`
--
ALTER TABLE `user_vouchers`
  MODIFY `user_voucher_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT for table `vouchers`
--
ALTER TABLE `vouchers`
  MODIFY `voucher_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT for table `wishlist_items`
--
ALTER TABLE `wishlist_items`
  MODIFY `wishlist_item_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `cart_items`
--
ALTER TABLE `cart_items`
  ADD CONSTRAINT `cart_items_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `cart_items_ibfk_2` FOREIGN KEY (`product_variant_id`) REFERENCES `product_variants` (`variant_id`) ON DELETE CASCADE;

--
-- Constraints for table `notifications`
--
ALTER TABLE `notifications`
  ADD CONSTRAINT `notifications_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

--
-- Constraints for table `orders`
--
ALTER TABLE `orders`
  ADD CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

--
-- Constraints for table `order_items`
--
ALTER TABLE `order_items`
  ADD CONSTRAINT `order_items_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `order_items_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE SET NULL;

--
-- Constraints for table `products`
--
ALTER TABLE `products`
  ADD CONSTRAINT `products_ibfk_1` FOREIGN KEY (`brand_id`) REFERENCES `product_brands` (`brand_id`) ON DELETE SET NULL,
  ADD CONSTRAINT `products_ibfk_2` FOREIGN KEY (`category_id`) REFERENCES `product_categories` (`category_id`) ON DELETE SET NULL;

--
-- Constraints for table `product_images`
--
ALTER TABLE `product_images`
  ADD CONSTRAINT `product_images_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE;

--
-- Constraints for table `product_variants`
--
ALTER TABLE `product_variants`
  ADD CONSTRAINT `product_variants_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE;

--
-- Constraints for table `sizes`
--
ALTER TABLE `sizes`
  ADD CONSTRAINT `sizes_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `product_categories` (`category_id`) ON DELETE CASCADE;

--
-- Constraints for table `user_addresses`
--
ALTER TABLE `user_addresses`
  ADD CONSTRAINT `user_id_fk` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

--
-- Constraints for table `user_vouchers`
--
ALTER TABLE `user_vouchers`
  ADD CONSTRAINT `user_vouchers_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `user_vouchers_ibfk_2` FOREIGN KEY (`voucher_id`) REFERENCES `vouchers` (`voucher_id`) ON DELETE CASCADE;

--
-- Constraints for table `wishlist_items`
--
ALTER TABLE `wishlist_items`
  ADD CONSTRAINT `wishlist_items_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `wishlist_items_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
