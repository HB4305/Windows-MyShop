-- ============================================================
-- MyShop Seed Data
-- Run in Supabase SQL Editor (paste & execute)
-- ============================================================

-- Clean slate (order matters for FK) — does NOT touch users
TRUNCATE supplydetails, supplyorders, orderdetails, customerorders, shifts,
         sportitem_variants, sportitems, categories, customers, suppliers
RESTART IDENTITY CASCADE;

-- USERS: not inserted — using existing accounts:
--   id=2  admin@prosport.com  (owner)
--   id=3  sale@prosport.com   (sale)
--   id=4  tkinculi@gmail.com  (sale)
--   id=5  sale1@prosport.com  (sale)
    
-- ============================================================
-- CATEGORIES
-- ============================================================
INSERT INTO categories (id, name, description) VALUES
(1, 'Running Shoes',   'Performance footwear for running'),
(2, 'Football Shoes',  'Cleats and futsal shoes'),
(3, 'Basketball Shoes','High-top shoes for basketball'),
(4, 'Tennis Shoes',    'Hard court and clay court shoes'),
(5, 'Sport Tops',      'Training tops and jerseys'),
(6, 'Sport Bottoms',   'Shorts and training pants'),
(7, 'Accessories',     'Socks, headbands, bags, bottles, and more');

SELECT setval('categories_id_seq', 7);

-- ============================================================
-- SUPPLIERS
-- ============================================================
INSERT INTO suppliers (id, name, contact_phone, supplier_type) VALUES
(1, 'Nike Vietnam Distribution',  '0901234567', 'Brand'),
(2, 'Adidas SEA Wholesale',       '0912345678', 'Brand'),
(3, 'Puma VN Import',             '0923456789', 'Brand'),
(4, 'New Balance Asia',           '0934567890', 'Brand'),
(5, 'Local Sports Wholesale',     '0845678901', 'Distributor');

SELECT setval('suppliers_id_seq', 5);

-- ============================================================
-- SPORT ITEMS (products)
-- ============================================================
INSERT INTO sportitems (id, category_id, name, cost_price, selling_price, stock_quantity, low_stock_threshold, image_urls, description) VALUES
-- Giày chạy bộ
(1,  1, 'Nike Air Zoom Pegasus 41',           57.00, 113.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600&q=80'],
 'Giày chạy bộ hàng đầu với đệm ZoomX siêu nhẹ, phù hợp mọi địa hình'),
(2,  1, 'Adidas Ultraboost 24',               66.00, 129.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=600&q=80'],
 'Boost midsole mang lại cảm giác đệm vượt trội, thiết kế upper Primeknit thoáng khí'),
(3,  1, 'New Balance Fresh Foam X 1080v13',    60.00, 117.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1539185441755-769473a23570?w=600&q=80'],
 'Đệm Fresh Foam X cực êm, lý tưởng cho chạy đường dài'),
(4,  1, 'Nike React Infinity Run Flyknit 3',   54.00, 108.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1600185365483-26d7a4cc7519?w=600&q=80'],
 'Thiết kế giảm chấn thương, phù hợp runner mới bắt đầu'),
(5,  1, 'Puma Velocity Nitro 2',               38.00,  78.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=600&q=80'],
 'Nitro foam nhẹ và bền bỉ, giá tốt cho người chạy hàng ngày'),

-- Giày bóng đá
(6,  2, 'Nike Mercurial Superfly 10 Elite FG',  82.00, 168.00, 0, 3,
 ARRAY['https://images.unsplash.com/photo-1511886929837-354d827aae26?w=600&q=80'],
 'Đinh tự nhiên cao cấp, vải Vaporposite+ siêu nhẹ cho tốc độ tối đa'),
(7,  2, 'Adidas Predator 24 Elite FG',          78.00, 156.00, 0, 3,
 ARRAY['https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa?w=600&q=80'],
 'Công nghệ Zone Skin giúp kiểm soát bóng hoàn hảo'),
(8,  2, 'Puma Future 7 Ultimate FG/AG',         69.00, 137.00, 0, 3,
 ARRAY['https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa?w=600&q=80'],
 'Dây buộc FUZIONFIT+ ôm sát chân, linh hoạt trên nhiều mặt sân'),

-- Giày bóng rổ
(9,  3, 'Nike LeBron XXII',                     92.00, 180.00, 0, 3,
 ARRAY['https://images.unsplash.com/photo-1579338559194-a162d19bf842?w=600&q=80'],
 'Đệm Air Max + Zoom Air kép, hỗ trợ cổ chân chuẩn bóng rổ chuyên nghiệp'),
(10, 3, 'Adidas Harden Vol. 8',                 73.00, 145.00, 0, 3,
 ARRAY['https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80'],
 'Boost toàn đế, ôm chân cực tốt cho guard tốc độ cao'),

