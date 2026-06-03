CREATE DATABASE QuanLyAmThucNhaTrang;
GO
 
USE QuanLyAmThucNhaTrang;
GO
 
-- ============================================================
-- BƯỚC 2: TẠO CÁC BẢNG DỮ LIỆU (10 bảng)
-- ============================================================
 
-- -----------------------------------------------
-- 1. Bảng TAIKHOAN
-- Lưu thông tin tài khoản người dùng
-- 3 loại: KhachHang, ChuCSKD, QuanTriVien
-- -----------------------------------------------
CREATE TABLE TAIKHOAN (
    MaTK            INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap     VARCHAR(30)     NOT NULL,
    MatKhau         VARCHAR(255)    NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,
    Email           VARCHAR(100)    NULL,
    SDT             VARCHAR(15)     NULL,
    LoaiTK          VARCHAR(20)     NOT NULL,
    AvatarUrl       VARCHAR(500)    NULL,
    NgayTao         DATE            NOT NULL DEFAULT GETDATE(),
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'HoatDong',
 
    -- Ràng buộc
    CONSTRAINT UQ_TaiKhoan_TenDangNhap  UNIQUE (TenDangNhap),
    CONSTRAINT UQ_TaiKhoan_Email        UNIQUE (Email),
    CONSTRAINT UQ_TaiKhoan_SDT          UNIQUE (SDT),
    CONSTRAINT CK_TaiKhoan_LoaiTK       CHECK (LoaiTK IN ('KhachHang', 'ChuCSKD', 'QuanTriVien')),
    CONSTRAINT CK_TaiKhoan_TrangThai    CHECK (TrangThai IN ('HoatDong', 'BiKhoa'))
);
GO
 
-- -----------------------------------------------
-- 2. Bảng DANHMUC
-- Phân loại ẩm thực (Quán ăn, Nhà hàng, Cà phê...)
-- -----------------------------------------------
CREATE TABLE DANHMUC (
    MaDM            INT IDENTITY(1,1) PRIMARY KEY,
    TenDM           NVARCHAR(100)   NOT NULL,
    MoTa            NVARCHAR(300)   NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'HoatDong',
 
    CONSTRAINT UQ_DanhMuc_TenDM         UNIQUE (TenDM),
    CONSTRAINT CK_DanhMuc_TrangThai     CHECK (TrangThai IN ('HoatDong', 'NgungSuDung'))
);
GO
 
-- -----------------------------------------------
-- 3. Bảng KHUVUC
-- Danh sách khu vực trên địa bàn TP Nha Trang
-- ToaDoTrungTam: dùng để di chuyển camera Google Maps
-- -----------------------------------------------
CREATE TABLE KHUVUC (
    MaKV            INT IDENTITY(1,1) PRIMARY KEY,
    TenKV           NVARCHAR(100)   NOT NULL,
    MoTa            NVARCHAR(300)   NULL,
    ToaDoTrungTam   VARCHAR(30)     NULL,
 
    CONSTRAINT UQ_KhuVuc_TenKV          UNIQUE (TenKV)
);
GO
 
