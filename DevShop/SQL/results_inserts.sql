-- Generated from results.xlsx
-- Table assumed: Inventory (InventoryID, ProductID, SizeXS, SizeS, SizeM, SizeL, SizeXL, SizeXXL, SizeXXXL, LastUpdated)

SET IDENTITY_INSERT dbo.Inventory ON;

INSERT INTO dbo.Inventory (InventoryID, ProductID, SizeXS, SizeS, SizeM, SizeL, SizeXL, SizeXXL, SizeXXXL, LastUpdated) VALUES
	(1, 1,  35,  4, 33, 11, 35,  9, 18, '2026-05-27T20:33:45.913'),
	(2, 2, 100, 15, 20, 18,  5, 50,  8, '2026-05-27T20:30:44.433'),
	(3, 3,  14, 18, 15, 10,  4,  8,  5, '2026-05-27T20:28:23.633'),
	(4, 4,  42, 11, 48, 45,  6, 36, 36, '2026-05-27T13:12:08.993');

SET IDENTITY_INSERT dbo.Inventory OFF;