-- Giày tennis
(11, 4, 'Nike Court Air Zoom Vapor Pro 2',       49.00,  98.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1549298916-b41d501d3772?w=600&q=80'],
 'Zoom Air mũi giày, phù hợp sân cứng indoor và outdoor'),
(12, 4, 'Adidas Barricade 13',                  46.00,  90.00, 0, 5,
 ARRAY['https://images.unsplash.com/photo-1540539234-c14a20fb7c7b?w=600&q=80'],
 'Đế ngoài ADITUFF siêu bền cho sân đất nện'),

-- Áo thể thao
(13, 5, 'Nike Dri-FIT ADV TechKnit Ultra',       18.00,  35.00, 0, 10,
 ARRAY['https://supersports.com.vn/cdn/shop/files/HV5204-010-1.jpg?v=1742530809&width=1000','https://supersports.com.vn/cdn/shop/files/HV5204-100-1.jpg?v=1765428425&width=1000'],
 'Vải TechKnit thông thoáng, thoát mồ hôi nhanh'),
(14, 5, 'Adidas Designed for Training Tee',      15.00,  29.00, 0, 10,
 ARRAY['https://images.unsplash.com/photo-1562157873-818bc0726f68?w=600&q=80'],
 'AEROREADY hút ẩm tức thì, phù hợp gym và outdoor'),
(15, 5, 'Puma Run Favourite Running Tee',         13.00,  24.00, 0, 10,
 ARRAY['https://images.puma.com/image/upload/f_auto,q_auto,b_rgb:fafafa,w_750,h_750/global/525058/21/fnd/PNA/fmt/png/RUN-FAVORITE-Men''s-Tee'],
 'Vải DryCell thoáng khí nhẹ nhàng cho buổi chạy hàng ngày'),

-- Quần thể thao
(16, 6, 'Nike Dri-FIT Challenger 5" Short',      13.00,  25.00, 0, 10,
 ARRAY['https://www.runningxpert.com/media/catalog/product/cache/e1bfa30f5f000aa573b2ee969a7a0fde/w/p/wp80r_bef-1391_80_.jpg'],
 'Quần short 5 inch nhẹ thoáng, lưới lót bên trong'),
(17, 6, 'Adidas Own the Run Short',              11.00,  23.00, 0, 10,
 ARRAY['https://assets.adidas.com/images/e_trim:EAEEEF/c_lpad,w_iw,h_ih/b_rgb:EAEEEF/w_180,f_auto,q_auto,fl_lossy,c_fill,g_auto/5705e1c915c742ce858af1d607b55581_9366/Own_The_Run_Shorts_Blue_IY0706_000_plp_model.jpg'],
 'AEROREADY, thiết kế phản quang an toàn ban đêm'),

-- Phụ kiện
(18, 7, 'Nike Everyday Cushion Crew Socks (3 đôi)',  5.00,  10.00, 0, 15,
 ARRAY['https://static.nike.com/a/images/t_web_pdp_535_v2/f_auto,u_9ddf04c7-2a9a-4d76-add1-d15af8f0263d,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/dfa68bbe-e102-4e33-9b6e-6763e2a75f19/U+NK+EVERYDAY+CSH+CRW+3PR+132.png'],
 'Cotton cushion đệm gót và mũi, pack 3 đôi'),
(19, 7, 'Adidas Linear Core Backpack',           23.00,  45.00, 0, 8,
 ARRAY['https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=600&q=80'],
 'Ba lô 28L ngăn laptop 15.6", ngăn nước bên hông'),
(20, 7, 'Nike Hyperfuel Water Bottle 1.9L',      11.00,  22.00, 0, 10,
 ARRAY['https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=600&q=80'],
 'Bình nước BPA-free 1.9L, nắp xoắn chống rò rỉ');

SELECT setval('sportitems_id_seq', 20);

-- ============================================================
-- VARIANTS (size + color per product)
-- ============================================================
-- Helper: shoes have sizes 39-44, clothes have S/M/L/XL
-- Colors vary per product

-- Nike Air Zoom Pegasus 41 (id=1) - Black & White
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(1,'39','Black',0,'PEG41-39-BLK'),(1,'40','Black',0,'PEG41-40-BLK'),(1,'41','Black',1,'PEG41-41-BLK'),
(1,'42','Black',1,'PEG41-42-BLK'),(1,'43','Black',0,'PEG41-43-BLK'),(1,'44','Black',0,'PEG41-44-BLK'),
(1,'39','White',0,'PEG41-39-WHT'),(1,'40','White',0,'PEG41-40-WHT'),(1,'41','White',1,'PEG41-41-WHT'),
(1,'42','White',1,'PEG41-42-WHT'),(1,'43','White',0,'PEG41-43-WHT'),(1,'44','White',0,'PEG41-44-WHT');