-- -----------------------------------------------
-- 4. Bảng DIADIEM
-- Lưu thông tin địa điểm ẩm thực
-- ViDo, KinhDo: tọa độ cho Google Maps
-- DiemDanhGiaTB, SoLuotDanhGia: thuộc tính tính toán
-- -----------------------------------------------
CREATE TABLE DIADIEM (
    MaDD            INT IDENTITY(1,1) PRIMARY KEY,
    TenDD           NVARCHAR(200)   NOT NULL,
    DiaChiChiTiet   NVARCHAR(300)   NOT NULL,
    ViDo            FLOAT           NOT NULL,       -- Latitude (Google Maps)
    KinhDo          FLOAT           NOT NULL,       -- Longitude (Google Maps)
    SDT             VARCHAR(15)     NULL,            -- SĐT liên hệ quán
    GioMoCua        TIME            NULL,
    GioDongCua      TIME            NULL,
    MoTa            NTEXT           NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'ChoDuyet',
    DiemDanhGiaTB   FLOAT           NOT NULL DEFAULT 0,
    SoLuotDanhGia   INT             NOT NULL DEFAULT 0,
    NgayDangKy      DATE            NOT NULL DEFAULT GETDATE(),
	LyDoTuChoi NVARCHAR(500) NULL,
	MaDD_Goc INT NULL,
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN (chủ quán)
    MaDM            INT             NOT NULL,        -- FK → DANHMUC
    MaKV            INT             NOT NULL,        -- FK → KHUVUC
 
    CONSTRAINT CK_DiaDiem_TrangThai     CHECK (TrangThai IN ('ChoDuyet', 'ChoDuyetSua', 'DangHoatDong', 'TamNgung', 'TuChoi', 'TuChoiSua')),
    CONSTRAINT CK_DiaDiem_DiemTB        CHECK (DiemDanhGiaTB >= 0 AND DiemDanhGiaTB <= 5),
    CONSTRAINT CK_DiaDiem_SoLuot        CHECK (SoLuotDanhGia >= 0),
    CONSTRAINT FK_DiaDiem_TaiKhoan      FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK),
    CONSTRAINT FK_DiaDiem_DanhMuc       FOREIGN KEY (MaDM) REFERENCES DANHMUC(MaDM),
    CONSTRAINT FK_DiaDiem_KhuVuc        FOREIGN KEY (MaKV) REFERENCES KHUVUC(MaKV),
	CONSTRAINT FK_DiaDiem_DiaDiemGoc    FOREIGN KEY (MaDD_Goc) REFERENCES DIADIEM(MaDD)
);
GO

 
-- -----------------------------------------------
-- 5. Bảng DANHGIA
-- Lưu đánh giá của khách hàng về địa điểm
-- Ràng buộc: mỗi TK chỉ đánh giá 1 lần / ĐĐ (QĐ3)
-- -----------------------------------------------
CREATE TABLE DANHGIA (
    MaDG            INT IDENTITY(1,1) PRIMARY KEY,
    SoSao           INT             NOT NULL,
    NoiDung         NTEXT           NULL,
    NgayDanhGia     DATETIME        NOT NULL DEFAULT GETDATE(),
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'HienThi',
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN (khách hàng)
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT CK_DanhGia_SoSao        CHECK (SoSao >= 1 AND SoSao <= 5),
    CONSTRAINT CK_DanhGia_TrangThai    CHECK (TrangThai IN ('HienThi', 'DaAn', 'DaXoa')),
    CONSTRAINT UQ_DanhGia_TK_DD        UNIQUE (MaTK, MaDD),    -- QĐ3: mỗi TK chỉ ĐG 1 lần / ĐĐ
    CONSTRAINT FK_DanhGia_TaiKhoan     FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK),
    CONSTRAINT FK_DanhGia_DiaDiem      FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- -----------------------------------------------
-- 6. Bảng PHANHOI
-- Phản hồi của chủ quán đối với đánh giá
-- Quan hệ 1-1 với DANHGIA (MaDG là Unique)
-- -----------------------------------------------
CREATE TABLE PHANHOI (
    MaPH            INT IDENTITY(1,1) PRIMARY KEY,
    NoiDung         NTEXT           NOT NULL,
    NgayPhanHoi     DATETIME        NOT NULL DEFAULT GETDATE(),
    MaDG            INT             NOT NULL,        -- FK → DANHGIA (1-1)
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN (chủ quán)
 
    CONSTRAINT UQ_PhanHoi_MaDG         UNIQUE (MaDG), -- 1 ĐG chỉ có 1 PH
    CONSTRAINT FK_PhanHoi_DanhGia      FOREIGN KEY (MaDG) REFERENCES DANHGIA(MaDG),
    CONSTRAINT FK_PhanHoi_TaiKhoan     FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK)
);
GO
 
-- -----------------------------------------------
-- 7. Bảng YEUTHICH
-- Bảng trung gian: Tài khoản ↔ Địa điểm (m-n)
-- Ràng buộc: mỗi TK chỉ lưu 1 lần / ĐĐ (QĐ4)
-- -----------------------------------------------
CREATE TABLE YEUTHICH (
    MaYT            INT IDENTITY(1,1) PRIMARY KEY,
    NgayLuu         DATETIME        NOT NULL DEFAULT GETDATE(),
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT UQ_YeuThich_TK_DD       UNIQUE (MaTK, MaDD),    -- QĐ4
    CONSTRAINT FK_YeuThich_TaiKhoan    FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK),
    CONSTRAINT FK_YeuThich_DiaDiem     FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- -----------------------------------------------
