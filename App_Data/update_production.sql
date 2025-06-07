-- Update category names
UPDATE categories SET name = 'Promotions' WHERE id = 0;
UPDATE categories SET name = 'Pizza' WHERE id = 1;
UPDATE categories SET name = 'Bowls' WHERE id = 2;
UPDATE categories SET name = 'Hamburgers' WHERE id = 3;
UPDATE categories SET name = 'Salads' WHERE id = 4;
UPDATE categories SET name = 'Breakfast' WHERE id = 5;
UPDATE categories SET name = 'Drinks' WHERE id = 6;
UPDATE categories SET name = 'Soups' WHERE id = 7;
UPDATE categories SET name = 'Desserts' WHERE id = 8;

-- Delete existing items to avoid duplicates
DELETE FROM items;

-- Insert all items
INSERT INTO items (id, category_id, description, image_url, name, price) VALUES
(1, 6, 'Classic carbonated soft drink', NULL, 'Cola', 4),
(2, 4, 'Salad with fresh vegetables and cheese', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/salad.jpg', 'Salad', 8),
(3, 1, 'A delightful plant-based pizza with a variety of fresh toppings', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/veganpizza.jpg', 'Vegan Pizza', 12),
(4, 2, 'Filled with fresh ingredients, perfect for a healthy and satisfying meal.', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/bowll.jpg', 'Bowl', 10),
(5, 6, 'Pure, crisp, and refreshing still water', NULL, 'Water', 1),
(6, 6, 'Traditional Turkish yogurt-based drink, cool and refreshing with a creamy texture', NULL, 'Ayran', 2),
(7, 6, 'A light, refreshing lemon-lime soda with a crisp, bubbly taste to quench your thirst', NULL, 'Sprite', 3),
(8, 1, 'Classic pizza with rich tomato sauce, fresh mozzarella, and basil', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/margarita.jpg', 'Margarita Pizza', 9),
(9, 6, 'A premium Turkish soda, light and refreshing, perfect for pairing with your meal.', NULL, 'Soda', 2),
(10, 3, 'A delicious burger made with a juicy beef patty, crispy bacon, melted cheese, and fresh onions, all nestled in a soft bun', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/baconburger.jpg', 'Baconburger', 6),
(11, 3, 'Classic American cheeseburger', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/cheeseburger.jpg', 'Cheeseburger', 7),
(12, 4, 'A healthy salad with grilled chicken, fresh veggies, and dressing', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/chickensalad.jpg', 'Chicken Salad', 9),
(13, 1, 'A savory pizza topped with grilled chicken and fresh vegetables', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/chickenpizza.jpg', 'Chicken Pizza', 10),
(14, 1, 'A flavorful pizza with spicy sausage and cheese', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/sausagepizza.jpg', 'Sausage Pizza', 10),
(15, 5, 'A delicious breakfast with bacon, eggs, and fresh vegetables', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/baconbreakfast.jpg', 'Bacon Breakfast Menu', 12),
(16, 7, 'Spicy, sweet, and fragrant with lemongrass, lime, and tender chicken', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/thaisoup.jpg', 'Thai Coconut Chicken Soup', 7),
(17, 7, 'Rich, earthy, and velvety with sautéed mushrooms and a hint of garlic', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/mushroomsoup.jpg', 'Creamy Wild Mushroom Soup', 6),
(18, 7, 'Smooth, tangy, and topped with a swirl of cream and fresh basil', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/tomatosoup.jpg', 'Roasted Tomato Basil Soup', 6),
(19, 8, 'Warm, gooey center served with a scoop of vanilla ice cream', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/moltendessert.jpg', 'Molten Chocolate Lava Cake', 5),
(20, 8, 'Zesty lemon curd topped with pillowy toasted meringue in a buttery crust', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/lemontartdessert.jpg', 'Lemon Meringue Tart', 5),
(21, 8, 'Layers of fluffy cake, whipped cream, and fresh strawberries', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/strawberrydessert.jpg', 'Strawberry Shortcake Parfait', 6),
(22, 0, 'A delightful plant-based pizza with a variety of fresh toppings', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/veganpizza.jpg', 'Vegan Pizza (Promo)', 10),
(23, 0, 'Classic pizza with rich tomato sauce, fresh mozzarella, and basil', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/margarita.jpg', 'Margarita Pizza (Promo)', 7),
(24, 0, 'A savory pizza topped with grilled chicken and fresh vegetables', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/chickenpizza.jpg', 'Chicken Pizza (Promo)', 8),
(25, 0, 'A flavorful pizza with spicy sausage and cheese', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/sausagepizza.jpg', 'Sausage Pizza (Promo)', 8),
(26, 0, 'Large pizza with 3 toppings of your choice', NULL, 'Special Pizza Deal', 20),
(27, 0, '2 medium pizzas with 2 toppings each', NULL, 'Family Pizza Pack', 20),
(28, 0, 'Juicy beef burger with fries', NULL, 'Classic Burger Combo', 15),
(29, 0, 'Double beef patty with cheese and special sauce', NULL, 'Double Cheeseburger Special', 15),
(47, 1, 'Customize your perfect pizza with your favorite toppings', 'https://restaurant-images33.s3.eu-north-1.amazonaws.com/sausagepizza.jpg', 'Create Your Pizza', 12.99); 