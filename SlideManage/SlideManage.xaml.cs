using System;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Media;

using System.Windows.Media.Animation;

using System.Windows.Navigation;



namespace login_store

{

    public partial class SlideManage : Window

    {

        public static SlideManage Instance;

        private bool isFirstPage = true;



        public SlideManage()

        {

            InitializeComponent();

            Instance = this;

            NavigateWithSlide(new LoginPage(this));

        }



        public void NavigateWithSlide(Page newPage, bool slideToLeft = false)

        {

            if (isFirstPage)

            {

                MainFrame.Content = newPage;

                isFirstPage = false;

                return;

            }



            Frame overlayFrame = new Frame

            {

                Content = newPage,

                NavigationUIVisibility = NavigationUIVisibility.Hidden,

                Opacity = 0.0 // (เพิ่ม) 1. เริ่มต้นให้โปร่งใส

            };



            double startX = slideToLeft ? -this.ActualWidth : this.ActualWidth;

            overlayFrame.RenderTransform = new TranslateTransform(startX, 0);

            MainGrid.Children.Add(overlayFrame);



            // 6. สร้างแอนิเมชั่นสไลด์ "เข้า"

            //var animationIn = new DoubleAnimation

            //{

            //    From = startX,

            //    To = 0,

            //    Duration = TimeSpan.FromMilliseconds(600), // (ปรับ) 1. เพิ่มเวลาให้พอกับการเด้ง



            //    // (แทนที่) 2. เปลี่ยนจาก QuadraticEase เป็น BackEase

            //    // EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }

            //    EasingFunction = new BackEase

            //    {

            //        EasingMode = EasingMode.EaseOut,

            //        Amplitude = 0.4 // (ปรับ) 3. ตัวเลขนี้คือ "ความแรง" ของการเด้ง (0.3 กำลังดี)

            //    }

            //};



            var animationIn = new DoubleAnimation

            {

                From = startX,

                To = 0,

                Duration = TimeSpan.FromMilliseconds(500), // (ปรับ) 2. เพิ่มเวลาเล็กน้อย

                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }

            };



            // (เพิ่ม) 3. สร้างแอนิเมชั่น "จางเข้า" (Fade-in)

            var animationFadeIn = new DoubleAnimation

            {

                From = 0.0, // จากโปร่งใส

                To = 1.0,   // ไปทึบแสง

                Duration = TimeSpan.FromMilliseconds(400) // (ปรับ) 4. ใช้เวลาเท่ากัน

            };



            animationIn.Completed += (s, e) =>

            {

                MainFrame.Content = newPage;

                MainFrame.Opacity = 1.0; // (เพิ่ม) 5. ทำให้ Frame หลักทึบแสง (เผื่อไว้)

                MainGrid.Children.Remove(overlayFrame);

            };



            // 8. เริ่มสไลด์!

            (overlayFrame.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.XProperty, animationIn);



            // (เพิ่ม) 6. เริ่ม Fade-in!

            overlayFrame.BeginAnimation(Frame.OpacityProperty, animationFadeIn);

        }

    }

}