-- 8. Bảng KHUYENMAI
-- Chương trình khuyến mãi của địa điểm
-- Ràng buộc: NgayBatDau <= NgayKetThuc (QĐ7)
-- -----------------------------------------------
CREATE TABLE KHUYENMAI (
    MaKM            INT IDENTITY(1,1) PRIMARY KEY,
    TenKM           NVARCHAR(200)   NOT NULL,
    NgayBatDau      DATE            NOT NULL,
    NgayKetThuc     DATE            NOT NULL,
    NoiDungUuDai    NTEXT           NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'ConHieuLuc',
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT CK_KhuyenMai_Ngay       CHECK (NgayBatDau <= NgayKetThuc),   -- QĐ7
    CONSTRAINT CK_KhuyenMai_TrangThai  CHECK (TrangThai IN ('ConHieuLuc', 'HetHan')),
    CONSTRAINT FK_KhuyenMai_DiaDiem    FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- -----------------------------------------------
-- 9. Bảng HINHANH
-- Hình ảnh đính kèm của địa điểm
-- -----------------------------------------------
CREATE TABLE HINHANH (
    MaHA            INT IDENTITY(1,1) PRIMARY KEY,
    DuongDan        VARCHAR(500)    NOT NULL,
    LoaiHinhAnh     VARCHAR(30)     NULL,
    ThuTu           INT             NOT NULL DEFAULT 0,
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT CK_HinhAnh_Loai         CHECK (LoaiHinhAnh IN ('MatTien', 'ThucDon', 'KhongGian', 'DanhGia') OR LoaiHinhAnh IS NULL),
    CONSTRAINT FK_HinhAnh_DiaDiem      FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- ============================================================
-- BƯỚC 3: TẠO INDEX (tối ưu truy vấn)
-- ============================================================
 
-- Index tìm kiếm địa điểm theo trạng thái
CREATE INDEX IX_DiaDiem_TrangThai ON DIADIEM(TrangThai);
 
-- Index tìm kiếm theo danh mục, khu vực
CREATE INDEX IX_DiaDiem_MaDM ON DIADIEM(MaDM);
CREATE INDEX IX_DiaDiem_MaKV ON DIADIEM(MaKV);
 
-- Index sắp xếp theo điểm đánh giá TB
CREATE INDEX IX_DiaDiem_DiemTB ON DIADIEM(DiemDanhGiaTB DESC);
 
-- Index đánh giá theo địa điểm
CREATE INDEX IX_DanhGia_MaDD ON DANHGIA(MaDD);
 
-- Index yêu thích theo tài khoản
CREATE INDEX IX_YeuThich_MaTK ON YEUTHICH(MaTK);
 
-- Index khuyến mãi theo địa điểm
CREATE INDEX IX_KhuyenMai_MaDD ON KHUYENMAI(MaDD);
 
-- Index hình ảnh theo địa điểm
CREATE INDEX IX_HinhAnh_MaDD ON HINHANH(MaDD);
GO
 
-- ============================================================
-- BƯỚC 4: CHÈN DỮ LIỆU MẪU (SEED DATA)
-- ============================================================
 
-- -----------------------------------------------
-- 4.2. Dữ liệu DANHMUC (Danh mục ẩm thực)
-- -----------------------------------------------
INSERT INTO DANHMUC (TenDM, MoTa, TrangThai) VALUES
    (N'Quán ăn',                N'Quán ăn bình dân, cơm, phở, bún, hủ tiếu...',       N'HoatDong'),
    (N'Nhà hàng',               N'Nhà hàng cao cấp, hải sản, đặc sản...',             N'HoatDong'),
    (N'Quán cà phê',            N'Cà phê, trà, nước giải khát, sinh tố...',           N'HoatDong'),
    (N'Quán bar / pub',         N'Bar, pub, cocktail, nhạc sống...',                   N'HoatDong'),
    (N'Ẩm thực đường phố',     N'Xe đẩy, gánh hàng rong, vỉa hè, chợ đêm...',      N'HoatDong'),
    (N'Tiệm bánh',              N'Bánh ngọt, bakery, dessert, kem...',                N'HoatDong');
GO
 
-- -----------------------------------------------
-- 4.3. Dữ liệu KHUVUC (Khu vực TP Nha Trang)
-- ToaDoTrungTam: dùng cho map.panTo() khi lọc khu vực
-- -----------------------------------------------
INSERT INTO KHUVUC (TenKV, MoTa, ToaDoTrungTam) VALUES
    (N'Trung tâm TP',   N'Khu vực trung tâm thành phố Nha Trang',    '12.2388,109.1967'),
    (N'Vĩnh Hải',       N'Phường Vĩnh Hải, phía Bắc thành phố',      '12.2697,109.1892'),
    (N'Vĩnh Phước',     N'Phường Vĩnh Phước',                          '12.2562,109.1878'),
    (N'Phước Hải',       N'Phường Phước Hải, gần biển',                '12.2451,109.1953'),
    (N'Lộc Thọ',        N'Phường Lộc Thọ, khu phố Tây',               '12.2389,109.1958'),
    (N'Vĩnh Nguyên',    N'Phường Vĩnh Nguyên, gần cảng',              '12.2200,109.2100'),
    (N'Phước Tân',       N'Phường Phước Tân',                           '12.2520,109.2020'),
    (N'Vĩnh Thọ',       N'Phường Vĩnh Thọ',                           '12.2630,109.1950'),
    (N'Phước Long',      N'Phường Phước Long',                          '12.2350,109.2050'),
    (N'Vĩnh Trường',    N'Phường Vĩnh Trường, phía Nam',              '12.2150,109.1980');
GO
 
-- -----------------------------------------------
-- 4.4. Tài khoản Quản trị viên mặc định
-- Mật khẩu: Admin@123 (đã hash SHA256 - thay bằng hash thật khi triển khai)
-- -----------------------------------------------
INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, HoTen, Email, SDT, LoaiTK, NgayTao, TrangThai) VALUES
    ('admin', 
     'A4B6157319B1C84EF2A0BA1C40B365D7C2CC1C0F7E1F4A7E3C4D5F6A7B8C9D0E',  -- hash mẫu, thay bằng hash thật
     N'Quản trị viên', 
     'admin@amthucnhatrang.vn', 
     '0258.1234567', 
     'QuanTriVien', 
     GETDATE(), 
     'HoatDong');
GO
 
-- -----------------------------------------------
-- 4.5. Tài khoản mẫu (Khách hàng + Chủ CSKD)
-- Dùng để test hệ thống
-- -----------------------------------------------
INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, HoTen, Email, SDT, LoaiTK, NgayTao, TrangThai) VALUES
    ('khachhang01', 
     'HASH_MAU_MK_KHACHHANG01', 
     N'Nguyễn Văn An', 
     'nguyenvanan@gmail.com', 
     '0901234567', 
     'KhachHang', 
     GETDATE(), 
     'HoatDong'),
 
    ('khachhang02', 
     'HASH_MAU_MK_KHACHHANG02', 
     N'Trần Thị Bích', 
     'tranthibich@gmail.com', 
     '0912345678', 
     'KhachHang', 
     GETDATE(), 
     'HoatDong'),
 
    ('chuquan01', 
     'HASH_MAU_MK_CHUQUAN01', 
     N'Lê Minh Tuấn', 
     'leminhtuan@gmail.com', 
     '0923456789', 
     'ChuCSKD', 
     GETDATE(), 
     'HoatDong'),
 
    ('chuquan02', 
     'HASH_MAU_MK_CHUQUAN02', 
     N'Phạm Thị Hương', 
     'phamthihuong@gmail.com', 
     '0934567890', 
     'ChuCSKD', 
     GETDATE(), 
     'HoatDong');
