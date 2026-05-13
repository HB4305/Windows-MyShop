-- ============================================================
-- PART 7: 30 NEW PRODUCTS (ids 21-50) + ORDER STATUS VARIETY
-- Run after parts 1-6
-- ============================================================

-- ============================================================
-- NEW PRODUCTS (ids 21-50)
-- ============================================================
INSERT INTO sportitems (id, category_id, name, cost_price, selling_price, stock_quantity, low_stock_threshold, image_urls, description) VALUES

-- Running Shoes (cat 1) - ids 21-24
(21, 1, 'Mizuno Wave Rider 27',              46.00,  94.00, 0, 5,
 ARRAY['https://cdn.fleetfeet.com/productMini/products/411415_NP4H_06-copy.jpg'],
 'Wave plate công nghệ Mizuno, đệm êm ổn định cho runner trung cấp'),
(22, 1, 'Adidas Supernova Rise',            38.00,  78.00, 0, 5,
 ARRAY['https://cdn.runrepeat.com/storage/gallery/product_primary/40262/adidas-supernova-rise-21380535-main.jpg'],
 'DREAMSTRIKE+ foam mới, nhẹ hơn Boost 20%, lý tưởng cho daily run'),
(23, 1, 'Under Armour HOVR Sonic 6',        37.00,  74.00, 0, 5,
 ARRAY['https://shop.simon.com/cdn/shop/files/2e93cae86c0d4b648d05149367960acb.jpg?v=1747871417'],
 'HOVR cushioning + MapMyRun connectivity, connected running shoe'),
(24, 1, 'New Balance 574 Classic',          31.00,  62.00, 0, 5,
 ARRAY['https://boathousestores.com/cdn/shop/files/NEB-WL574EVW-CLD-1.jpg?v=1773323373&width=1000'],
 'Lifestyle sneaker kinh điển, đế ENCAP bền bỉ, phù hợp casual và nhẹ tập'),

-- Football Shoes (cat 2) - ids 25-27
(25, 2, 'Nike Phantom GX II Elite FG',     80.00, 163.00, 0, 3,
 ARRAY['https://www.futbolemotion.com/imagesarticulos/237581/750/bota-nike-phantom-gx-ii-elite-fg-mtlc-silver-black-volt-0.webp'],
 'Grip-Knit upper ôm chân như bít tất, kiểm soát bóng đỉnh cao'),
(26, 2, 'Adidas Copa Pure II Elite FG',    75.00, 153.00, 0, 3,
 ARRAY['https://europeansports.com/cdn/shop/files/IG8711_11_FOOTWEAR_Photography_SideLateralBottomView_transparent.png?v=1721929675&width=2400'],
 'Da kangaroo thật, cảm giác chạm bóng cổ điển của dòng Copa'),
(27, 2, 'Puma King Ultimate FG',           65.00, 129.00, 0, 3,
 ARRAY['https://images.puma.com/image/upload/f_auto,q_auto,b_rgb:fafafa,w_600,h_600/global/108303/01/sv01/fnd/PNA/fmt/png/KING-ULTIMATE-Firm-Ground/Artificial-Ground-Men''s-Soccer-Cleats'],
 'Da bê nguyên chất, thiết kế retro hiện đại, tốt cho kỹ thuật'),

-- Basketball Shoes (cat 3) - ids 28-29
(28, 3, 'Nike Giannis Immortality 3',      38.00,  78.00, 0, 5,
 ARRAY['https://www.basketballstore.net/cdn/shop/files/DZ7533-003nike-giannis-immortality-3-scarpe-basket-leggere-0-1000x1000.jpg?v=1711552146&width=1000'],
 'Phiên bản giá tốt của dòng Giannis, Cushlon foam, phù hợp sân trong nhà'),
(29, 3, 'Under Armour Curry 12',           66.00, 133.00, 0, 3,
 ARRAY['https://cdn11.bigcommerce.com/s-4d06e/images/stencil/1024x1024/products/22234/35873/6000736-014_PAIR__61360.1753819486.jpg?c=2'],
 'UA Flow công nghệ không cần outsole cao su, nhẹ và bám sàn cực tốt'),