-- Adidas Ultraboost 24 (id=2) - Core Black & Cloud White
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(2,'39','Core Black',7,'UB24-39-BLK'),(2,'40','Core Black',11,'UB24-40-BLK'),(2,'41','Core Black',14,'UB24-41-BLK'),
(2,'42','Core Black',13,'UB24-42-BLK'),(2,'43','Core Black',9,'UB24-43-BLK'),(2,'44','Core Black',5,'UB24-44-BLK'),
(2,'39','Cloud White',5,'UB24-39-WHT'),(2,'40','Cloud White',9,'UB24-40-WHT'),(2,'41','Cloud White',11,'UB24-41-WHT'),
(2,'42','Cloud White',10,'UB24-42-WHT'),(2,'43','Cloud White',7,'UB24-43-WHT'),(2,'44','Cloud White',3,'UB24-44-WHT');

-- New Balance 1080v13 (id=3)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(3,'39','Eclipse',6,'NB1080-39-ECL'),(3,'40','Eclipse',10,'NB1080-40-ECL'),(3,'41','Eclipse',13,'NB1080-41-ECL'),
(3,'42','Eclipse',12,'NB1080-42-ECL'),(3,'43','Eclipse',8,'NB1080-43-ECL'),(3,'44','Eclipse',4,'NB1080-44-ECL'),
(3,'39','White',5,'NB1080-39-WHT'),(3,'40','White',8,'NB1080-40-WHT'),(3,'41','White',10,'NB1080-41-WHT'),
(3,'42','White',9,'NB1080-42-WHT'),(3,'43','White',6,'NB1080-43-WHT'),(3,'44','White',3,'NB1080-44-WHT');

-- Nike React Infinity Run 3 (id=4)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(4,'39','Black/White',0,'RIF3-39-BW'),(4,'40','Black/White',1,'RIF3-40-BW'),(4,'41','Black/White',1,'RIF3-41-BW'),
(4,'42','Black/White',1,'RIF3-42-BW'),(4,'43','Black/White',0,'RIF3-43-BW'),(4,'44','Black/White',0,'RIF3-44-BW');

-- Puma Velocity Nitro 2 (id=5)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(5,'39','Sunset Glow',6,'VN2-39-SG'),(5,'40','Sunset Glow',9,'VN2-40-SG'),(5,'41','Sunset Glow',12,'VN2-41-SG'),
(5,'42','Sunset Glow',11,'VN2-42-SG'),(5,'43','Sunset Glow',7,'VN2-43-SG'),(5,'44','Sunset Glow',3,'VN2-44-SG'),
(5,'39','Black',5,'VN2-39-BLK'),(5,'40','Black',8,'VN2-40-BLK'),(5,'41','Black',10,'VN2-41-BLK'),
(5,'42','Black',9,'VN2-42-BLK'),(5,'43','Black',6,'VN2-43-BLK'),(5,'44','Black',3,'VN2-44-BLK');

-- Nike Mercurial Superfly 10 (id=6)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(6,'39','Volt/Black',4,'MSF10-39-VB'),(6,'40','Volt/Black',7,'MSF10-40-VB'),(6,'41','Volt/Black',9,'MSF10-41-VB'),
(6,'42','Volt/Black',8,'MSF10-42-VB'),(6,'43','Volt/Black',5,'MSF10-43-VB'),(6,'44','Volt/Black',3,'MSF10-44-VB');

-- Adidas Predator 24 Elite (id=7)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(7,'39','Core Black',4,'PRE24-39-BLK'),(7,'40','Core Black',7,'PRE24-40-BLK'),(7,'41','Core Black',9,'PRE24-41-BLK'),
(7,'42','Core Black',8,'PRE24-42-BLK'),(7,'43','Core Black',5,'PRE24-43-BLK'),(7,'44','Core Black',3,'PRE24-44-BLK');

-- Puma Future 7 (id=8)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(8,'39','Blue/Yellow',3,'FUT7-39-BY'),(8,'40','Blue/Yellow',6,'FUT7-40-BY'),(8,'41','Blue/Yellow',8,'FUT7-41-BY'),
(8,'42','Blue/Yellow',7,'FUT7-42-BY'),(8,'43','Blue/Yellow',4,'FUT7-43-BY'),(8,'44','Blue/Yellow',2,'FUT7-44-BY');

