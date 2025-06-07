-- First, ensure all required selection groups exist
INSERT OR IGNORE INTO selection_groups (id, name, type, is_required, min_select, max_select, threshold, display_order)
VALUES 
(1, 'Pizza Size', 'SINGLE', 1, 1, 1, 0, 1),
(2, 'Crust Type', 'SINGLE', 1, 1, 1, 0, 2),
(3, 'Extra Toppings', 'MULTIPLE', 0, 0, 5, 0, 5),
(26, 'Vegan Ingredients', 'MULTIPLE', 0, 0, 3, 0, 3),
(27, 'Drink Options', 'MULTIPLE', 0, 0, 2, 0, 6),
(28, 'Side Options', 'MULTIPLE', 0, 0, 2, 0, 7),
(34, 'Exclude Ingredients', 'MULTIPLE', 0, 0, 3, 0, 4),
(35, 'Sauces', 'MULTIPLE', 0, 0, 2, 0, 8);

-- Delete existing item-selection group relationships for Vegan Pizza
DELETE FROM item_selection_groups WHERE item_id = 3;

-- Insert new item-selection group relationships with correct display order
INSERT INTO item_selection_groups (item_id, selection_group_id, display_order)
VALUES 
(3, 1, 1),  -- Pizza Size
(3, 2, 2),  -- Crust Type
(3, 26, 3), -- Vegan Ingredients
(3, 34, 4), -- Exclude Ingredients
(3, 3, 5),  -- Extra Toppings
(3, 27, 6), -- Drink Options
(3, 28, 7), -- Side Options
(3, 35, 8); -- Sauces 