GO
 
-- -----------------------------------------------
-- 4.6. Dữ liệu mẫu DIADIEM
-- Các địa điểm ẩm thực nổi tiếng Nha Trang
-- MaTK = 4,5 (chuquan01, chuquan02)
-- -----------------------------------------------
INSERT INTO DIADIEM (TenDD, DiaChiChiTiet, ViDo, KinhDo, SDT, GioMoCua, GioDongCua, MoTa, TrangThai, DiemDanhGiaTB, SoLuotDanhGia, NgayDangKy, MaTK, MaDM, MaKV) VALUES
    (N'Bún cá Hà Dừa',
     N'1A Nguyễn Đình Chiểu, Phước Hải, Nha Trang',
     12.2451, 109.1953,
     '0258.3524668',
     '06:00', '14:00',
     N'Quán bún cá nổi tiếng nhất Nha Trang, đông khách từ sáng sớm. Nước dùng ngọt tự nhiên, cá tươi hàng ngày.',
     'DangHoatDong', 4.5, 120,
     '2026-01-15', 4, 1, 4),
 
    (N'Nem nướng Ninh Hòa - Đặng Văn Quyên',
     N'16A Lãn Ông, Lộc Thọ, Nha Trang',
     12.2389, 109.1958,
     '0258.3825391',
     '10:00', '21:00',
     N'Nem nướng Ninh Hòa chính gốc, phục vụ kèm bánh tráng, rau sống và nước chấm đặc biệt.',
     'DangHoatDong', 4.3, 85,
     '2026-01-20', 4, 1, 5),
 
    (N'Nhà hàng Hải sản Louisiane Brewhouse',
     N'29 Trần Phú, Lộc Thọ, Nha Trang',
     12.2420, 109.1935,
     '0258.3521948',
     '07:00', '23:00',
     N'Nhà hàng hải sản cao cấp ven biển, có bể bơi, phong cách phương Tây kết hợp ẩm thực Việt.',
     'DangHoatDong', 4.0, 200,
     '2026-02-01', 5, 2, 5),
 
    (N'Bánh căn Cô Ba',
     N'51 Tô Hiến Thành, Phước Hải, Nha Trang',
     12.2460, 109.1940,
     '0905.123456',
     '15:00', '21:00',
     N'Bánh căn truyền thống Nha Trang, nướng than hồng, ăn kèm mỡ hành và nước mắm pha chua ngọt.',
     'DangHoatDong', 4.7, 95,
     '2026-02-10', 5, 5, 4),
 
    (N'Cà phê Rainforest',
     N'78B Trần Quang Khải, Lộc Thọ, Nha Trang',
     12.2395, 109.1970,
     '0258.6688899',
     '07:00', '22:00',
     N'Quán cà phê phong cách rừng nhiệt đới giữa lòng thành phố, không gian xanh mát, thích hợp làm việc.',
     'DangHoatDong', 4.2, 60,
     '2026-02-15', 4, 3, 5),
 
    (N'Phở bò Hùng - chợ Đầm',
     N'Chợ Đầm, Vĩnh Phước, Nha Trang',
     12.2562, 109.1878,
     '0258.3510123',
     '05:30', '10:00',
     N'Phở bò truyền thống, nước dùng ninh xương 12 tiếng, phục vụ từ sáng sớm.',
     'ChoDuyet', 0, 0,
     '2026-03-01', 5, 1, 3);
