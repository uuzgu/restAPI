-- Delete existing selection groups to avoid duplicates
DELETE FROM selection_options;
DELETE FROM item_selection_groups;
DELETE FROM category_selection_groups;
DELETE FROM selection_groups;

-- Insert selection groups
INSERT INTO selection_groups (id, name, type, is_required, min_select, max_select, threshold, display_order) VALUES
(1, 'Pizza Size', 'SINGLE', true, 1, 1, 0, 1),
(2, 'Crust Type', 'SINGLE', true, 1, 1, 0, 2),
(3, 'Extra Toppings', 'MULTIPLE', false, 0, 5, 0, 3),
(4, 'Sauces', 'MULTIPLE', false, 0, 3, 0, 4),
(5, 'Vegan Ingredients', 'MULTIPLE', true, 1, 5, 0, 5),
(6, 'Drink Options', 'SINGLE', false, 0, 1, 0, 6),
(7, 'Side Options', 'MULTIPLE', false, 0, 2, 0, 7),
(8, 'Exclude Ingredients', 'MULTIPLE', false, 0, 3, 0, 8);

-- Insert selection options for Pizza Size
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(1, 'Small (10")', 0, 1, 1),
(2, 'Medium (12")', 2, 2, 1),
(3, 'Large (14")', 4, 3, 1);

-- Insert selection options for Crust Type
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(4, 'Classic Crust', 0, 1, 2),
(5, 'Thin Crust', 0, 2, 2),
(6, 'Gluten-Free Crust', 2, 3, 2);

-- Insert selection options for Extra Toppings
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(7, 'Extra Vegan Cheese', 1.5, 1, 3),
(8, 'Extra Mushrooms', 1, 2, 3),
(9, 'Extra Bell Peppers', 1, 3, 3),
(10, 'Extra Onions', 0.5, 4, 3),
(11, 'Extra Olives', 1, 5, 3);

-- Insert selection options for Sauces
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(12, 'Extra Tomato Sauce', 0, 1, 4),
(13, 'BBQ Sauce', 0.5, 2, 4),
(14, 'Garlic Sauce', 0.5, 3, 4);

-- Insert selection options for Vegan Ingredients
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(15, 'Vegan Cheese', 0, 1, 5),
(16, 'Mushrooms', 0, 2, 5),
(17, 'Bell Peppers', 0, 3, 5),
(18, 'Onions', 0, 4, 5),
(19, 'Olives', 0, 5, 5);

-- Insert selection options for Drink Options
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(20, 'Water', 0, 1, 6),
(21, 'Cola', 2, 2, 6),
(22, 'Sprite', 2, 3, 6);

-- Insert selection options for Side Options
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(23, 'Garlic Bread', 3, 1, 7),
(24, 'Side Salad', 4, 2, 7);

-- Insert selection options for Exclude Ingredients
INSERT INTO selection_options (id, name, price, display_order, selection_group_id) VALUES
(25, 'No Onions', 0, 1, 8),
(26, 'No Mushrooms', 0, 2, 8),
(27, 'No Olives', 0, 3, 8);

-- Link selection groups to Vegan Pizza (item_id = 3)
INSERT INTO item_selection_groups (item_id, selection_group_id) VALUES
(3, 1), -- Pizza Size
(3, 2), -- Crust Type
(3, 3), -- Extra Toppings
(3, 4), -- Sauces
(3, 5), -- Vegan Ingredients
(3, 6), -- Drink Options
(3, 7), -- Side Options
(3, 8); -- Exclude Ingredients

-- Link selection groups to Pizza category (category_id = 1)
INSERT INTO category_selection_groups (category_id, selection_group_id) VALUES
(1, 1), -- Pizza Size
(1, 2), -- Crust Type
(1, 3), -- Extra Toppings
(1, 4), -- Sauces
(1, 6), -- Drink Options
(1, 7), -- Side Options
(1, 8); -- Exclude Ingredients 