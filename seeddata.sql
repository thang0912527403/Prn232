USE [EbayDB];
GO
DECLARE @seller1 UNIQUEIDENTIFIER = NEWID();
DECLARE @seller2 UNIQUEIDENTIFIER = NEWID();
DECLARE @seller3 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Users] ([UserId],[username], [email], [PasswordHash], [role],TotalSales,TrustLevel,Rating)
VALUES
(@seller1,N'seller1', N'seller1@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'seller',3,21,0),
(@seller2,N'seller2', N'seller2@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'seller',98,7,0),
(@seller3,N'seller3', N'seller3@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'seller',1000,0,0),

(NEWID(),N'buyer1', N'buyer1@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'buyer',0,0,0),
(NEWID(),N'buyer2', N'buyer2@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'buyer',0,0,0),
(NEWID(),N'buyer3', N'buyer3@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'buyer',0,0,0),
(NEWID(),N'buyer4', N'buyer4@example.com', N'AQAAAAIAAYagAAAAEO/sSkgVQt/6GpbA5qw++IFFBhFeA2/hYI1EdGR9ukHJfAE+eEcP/2+/zhU2dX/CQw==', N'buyer',0,0,0);

DECLARE @1 UNIQUEIDENTIFIER = NEWID();
DEClare @2 UNIQUEIDENTIFIER = NEWID();
DEClare @3 UNIQUEIDENTIFIER = NEWID();
DEClare @4 UNIQUEIDENTIFIER = NEWID();
DEClare @5 UNIQUEIDENTIFIER = NEWID();
DEClare @6 UNIQUEIDENTIFIER = NEWID();
DEClare @7 UNIQUEIDENTIFIER = NEWID();
DEClare @8 UNIQUEIDENTIFIER = NEWID();
DEClare @9 UNIQUEIDENTIFIER = NEWID();
DEClare @10 UNIQUEIDENTIFIER = NEWID();


INSERT INTO [dbo].[Categories] ([CategoryId],[name])
VALUES 
(@1,N'Electronics'),
(@2,N'Fashion'),
(@3,N'Home & Garden'),
(@4,N'Toys & Hobbies'),
(@5,N'Automotive'),
(@6,N'Health & Beauty'),
(@7,N'Sports'),
(@8,N'Books & Media'),
(@9,N'Art & Collectibles'),
(@10,N'Pet Supplies');

INSERT INTO [dbo].[Products]
([ProductId],[Name], [Description], [Price], [ImageUrl], [CategoryId], [SellerId], [IsAuction], [AuctionEndTime])
VALUES
-- CATEGORY 1: Electronics
(NEWID(),N'iPhone 15 Pro Max 256GB', N'Brand new Apple flagship phone.', 34990, N'https://surl.li/lbexyd', @1, @seller1, 0, NULL),
(NEWID(),N'Samsung Galaxy S24 Ultra', N'Flagship Android device with best camera.', 28990, N'https://surl.li/yayvml', @1, @seller1, 0, NULL),
(NEWID(),N'Sony WH-1000XM5', N'Wireless noise-cancelling headphones.', 8990, N'https://surl.lu/decowq', @1, @seller1, 0, NULL),
(NEWID(),N'Apple Watch Ultra 2', N'Premium titanium smartwatch.', 19990, N'https://surl.lu/kmvpow', @1, @seller1, 1, DATEADD(DAY, 5, GETDATE())),
(NEWID(),N'MacBook Air M3', N'Lightweight laptop with Apple Silicon chip.', 28990, N'https://surl.li/ueamjc', @1, @seller1, 0, NULL),
(NEWID(),N'GoPro Hero 12 Black', N'Action camera, 5.3K recording.', 11990, N'https://surl.li/ntrdie', @1, @seller1, 0, NULL),
(NEWID(),N'Canon EOS R8', N'Full-frame mirrorless camera.', 32500, N'https://surl.li/vlknmr', @1, @seller1, 0, NULL),
(NEWID(),N'LG OLED C4 55 inch', N'4K OLED TV, smart AI.', 25900, N'https://surl.li/aanadd', @1, @seller1, 1, DATEADD(DAY, 10, GETDATE())),
(NEWID(),N'Anker Power Bank 20000mAh', N'Fast charging portable battery.', 1200, N'https://surl.li/aiyizz', @1, @seller1, 0, NULL),
(NEWID(),N'Logitech MX Master 3S', N'Wireless ergonomic mouse.', 2490, N'https://surl.li/jkrocu', @1, @seller1, 0, NULL),

-- CATEGORY 2: Fashion
(NEWID(),N'Nike Air Force 1', N'Classic white sneakers.', 2500, N'https://surl.lt/pjqxhf', @2, @seller1, 0, NULL),
(NEWID(),N'Adidas Ultraboost 24', N'Performance running shoes.', 3200, N'https://surl.li/wmsgzk', @2, @seller1, 0, NULL),
(NEWID(),N'Levi’s 511 Slim Jeans', N'Blue denim, slim fit.', 1800, N'https://surl.li/rywvux', @2, @seller1, 0, NULL),
(NEWID(),N'Gucci Leather Belt', N'Luxury men belt.', 9500, N'https://surl.li/wmtgef', @2, @seller1, 1, DATEADD(DAY, 4, GETDATE())),
(NEWID(),N'Louis Vuitton Handbag', N'High-end women handbag.', 32900, N'https://surl.li/vzmmir', @2, @seller1, 1, DATEADD(DAY, 8, GETDATE())),
(NEWID(),N'Uniqlo Hoodie', N'Comfort cotton hoodie.', 690, N'https://surl.li/qhozqr', @2, @seller1, 0, NULL),
(NEWID(),N'H&M Casual Shirt', N'Slim fit cotton shirt.', 590, N'https://surl.li/mntzrs', @2, @seller1, 0, NULL),
(NEWID(),N'Ray-Ban Aviator Sunglasses', N'Unisex eyewear.', 3500, N'https://surli.cc/ixkkuo', @2, @seller1, 0, NULL),
(NEWID(),N'Zara Men Jacket', N'Stylish winter jacket.', 2500, N'https://surli.cc/jqerkq', @2, @seller1, 0, NULL),
(NEWID(),N'Calvin Klein Perfume', N'Euphoria 100ml fragrance.', 1800, N'https://surl.lu/ozbget', @2, @seller1, 0, NULL),

-- CATEGORY 3: Home & Living
(NEWID(),N'Wooden Coffee Table', N'Oak wood, minimalist design.', 1800, N'https://surl.li/tkfpln', @3, @seller1, 0, NULL),
(NEWID(),N'Ceramic Vase', N'Handcrafted decor piece.', 350, N'https://surl.li/rflxuq', @3, @seller1, 0, NULL),
(NEWID(),N'LED Ceiling Lamp', N'Energy saving warm light.', 900, N'https://surl.lu/sfophb', @3, @seller1, 0, NULL),
(NEWID(),N'Luxury Curtains', N'Velvet material for living room.', 1200, N'https://surl.lu/buwszw', @3, @seller1, 0, NULL),
(NEWID(),N'Dyson Vacuum Cleaner', N'Cordless V12 model.', 15900, N'https://surl.li/ylntzo', @3, @seller1, 1, DATEADD(DAY, 6, GETDATE())),
(NEWID(),N'Air Purifier Xiaomi Pro', N'Smart HEPA filter purifier.', 4200, N'https://surl.li/faajpn', @3, @seller1, 0, NULL),
(NEWID(),N'Wall Clock', N'Minimalist design, wood finish.', 350, N'https://surl.li/ngazhs', @3, @seller1, 0, NULL),
(NEWID(),N'IKEA Bookshelf', N'5-tier wooden shelf.', 1100, N'https://surl.li/mesdqx', @3, @seller1, 0, NULL),
(NEWID(),N'Kitchen Knife Set', N'Stainless steel 6-piece set.', 590, N'https://surl.li/hbprqa', @3, @seller1, 0, NULL),
(NEWID(),N'Philips Air Fryer XXL', N'Healthy oil-free cooking.', 3900, N'https://surl.li/gcqean', @3, @seller1, 0, NULL),

-- CATEGORY 4: Toys & Hobbies
(NEWID(),N'Lego Millennium Falcon', N'Collector’s edition.', 5500, N'https://surl.li/plkdft', @4, @seller1, 0, NULL),
(NEWID(),N'Hot Wheels Car Set', N'Pack of 10 cars.', 350, N'https://surl.li/trqeft', @4, @seller1, 0, NULL),
(NEWID(),N'DJI Mini 3 Drone', N'Compact 4K drone.', 14500, N'https://surl.li/crofrc', @4, @seller1, 1, DATEADD(DAY, 9, GETDATE())),
(NEWID(),N'UNO Card Game', N'Classic family game.', 150, N'https://surl.li/idmtyt', @4, @seller1, 0, NULL),
(NEWID(),N'LEGO Technic Car', N'Advanced vehicle model.', 4500, N'https://surl.li/knvhey', @4, @seller1, 0, NULL),
(NEWID(),N'RC Monster Truck', N'High-speed offroad RC car.', 2200, N'https://surl.li/vmhsie', @4, @seller1, 0, NULL),
(NEWID(),N'Magic Kit for Kids', N'Simple magician starter kit.', 590, N'https://surl.li/aiwsln', @4, @seller1, 0, NULL),
(NEWID(),N'Puzzle 1000 Pieces', N'Beautiful landscape puzzle.', 280, N'https://surl.li/hsamwe', @4, @seller1, 0, NULL),
(NEWID(),N'RC Train Set', N'Electric model train.', 2500, N'https://surl.li/ddofxa', @4, @seller1, 1, DATEADD(DAY, 7, GETDATE())),
(NEWID(),N'Action Figure Batman', N'12-inch collectible figure.', 650, N'https://surli.cc/zhgqjy', @4, @seller1, 0, NULL),

-- CATEGORY 5: Automotive
(NEWID(),N'Mobil 1 Engine Oil 5W-30', N'Premium synthetic oil.', 900, N'https://surl.li/npnhug', @5, @seller2, 0, NULL),
(NEWID(),N'Car Vacuum Cleaner', N'Portable for car interior.', 350, N'https://surl.li/uepyxo', @5, @seller2, 0, NULL),
(NEWID(),N'Michelin Tire 205/55R16', N'All-season durable tire.', 2000, N'https://surl.lu/ivsqzi', @5, @seller2, 0, NULL),
(NEWID(),N'LED Headlight Bulb', N'Bright energy-saving bulbs.', 650, N'https://surl.li/uezdpi', @5, @seller2, 0, NULL),
(NEWID(),N'Car Battery 12V', N'Long-lasting AGM battery.', 2500, N'https://surl.li/iyvevl', @5, @seller2, 1, DATEADD(DAY, 3, GETDATE())),
(NEWID(),N'Dash Cam 4K', N'Front and rear camera system.', 2800, N'https://surl.li/avedvs', @5, @seller2, 0, NULL),
(NEWID(),N'Car Wax Kit', N'Professional detailing wax.', 450, N'https://surl.li/mhtemo', @5, @seller2, 0, NULL),
(NEWID(),N'Windshield Wipers', N'Universal 24-inch pair.', 300, N'https://surl.li/fjzntm', @5, @seller2, 0, NULL),
(NEWID(),N'Brake Pad Set', N'High-quality ceramic pads.', 990, N'https://surl.lu/smxfvl', @5, @seller2, 0, NULL),
(NEWID(),N'Car Shampoo', N'pH-balanced car cleaner.', 280, N'https://surl.li/dnuoqs', @5, @seller2, 0, NULL),

-- CATEGORY 6: Health & Beauty
(NEWID(),N'L’Oreal Shampoo 400ml', N'Nourishing hair repair formula.', 180, N'https://surl.li/ilpwxp', @6, @seller2, 0, NULL),
(NEWID(),N'Nivea Body Lotion', N'Softens and moisturizes skin.', 220, N'https://surl.li/tyqweo', @6, @seller2, 0, NULL),
(NEWID(),N'Maybelline Mascara', N'Long-lasting waterproof mascara.', 250, N'https://surl.lt/fkemai', @6, @seller2, 0, NULL),
(NEWID(),N'Innisfree Green Tea Cream', N'Korean natural moisturizer.', 490, N'https://surl.li/fibvnw', @6, @seller2, 0, NULL),
(NEWID(),N'Laneige Lip Sleeping Mask', N'Popular overnight lip care.', 390, N'https://surl.li/saojby', @6, @seller2, 1, DATEADD(DAY, 4, GETDATE())),
(NEWID(),N'Clinique Foundation', N'Full coverage liquid foundation.', 980, N'https://surli.cc/wsafzv', @6, @seller2, 0, NULL),
(NEWID(),N'Vichy Mineral 89 Serum', N'Lightweight daily booster.', 890, N'https://surl.li/rpmwxh', @6, @seller2, 0, NULL),
(NEWID(),N'Foreo Luna Mini 3', N'Sonic facial cleansing device.', 2900, N'https://surl.li/lfdwpl', @6, @seller2, 1, DATEADD(DAY, 8, GETDATE())),
(NEWID(),N'Revlon Hair Dryer', N'Compact travel size.', 650, N'https://surl.li/oesgra', @6, @seller2, 0, NULL),
(NEWID(),N'SK-II Facial Treatment Essence', N'Legendary Pitera skincare.', 3190, N'https://surl.li/nbhshu', @6, @seller2, 0, NULL),
(NEWID(),N'Garnier Micellar Water', N'Makeup remover 400ml.', 230, N'https://surl.li/dvtlof', @6, @seller2, 0, NULL),
(NEWID(),N'Colgate Optic White Toothpaste', N'Teeth whitening toothpaste.', 120, N'https://surl.lu/mwcrsx', @6, @seller2, 0, NULL),
(NEWID(),N'Oral-B Electric Toothbrush', N'Rechargeable toothbrush.', 1250, N'https://surl.li/dhukpq', @6, @seller2, 1, DATEADD(DAY, 6, GETDATE())),
(NEWID(),N'Neutrogena Sunscreen SPF50', N'Oil-free sun protection.', 420, N'https://surl.lt/fwejyy', @6, @seller2, 0, NULL),
(NEWID(),N'Clinique Eye Cream', N'Reduces dark circles.', 850, N'https://surl.li/dqtqxn', @6, @seller2, 0, NULL),

-- CATEGORY 7: Sports & Outdoors
(NEWID(),N'Adidas Football', N'Official size 5 training ball.', 450, N'https://surl.lt/mmnkwu', @7, @seller3, 0, NULL),
(NEWID(),N'Yonex Badminton Racket', N'Carbon fiber lightweight racket.', 1250, N'https://surl.lt/sgrfqy', @7, @seller3, 0, NULL),
(NEWID(),N'Wilson Tennis Racket', N'Professional tennis racket.', 2400, N'https://surl.lt/xkwywf', @7, @seller3, 0, NULL),
(NEWID(),N'Decathlon Camping Tent', N'2-person waterproof tent.', 1600, N'https://surl.li/ubfwft', @7, @seller3, 1, DATEADD(DAY, 7, GETDATE())),
(NEWID(),N'Nike Running Shorts', N'Lightweight dri-fit shorts.', 450, N'https://surl.li/tbuque', @7, @seller3, 0, NULL),
(NEWID(),N'Garmin Forerunner 265', N'Smart GPS running watch.', 8900, N'https://surl.li/sisont', @7, @seller3, 0, NULL),
(NEWID(),N'Yoga Mat 10mm', N'High-density non-slip mat.', 320, N'https://surl.li/agxjdw', @7, @seller3, 0, NULL),
(NEWID(),N'Bicycle Helmet', N'Adjustable with LED light.', 540, N'https://surl.li/rqbjxu', @7, @seller3, 0, NULL),
(NEWID(),N'Reebok Gym Bag', N'Spacious travel duffel.', 790, N'https://surl.li/yhsuup', @7, @seller3, 0, NULL),
(NEWID(),N'Dumbbell Set 20kg', N'Adjustable chrome dumbbells.', 1750, N'https://surl.li/xjgqyq', @7, @seller3, 0, NULL),
(NEWID(),N'Adidas Tracksuit', N'Men training wear.', 1800, N'https://surl.li/zfxfld', @7, @seller3, 0, NULL),
(NEWID(),N'Puma Running Shoes', N'Comfort cushioning shoes.', 2100, N'https://surl.li/zgngoo', @7, @seller3, 0, NULL),
(NEWID(),N'Mountain Bike 26 inch', N'Aluminum frame, 21 speed.', 6200, N'https://surl.lu/wicmcn', @7, @seller3, 1, DATEADD(DAY, 9, GETDATE())),
(NEWID(),N'Trekking Backpack 50L', N'Water-resistant hiking bag.', 1650, N'https://surl.li/vdyqex', @7, @seller3, 0, NULL),
(NEWID(),N'Kettlebell 12kg', N'Rubber coated gym equipment.', 800, N'https://surl.li/ntrvxz', @7, @seller3, 0, NULL),

-- CATEGORY 8: Books & Stationery
(NEWID(),N'Atomic Habits', N'Bestseller self-improvement book.', 280, N'https://surl.li/ndlaop', @8, @seller3, 0, NULL),
(NEWID(),N'The Subtle Art of Not Giving a F*ck', N'Mark Manson classic.', 320, N'https://surl.li/vbcmme', @8, @seller3, 0, NULL),
(NEWID(),N'Python Programming 101', N'Beginner guide to coding.', 480, N'https://surl.li/uejbax', @8, @seller3, 0, NULL),
(NEWID(),N'Moleskine Notebook', N'Hardcover ruled journal.', 390, N'https://surl.li/iccwyw', @8, @seller3, 0, NULL),
(NEWID(),N'Stabilo Marker Set', N'Highlighter pastel set.', 180, N'https://surl.li/dpdyom', @8, @seller3, 0, NULL),
(NEWID(),N'Kindle Paperwhite 11th Gen', N'E-ink reader with 8GB.', 3300, N'https://surl.li/lwvnpw', @8, @seller3, 1, DATEADD(DAY, 6, GETDATE())),
(NEWID(),N'Japanese Calligraphy Pen', N'Fine brush tip pen.', 250, N'https://surl.lt/tmajec', @8, @seller3, 0, NULL),
(NEWID(),N'To Kill a Mockingbird', N'Harper Lee classic novel.', 270, N'https://surl.li/zgwuau', @8, @seller3, 0, NULL),
(NEWID(),N'Canvas Book Bag', N'Lightweight eco tote.', 160, N'https://surl.li/sjatjk', @8, @seller3, 0, NULL),
(NEWID(),N'Ergonomic Study Lamp', N'LED desk lamp with dimmer.', 520, N'https://surl.li/nmsbmd', @8, @seller3, 0, NULL),
(NEWID(),N'Blue Gel Pen Set', N'Box of 10 smooth pens.', 85, N'https://surli.cc/ljxzro', @8, @seller3, 0, NULL),
(NEWID(),N'World Atlas 2025 Edition', N'Comprehensive world map.', 410, N'https://surl.li/yguqxp', @8, @seller3, 0, NULL),
(NEWID(),N'Notebook Refill Paper A5', N'Pack of 100 sheets.', 75, N'https://surli.cc/wqqgjl', @8, @seller3, 0, NULL),
(NEWID(),N'Art of War', N'Sun Tzu classic edition.', 190, N'https://surl.li/dzchsj', @8, @seller3, 0, NULL),
(NEWID(),N'High-Quality Pencil Case', N'Multi-compartment PU case.', 120, N'https://surl.li/udhbme', @8, @seller3, 0, NULL),

-- CATEGORY 9: Art & Collectibles
(NEWID(),N'Canvas Painting Sunset', N'Hand-painted wall decor.', 890, N'https://surl.li/zlapur', @9, @seller3, 0, NULL),
(NEWID(),N'Acrylic Paint Set 24 Colors', N'Professional artist kit.', 420, N'https://surl.lt/kilxbp', @9, @seller3, 0, NULL),
(NEWID(),N'Wood Frame 30x40cm', N'Natural oak photo frame.', 190, N'https://surl.li/vjbmvc', @9, @seller3, 0, NULL),
(NEWID(),N'Watercolor Brush Set', N'10 pieces soft hair brushes.', 250, N'https://surl.li/ejjxon', @9, @seller3, 0, NULL),
(NEWID(),N'Handmade Ceramic Sculpture', N'Artisan crafted decor.', 1200, N'https://surl.lu/jjtxcj', @9, @seller3, 1, DATEADD(DAY, 8, GETDATE())),
(NEWID(),N'Vinyl Record Beatles', N'Original reissue album.', 950, N'https://surl.li/gnuruk', @9, @seller3, 0, NULL),
(NEWID(),N'Art Easel Stand', N'Adjustable metal tripod.', 690, N'https://surl.li/lgmrua', @9, @seller3, 0, NULL),
(NEWID(),N'Calligraphy Ink Bottle', N'Black ink 50ml.', 120, N'https://surl.li/dewdmk', @9, @seller3, 0, NULL),
(NEWID(),N'Photography Light Box', N'Portable photo studio box.', 990, N'https://surl.li/tzvrsw', @9, @seller3, 0, NULL),
(NEWID(),N'Poster Mona Lisa', N'High quality art print.', 290, N'https://surl.lt/xqpcrm', @9, @seller3, 0, NULL),
(NEWID(),N'Handmade Pottery Bowl', N'Simple Japanese style.', 350, N'https://surl.li/fiycbj', @9 , @seller3, 0, NULL),
(NEWID(),N'Origami Paper Pack', N'Set of 100 colorful sheets.', 120, N'https://surl.li/ufwwzu', @9, @seller3, 0, NULL),
(NEWID(),N'Mini Sculpture Marble', N'Classical Greek replica.', 880, N'https://surl.li/inkqoy', @9, @seller3, 0, NULL),
(NEWID(),N'Art Display Frame A3', N'Black wooden frame.', 320, N'https://surl.li/airikd', @9, @seller3, 0, NULL),
(NEWID(),N'Artist Sketchbook', N'A4 spiral-bound drawing pad.', 250, N'https://surl.li/jnjhnu', @9, @seller3, 0, NULL),

-- CATEGORY 10: Pets
(NEWID(),N'Pet Food Dog Adult 5kg', N'Premium dry dog food.', 520, N'https://surl.li/asnnur', @10, @seller3, 0, NULL),
(NEWID(),N'Cat Toy Feather Wand', N'Interactive cat toy.', 120, N'https://surl.lu/oakmaz', @10, @seller3, 0, NULL),
(NEWID(),N'Fish Tank 60L', N'Glass aquarium with filter.', 1300, N'https://surl.li/skfcaa', @10, @seller3, 0, NULL),
(NEWID(),N'Dog Leash Nylon', N'Durable 1.5m leash.', 140, N'https://surl.li/whnezj', @10, @seller3, 0, NULL),
(NEWID(),N'Cat Scratching Post', N'Stable wooden structure.', 480, N'https://surl.lt/mixjro', @10, @seller3, 0, NULL),
(NEWID(),N'Pet Shampoo Aloe Vera', N'Gentle formula for all pets.', 180, N'https://surl.li/mmmifn', @10, @seller3, 0, NULL),
(NEWID(),N'Hamster Wheel', N'Silent running wheel.', 160, N'https://surl.li/pkcypf', @10, @seller3, 0, NULL),
(NEWID(),N'Pet Carrier Bag', N'Foldable travel bag.', 350, N'https://surl.li/fawxxu', @10, @seller3, 0, NULL),
(NEWID(),N'Bird Cage Medium', N'Metal cage for small birds.', 780, N'https://surl.li/jofmja', @10, @seller3, 1, DATEADD(DAY, 7, GETDATE())),
(NEWID(),N'Cat Litter Box', N'Easy-clean enclosed design.', 540, N'https://surl.li/kftumj', @10, @seller3, 0, NULL),
(NEWID(),N'Dog Bed Plush', N'Soft large dog bed.', 650, N'https://surl.li/ksmjzp', @10, @seller3, 0, NULL),
(NEWID(),N'Pet Nail Clipper', N'Safe stainless tool.', 120, N'https://surl.lu/plqrzv', @10, @seller3, 0, NULL),
(NEWID(),N'Fish Food Flakes 200g', N'Balanced nutrition.', 95, N'https://surl.li/fmrlpn', @10, @seller3, 0, Null)
GO