GO
 
-- -----------------------------------------------
-- 4.7. Dữ liệu mẫu HINHANH
-- -----------------------------------------------
INSERT INTO HINHANH (DuongDan, LoaiHinhAnh, ThuTu, MaDD) VALUES
    ('/images/uploads/bunca_mattien.jpg',    'MatTien',    1, 1),
    ('/images/uploads/bunca_thucdon.jpg',    'ThucDon',    2, 1),
    ('/images/uploads/bunca_khonggian.jpg',  'KhongGian',  3, 1),
    ('/images/uploads/nem_mattien.jpg',      'MatTien',    1, 2),
    ('/images/uploads/nem_thucdon.jpg',      'ThucDon',    2, 2),
    ('/images/uploads/louisiane_mattien.jpg', 'MatTien',   1, 3),
    ('/images/uploads/louisiane_bien.jpg',   'KhongGian',  2, 3),
    ('/images/uploads/banhcan_mattien.jpg',  'MatTien',    1, 4),
    ('/images/uploads/banhcan_thucdon.jpg',  'ThucDon',    2, 4),
    ('/images/uploads/cafe_mattien.jpg',     'MatTien',    1, 5),
    ('/images/uploads/cafe_khonggian.jpg',   'KhongGian',  2, 5);
GO
 