-- Tennis Shoes (cat 4) - ids 30-31
(30, 4, 'Asics Gel-Resolution 9',          50.00, 102.00, 0, 5,
 ARRAY['https://img.tenniswarehouse-europe.com/fpcache/576/marketing/AGR9PP-cat.jpg'],
 'GEL technology giảm chấn mũi và gót, AHAR outsole siêu bền sân cứng'),
(31, 4, 'Babolat Propulse Fury Clay',      41.00,  82.00, 0, 5,
 ARRAY['https://img.tennis-warehouse.com/watermark/rs.php?path=BMPFWDB-1.jpg&nw=339'],
 'Đế Michelin chuyên sân đất nện, ổn định bên hông vượt trội'),

-- Sport Tops (cat 5) - ids 32-36
(32, 5, 'Under Armour HeatGear Fitted Tee', 11.00,  22.00, 0, 10,
 ARRAY['https://images.footlocker.com/is/image/EBFL2/61518001?wid=600&hei=600'],
 'Compression nhẹ, HeatGear thoát nhiệt nhanh khi cường độ cao'),
(33, 5, 'New Balance Athletics Tee',        10.00,  19.00, 0, 10,
 ARRAY['https://therunhouse.com/cdn/shop/files/mt41253lrc_nb_70_i.webp?v=1715626366'],
 'Cotton blend thoải mái, in logo NB nổi bật, phù hợp gym và đường phố'),
(34, 5, 'Nike Dri-FIT Park VII Jersey',     11.00,  22.00, 0, 10,
 ARRAY['https://xtremesocceronline.com/cdn/shop/files/1-nike-dri-fit-park-vii-bv6710-010.jpg?v=1740628358&width=2000'],
 'Áo đấu bóng đá cơ bản, Dri-FIT thoáng khí, có thể in số và tên'),
(35, 5, 'Adidas Tiro 24 Training Jersey',   12.00,  24.00, 0, 10,
 ARRAY['https://xtremesocceronline.com/cdn/shop/files/1-adidas-tiro-24-training-ij9959.jpg?v=1740626651&width=2000'],
 'AEROREADY tối ưu, thiết kế Tiro cổ điển phù hợp training đội nhóm'),
(36, 5, 'Mizuno Soukyu Polo',              15.00,  29.00, 0, 8,
 ARRAY['https://www.fit2run.com/cdn/shop/files/ms93187bk_70.png?v=1720045568&width=1200'],
 'Polo thể thao cao cấp, Quick Dry, thiết kế lịch sự cho tennis và golf'),

-- Sport Bottoms (cat 6) - ids 37-41
(37, 6, 'Nike Dri-FIT Academy Pant',       15.00,  29.00, 0, 10,
 ARRAY['https://xtremesocceronline.com/cdn/shop/files/1-nike-academy-25-fz9805-010.jpg?v=1745989555&width=2000'],
 'Quần dài training với túi có khóa kéo, phù hợp sân bóng và gym'),
(38, 6, 'Adidas Tiro 24 Training Pant',    14.00,  27.00, 0, 10,
 ARRAY['https://www.bestbuysoccer.com/cdn/shop/files/adidas-tiro-24-competition-training-pant-2319903.jpg?v=1752534041&width=1500'],
 'AEROREADY, mắt cá có thể chỉnh, túi hai bên tiện lợi'),
(39, 6, 'Under Armour Challenger Pant',    13.00,  25.00, 0, 10,
 ARRAY['https://nerionathletics.com/media/catalog/product/cache/2a78c6573da316df0c838f4555c19b70/u/n/under_armour_challenger_ii_training_pants_black_1.jpg'],
 'Woven fabric chống gió nhẹ, cạp chun co giãn thoải mái'),
(40, 6, 'Puma teamGOAL Shorts',             9.00,  17.00, 0, 10,
 ARRAY['https://images.puma.com/image/upload/f_auto,q_auto,b_rgb:fafafa,w_600,h_600/global/705752/03/fnd/PNA/fmt/png/teamGOAL-Men''s-Soccer-Shorts'],
 'Quần short đội nhóm, dryCELL, có thể in logo tùy chỉnh'),
(41, 6, 'New Balance Accelerate Short 5"', 10.00,  20.00, 0, 10,
 ARRAY['https://www.fit2run.com/cdn/shop/files/ms93187bk_70.png?v=1720045568&width=1200'],
 'Short chạy bộ 5 inch, túi khóa kéo sau, co giãn 4 chiều'),

