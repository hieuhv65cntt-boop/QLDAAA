/* ============================================================
   SITE.JS – Hệ thống Quản lý Địa điểm Ẩm thực Nha Trang
   Dark Mode Toggle | Scroll-to-Top | AOS | Utilities
   ============================================================ */

(function () {
    'use strict';

    /* ════════════════════════════════════════════════
       1. DARK MODE TOGGLE
       Lưu trạng thái vào localStorage
       ════════════════════════════════════════════════ */
    const THEME_KEY = 'amthuc-theme';
    const html = document.documentElement;
    const btnToggle = document.getElementById('btnThemeToggle');
    const themeIcon = document.getElementById('themeIcon');

    // Khôi phục theme đã lưu
    function loadTheme() {
        const saved = localStorage.getItem(THEME_KEY);
        if (saved) {
            html.setAttribute('data-bs-theme', saved);
        } else {
            // Tự động phát hiện preference hệ thống
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            html.setAttribute('data-bs-theme', prefersDark ? 'dark' : 'light');
        }
        updateIcon();
    }

    function updateIcon() {
        const isDark = html.getAttribute('data-bs-theme') === 'dark';
        if (themeIcon) {
            themeIcon.className = isDark ? 'bi bi-sun-fill' : 'bi bi-moon-stars';
        }
    }

    function toggleTheme() {
        // Thêm class transition mượt
        html.classList.add('theme-transitioning');

        const isDark = html.getAttribute('data-bs-theme') === 'dark';
        const newTheme = isDark ? 'light' : 'dark';
        html.setAttribute('data-bs-theme', newTheme);
        localStorage.setItem(THEME_KEY, newTheme);
        updateIcon();

        // Xóa class transition sau khi hoàn thành
        setTimeout(function () {
            html.classList.remove('theme-transitioning');
        }, 500);
    }

    if (btnToggle) {
        btnToggle.addEventListener('click', toggleTheme);
    }

    // Lắng nghe thay đổi preference hệ thống
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
        if (!localStorage.getItem(THEME_KEY)) {
            html.setAttribute('data-bs-theme', e.matches ? 'dark' : 'light');
            updateIcon();
        }
    });

    loadTheme();


    /* ════════════════════════════════════════════════
       2. SCROLL TO TOP BUTTON
       ════════════════════════════════════════════════ */
    const btnScrollTop = document.getElementById('btnScrollTop');

    if (btnScrollTop) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 300) {
                btnScrollTop.classList.add('visible');
            } else {
                btnScrollTop.classList.remove('visible');
            }
        }, { passive: true });

        btnScrollTop.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }


    /* ════════════════════════════════════════════════
       3. NAVBAR – Active Link Highlight
       Dựa trên URL hiện tại
       ════════════════════════════════════════════════ */
    function setActiveNavLink() {
        var currentPath = window.location.pathname.toLowerCase();
        var navLinks = document.querySelectorAll('.site-navbar .nav-link');

        navLinks.forEach(function (link) {
            var href = link.getAttribute('href');
            if (href) {
                href = href.toLowerCase();
                link.classList.remove('active');

                if (currentPath === href ||
                    (href !== '/' && currentPath.startsWith(href))) {
                    link.classList.add('active');
                }
                // Trang chủ
                if (href === '/' && (currentPath === '/' || currentPath === '/home' || currentPath === '/home/index')) {
                    link.classList.add('active');
                }
            }
        });
    }
    setActiveNavLink();


    /* ════════════════════════════════════════════════
       4. NAVBAR – Hide on scroll down, show on scroll up
       Chỉ áp dụng trên mobile
       ════════════════════════════════════════════════ */
    var lastScrollY = 0;
    var navbar = document.querySelector('.site-navbar');

    if (navbar && window.innerWidth < 992) {
        window.addEventListener('scroll', function () {
            var currentScrollY = window.scrollY;

            if (currentScrollY > lastScrollY && currentScrollY > 100) {
                navbar.style.transform = 'translateY(-100%)';
            } else {
                navbar.style.transform = 'translateY(0)';
            }
            navbar.style.transition = 'transform 0.3s ease';
            lastScrollY = currentScrollY;
        }, { passive: true });
    }


    /* ════════════════════════════════════════════════
       5. AOS – Animate On Scroll
       Khởi tạo thư viện AOS
       ════════════════════════════════════════════════ */
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 600,
            easing: 'ease-out-cubic',
            once: true,
            offset: 50,
            delay: 0
        });
    }


    /* ════════════════════════════════════════════════
       6. AUTO-DISMISS ALERTS
       Tự đóng thông báo sau 5 giây
       ════════════════════════════════════════════════ */
    var alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 5000);
    });


    /* ════════════════════════════════════════════════
       7. TOOLTIPS – Khởi tạo Bootstrap Tooltips
       ════════════════════════════════════════════════ */
    var tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function (el) {
        new bootstrap.Tooltip(el);
    });


    /* ════════════════════════════════════════════════
       8. MOBILE MENU – Tự đóng khi click link
       ════════════════════════════════════════════════ */
    var navbarCollapse = document.getElementById('navbarMain');
    if (navbarCollapse) {
        var mobileLinks = navbarCollapse.querySelectorAll('.nav-link');
        mobileLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                var bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse) {
                    bsCollapse.hide();
                }
            });
        });
    }


    /* ════════════════════════════════════════════════
       9. FORM VALIDATION – Bootstrap 5 custom validation
       ════════════════════════════════════════════════ */
    var forms = document.querySelectorAll('.needs-validation');
    forms.forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });


    /* ════════════════════════════════════════════════
       10. IMAGE LAZY LOADING (fallback)
       Cho trình duyệt không hỗ trợ loading="lazy"
       ════════════════════════════════════════════════ */
    if ('IntersectionObserver' in window) {
        var lazyImages = document.querySelectorAll('img[data-src]');
        var imageObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var img = entry.target;
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    imageObserver.unobserve(img);
                }
            });
        });
        lazyImages.forEach(function (img) {
            imageObserver.observe(img);
        });
    }


    /* ════════════════════════════════════════════════
       11. COUNTER ANIMATION
       Dùng cho thống kê (data-count="120")
       ════════════════════════════════════════════════ */
    function animateCounters() {
        var counters = document.querySelectorAll('[data-count]');
        counters.forEach(function (counter) {
            if (counter.dataset.animated) return;

            var target = parseInt(counter.dataset.count);
            var duration = 1500;
            var start = 0;
            var startTime = null;

            function step(timestamp) {
                if (!startTime) startTime = timestamp;
                var progress = Math.min((timestamp - startTime) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3); // easeOutCubic
                counter.textContent = Math.floor(eased * target);
                if (progress < 1) {
                    requestAnimationFrame(step);
                } else {
                    counter.textContent = target;
                }
            }

            requestAnimationFrame(step);
            counter.dataset.animated = 'true';
        });
    }

    if ('IntersectionObserver' in window) {
        var counterObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animateCounters();
                }
            });
        }, { threshold: 0.5 });

        var counterSection = document.querySelector('[data-count]');
        if (counterSection) {
            counterObserver.observe(counterSection);
        }
    }

})();