-- Nike LeBron XXII (id=9)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(9,'39','Black/Gold',0,'LBR22-39-BG'),(9,'40','Black/Gold',0,'LBR22-40-BG'),(9,'41','Black/Gold',1,'LBR22-41-BG'),
(9,'42','Black/Gold',0,'LBR22-42-BG'),(9,'43','Black/Gold',0,'LBR22-43-BG'),(9,'44','Black/Gold',0,'LBR22-44-BG'),
(9,'39','White/Red',0,'LBR22-39-WR'),(9,'40','White/Red',0,'LBR22-40-WR'),(9,'41','White/Red',1,'LBR22-41-WR'),
(9,'42','White/Red',0,'LBR22-42-WR'),(9,'43','White/Red',0,'LBR22-43-WR'),(9,'44','White/Red',0,'LBR22-44-WR');

-- Adidas Harden Vol.8 (id=10)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(10,'39','Black',4,'HDN8-39-BLK'),(10,'40','Black',7,'HDN8-40-BLK'),(10,'41','Black',9,'HDN8-41-BLK'),
(10,'42','Black',8,'HDN8-42-BLK'),(10,'43','Black',5,'HDN8-43-BLK'),(10,'44','Black',3,'HDN8-44-BLK');

-- Nike Court Air Zoom Vapor Pro 2 (id=11)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(11,'38','White/Black',5,'CAVP2-38-WB'),(11,'39','White/Black',8,'CAVP2-39-WB'),(11,'40','White/Black',10,'CAVP2-40-WB'),
(11,'41','White/Black',9,'CAVP2-41-WB'),(11,'42','White/Black',7,'CAVP2-42-WB'),(11,'43','White/Black',4,'CAVP2-43-WB');

-- Adidas Barricade 13 (id=12)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(12,'38','White/Green',5,'BAR13-38-WG'),(12,'39','White/Green',8,'BAR13-39-WG'),(12,'40','White/Green',10,'BAR13-40-WG'),
(12,'41','White/Green',9,'BAR13-41-WG'),(12,'42','White/Green',7,'BAR13-42-WG'),(12,'43','White/Green',4,'BAR13-43-WG');

-- Áo Nike Dri-FIT (id=13)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(13,'S','Black',1,'NKDF-S-BLK'),(13,'M','Black',2,'NKDF-M-BLK'),(13,'L','Black',1,'NKDF-L-BLK'),(13,'XL','Black',0,'NKDF-XL-BLK'),
(13,'S','White',1,'NKDF-S-WHT'),(13,'M','White',2,'NKDF-M-WHT'),(13,'L','White',1,'NKDF-L-WHT'),(13,'XL','White',0,'NKDF-XL-WHT');

-- Adidas Training Tee (id=14)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(14,'S','Black',10,'ADTT-S-BLK'),(14,'M','Black',15,'ADTT-M-BLK'),(14,'L','Black',14,'ADTT-L-BLK'),(14,'XL','Black',10,'ADTT-XL-BLK'),
(14,'S','Legend Ink',8,'ADTT-S-INK'),(14,'M','Legend Ink',12,'ADTT-M-INK'),(14,'L','Legend Ink',11,'ADTT-L-INK'),(14,'XL','Legend Ink',7,'ADTT-XL-INK');

-- Puma Run Tee (id=15)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(15,'S','Puma Black',1,'PMRT-S-BLK'),(15,'M','Puma Black',2,'PMRT-M-BLK'),(15,'L','Puma Black',1,'PMRT-L-BLK'),(15,'XL','Puma Black',0,'PMRT-XL-BLK');

-- Nike Shorts (id=16)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(16,'S','Black',10,'NKSH-S-BLK'),(16,'M','Black',14,'NKSH-M-BLK'),(16,'L','Black',13,'NKSH-L-BLK'),(16,'XL','Black',9,'NKSH-XL-BLK'),
(16,'S','Navy',8,'NKSH-S-NVY'),(16,'M','Navy',11,'NKSH-M-NVY'),(16,'L','Navy',10,'NKSH-L-NVY'),(16,'XL','Navy',6,'NKSH-XL-NVY');

-- Adidas Shorts (id=17)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(17,'S','Black',10,'ADSH-S-BLK'),(17,'M','Black',14,'ADSH-M-BLK'),(17,'L','Black',13,'ADSH-L-BLK'),(17,'XL','Black',9,'ADSH-XL-BLK');

-- Socks (id=18) - one size fits all
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(18,'One Size','White',4,'NKSK-OS-WHT'),(18,'One Size','Black',3,'NKSK-OS-BLK');

-- Backpack (id=19) - no size
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(19,NULL,'Black',15,'ADBP-BLK'),(19,NULL,'Grey',10,'ADBP-GRY');

-- Water Bottle (id=20) - no size
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(20,NULL,'Black',20,'NKWB-BLK'),(20,NULL,'Blue',15,'NKWB-BLU');