-- Accessories (cat 7) - ids 42-50
(42, 7, 'Adidas Running Socks (3 đôi)',     4.00,   7.50, 0, 15,
 ARRAY['https://img.tennis-warehouse.com/watermark/rs.php?path=AMC3SCW-1.jpg&nw=455'],
 'No-show cushioned, co giãn arch support, pack 3 đôi'),
(43, 7, 'Nike Headband Dri-FIT 2.0',        3.50,   7.00, 0, 20,
 ARRAY['https://img.runningwarehouse.com/watermark/rs.php?path=NISHHBA-BK-1.jpg&nw=455'],
 'Băng đầu Dri-FIT thấm mồ hôi, thiết kế ôm không tuột'),
(44, 7, 'Under Armour Wristband (2 cái)',    3.00,   6.00, 0, 20,
 ARRAY['https://mistertennis.com/media/catalog/product/1/2/1276991-001_1.jpg'],
 'Terry cloth thấm mồ hôi, pack 2 cái, dùng cho tennis và gym'),
(45, 7, 'Adidas Stadium II Backpack',       25.00,  51.00, 0, 8,
 ARRAY['https://surf.soccerpost.com/cdn/shop/files/adidas_stadium_ii_backpack_org_os_1.jpg?v=1748664601'],
 'Ba lô 32L ngăn giày riêng, túi mặt trước lớn, dây điều chỉnh'),
(46, 7, 'Nike Brasilia 9.5 Duffel M',       19.00,  37.00, 0, 8,
 ARRAY['https://www.vsathletics.com/store/images/P/dm3977-522.jpg'],
 'Túi gym 60L, ngăn giày có thông khí, dây đeo vai có đệm'),
(47, 7, 'Puma Phase Backpack',              13.00,  25.00, 0, 10,
 ARRAY['https://fitsole.shop/cdn/shop/files/079943_36_600x.png?v=1761576351'],
 'Ba lô cơ bản 21L, ngăn laptop 13", gọn nhẹ phù hợp daily use'),
(48, 7, 'Adidas Water Bottle 1L',            7.00,  14.00, 0, 15,
 ARRAY['https://www.vsathletics.com/store/images/P/5148626.jpg'],
 'BPA-free 1L, nắp lật tay một tay, in logo Adidas'),
(49, 7, 'Garmin HRM-Dual Heart Rate Monitor', 73.00, 145.00, 0, 5,
 ARRAY['https://cdn.fleetfeet.com/productMini/products/R_010-12883-00_HR_1002.png'],
 'Đo nhịp tim chính xác, kết nối ANT+ và Bluetooth, pin 3.5 năm'),
(50, 7, 'Nike Pro Elite Running Cap',        7.00,  14.00, 0, 15,
 ARRAY['https://performancerunning.com/cdn/shop/files/005dbea2AURORA_FB5625-011_PHSFH001-2000.jpg'],
 'Dri-FIT, lưỡi trai cứng che nắng, phản quang sau mũ');

SELECT setval('sportitems_id_seq', 50);

-- ============================================================
-- VARIANTS FOR NEW PRODUCTS
-- ============================================================

-- Mizuno Wave Rider 27 (id=21)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(21,'39','White/Silver',6,'MWR27-39-WS'),(21,'40','White/Silver',10,'MWR27-40-WS'),(21,'41','White/Silver',13,'MWR27-41-WS'),
(21,'42','White/Silver',12,'MWR27-42-WS'),(21,'43','White/Silver',8,'MWR27-43-WS'),(21,'44','White/Silver',4,'MWR27-44-WS'),
(21,'39','Black',5,'MWR27-39-BLK'),(21,'40','Black',8,'MWR27-40-BLK'),(21,'41','Black',10,'MWR27-41-BLK'),
(21,'42','Black',9,'MWR27-42-BLK'),(21,'43','Black',6,'MWR27-43-BLK'),(21,'44','Black',3,'MWR27-44-BLK');

