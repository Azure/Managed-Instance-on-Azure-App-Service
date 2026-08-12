-- Product Inventory Table Creation Script
-- This script creates the ProductInventory table for managing stock levels by size

-- Create ProductInventory table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductInventory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductInventory](
        [InventoryID] [int] IDENTITY(1,1) NOT NULL,
        [ProductID] [int] NOT NULL,
   [SizeXS] [int] NOT NULL DEFAULT 0,
        [SizeS] [int] NOT NULL DEFAULT 0,
        [SizeM] [int] NOT NULL DEFAULT 0,
        [SizeL] [int] NOT NULL DEFAULT 0,
     [SizeXL] [int] NOT NULL DEFAULT 0,
        [SizeXXL] [int] NOT NULL DEFAULT 0,
        [SizeXXXL] [int] NOT NULL DEFAULT 0,
   [LastUpdated] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_ProductInventory] PRIMARY KEY CLUSTERED ([InventoryID] ASC),
        CONSTRAINT [FK_ProductInventory_Products] FOREIGN KEY([ProductID]) 
      REFERENCES [dbo].[Products] ([ProductID]) ON DELETE CASCADE,
        CONSTRAINT [UQ_ProductInventory_ProductID] UNIQUE ([ProductID])
    )
    
    PRINT 'ProductInventory table created successfully'
END
ELSE
BEGIN
    PRINT 'ProductInventory table already exists'
END
GO

-- Sample inventory data based on the clothing inventory example
-- Modify this data to match your actual products

-- Insert sample inventory for existing products
-- Assuming you have products with IDs 1-4 in your Products table

-- Product 1: Navy Single-Breasted Slim Fit Formal Blazer
IF NOT EXISTS (SELECT 1 FROM ProductInventory WHERE ProductID = 1)
BEGIN
    INSERT INTO ProductInventory (ProductID, SizeXS, SizeS, SizeM, SizeL, SizeXL, SizeXXL, SizeXXXL)
    VALUES (1, 0, 0, 0, 0, 0, 0, 0)
    PRINT 'Inventory added for Product ID 1'
END

-- Product 2: White & Navy Blue Slim Fit Printed Casual Shirt
IF NOT EXISTS (SELECT 1 FROM ProductInventory WHERE ProductID = 2)
BEGIN
    INSERT INTO ProductInventory (ProductID, SizeXS, SizeS, SizeM, SizeL, SizeXL, SizeXXL, SizeXXXL)
    VALUES (2, 8, 15, 0, 18, 12, 0, 4)
    PRINT 'Inventory added for Product ID 2'
END

-- Product 3: Red Slim Fit Checked Casual Shirt
IF NOT EXISTS (SELECT 1 FROM ProductInventory WHERE ProductID = 3)
BEGIN
    INSERT INTO ProductInventory (ProductID, SizeXS, SizeS, SizeM, SizeL, SizeXL, SizeXXL, SizeXXXL)
    VALUES (3, 5, 8, 12, 10, 4, 2, 5)
    PRINT 'Inventory added for Product ID 3'
END

-- Product 4: Navy Blue Washed Denim Jacket
IF NOT EXISTS (SELECT 1 FROM ProductInventory WHERE ProductID = 4)
BEGIN
    INSERT INTO ProductInventory (ProductID, SizeXS, SizeS, SizeM, SizeL, SizeXL, SizeXXL, SizeXXXL)
    VALUES (4, 0, 10, 15, 12, 8, 3, 4)
    PRINT 'Inventory added for Product ID 4'
END
GO

-- Create index for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductInventory_ProductID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductInventory_ProductID] 
    ON [dbo].[ProductInventory] ([ProductID] ASC)
    PRINT 'Index created on ProductID'
END
GO

-- Create trigger to update LastUpdated timestamp
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_ProductInventory_Update')
    DROP TRIGGER TR_ProductInventory_Update
GO

CREATE TRIGGER TR_ProductInventory_Update
ON ProductInventory
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ProductInventory
    SET LastUpdated = GETDATE()
    FROM ProductInventory pi
    INNER JOIN inserted i ON pi.InventoryID = i.InventoryID
END
GO

PRINT 'Product Inventory setup complete'
GO

-- View to check inventory status
CREATE OR ALTER VIEW vw_InventoryStatus
AS
SELECT 
    p.ProductID,
    p.ProductName,
    c.CategoryName,
    ISNULL(i.SizeXS, 0) AS SizeXS,
    ISNULL(i.SizeS, 0) AS SizeS,
    ISNULL(i.SizeM, 0) AS SizeM,
    ISNULL(i.SizeL, 0) AS SizeL,
    ISNULL(i.SizeXL, 0) AS SizeXL,
    ISNULL(i.SizeXXL, 0) AS SizeXXL,
    ISNULL(i.SizeXXXL, 0) AS SizeXXXL,
    (ISNULL(i.SizeXS, 0) + ISNULL(i.SizeS, 0) + ISNULL(i.SizeM, 0) + 
     ISNULL(i.SizeL, 0) + ISNULL(i.SizeXL, 0) + ISNULL(i.SizeXXL, 0) + 
     ISNULL(i.SizeXXXL, 0)) AS TotalStock,
    i.LastUpdated
FROM Products p
LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
GO

PRINT 'Inventory view created'
GO

-- Sample query to verify setup
SELECT * FROM vw_InventoryStatus
ORDER BY ProductName
GO