-- -----------------------------------------------
-- 4.8. Dữ liệu mẫu DANHGIA
-- MaTK = 2 (khachhang01), MaTK = 3 (khachhang02)
-- -----------------------------------------------
INSERT INTO DANHGIA (SoSao, NoiDung, NgayDanhGia, TrangThai, MaTK, MaDD) VALUES
    (5, N'Bún cá tuyệt vời, nước dùng ngọt tự nhiên, cá rất tươi. Sẽ quay lại lần nữa!', 
     '2026-03-10 08:30:00', 'HienThi', 2, 1),
 
    (4, N'Nem nướng ngon, nước chấm đậm đà. Tuy nhiên phải chờ hơi lâu vào cuối tuần.', 
     '2026-03-12 12:15:00', 'HienThi', 2, 2),
 
    (4, N'Nhà hàng đẹp, view biển tuyệt vời. Giá hơi cao nhưng chất lượng tương xứng.', 
     '2026-03-15 19:00:00', 'HienThi', 3, 3),
 
    (5, N'Bánh căn nướng than cực ngon, mỡ hành thơm phức. Quán nhỏ nhưng ấm cúng.', 
     '2026-03-18 17:30:00', 'HienThi', 3, 4),
 
    (4, N'Không gian đẹp, cà phê ngon. Rất thích hợp để ngồi làm việc buổi sáng.', 
     '2026-03-20 09:00:00', 'HienThi', 2, 5);
GO
 
-- -----------------------------------------------
-- 4.9. Dữ liệu mẫu PHANHOI
-- -----------------------------------------------
INSERT INTO PHANHOI (NoiDung, NgayPhanHoi, MaDG, MaTK) VALUES
    (N'Cảm ơn bạn đã ủng hộ! Hẹn gặp lại bạn nhé!', 
     '2026-03-10 14:00:00', 1, 4),
 
    (N'Cảm ơn góp ý! Chúng tôi sẽ cải thiện tốc độ phục vụ vào cuối tuần.', 
     '2026-03-13 10:00:00', 2, 4);
GO
 
-- -----------------------------------------------
-- 4.10. Dữ liệu mẫu YEUTHICH
-- -----------------------------------------------
INSERT INTO YEUTHICH (NgayLuu, MaTK, MaDD) VALUES
    (GETDATE(), 2, 1),   -- khachhang01 thích Bún cá
    (GETDATE(), 2, 4),   -- khachhang01 thích Bánh căn
    (GETDATE(), 3, 3),   -- khachhang02 thích Louisiane
    (GETDATE(), 3, 5);   -- khachhang02 thích Cà phê
GO
 
-- -----------------------------------------------
-- 4.11. Dữ liệu mẫu KHUYENMAI
-- -----------------------------------------------
INSERT INTO KHUYENMAI (TenKM, NgayBatDau, NgayKetThuc, NoiDungUuDai, TrangThai, MaDD) VALUES
    (N'Giảm 20% dịp hè 2026',
     '2026-06-01', '2026-08-31',
     N'Giảm 20% tổng hóa đơn cho khách hàng đặt bàn trước qua hệ thống. Áp dụng từ T2 đến T6.',
     'ConHieuLuc', 3),
 
    (N'Mua 2 tặng 1 bánh căn',
     '2026-05-15', '2026-06-15',
     N'Mua 2 phần bánh căn tặng 1 phần. Áp dụng tại quán, không áp dụng mang đi.',
     'ConHieuLuc', 4),
 
    (N'Free cà phê thứ 2',
     '2026-05-01', '2026-05-31',
     N'Mua 1 ly cà phê bất kỳ, tặng 1 ly cà phê đen nhỏ. Áp dụng mỗi ngày.',
     'ConHieuLuc', 5);
GO