-- Adidas Supernova Rise (id=22)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(22,'39','Core Black',6,'SNR-39-BLK'),(22,'40','Core Black',10,'SNR-40-BLK'),(22,'41','Core Black',13,'SNR-41-BLK'),
(22,'42','Core Black',12,'SNR-42-BLK'),(22,'43','Core Black',8,'SNR-43-BLK'),(22,'44','Core Black',4,'SNR-44-BLK'),
(22,'39','Cloud White',5,'SNR-39-WHT'),(22,'40','Cloud White',8,'SNR-40-WHT'),(22,'41','Cloud White',10,'SNR-41-WHT'),
(22,'42','Cloud White',9,'SNR-42-WHT'),(22,'43','Cloud White',5,'SNR-43-WHT'),(22,'44','Cloud White',2,'SNR-44-WHT');

-- Under Armour HOVR Sonic 6 (id=23)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(23,'39','Black',6,'HSN6-39-BLK'),(23,'40','Black',10,'HSN6-40-BLK'),(23,'41','Black',13,'HSN6-41-BLK'),
(23,'42','Black',12,'HSN6-42-BLK'),(23,'43','Black',8,'HSN6-43-BLK'),(23,'44','Black',4,'HSN6-44-BLK'),
(23,'39','White',4,'HSN6-39-WHT'),(23,'40','White',7,'HSN6-40-WHT'),(23,'41','White',9,'HSN6-41-WHT'),
(23,'42','White',8,'HSN6-42-WHT'),(23,'43','White',5,'HSN6-43-WHT'),(23,'44','White',2,'HSN6-44-WHT');

-- New Balance 574 (id=24)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(24,'39','Grey/Navy',7,'NB574-39-GN'),(24,'40','Grey/Navy',11,'NB574-40-GN'),(24,'41','Grey/Navy',14,'NB574-41-GN'),
(24,'42','Grey/Navy',13,'NB574-42-GN'),(24,'43','Grey/Navy',9,'NB574-43-GN'),(24,'44','Grey/Navy',5,'NB574-44-GN'),
(24,'39','White/Red',5,'NB574-39-WR'),(24,'40','White/Red',8,'NB574-40-WR'),(24,'41','White/Red',10,'NB574-41-WR'),
(24,'42','White/Red',9,'NB574-42-WR'),(24,'43','White/Red',6,'NB574-43-WR'),(24,'44','White/Red',3,'NB574-44-WR');

-- Nike Phantom GX II (id=25)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(25,'39','Black/Chrome',4,'PGX2-39-BC'),(25,'40','Black/Chrome',7,'PGX2-40-BC'),(25,'41','Black/Chrome',9,'PGX2-41-BC'),
(25,'42','Black/Chrome',8,'PGX2-42-BC'),(25,'43','Black/Chrome',5,'PGX2-43-BC'),(25,'44','Black/Chrome',3,'PGX2-44-BC');

-- Adidas Copa Pure II (id=26)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(26,'39','Core Black',4,'CP2-39-BLK'),(26,'40','Core Black',7,'CP2-40-BLK'),(26,'41','Core Black',9,'CP2-41-BLK'),
(26,'42','Core Black',8,'CP2-42-BLK'),(26,'43','Core Black',5,'CP2-43-BLK'),(26,'44','Core Black',3,'CP2-44-BLK');

-- Puma King Ultimate (id=27)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(27,'39','Puma Black',4,'PKU-39-BLK'),(27,'40','Puma Black',7,'PKU-40-BLK'),(27,'41','Puma Black',9,'PKU-41-BLK'),
(27,'42','Puma Black',8,'PKU-42-BLK'),(27,'43','Puma Black',5,'PKU-43-BLK'),(27,'44','Puma Black',3,'PKU-44-BLK');

-- Nike Giannis Immortality 3 (id=28)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(28,'39','Black',5,'GI3-39-BLK'),(28,'40','Black',9,'GI3-40-BLK'),(28,'41','Black',12,'GI3-41-BLK'),
(28,'42','Black',11,'GI3-42-BLK'),(28,'43','Black',7,'GI3-43-BLK'),(28,'44','Black',4,'GI3-44-BLK'),
(28,'39','White',4,'GI3-39-WHT'),(28,'40','White',7,'GI3-40-WHT'),(28,'41','White',9,'GI3-41-WHT'),
(28,'42','White',8,'GI3-42-WHT'),(28,'43','White',5,'GI3-43-WHT'),(28,'44','White',3,'GI3-44-WHT');

