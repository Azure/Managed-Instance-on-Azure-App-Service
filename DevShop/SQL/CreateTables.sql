IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
    	CREATE TABLE [dbo].[Categories](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](50) NULL,
 CONSTRAINT [PK_DevShop_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')


   CREATE TABLE [dbo].[Products](
	[ProductID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryID] [int] NOT NULL,
	[ProductBrand] [nvarchar](max) NOT NULL,
	[ProductName] [nvarchar](max) NOT NULL,
	[ProductImage] [nvarchar](max) NOT NULL,
	[ProductPrice] [int] NOT NULL,
	[ProductDescription] [nvarchar](max) NOT NULL,
	[ProductColor] [nvarchar](max) NOT NULL,
	[ProductGender] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_DevShop_Product_Master] PRIMARY KEY CLUSTERED 
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Categories] ([CategoryID])
GO

ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories]
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')

CREATE TABLE [dbo].[Orders](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NULL,
	[ProductDesc] [nvarchar](max) NULL,
	[CatID] [int] NULL,
	[Quantity] [int] NULL,
	[TotalPrice] [int] NULL,
	[CatName] [nvarchar](max) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ProductInventory]    Script Date: 5/27/2026 4:37:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductInventory](
	[InventoryID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[SizeXS] [int] NOT NULL,
	[SizeS] [int] NOT NULL,
	[SizeM] [int] NOT NULL,
	[SizeL] [int] NOT NULL,
	[SizeXL] [int] NOT NULL,
	[SizeXXL] [int] NOT NULL,
	[SizeXXXL] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_ProductInventory] PRIMARY KEY CLUSTERED 
(
	[InventoryID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ProductInventory_ProductID] UNIQUE NONCLUSTERED 
(
	[ProductID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeXS]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeS]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeM]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeL]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeXL]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeXXL]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SizeXXXL]
GO

ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT (getdate()) FOR [LastUpdated]
GO

ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD  CONSTRAINT [FK_ProductInventory_Products] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Products] ([ProductID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ProductInventory] CHECK CONSTRAINT [FK_ProductInventory_Products]
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductReviews' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ProductReviews](
        [ReviewID] [int] IDENTITY(1,1) NOT NULL,
        [ProductID] [int] NOT NULL,
        [ReviewerName] [nvarchar](200) NOT NULL,
        [Rating] [int] NOT NULL,
        [Comments] [nvarchar](4000) NOT NULL,
        [ReviewDate] [datetime2](7) NOT NULL CONSTRAINT [DF_ProductReviews_ReviewDate] DEFAULT (sysutcdatetime()),
        [Category] [nvarchar](200) NULL,
        [Product] [nvarchar](500) NULL,
        CONSTRAINT [PK_ProductReviews] PRIMARY KEY CLUSTERED ([ReviewID] ASC),
        CONSTRAINT [CK_ProductReviews_Rating] CHECK ([Rating] >= 1 AND [Rating] <= 5),
        CONSTRAINT [FK_ProductReviews_Products] FOREIGN KEY([ProductID])
            REFERENCES [dbo].[Products] ([ProductID]) ON DELETE CASCADE
    ) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductReviews_ProductID_ReviewDate' AND object_id = OBJECT_ID(N'[dbo].[ProductReviews]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductReviews_ProductID_ReviewDate]
        ON [dbo].[ProductReviews] ([ProductID] ASC, [ReviewDate] DESC)
END
GO