-- Under Armour Curry 12 (id=29)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(29,'39','White/Gold',4,'CRY12-39-WG'),(29,'40','White/Gold',7,'CRY12-40-WG'),(29,'41','White/Gold',9,'CRY12-41-WG'),
(29,'42','White/Gold',8,'CRY12-42-WG'),(29,'43','White/Gold',5,'CRY12-43-WG'),(29,'44','White/Gold',3,'CRY12-44-WG'),
(29,'39','Black',3,'CRY12-39-BLK'),(29,'40','Black',5,'CRY12-40-BLK'),(29,'41','Black',7,'CRY12-41-BLK'),
(29,'42','Black',6,'CRY12-42-BLK'),(29,'43','Black',4,'CRY12-43-BLK'),(29,'44','Black',2,'CRY12-44-BLK');

-- Asics Gel-Resolution 9 (id=30)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(30,'38','White/Black',0,'GR9-38-WB'),(30,'39','White/Black',1,'GR9-39-WB'),(30,'40','White/Black',1,'GR9-40-WB'),
(30,'41','White/Black',1,'GR9-41-WB'),(30,'42','White/Black',0,'GR9-42-WB'),(30,'43','White/Black',0,'GR9-43-WB');

-- Babolat Propulse Clay (id=31)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(31,'38','Red/Black',5,'BPF-38-RB'),(31,'39','Red/Black',8,'BPF-39-RB'),(31,'40','Red/Black',10,'BPF-40-RB'),
(31,'41','Red/Black',9,'BPF-41-RB'),(31,'42','Red/Black',7,'BPF-42-RB'),(31,'43','Red/Black',4,'BPF-43-RB');

-- UA HeatGear Tee (id=32)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(32,'S','Black',12,'UAHG-S-BLK'),(32,'M','Black',18,'UAHG-M-BLK'),(32,'L','Black',16,'UAHG-L-BLK'),(32,'XL','Black',11,'UAHG-XL-BLK'),
(32,'S','White',10,'UAHG-S-WHT'),(32,'M','White',14,'UAHG-M-WHT'),(32,'L','White',13,'UAHG-L-WHT'),(32,'XL','White',8,'UAHG-XL-WHT'),
(32,'S','Red',8,'UAHG-S-RED'),(32,'M','Red',11,'UAHG-M-RED'),(32,'L','Red',10,'UAHG-L-RED'),(32,'XL','Red',6,'UAHG-XL-RED');

-- NB Athletics Tee (id=33)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(33,'S','Grey',10,'NBAT-S-GRY'),(33,'M','Grey',15,'NBAT-M-GRY'),(33,'L','Grey',14,'NBAT-L-GRY'),(33,'XL','Grey',9,'NBAT-XL-GRY'),
(33,'S','Navy',8,'NBAT-S-NVY'),(33,'M','Navy',12,'NBAT-M-NVY'),(33,'L','Navy',11,'NBAT-L-NVY'),(33,'XL','Navy',7,'NBAT-XL-NVY');

-- Nike Park VII Jersey (id=34)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(34,'S','Black',10,'NPK7-S-BLK'),(34,'M','Black',14,'NPK7-M-BLK'),(34,'L','Black',13,'NPK7-L-BLK'),(34,'XL','Black',9,'NPK7-XL-BLK'),
(34,'S','Royal Blue',8,'NPK7-S-BLU'),(34,'M','Royal Blue',12,'NPK7-M-BLU'),(34,'L','Royal Blue',11,'NPK7-L-BLU'),(34,'XL','Royal Blue',7,'NPK7-XL-BLU'),
(34,'S','Red',8,'NPK7-S-RED'),(34,'M','Red',11,'NPK7-M-RED'),(34,'L','Red',10,'NPK7-L-RED'),(34,'XL','Red',6,'NPK7-XL-RED');

-- Adidas Tiro 24 Jersey (id=35)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(35,'S','Black',1,'AT24-S-BLK'),(35,'M','Black',2,'AT24-M-BLK'),(35,'L','Black',1,'AT24-L-BLK'),(35,'XL','Black',0,'AT24-XL-BLK'),
(35,'S','White',1,'AT24-S-WHT'),(35,'M','White',2,'AT24-M-WHT'),(35,'L','White',1,'AT24-L-WHT'),(35,'XL','White',0,'AT24-XL-WHT');

-- Mizuno Polo (id=36)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(36,'S','Navy',8,'MZPL-S-NVY'),(36,'M','Navy',12,'MZPL-M-NVY'),(36,'L','Navy',11,'MZPL-L-NVY'),(36,'XL','Navy',7,'MZPL-XL-NVY'),
(36,'S','White',6,'MZPL-S-WHT'),(36,'M','White',9,'MZPL-M-WHT'),(36,'L','White',8,'MZPL-L-WHT'),(36,'XL','White',5,'MZPL-XL-WHT');

-- Nike Academy Pant (id=37)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(37,'S','Black',10,'NKAP-S-BLK'),(37,'M','Black',14,'NKAP-M-BLK'),(37,'L','Black',13,'NKAP-L-BLK'),(37,'XL','Black',9,'NKAP-XL-BLK'),
(37,'S','Navy',7,'NKAP-S-NVY'),(37,'M','Navy',10,'NKAP-M-NVY'),(37,'L','Navy',9,'NKAP-L-NVY'),(37,'XL','Navy',6,'NKAP-XL-NVY');

-- Adidas Tiro 24 Pant (id=38)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(38,'S','Black',10,'AT24P-S-BLK'),(38,'M','Black',14,'AT24P-M-BLK'),(38,'L','Black',13,'AT24P-L-BLK'),(38,'XL','Black',9,'AT24P-XL-BLK'),
(38,'S','Dark Grey',8,'AT24P-S-DGR'),(38,'M','Dark Grey',11,'AT24P-M-DGR'),(38,'L','Dark Grey',10,'AT24P-L-DGR'),(38,'XL','Dark Grey',6,'AT24P-XL-DGR');

-- UA Challenger Pant (id=39)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(39,'S','Black',10,'UACP-S-BLK'),(39,'M','Black',14,'UACP-M-BLK'),(39,'L','Black',13,'UACP-L-BLK'),(39,'XL','Black',9,'UACP-XL-BLK');

-- Puma teamGOAL Shorts (id=40)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(40,'S','Black',12,'PTGS-S-BLK'),(40,'M','Black',16,'PTGS-M-BLK'),(40,'L','Black',15,'PTGS-L-BLK'),(40,'XL','Black',10,'PTGS-XL-BLK'),
(40,'S','Red',8,'PTGS-S-RED'),(40,'M','Red',11,'PTGS-M-RED'),(40,'L','Red',10,'PTGS-L-RED'),(40,'XL','Red',6,'PTGS-XL-RED');

-- NB Accelerate Short (id=41)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(41,'S','Black',10,'NBAS-S-BLK'),(41,'M','Black',14,'NBAS-M-BLK'),(41,'L','Black',13,'NBAS-L-BLK'),(41,'XL','Black',9,'NBAS-XL-BLK'),
(41,'S','Blue',7,'NBAS-S-BLU'),(41,'M','Blue',10,'NBAS-M-BLU'),(41,'L','Blue',9,'NBAS-L-BLU'),(41,'XL','Blue',6,'NBAS-XL-BLU');

-- Adidas Running Socks (id=42)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(42,'One Size','White',4,'ADSK-OS-WHT'),(42,'One Size','Black',3,'ADSK-OS-BLK');

-- Nike Headband (id=43)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(43,'One Size','Black',30,'NKHB-OS-BLK'),(43,'One Size','White',20,'NKHB-OS-WHT'),(43,'One Size','Red',15,'NKHB-OS-RED');

-- UA Wristband (id=44)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(44,'One Size','White',25,'UAWB-OS-WHT'),(44,'One Size','Black',25,'UAWB-OS-BLK');

-- Adidas Stadium Backpack (id=45)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(45,NULL,'Black',2,'ADBP2-BLK'),(45,NULL,'Grey',1,'ADBP2-GRY'),(45,NULL,'Navy',1,'ADBP2-NVY');

-- Nike Brasilia Duffel (id=46)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(46,NULL,'Black',10,'NKDF-BLK'),(46,NULL,'Blue',7,'NKDF-BLU');

-- Puma Phase Backpack (id=47)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(47,NULL,'Black',15,'PMBP-BLK'),(47,NULL,'Red',8,'PMBP-RED'),(47,NULL,'Blue',8,'PMBP-BLU');

-- Adidas Water Bottle (id=48)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(48,NULL,'Black',20,'ADWB-BLK'),(48,NULL,'White',15,'ADWB-WHT');

-- Garmin HRM-Dual (id=49)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(49,NULL,'Black',2,'GRM-HRM-BLK');

-- Nike Pro Cap (id=50)
INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku) VALUES
(50,'One Size','Black',20,'NKCP-OS-BLK'),(50,'One Size','White',15,'NKCP-OS-WHT'),(50,'One Size','Red',10,'NKCP-OS-RED');

-- ============================================================
-- ORDER STATUS VARIETY
-- Target distribution across all 540 orders:
--   Delivered (= Completed tab): ~400
--   Cancelled:                   ~65
--   Processing:                  ~35
--   Pending:                     ~15
-- Seed parts 1-5 inserted all as Completed/Paid.
-- Part 6 (2026) already has some Pending/Completed mix.
-- These UPDATEs convert ~140 historical orders away from Completed.
-- ============================================================

-- Cancelled before payment (~50 orders: walk-outs, no-shows, changed mind)
UPDATE customerorders SET status='Cancelled', payment_status='Unpaid', received_amount=0
WHERE id IN (
  15, 22, 32, 44, 58, 67, 79, 89, 97, 108,
  124, 137, 152, 164, 178, 189, 201, 213, 228, 245,
  259, 270, 287, 295, 312, 325, 338, 347, 356, 368,
  371, 383, 390, 399, 411, 419, 428, 437, 446, 453,
  462, 471, 478, 485, 492, 499, 505, 513, 519, 525
);

-- Cancelled after payment (~15 orders: refunded)
UPDATE customerorders SET status='Cancelled', payment_status='Paid'
WHERE id IN (
  27, 63, 118, 145, 183, 220, 252, 276, 301, 333,
  365, 388, 414, 441, 468
);

-- Processing (~35 orders: delivery being packed, not yet shipped)
UPDATE customerorders SET status='Processing', payment_status='Unpaid'
WHERE id IN (
  403, 404, 406, 408, 410, 412, 415, 417, 420, 422,
  424, 426, 429, 431, 433, 436, 438, 440, 443, 445,
  448, 450, 452, 455, 457, 459, 461, 464, 466, 469,
  472, 474, 476, 479, 481
);

-- Pending (~15 orders: very recent, not yet actioned)
UPDATE customerorders SET status='Pending', payment_status='Unpaid', received_amount=0
WHERE id IN (
  483, 486, 488, 490, 494, 497, 501, 503, 507,
  510, 515, 518, 522, 527, 530
);

-- ============================================================
-- ORDER TYPE VARIETY
-- Convert ~110 AtStore orders to Delivery with address + COD/BankTransfer
-- Target: ~70% AtStore (~380), ~30% Delivery (~160)
-- Uses customer addresses from the customers table
-- ============================================================

UPDATE customerorders SET
  order_type = 'Delivery',
  shipping_address = (SELECT address FROM customers WHERE id = customerorders.customer_id),
  payment_method = 'COD'
WHERE id IN (
  -- Jun-Sep 2024 (spread ~20)
  3, 8, 14, 21, 29, 36, 48, 57, 66, 74,
  82, 91, 99, 107, 116, 125, 133, 141, 149, 158,
  -- Oct-Dec 2024 (spread ~30, busier period)
  167, 175, 184, 193, 202, 211, 219, 228, 237, 246,
  255, 263, 272, 281, 290, 299, 308, 317, 326, 335,
  -- Jan-Mar 2025 (spread ~30)
  142, 153, 162, 171, 180, 190, 200, 210, 222, 233,
  244, 254, 266, 277, 288, 300, 311, 322, 333, 344,
  -- Apr-May 2025 + 2026 (spread ~30)
  355, 366, 375, 384, 393, 402, 405, 409, 413, 418,
  423, 427, 432, 439, 444, 449, 454, 458, 463, 467,
  473, 477, 482, 487, 491, 496, 500, 504, 508, 512
);

-- Some delivery orders use BankTransfer instead of COD
UPDATE customerorders SET payment_method = 'BankTransfer'
WHERE order_type = 'Delivery'
  AND id IN (
  3, 21, 48, 74, 99, 125, 149, 175, 202, 228,
  255, 281, 308, 335, 153, 180, 210, 244, 277, 311,
  344, 375, 409, 427, 449, 467, 487, 500, 512, 333
);
